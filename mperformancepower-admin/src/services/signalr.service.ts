import * as signalR from '@microsoft/signalr'

let connection: signalR.HubConnection | null = null

export async function startSignalR(token: string): Promise<signalR.HubConnection> {
  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${import.meta.env.VITE_API_URL.replace('/api', '')}/api/hubs/notifications`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  await connection.start()
  return connection
}

export async function stopSignalR(): Promise<void> {
  if (connection) {
    await connection.stop()
    connection = null
  }
}
