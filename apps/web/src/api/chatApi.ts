import {
  ConversationDto,
  CurrentUserDto,
  MessageDto,
  UserDto,
} from "../common/types";

const api = process.env.REACT_APP_API_ENDPOINT;

export type AuthResult = {
  userId: string;
  username: string;
  displayName: string;
};

let pendingRefresh: Promise<boolean> | null = null;

function doRefresh(): Promise<boolean> {
  if (!pendingRefresh) {
    pendingRefresh = fetch(`${api}/api/auth/refresh`, {
      method: "POST",
      credentials: "include",
    })
      .then((r) => r.ok)
      .finally(() => {
        pendingRefresh = null;
      });
  }
  return pendingRefresh;
}

async function fetchWithRefresh(
  input: RequestInfo,
  init?: RequestInit,
): Promise<Response> {
  let res = await fetch(input, { ...init, credentials: "include" });

  if (res.status === 401) {
    const refreshed = await doRefresh();
    if (refreshed) {
      res = await fetch(input, { ...init, credentials: "include" });
    }
  }

  return res;
}

export async function logout(): Promise<void> {
  await fetch(`${api}/api/auth/logout`, {
    method: "POST",
    credentials: "include",
  });
}

export async function registerAuth(payload: {
  username: string;
  displayName: string;
  password: string;
}): Promise<AuthResult> {
  const res = await fetch(`${api}/api/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error(`Register failed: ${res.status}`);
  return res.json();
}

export async function loginAuth(payload: {
  username: string;
  password: string;
}): Promise<AuthResult> {
  const res = await fetch(`${api}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error(`Login failed: ${res.status}`);
  return res.json();
}

export async function fetchMe(): Promise<CurrentUserDto> {
  const res = await fetchWithRefresh(`${api}/api/auth/me`);
  if (!res.ok) throw new Error(`Me fetch failed: ${res.status}`);
  return res.json();
}

export async function fetchUsers(): Promise<UserDto[]> {
  const res = await fetchWithRefresh(`${api}/api/users`);
  if (!res.ok) throw new Error(`Users fetch failed: ${res.status}`);
  return res.json();
}

export async function createUser(
  userId: string,
  username: string,
  displayName: string,
): Promise<UserDto> {
  const res = await fetchWithRefresh(`${api}/api/users`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ userId, username, displayName }),
  });

  if (!res.ok) throw new Error(`Create user failed: ${res.status}`);
  return res.json();
}

export async function updatePresence(
  userId: string,
  isOnline: boolean,
): Promise<void> {
  const res = await fetchWithRefresh(
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

  const res = await fetchWithRefresh(
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
  const res = await fetchWithRefresh(`${api}/api/conversations`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error(`Create conversation failed: ${res.status}`);
  return res.json();
}

export async function fetchMessages(
  conversationId: string,
): Promise<MessageDto[]> {
  if (!conversationId.trim()) return [];
  const res = await fetchWithRefresh(
    `${api}/api/chat/conversations/${encodeURIComponent(conversationId)}/messages`,
  );
  if (!res.ok) throw new Error(`History fetch failed: ${res.status}`);
  return res.json();
}
