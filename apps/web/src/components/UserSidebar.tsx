import { UserDto } from "../common/types";

type UserSidebarProps = {
  users: UserDto[];
  activeUserId: string;
  directUserId: string;
  groupName: string;
  groupMembers: string;
  onDirectUserIdChange: (value: string) => void;
  onGroupNameChange: (value: string) => void;
  onGroupMembersChange: (value: string) => void;
  onSelectUser: (userId: string) => void;
  onCreateDirect: () => void;
  onCreateGroup: () => void;
};

export function UserSidebar({
  users,
  activeUserId,
  directUserId,
  groupName,
  groupMembers,
  onDirectUserIdChange,
  onGroupNameChange,
  onGroupMembersChange,
  onSelectUser,
  onCreateDirect,
  onCreateGroup,
}: UserSidebarProps) {
  return (
    <div>
      <div style={{ border: "1px solid #ddd", padding: 12, marginBottom: 12 }}>
        <h3>Users</h3>
        <div style={{ display: "flex", flexDirection: "column", gap: 8 }}>
          {users.map((user) => (
            <button
              key={user.userId}
              onClick={() => onSelectUser(user.userId)}
              style={{
                textAlign: "left",
                padding: 8,
                border: "1px solid #ccc",
                background: user.userId === activeUserId ? "#eee" : "white",
              }}
            >
              {user.displayName} ({user.username}) ({user.userId}){" "}
              {user.isOnline ? "[online]" : "[offline]"}
            </button>
          ))}
        </div>
      </div>

      <div style={{ border: "1px solid #ddd", padding: 12, marginBottom: 12 }}>
        <h3>Create Direct Chat</h3>
        <input
          value={directUserId}
          onChange={(e) => onDirectUserIdChange(e.target.value)}
          placeholder='other user id'
          style={{ width: "100%", padding: 8, marginBottom: 8 }}
        />
        <button onClick={onCreateDirect} disabled={!activeUserId}>
          Create direct
        </button>
      </div>

      <div style={{ border: "1px solid #ddd", padding: 12 }}>
        <h3>Create Group</h3>
        <input
          value={groupName}
          onChange={(e) => onGroupNameChange(e.target.value)}
          placeholder='group name'
          style={{ width: "100%", padding: 8, marginBottom: 8 }}
        />
        <input
          value={groupMembers}
          onChange={(e) => onGroupMembersChange(e.target.value)}
          placeholder='other user ids, comma separated'
          style={{ width: "100%", padding: 8, marginBottom: 8 }}
        />
        <button onClick={onCreateGroup} disabled={!activeUserId}>
          Create group
        </button>
      </div>
    </div>
  );
}
