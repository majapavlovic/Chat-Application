import { ConversationDto, UserDto } from "../common/types";

const api = process.env.REACT_APP_API_ENDPOINT ?? "http://localhost:5046";

export async function fetchUsers(): Promise<UserDto[]> {
  const res = await fetch(`${api}/api/users`);
  if (!res.ok) throw new Error(`Users fetch failed: ${res.status}`);
  return res.json();
}

export async function createUser(
  userId: string,
  displayName: string,
): Promise<UserDto> {
  const res = await fetch(`${api}/api/users`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userId, displayName }),
  });

  if (!res.ok) throw new Error(`Create user failed: ${res.status}`);
  return res.json();
}

export async function updatePresence(
  userId: string,
  isOnline: boolean,
): Promise<void> {
  const res = await fetch(
    `${api}/api/users/${encodeURIComponent(userId)}/presence`,
    {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ isOnline }),
    },
  );

  if (!res.ok) throw new Error(`Update presence failed: ${res.status}`);
}

export async function fetchConversations(
  userId: string,
): Promise<ConversationDto[]> {
  if (!userId.trim()) return [];

  const res = await fetch(
    `${api}/api/conversations?userId=${encodeURIComponent(userId)}`,
  );
  if (!res.ok) throw new Error(`Conversations fetch failed: ${res.status}`);
  return res.json();
}

export async function createConversation(payload: {
  type: number;
  name: string | null;
  participantIds: string[];
}): Promise<ConversationDto> {
  const res = await fetch(`${api}/api/conversations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error(`Create conversation failed: ${res.status}`);
  return res.json();
}