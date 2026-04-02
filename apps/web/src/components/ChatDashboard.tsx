import { useEffect, useMemo, useState } from "react";
import {
  createConversation,
  createUser,
  fetchConversations,
  fetchMe,
  fetchUsers,
  logout,
  updatePresence,
} from "../api/chatApi";
import { ConversationDto, UserDto } from "../common/types";
import { Conversation } from "./ConversationComponent";
import { LoginPage } from "./LoginPage";
import { resetChatConnection } from "../signalr/connection";
import "./ChatLayout.css";

export function ChatDashboard() {
  const [users, setUsers] = useState<UserDto[]>([]);
  const [conversations, setConversations] = useState<ConversationDto[]>([]);
  const [activeUserId, setActiveUserId] = useState("");
  const [selectedConversationId, setSelectedConversationId] = useState("");
  const [directUserId, setDirectUserId] = useState("");
  const [groupName, setGroupName] = useState("");
  const [groupMembers, setGroupMembers] = useState<string[]>([]);
  const [panelError, setPanelError] = useState("");
  const [authUsernameInput, setAuthUsernameInput] = useState("");
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [showNewChat, setShowNewChat] = useState(false);
  const [newChatTab, setNewChatTab] = useState<"direct" | "group">("direct");

  const userMap = useMemo(
    () => Object.fromEntries(users.map((u) => [u.userId, u])),
    [users],
  );

  const otherUsers = useMemo(
    () => users.filter((u) => u.userId !== activeUserId),
    [users, activeUserId],
  );

  const conversationTitle = (conv: ConversationDto): string => {
    if (conv.name?.trim()) return conv.name.trim();
    if (conv.type === 1) {
      const otherId = conv.participantIds.find((id) => id !== activeUserId);
      return otherId
        ? (userMap[otherId]?.displayName ??
            userMap[otherId]?.username ??
            otherId)
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
      if (!String(e).includes("404")) throw e;
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
      if (current && items.some((u) => u.userId === current)) return current;
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

  // Session restore on mount
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

  // Reload conversations when active user changes
  useEffect(() => {
    if (!activeUserId) {
      setConversations([]);
      setSelectedConversationId("");
      return;
    }
    void loadConversations(activeUserId).catch(() => {});
  }, [activeUserId]);

  const handleAuth = async (
    userId: string,
    username: string,
    displayName: string,
  ) => {
    setActiveUserId(userId);
    setAuthUsernameInput(username);
    setIsAuthenticated(true);
    await ensureUserPresence(userId, username, displayName);
    await loadUsers(userId);
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
    setPanelError("");
    if (!activeUserId || !directUserId) return;
    try {
      const conv = await createConversation({
        type: 1,
        name: null,
        participantIds: [activeUserId, directUserId],
      });
      await loadConversations(activeUserId);
      setSelectedConversationId(conv.conversationId);
      setShowNewChat(false);
      setDirectUserId("");
    } catch (e) {
      setPanelError(String(e).replace(/^Error:\s*/, ""));
    }
  };

  const handleCreateGroup = async () => {
    setPanelError("");
    if (!activeUserId || groupMembers.length === 0) return;
    try {
      const conv = await createConversation({
        type: 2,
        name: groupName.trim() || null,
        participantIds: [activeUserId, ...groupMembers],
      });
      await loadConversations(activeUserId);
      setSelectedConversationId(conv.conversationId);
      setShowNewChat(false);
      setGroupName("");
      setGroupMembers([]);
    } catch (e) {
      setPanelError(String(e).replace(/^Error:\s*/, ""));
    }
  };

  const toggleGroupMember = (userId: string) =>
    setGroupMembers((prev) =>
      prev.includes(userId)
        ? prev.filter((id) => id !== userId)
        : [...prev, userId],
    );

  if (!isAuthenticated) {
    return (
      <LoginPage
        onAuth={(uid, uname, dname) => void handleAuth(uid, uname, dname)}
      />
    );
  }

  const selectedConv = conversations.find(
    (c) => c.conversationId === selectedConversationId,
  );
  const myDisplayName = userMap[activeUserId]?.displayName ?? authUsernameInput;

  return (
    <div className='chat-layout'>
      <aside className='chat-sidebar'>
        <div className='sidebar-header'>
          <div className='sidebar-avatar'>
            {myDisplayName[0]?.toUpperCase() ?? "?"}
          </div>
          <div className='sidebar-profile'>
            <div className='sidebar-name'>{myDisplayName}</div>
            <div className='sidebar-username'>@{authUsernameInput}</div>
          </div>
          <button
            className='sidebar-logout'
            onClick={() => void handleLogout()}
            title='Sign out'
          >
            Sign out
          </button>
        </div>

        <div className='sidebar-actions'>
          <button
            className='btn-new-chat'
            onClick={() => {
              setShowNewChat((v) => !v);
              setPanelError("");
            }}
          >
            {showNewChat ? "✕ Cancel" : "+ New Chat"}
          </button>
        </div>

        {showNewChat && (
          <div className='new-chat-panel'>
            <div className='new-chat-tabs'>
              <button
                className={newChatTab === "direct" ? "active" : ""}
                onClick={() => setNewChatTab("direct")}
              >
                Direct
              </button>
              <button
                className={newChatTab === "group" ? "active" : ""}
                onClick={() => setNewChatTab("group")}
              >
                Group
              </button>
            </div>

            {newChatTab === "direct" && (
              <>
                <label>Start chat with</label>
                <select
                  value={directUserId}
                  onChange={(e) => setDirectUserId(e.target.value)}
                >
                  <option value=''>— pick a user —</option>
                  {otherUsers.map((u) => (
                    <option key={u.userId} value={u.userId}>
                      {u.displayName} (@{u.username})
                    </option>
                  ))}
                </select>
                <button
                  className='btn-create'
                  disabled={!directUserId}
                  onClick={() => void handleCreateDirect()}
                >
                  Start chat
                </button>
              </>
            )}

            {newChatTab === "group" && (
              <>
                <label>Group name</label>
                <input
                  value={groupName}
                  onChange={(e) => setGroupName(e.target.value)}
                  placeholder='e.g. Project team'
                />
                <label>Members</label>
                <div className='group-member-list'>
                  {otherUsers.map((u) => (
                    <label key={u.userId} className='group-member-item'>
                      <input
                        type='checkbox'
                        checked={groupMembers.includes(u.userId)}
                        onChange={() => toggleGroupMember(u.userId)}
                      />
                      {u.displayName} (@{u.username})
                    </label>
                  ))}
                </div>
                <button
                  className='btn-create'
                  disabled={groupMembers.length === 0}
                  onClick={() => void handleCreateGroup()}
                >
                  Create group
                </button>
              </>
            )}

            {panelError && <div className='panel-error'>{panelError}</div>}
          </div>
        )}

        <div className='conv-list'>
          {conversations.map((conv) => (
            <div
              key={conv.conversationId}
              className={`conv-item ${conv.conversationId === selectedConversationId ? "active" : ""}`}
              onClick={() => setSelectedConversationId(conv.conversationId)}
            >
              <div className='conv-avatar'>
                {conversationTitle(conv)[0]?.toUpperCase() ?? "?"}
              </div>
              <div className='conv-info'>
                <div className='conv-title'>{conversationTitle(conv)}</div>
                <div className='conv-badge'>
                  {conv.type === 1
                    ? "Direct message"
                    : `Group · ${conv.participantIds.length} members`}
                </div>
              </div>
            </div>
          ))}
          {conversations.length === 0 && (
            <div className='conv-empty'>
              No conversations yet.
              <br />
              Click <strong>+ New Chat</strong> to start.
            </div>
          )}
        </div>
      </aside>

      {/* ── Main area ────────────────────────────────────────────────────── */}
      <main className='chat-main'>
        {selectedConversationId ? (
          <Conversation
            conversationId={selectedConversationId}
            senderId={activeUserId}
            title={selectedConv ? conversationTitle(selectedConv) : "Chat"}
            userMap={userMap}
          />
        ) : (
          <div className='chat-welcome'>
            <h3>Welcome, {myDisplayName}!</h3>
            <p>Select a conversation or start a new chat.</p>
          </div>
        )}
      </main>
    </div>
  );
}
