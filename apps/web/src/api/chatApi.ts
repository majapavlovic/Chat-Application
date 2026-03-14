import {
  AuthResponse,
  ConversationDto,
  CurrentUserDto,
  UserDto,
} from "../common/types";
import {
  clearAccessToken,
  getAccessToken,
  setAccessToken,
} from "../auth/tokenStore";

const api = process.env.REACT_APP_API_ENDPOINT ?? "http://localhost:5046";

function authHeaders(): HeadersInit {
  const token = getAccessToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export function logout(): void {
  clearAccessToken();
}

export async function registerAuth(payload: {
  username: string;
  displayName: string;
  password: string;
}): Promise<AuthResponse> {
  const res = await fetch(`${api}/api/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error(`Register failed: ${res.status}`);
  const data: AuthResponse = await res.json();
  setAccessToken(data.accessToken);
  return data;
}

export async function loginAuth(payload: {
  username: string;
  password: string;
}): Promise<AuthResponse> {
  const res = await fetch(`${api}/api/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error(`Login failed: ${res.status}`);
  const data: AuthResponse = await res.json();
  setAccessToken(data.accessToken);
  return data;
}

export async function fetchMe(): Promise<CurrentUserDto> {
  const res = await fetch(`${api}/api/auth/me`, {
    headers: {
      ...authHeaders(),
    },
  });

  if (!res.ok) throw new Error(`Me fetch failed: ${res.status}`);
  return res.json();
}

export async function fetchUsers(): Promise<UserDto[]> {
  const res = await fetch(`${api}/api/users`, {
    headers: {
      ...authHeaders(),
    },
  });
  if (!res.ok) throw new Error(`Users fetch failed: ${res.status}`);
  return res.json();
}

export async function createUser(
  userId: string,
  username: string,
  displayName: string,
): Promise<UserDto> {
  const res = await fetch(`${api}/api/users`, {
    method: "POST",
    headers: { "Content-Type": "application/json", ...authHeaders() },
    body: JSON.stringify({ userId, username, displayName }),
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
      headers: { "Content-Type": "application/json", ...authHeaders() },
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
    {
      headers: {
        ...authHeaders(),
      },
    },
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
    headers: { "Content-Type": "application/json", ...authHeaders() },
    body: JSON.stringify(payload),
  });

  if (!res.ok) throw new Error(`Create conversation failed: ${res.status}`);
  return res.json();
}
