import { useEffect, useMemo, useRef, useState } from "react";
import { getChatConnection } from "../signalr/connection";
import * as signalR from "@microsoft/signalr";
import { ChatPayload, MessageDto, UserDto } from "../common/types";
import { fetchMessages } from "../api/chatApi";
import "./ConversationComponent.css";

type ChatEntry = {
  id: string;
  senderId: string;
  text: string;
  ts: string;
  isSystem?: boolean;
};

type Props = {
  conversationId: string;
  senderId: string;
  title: string;
  userMap: Record<string, UserDto>;
};

export const Conversation = ({
  conversationId,
  senderId,
  title,
  userMap,
}: Props) => {
  const [text, setText] = useState("");
  const [log, setLog] = useState<ChatEntry[]>([]);
  const [connected, setConnected] = useState(false);

  const startingRef = useRef(false);
  const activeConversationRef = useRef("");
  const prevConversationRef = useRef("");
  const userMapRef = useRef(userMap);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    userMapRef.current = userMap;
  }, [userMap]);

  const connection = useMemo(() => getChatConnection(), []);

  const displayName = (userId: string) =>
    userMapRef.current[userId]?.displayName ??
    userMapRef.current[userId]?.username ??
    userId;

  useEffect(() => {
    const onSystem = (msg: string) => {
      setLog((prev) => [
        ...prev,
        {
          id: crypto.randomUUID(),
          senderId: "system",
          text: msg,
          ts: new Date().toISOString(),
          isSystem: true,
        },
      ]);
    };

    const onReceive = (payload: ChatPayload) => {
      setLog((prev) => [
        ...prev,
        {
          id: crypto.randomUUID(),
          senderId: payload.senderId,
          text: payload.message,
          ts: payload.ts ?? new Date().toISOString(),
        },
      ]);
    };

    connection.off("System");
    connection.off("ReceiveMessage");
    connection.on("System", onSystem);
    connection.on("ReceiveMessage", onReceive);

    connection.onclose(() => setConnected(false));
    connection.onreconnecting(() => setConnected(false));
    connection.onreconnected(async () => {
      setConnected(true);
      if (activeConversationRef.current)
        await connection
          .invoke("JoinConversation", activeConversationRef.current)
          .catch(() => {});
    });

    const start = async () => {
      if (startingRef.current) return;
      startingRef.current = true;
      try {
        if (connection.state === signalR.HubConnectionState.Disconnected)
          await connection.start();
        setConnected(true);
      } catch {
        setConnected(false);
      } finally {
        startingRef.current = false;
      }
    };
    void start();

    return () => {
      connection.off("System", onSystem);
      connection.off("ReceiveMessage", onReceive);
      if (connection.state === signalR.HubConnectionState.Connected)
        void connection.stop();
    };
  }, [connection]);

  // Load message history when conversation changes
  useEffect(() => {
    let cancelled = false;
    setLog([]);
    setText("");
    if (!conversationId) return;

    void (async () => {
      try {
        const items: MessageDto[] = await fetchMessages(conversationId);
        if (cancelled) return;
        const entries = items
          .sort(
            (a, b) =>
              new Date(a.persistedAtUtc).getTime() -
              new Date(b.persistedAtUtc).getTime(),
          )
          .map(
            (m): ChatEntry => ({
              id: m.messageId,
              senderId: m.senderId,
              text: m.text,
              ts: m.persistedAtUtc,
            }),
          );
        setLog(entries);
      } catch (e) {
        setLog([
          {
            id: "err",
            senderId: "system",
            text: String(e),
            ts: new Date().toISOString(),
            isSystem: true,
          },
        ]);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [conversationId]);

  // Join / leave conversation rooms
  useEffect(() => {
    activeConversationRef.current = conversationId;
    if (
      !conversationId ||
      connection.state !== signalR.HubConnectionState.Connected
    )
      return;

    void (async () => {
      if (
        prevConversationRef.current &&
        prevConversationRef.current !== conversationId
      )
        await connection
          .invoke("LeaveConversation", prevConversationRef.current)
          .catch(() => {});
      await connection
        .invoke("JoinConversation", conversationId)
        .catch(() => {});
      prevConversationRef.current = conversationId;
    })();
  }, [conversationId, connection, connected]);

  // Auto-scroll to bottom on new messages
  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [log]);

  const send = async () => {
    const msg = text.trim();
    if (!msg || !conversationId || !senderId) return;
    await connection.invoke(
      "SendMessage",
      conversationId,
      msg,
      crypto.randomUUID(),
    );
    setText("");
  };

  const formatTime = (iso: string) => {
    try {
      return new Date(iso).toLocaleTimeString([], {
        hour: "2-digit",
        minute: "2-digit",
      });
    } catch {
      return "";
    }
  };

  return (
    <div className='cc-root'>
      <div className='cc-header'>
        <div className='cc-header-avatar'>{title[0]?.toUpperCase() ?? "?"}</div>
        <div className='cc-header-info'>
          <div className='cc-header-title'>{title}</div>
          <div className={`cc-status ${connected ? "online" : "offline"}`}>
            <span className='cc-status-dot' />
            {connected ? "Connected" : "Connecting…"}
          </div>
        </div>
      </div>

      <div className='cc-messages'>
        {log.length === 0 && (
          <div className='cc-empty'>
            No messages yet. Start the conversation.
          </div>
        )}

        {log.map((entry) => {
          if (entry.isSystem) {
            return (
              <div key={entry.id} className='cc-system'>
                {entry.text}
              </div>
            );
          }

          const isMe = entry.senderId === senderId;

          return (
            <div
              key={entry.id}
              className={`cc-row ${isMe ? "mine" : "theirs"}`}
            >
              {!isMe && (
                <div className='cc-bubble-avatar'>
                  {displayName(entry.senderId)[0]?.toUpperCase() ?? "?"}
                </div>
              )}
              <div className='cc-bubble-col'>
                {!isMe && (
                  <div className='cc-sender'>{displayName(entry.senderId)}</div>
                )}
                <div className={`cc-bubble ${isMe ? "mine" : "theirs"}`}>
                  {entry.text}
                </div>
                <div className={`cc-time ${isMe ? "mine" : ""}`}>
                  {formatTime(entry.ts)}
                </div>
              </div>
            </div>
          );
        })}

        <div ref={bottomRef} />
      </div>

      <div className='cc-input-bar'>
        <input
          className='cc-input'
          value={text}
          onChange={(e) => setText(e.target.value)}
          placeholder={connected ? "Type a message…" : "Connecting…"}
          disabled={!connected || !conversationId}
          onKeyDown={(e) => {
            if (e.key === "Enter" && !e.shiftKey) {
              e.preventDefault();
              void send();
            }
          }}
        />
        <button
          className='cc-send-btn'
          onClick={() => void send()}
          disabled={!connected || !conversationId || !text.trim()}
        >
          <svg viewBox='0 0 24 24' fill='currentColor' width='20' height='20'>
            <path d='M2 21l21-9L2 3v7l15 2-15 2v7z' />
          </svg>
        </button>
      </div>
    </div>
  );
};
