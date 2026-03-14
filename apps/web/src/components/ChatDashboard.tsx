import { useEffect, useState } from "react";
import {
  createConversation,
  createUser,
  fetchConversations,
  fetchUsers,
  updatePresence,
} from "../api/chatApi";
import { ConversationDto, UserDto } from "../common/types";
import { Conversation } from "./ConversationComponent";
import { UserSidebar } from "./UserSidebar";

export function ChatDashboard() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [activeUserId, setActiveUserId] = useState("");
  const [selectedConversationId, setSelectedConversationId] = useState("");
  const [newUserId, setNewUserId] = useState("");
  const [newDisplayName, setNewDisplayName] = useState("");
  const [directUserId, setDirectUserId] = useState("");
  const [groupName, setGroupName] = useState("");
  const [groupMembers, setGroupMembers] = useState("");
  const [error, setError] = useState("");

  const loadUsers = async () => {
    const items = await fetchUsers();
    setUsers(items);
    if (!activeUserId && items.length > 0) {
      setActiveUserId(items[0].userId);
    }
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
    void loadUsers().catch((e) => setError(String(e)));
  }, []);

  useEffect(() => {
    if (!activeUserId) {
      setConversations([]);
      setSelectedConversationId("");
      return;
    }

    void loadConversations(activeUserId).catch((e) => setError(String(e)));
  }, [activeUserId]);

  const handleCreateUser = async () => {
    setError("");
    try {
      const created = await createUser(newUserId.trim(), newDisplayName.trim());
      await updatePresence(created.userId, true);
      await loadUsers();
      setActiveUserId(created.userId);
      setNewUserId("");
      setNewDisplayName("");
    } catch (e) {
      setError(String(e));
    }
  };

  const handleSelectUser = async (userId: string) => {
    setError("");
    try {
      if (activeUserId && activeUserId !== userId) {
        await updatePresence(activeUserId, false);
      }

      await updatePresence(userId, true);
      setActiveUserId(userId);
      await loadUsers();
    } catch (e) {
      setError(String(e));
    }
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
          newUserId={newUserId}
          newDisplayName={newDisplayName}
          directUserId={directUserId}
          groupName={groupName}
          groupMembers={groupMembers}
          onNewUserIdChange={setNewUserId}
          onNewDisplayNameChange={setNewDisplayName}
          onDirectUserIdChange={setDirectUserId}
          onGroupNameChange={setGroupName}
          onGroupMembersChange={setGroupMembers}
          onCreateUser={() => void handleCreateUser()}
          onSelectUser={(userId) => void handleSelectUser(userId)}
          onCreateDirect={() => void handleCreateDirect()}
          onCreateGroup={() => void handleCreateGroup()}
        />

        <div>
          <div
            style={{ border: "1px solid #ddd", padding: 12, marginBottom: 12 }}
          >
            <h3>Conversations for {activeUserId || "-"}</h3>
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
                  {(conversation.name && conversation.name.trim()) ||
                    (conversation.type === 1 ? "Direct chat" : "Group")}
                  <div>{conversation.conversationId}</div>
                  <div>{conversation.participantIds.join(", ")}</div>
                </button>
              ))}
            </div>
          </div>

          {error ? (
            <div style={{ color: "crimson", marginBottom: 12 }}>{error}</div>
          ) : null}

          <Conversation
            conversationId={selectedConversationId}
            senderId={activeUserId}
          />
        </div>
      </div>
    </div>
  );
}
