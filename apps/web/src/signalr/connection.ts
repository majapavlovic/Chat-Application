import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

export function getChatConnection() {
  if (connection) return connection;

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${process.env.REACT_APP_API_ENDPOINT}/chathub`, {
      withCredentials: true,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

  return connection;
}

export async function resetChatConnection(): Promise<void> {
  if (connection) {
    const conn = connection;
    connection = null;
    await conn.stop();
  }
}
