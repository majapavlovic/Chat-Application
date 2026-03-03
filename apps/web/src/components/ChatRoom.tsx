import { useEffect, useMemo, useRef, useState } from "react";
import { getChatConnection } from "../signalr/connection";
import * as signalR from "@microsoft/signalr";
import { ChatPayload, MessageDto } from "../common/types";

async function fetchHistory(roomId: string): Promise<MessageDto[]> {
  const res = await fetch(
    `http://localhost:5046/api/chat/rooms/${encodeURIComponent(
      roomId
    )}/messages`
  );
  if (!res.ok) throw new Error(`History fetch failed: ${res.status}`);
  return res.json();
}

export const ChatRoom = () => {
  const [roomId, setRoomId] = useState("");

  const [text, setText] = useState("");
  const [log, setLog] = useState<string[]>([]);
  const [connected, setConnected] = useState(false);

  const startingRef = useRef(false);
  const mountedRef = useRef(false);

  const connection = useMemo(() => getChatConnection(), []);

  useEffect(() => {
    mountedRef.current = true;

    const onSystem = (msg: string) => {
      setLog((prev) => [`[system] ${msg}`, ...prev]);
    };

    const onReceive = (payload: ChatPayload) => {
      setLog((prev) => [
        `[${payload.roomId}] ${payload.user}: ${payload.message}`,
        ...prev,
      ]);
    };

    const onClose = (err?: Error) => {
      setConnected(false);
      setLog((prev) => [
        `[system] Closed${err ? `: ${err.message}` : ""}`,
        ...prev,
      ]);
    };

    const onReconnecting = (err?: Error) => {
      setConnected(false);
      setLog((prev) => [
        `[system] Reconnecting...${err ? ` ${err.message}` : ""}`,
        ...prev,
      ]);
    };

    const onReconnected = async () => {
      setConnected(true);
      setLog((prev) => ["[system] Reconnected", ...prev]);
      try {
        await connection.invoke("JoinRoom", roomId);
        setLog((prev) => [`[system] Re-joined ${roomId}`, ...prev]);
      } catch (e) {
        setLog((prev) => [`[system] Re-join failed: ${String(e)}`, ...prev]);
      }
    };

    connection.off("System");
    connection.off("ReceiveMessage");
    connection.on("System", onSystem);
    connection.on("ReceiveMessage", onReceive);

    connection.onclose(onClose);
    connection.onreconnecting(onReconnecting);
    connection.onreconnected(onReconnected);

    const start = async () => {
      if (startingRef.current) return;
      startingRef.current = true;

      try {
        if (connection.state === signalR.HubConnectionState.Disconnected) {
          await connection.start();
        }
        if (!mountedRef.current) {
          try {
            await connection.stop();
          } catch {}
          return;
        }

        setConnected(true);
        setLog((prev) => ["Connected", ...prev]);
      } catch (err) {
        setConnected(false);
        setLog((prev) => [`Connect error: ${String(err)}`, ...prev]);

        try {
          if (connection.state !== signalR.HubConnectionState.Connecting) {
            await connection.stop();
          }
        } catch {}
      } finally {
        startingRef.current = false;
      }
    };

    start();

    return () => {
      mountedRef.current = false;

      if (connection.state === signalR.HubConnectionState.Connected) {
        connection.stop();
      }
      connection.off("System", onSystem);
      connection.off("ReceiveMessage", onReceive);
    };
  }, [connection]);

  const join = async () => {
    try {
      await connection.invoke("JoinRoom", roomId);
      setLog((prev) => [`[system] Joined ${roomId}`, ...prev]);
    } catch (err) {
      setLog((prev) => [`Join error: ${String(err)}`, ...prev]);
    }
  };

  useEffect(() => {
    let cancelled = false;
    setLog([]);
    setText("");

    (async () => {
      try {
        const items = await fetchHistory(roomId);
        if (cancelled) return;

        const lines = items
          .sort(
            (a, b) =>
              new Date(a.persistedAtUtc).getTime() -
              new Date(b.persistedAtUtc).getTime()
          )
          .map((m) => `[${m.roomId}] ${m.senderId}: ${m.text}`);

        setLog(lines.reverse());
      } catch (e) {
        setLog([`[system] ${String(e)}`]);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [roomId]);

  const send = async () => {
    const msg = text.trim();
    if (!msg) return;

    const clientMessageId = crypto.randomUUID();
    await connection.invoke("SendMessage", roomId, msg, clientMessageId);

    setText("");
  };

  const prevRoomRef = useRef<string | null>(null);

  useEffect(() => {
    (async () => {
      try {
        if (prevRoomRef.current && prevRoomRef.current !== roomId) {
          await connection.invoke("LeaveRoom", prevRoomRef.current);
        }

        await connection.invoke("JoinRoom", roomId);
        prevRoomRef.current = roomId;
      } catch (e) {
        setLog((p) => [`[system] Join/Leave error: ${String(e)}`, ...p]);
      }
    })();
  }, [roomId, connection]);

  return (
    <div style={{ padding: 16, fontFamily: "sans-serif", maxWidth: 700 }}>
      <h2>Chat Gateway (SignalR)</h2>

      <div style={{ display: "flex", gap: 8, marginBottom: 12 }}>
        <input
          value={roomId}
          onChange={(e) => setRoomId(e.target.value)}
          placeholder='room id'
          style={{ flex: 1, padding: 8 }}
        />
        <button onClick={join} disabled={!connected}>
          Join
        </button>
      </div>

      <div style={{ display: "flex", gap: 8, marginBottom: 12 }}>
        <input
          value={text}
          onChange={(e) => setText(e.target.value)}
          placeholder='message...'
          style={{ flex: 1, padding: 8 }}
          onKeyDown={(e) => (e.key === "Enter" ? send() : null)}
        />
        <button onClick={send} disabled={!connected}>
          Send
        </button>
      </div>

      <div style={{ opacity: 0.8, marginBottom: 8 }}>
        Status: {connected ? "Connected" : "Disconnected"}
      </div>

      <div
        style={{
          border: "1px solid #ddd",
          borderRadius: 8,
          padding: 12,
          height: 350,
          overflow: "auto",
        }}
      >
        {log.map((l, i) => (
          <div key={i} style={{ marginBottom: 6 }}>
            {l}
          </div>
        ))}
      </div>
    </div>
  );
};
