export type ChatPayload = {
  conversationId: string;
  senderId: string;
  message: string;
  ts?: string;
};

export type MessageDto = {
  messageId: string;
  conversationId: string;
  senderId: string;
  text: string;
  persistedAtUtc: string;
};

export type UserDto = {
  userId: string;
  username: string;
  displayName: string;
  isOnline: boolean;
  lastSeenAtUtc?: string | null;
  createdAtUtc: string;
};

export type ConversationDto = {
  conversationId: string;
  type: number;
  name?: string | null;
  createdAtUtc: string;
  participantIds: string[];
};

export type AuthResponse = {
  userId: string;
  username: string;
  displayName: string;
};

export type CurrentUserDto = {
  userId: string;
  username: string;
  displayName: string;
};
