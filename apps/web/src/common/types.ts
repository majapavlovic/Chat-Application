export type ChatPayload = {
  roomId: string;
  user: string;
  message: string;
  ts?: string;
};

export type MessageDto = {
  messageId: string;
  roomId: string;
  senderId: string;
  text: string;
  persistedAtUtc: string;
};
