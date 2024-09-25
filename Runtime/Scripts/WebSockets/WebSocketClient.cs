using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using UnityEngine.Networking;

public class WebSocketClient : MonoBehaviour
{

    public string Username = "";
    private ClientWebSocket websocket = null;
    private string serverUri = "wss://metareal-assistant.webis.de/chatting?username=";
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private CancellationToken cancellationToken;
    private AudioEncoder audioEncoder;
    private StringBuilder messageBuffer = new StringBuilder(); // Buffer for incomplete messages

    void Start()
    {
        serverUri = serverUri + Username;
        ConnectToServer();
    }

    private async void ConnectToServer()
    {
        websocket = new ClientWebSocket();
        cancellationToken = cancellationTokenSource.Token;

        try
        {
            await websocket.ConnectAsync(new Uri(serverUri), cancellationToken);
            Debug.Log("Connected to server");

            // Start listening for messages
            _ = ReceiveMessages();
        }
        catch (Exception e)
        {
            Debug.LogError("WebSocket connection error: " + e.Message);
        }
    }

    private async Task DisconnectFromServer()
    {
        if (websocket != null)
        {
            try
            {
                await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", cancellationToken);
            }
            catch (Exception e)
            {
                Debug.LogError("Error during WebSocket close: " + e.Message);
            }
            finally
            {
                websocket.Dispose();
                websocket = null;
            }
        }
        Debug.Log("Disconnected from server");
    }

    public async void SendMessage(string type, string payload)
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            Debug.LogError("Not connected to server");
            return;
        }

        WebSocketMessage message = new WebSocketMessage(type, payload);
        string jsonString = JsonUtility.ToJson(message);
        byte[] bytes = Encoding.UTF8.GetBytes(jsonString);

        try
        {
            await websocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
            Debug.Log("Message sent: " + jsonString);
        }
        catch (Exception e)
        {
            Debug.LogError("Error sending message: " + e.Message);
        }
    }

    private async Task ReceiveMessages()
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (websocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;
                messageBuffer.Clear();

                do
                {
                    result = await websocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    string partialMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    messageBuffer.Append(partialMessage);
                }
                while (!result.EndOfMessage);

                var fullMessage = messageBuffer.ToString();
                Debug.Log("Full message received from server: " + fullMessage);

                HandleReceivedMessage(fullMessage);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error receiving message: " + e.Message);
        }
    }

    async void OnDisable()
    {
        cancellationTokenSource.Cancel();
        await DisconnectFromServer();
    }

    private void HandleReceivedMessage(string jsonMessage)
    {
        try
        {
            WebSocketServerResponse response = JsonUtility.FromJson<WebSocketServerResponse>(jsonMessage);

            if (response != null)
            {
                if (response.type == "RESPONSE_STREAM")
                {
                    string base64Audio = response.payload;
                    Debug.Log("Received Base64 Audio chunk: " + base64Audio);

                    VoiceAssistantController.Instance.DecodeMessage(base64Audio);
                }
                else if (response.type == "RESPONSE_STREAM_ENDED")
                {
                    Debug.Log("Stream ended.");
                }
                else
                {
                    Debug.LogError("Invalid or unsupported message type: " + response.type);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error handling received message: " + e.Message);
        }
    }
}
