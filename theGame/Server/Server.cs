using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace Server
{
    class MainServer
    {
        // TODO change this to singleton rather than using static fields.
        // TODO add method that converts from packet to bytearray.

        private static void Log(string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Console.WriteLine("{0}_{1}({2}): {3}", Path.GetFileName(file), member, line, text);
        }
        /// <summary>
        /// The current available identifier for players.
        /// </summary>
        private static int _currentAvailableIdForPlayers = ServerConstants.PlayerIdPoolStart;

        /// <summary>
        /// The buffer in which messages will arrive.
        /// </summary>
        private static readonly byte[] Buffer = new byte[ServerConstants.BufferSize];


        /// <summary>
        /// List of clients.
        /// </summary>
        private static readonly List<ClientData> MyClients = new List<ClientData>(); // Updated list of clients to serve


        /// <summary>
        /// The server socket. Main communication end-point.
        /// </summary>
        private static readonly Socket ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        /// <summary>
        /// Setups the server.
        /// Must always be called. 
        /// </summary>
        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            // For now, using the console as log

            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, ServerConstants.UsedPort));
            // Make the socket listen on all ip addresses available

            ServerSocket.Listen(ServerConstants.ListenBacklog);
            // Explained under ServerConstants

            ServerSocket.BeginAccept(AcceptConnection, null);
            // Accepting connections, with callback function AcceptConnection

            Console.WriteLine("Awaiting connection...");
        }


        /// <summary>
        /// Accepts the connection.
        /// Gets called when a new client connects to the server
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private static void AcceptConnection(IAsyncResult asyncResult)
        {
            // Called when a new connection is established, with an async result.

            Log("");
            Socket newSocket = ServerSocket.EndAccept(asyncResult);
            // Get the socket from which we got connection, and end accepting.

            Log("");
            ClientData newClient = new ClientData(newSocket);
            // Create new cliend data entity for further reference.

            Log("");
            MyClients.Add(newClient);

            Console.WriteLine("Client Connected!");

            Log("");
            newSocket.BeginReceive(Buffer, ServerConstants.BufferOffset, Buffer.Length, SocketFlags.None, ReceiveMessage, newSocket);
            // Begin receiving on that socket, things that will be received will be put in buffer, and ReceiveMessage is the callback of a received message
            // We pass newSocket as objectState so we have access to it in the body of the callback

            Log("");
            ServerSocket.BeginAccept(AcceptConnection, null);
            // We need to begin accepting again in order to allow more than one connection
            // We begin accepting again.
        }


        /// <summary>
        /// Receives the message.
        /// Gets called when a new client sends something to the server
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private static void ReceiveMessage(IAsyncResult asyncResult)
        {

            Log("");
            Socket senderSocket = (Socket)asyncResult.AsyncState;
            // passed as argument to begin receive, so we know who send the message

            Log("");
            int sizeOfReceivedData = senderSocket.EndReceive(asyncResult);

            Log("");
            byte[] temporaryBuffer = new byte[sizeOfReceivedData];
            Log("");
            Array.Copy(Buffer, temporaryBuffer, sizeOfReceivedData);
            // Truncate the data so we do not deal with unnecessary null cells.

            Log("");
            string receivedData = Encoding.ASCII.GetString(temporaryBuffer);

            Log("");
            Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(receivedData);

            Log("");
            Console.WriteLine("I got: " + receivedData);

            // Handle the received packet.
            if (receivedPacket.RequestType == RequestType.Register)
            {
                Log("");
                HandleRegisterRequest(receivedPacket, senderSocket);
            }
            else if (receivedPacket.RequestType == RequestType.Send)
            {
                Log("");
                HandleSendRequest(receivedPacket);
            }

            Log("");
            senderSocket.BeginReceive(Buffer, ServerConstants.BufferOffset, Buffer.Length, SocketFlags.None, ReceiveMessage, senderSocket);
            // Begin receiveng again on the same socket

            // TODO handle exceptions
        }


        /// <summary>
        /// Callback for ending the send procedure.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private static void EndSend(IAsyncResult asyncResult)
        {
            // Here simply end send on the socket we were transmitting
            Log("");
            Socket senderSocket = (Socket)asyncResult.AsyncState;
            Log("");
            senderSocket.EndSend(asyncResult);
            Log("");
        }

        /// <summary>
        /// Handles the register request.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="senderSocket">Sender socket.</param>
        private static void HandleRegisterRequest(Packet packet, Socket senderSocket)
        {
            //  TODO clean this bit of ugly code
            Log("");
            ClientType clientType = (ClientType)((int)packet.Arguments[ServerConstants.ArgumentNames.SenderType]);
            Log("");
            switch (clientType)
            {
                case ClientType.Agent:
                    {
                        Log("");
                        int allocatedId = _currentAvailableIdForPlayers++;

                        foreach (var clientData in MyClients)
                        {
                            Log("");
                            if (clientData.Socket != senderSocket) continue;

                            clientData.Id = allocatedId;
                            clientData.ConnectionType = ConnectionType.Connected;
                            break;
                        }
                        Log("");
                        SendIdToClient(senderSocket, allocatedId);
                        break;
                    }
                case ClientType.GameMaster:
                    {
                        Log("");
                        int allocatedId = 0;

                        Log("");
                        foreach (var clientData in MyClients)
                        {
                            if (clientData.Socket != senderSocket) continue;

                            clientData.Id = allocatedId;
                            clientData.ConnectionType = ConnectionType.Connected;
                            break;
                        }

                        SendIdToClient(senderSocket, allocatedId);

                        break;
                    }
                default:
                    Console.WriteLine("Invalid request received, do nothing.");
                    break;
            }
        }


        /// <summary>
        /// Gets the index of destination in clients.
        /// </summary>
        /// <returns>The index of destination in clients.</returns>
        /// <param name="destinationId">Destination identifier.</param>
        private static int GetIndexOfDestinationInClients(int destinationId)
        {

            Log("");
            // TODO maybe find a more beautiful solution using container methods and closures.

            for (int i = 0; i < MyClients.Count; i++)
            {
                Log("");
                if (MyClients[i].Id == destinationId)
                {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>
        /// Handles the send request.
        /// </summary>
        /// <param name="packet">Packet.</param>
        private static void HandleSendRequest(Packet packet)
        {
            Log("");
            int destinationId = packet.DestinationId;

            Log("");
            int destinationClientIndex = GetIndexOfDestinationInClients(destinationId);

            Log("");
            if (destinationClientIndex != -1)
            {
                Socket destinationSocket = MyClients[destinationClientIndex].Socket;
                ForwardToClient(packet, destinationSocket);

            }
            else
            {
                Console.WriteLine("Packet received, but destination not available.");
            }

        }


        /// <summary>
        /// Forwards a given packet to the client via it's socket.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="socket">Socket.</param>
        private static void ForwardToClient(Packet packet, Socket socket)
        {
            Log("");
            String jsonString = JsonConvert.SerializeObject(packet);

            Log("");
            byte[] send = Encoding.ASCII.GetBytes(jsonString);

            Log("");
            socket.BeginSend(send, ServerConstants.BufferOffset, send.Length, SocketFlags.None, EndSend, socket);
            // Send response to request OR forward the message to the proper socket if communication. Call EndSend when send is done

            Log("");
            socket.BeginReceive(Buffer, ServerConstants.BufferOffset, Buffer.Length, SocketFlags.None, ReceiveMessage, socket);
            // Begin receiveng again on the same socket

        }


        /// <summary>
        /// Sends the allocated id back to the client.
        /// </summary>
        /// <param name="senderSocket">Sender socket.</param>
        /// <param name="allocatedId">Allocated identifier.</param>
        private static void SendIdToClient(Socket senderSocket, int allocatedId)
        {
            Log("");
            Packet toSend = new Packet(-1, allocatedId, RequestType.Send);

            Log("");
            toSend.AddArgument(ServerConstants.ArgumentNames.Id, allocatedId.ToString());

            Log("");
            String jsonString = JsonConvert.SerializeObject(toSend);

            Log("");
            byte[] send = Encoding.ASCII.GetBytes(jsonString);

            Log("");
            senderSocket.BeginSend(send, ServerConstants.BufferOffset, send.Length, SocketFlags.None, EndSend, senderSocket);
            // Send response to request OR forward the message to the proper socket if communication. Call EndSend when send is done

            Log("");
            senderSocket.BeginReceive(Buffer, ServerConstants.BufferOffset, Buffer.Length, SocketFlags.None, ReceiveMessage, senderSocket);
            // Begin receiveng again on the same socket

        }


        public static void Main(string[] args)
        {
            SetupServer();
            Console.ReadLine();

            // Start server and keep console running
        }


    }
}
