import { useEffect, useMemo, useState } from "react";
import {
  createConversation,
  createUser,
  fetchConversations,
  fetchMe,
  fetchUsers,
  loginAuth,
  logout,
  registerAuth,
  updatePresence,
} from "../api/chatApi";
import { ConversationDto, UserDto } from "../common/types";
import { Conversation } from "./ConversationComponent";
import { UserSidebar } from "./UserSidebar";
import { resetChatConnection } from "../signalr/connection";

export function ChatDashboard() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [activeUserId, setActiveUserId] = useState("");
  const [selectedConversationId, setSelectedConversationId] = useState("");
  const [directUserId, setDirectUserId] = useState("");
  const [groupName, setGroupName] = useState("");
  const [groupMembers, setGroupMembers] = useState("");
  const [error, setError] = useState("");
  const [authUsernameInput, setAuthUsernameInput] = useState("");
  const [authDisplayNameInput, setAuthDisplayNameInput] = useState("");
  const [authPasswordInput, setAuthPasswordInput] = useState("");
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  const userMap = useMemo(
    () => Object.fromEntries(users.map((u) => [u.userId, u])),
    [users],
  );

  const conversationTitle = (conv: ConversationDto): string => {
    if (conv.name?.trim()) return conv.name.trim();
    if (conv.type === 1) {
      const otherId = conv.participantIds.find((id) => id !== activeUserId);
      return otherId
        ? (userMap[otherId]?.displayName ?? userMap[otherId]?.username ?? otherId)
        : "Direct chat";
    }
    const names = conv.participantIds
      .filter((id) => id !== activeUserId)
      .map((id) => userMap[id]?.displayName ?? id)
      .join(", ");
    return names || "Group";
  };

  const ensureUserPresence = async (
    userId: string,
    username: string,
    displayName: string,
  ) => {
    try {
      await updatePresence(userId, true);
    } catch (e) {
      const message = String(e);
      if (!message.includes("404")) {
        throw e;
      }

      await createUser(userId, username, displayName);
      await updatePresence(userId, true);
    }
  };

  const loadUsers = async (preferredUserId?: string) => {
    const items = await fetchUsers();
    setUsers(items);

    if (preferredUserId && items.some((u) => u.userId === preferredUserId)) {
      setActiveUserId(preferredUserId);
      return;
    }

    setActiveUserId((current) => {
      if (current && items.some((u) => u.userId === current)) {
        return current;
      }

      return items[0]?.userId ?? "";
    });
  };

  const loadConversations = async (userId: string) => {
    const items = await fetchConversations(userId);
    setConversations(items);
    setSelectedConversationId((current) =>
      items.some((item) => item.conversationId === current)
        ? current
        : (items[0]?.conversationId ?? ""),
    );
  };

  useEffect(() => {
    void (async () => {
      try {
        const me = await fetchMe();
        setIsAuthenticated(true);
        setActiveUserId(me.userId);
        setAuthUsernameInput(me.username);
        await ensureUserPresence(me.userId, me.username, me.displayName);
        await loadUsers(me.userId);
      } catch {
        setIsAuthenticated(false);
      }
    })();
  }, []);

  useEffect(() => {
    if (!activeUserId) {
      setConversations([]);
      setSelectedConversationId("");
      return;
    }

    void loadConversations(activeUserId).catch((e) => setError(String(e)));
  }, [activeUserId]);

  const handleSelectUser = (userId: string) => {
    setError("");
    if (userId !== activeUserId) {
      setDirectUserId(userId);
    }
  };

  const handleRegister = async () => {
    setError("");
    try {
      await logout();
      await resetChatConnection();
      setIsAuthenticated(false);
      setActiveUserId("");

      const auth = await registerAuth({
        username: authUsernameInput.trim(),
        displayName: authDisplayNameInput.trim(),
        password: authPasswordInput,
      });

      setIsAuthenticated(true);
      setActiveUserId(auth.userId);
      setAuthUsernameInput(auth.username);
      await ensureUserPresence(auth.userId, auth.username, auth.displayName);
      await loadUsers(auth.userId);
    } catch (e) {
      setError(String(e));
    }
  };

  const handleLogin = async () => {
    setError("");
    try {
      await logout();
      await resetChatConnection();
      setIsAuthenticated(false);
      setActiveUserId("");

      await loginAuth({
        username: authUsernameInput.trim(),
        password: authPasswordInput,
      });

      const me = await fetchMe();
      setIsAuthenticated(true);
      setActiveUserId(me.userId);
      setAuthUsernameInput(me.username);
      await ensureUserPresence(me.userId, me.username, me.displayName);
      await loadUsers(me.userId);
    } catch (e) {
      setError(String(e));
    }
  };

  const handleLogout = async () => {
    if (activeUserId) {
      try {
        await updatePresence(activeUserId, false);
      } catch {}
    }

    await logout();
    await resetChatConnection();
    setIsAuthenticated(false);
    setActiveUserId("");
    setConversations([]);
    setSelectedConversationId("");
  };

  const handleCreateDirect = async () => {
    setError("");
    if (!activeUserId || !directUserId.trim()) return;

    try {
      const conversation = await createConversation({
        type: 1,
        name: null,
        participantIds: [activeUserId, directUserId.trim()],
      });

      await loadConversations(activeUserId);
      setSelectedConversationId(conversation.conversationId);
    } catch (e) {
      setError(String(e));
    }
  };

  const handleCreateGroup = async () => {
    setError("");
    if (!activeUserId) return;

    const extraMembers = groupMembers
      .split(",")
      .map((item) => item.trim())
      .filter(Boolean);

    try {
      const conversation = await createConversation({
        type: 2,
        name: groupName.trim() || null,
        participantIds: [activeUserId, ...extraMembers],
      });

      await loadConversations(activeUserId);
      setSelectedConversationId(conversation.conversationId);
      setGroupName("");
      setGroupMembers("");
    } catch (e) {
      setError(String(e));
    }
  };

  return (
    <div style={{ padding: 16, fontFamily: "sans-serif" }}>
      <h1>Dashboard</h1>

      <div style={{ border: "1px solid #ddd", padding: 12, marginBottom: 12 }}>
        <h3>Auth</h3>
        <div style={{ display: "flex", gap: 8, marginBottom: 8 }}>
          <input
            value={authUsernameInput}
            onChange={(e) => setAuthUsernameInput(e.target.value)}
            placeholder='username'
            style={{ flex: 1, padding: 8 }}
          />
          <input
            value={authDisplayNameInput}
            onChange={(e) => setAuthDisplayNameInput(e.target.value)}
            placeholder='display name (register)'
            style={{ flex: 1, padding: 8 }}
          />
          <input
            type='password'
            value={authPasswordInput}
            onChange={(e) => setAuthPasswordInput(e.target.value)}
            placeholder='password'
            style={{ flex: 1, padding: 8 }}
          />
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <button onClick={() => void handleRegister()}>Register</button>
          <button onClick={() => void handleLogin()}>Login</button>
          <button
            onClick={() => void handleLogout()}
            disabled={!isAuthenticated}
          >
            Logout
          </button>
          <div style={{ alignSelf: "center" }}>
            Current user: {(userMap[activeUserId]?.displayName ?? authUsernameInput) || "-"}
          </div>
        </div>
      </div>

      <div
        style={{
          display: "grid",
          gridTemplateColumns: "320px 1fr",
          gap: 16,
          alignItems: "start",
        }}
      >
        <UserSidebar
          users={users}
          activeUserId={activeUserId}
          directUserId={directUserId}
          groupName={groupName}
          groupMembers={groupMembers}
          onDirectUserIdChange={setDirectUserId}
          onGroupNameChange={setGroupName}
          onGroupMembersChange={setGroupMembers}
          onSelectUser={handleSelectUser}
          onCreateDirect={() => void handleCreateDirect()}
          onCreateGroup={() => void handleCreateGroup()}
        />

        <div>
          <div
            style={{ border: "1px solid #ddd", padding: 12, marginBottom: 12 }}
          >
            <h3>Conversations for {(userMap[activeUserId]?.displayName ?? authUsernameInput) || "-"}</h3>
            <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
              {conversations.map((conversation) => (
                <button
                  key={conversation.conversationId}
                  onClick={() =>
                    setSelectedConversationId(conversation.conversationId)
                  }
                  style={{
                    textAlign: "left",
                    padding: 8,
                    border: "1px solid #ccc",
                    background:
                      conversation.conversationId === selectedConversationId
                        ? "#eee"
                        : "white",
                  }}
                >
                  {conversationTitle(conversation)}
                </button>
              ))}
            </div>
          </div>

          {error ? (
            <div style={{ color: "crimson", marginBottom: 12 }}>{error}</div>
          ) : null}

          {isAuthenticated ? (
            <Conversation
              conversationId={selectedConversationId}
              senderId={activeUserId}
              title={conversationTitle(
                conversations.find(
                  (c) => c.conversationId === selectedConversationId,
                ) ?? { conversationId: "", type: 0, name: null, createdAtUtc: "", participantIds: [] },
              )}
              userMap={userMap}
            />
          ) : (
            <div style={{ color: "#555" }}>
              Login required to connect to chat.
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
