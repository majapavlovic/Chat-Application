import { useEffect, useMemo, useRef, useState } from "react";
import { getChatConnection } from "../signalr/connection";
import * as signalR from "@microsoft/signalr";
import { ChatPayload } from "../common/types";

export const ChatRoom = () => {
  const [roomId, setRoomId] = useState("room-1");

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
      setLog((prev) => ["[system] Reconnected ✅", ...prev]);
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

  const send = async () => {
    const msg = text.trim();
    if (!msg) return;

    try {
      await connection.invoke("SendMessage", roomId, msg);
      setText("");
    } catch (err) {
      setLog((prev) => [`Send error: ${String(err)}`, ...prev]);
    }
  };

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
