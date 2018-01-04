using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Server
{

    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = ServerConstants.ServerBufferSize;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }


    class MainServer
    {
        // TODO change this to singleton rather than using static fields.
        // TODO add method that converts from packet to bytearray.

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        /// <summary>
        /// The current available identifier for players.
        /// </summary>
        private static int _currentAvailableIdForPlayers = ServerConstants.PlayerIdPoolStart;
        /// <summary>
        /// List of clients.
        /// </summary>
        private static readonly List<ClientData> MyClients = new List<ClientData>(); // Updated list of clients to serve
        private static readonly List<int> MyGameMasters = new List<int>(); // Updated list of clients to serve
        private static readonly Mutex myMutex = new Mutex();

        /// <summary>
        /// Setups the server.
        /// Must always be called. 
        /// </summary>
        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            // For now, using the console as log

            byte[] bytes = new byte[ServerConstants.ServerBufferSize];


            // TODO - here change according to file found

            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, ServerConstants.UsedPort);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(ServerConstants.ListenBacklog);

                while (true)
                {
                    allDone.Reset();

                    Console.WriteLine("Waiting for connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptConnection), listener);

                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        /// MARK - AUTOMATICALLY CALLED CALLBACKS

        /// <summary>
        /// Accepts the connection.
        /// Gets called when a new client connects to the server
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private static void AcceptConnection(IAsyncResult asyncResult)
        {
            // Called when a new connection is established, with an async result.

            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)asyncResult.AsyncState;
            Socket handler = listener.EndAccept(asyncResult);

            ClientData newClient = new ClientData(handler);
            // Create new cliend data entity for further reference.

            MyClients.Add(newClient);
            
            Console.WriteLine("Client Connected!");

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;

            handler.BeginReceive(state.buffer, ServerConstants.BufferOffset, StateObject.BufferSize, SocketFlags.None,
                new AsyncCallback(ReceiveMessage), state);

            try
            {
                allDone.Reset();
                Console.WriteLine("Waiting for another connection...");
                listener.BeginAccept(new AsyncCallback(AcceptConnection), listener);
                allDone.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Receives the message.
        /// Gets called when a new client sends something to the server
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private static void ReceiveMessage(IAsyncResult asyncResult)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)asyncResult.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(asyncResult);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                int eofIndex = content.IndexOf(ServerConstants.endOfPacket, StringComparison.Ordinal);
                if (eofIndex > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);

                    string[] stringSeparators = new string[] { "<EOF>" };

                    String[] packets = content.Split(stringSeparators, StringSplitOptions.None);


                    foreach (String packet in packets) 
                    {
                        Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(packet);

                        if (receivedPacket!=null && receivedPacket.RequestType == RequestType.Register)
                        {
                            HandleRegisterRequest(receivedPacket, handler);
                        }
                        else if (receivedPacket != null && receivedPacket.RequestType == RequestType.Send)
                        {
                            HandleSendRequest(receivedPacket);
                        }
                    }

                    state.sb.Clear();
                

                    handler.BeginReceive(state.buffer, ServerConstants.BufferOffset, StateObject.BufferSize, SocketFlags.None,
 new AsyncCallback(ReceiveMessage), state);
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, ServerConstants.BufferOffset, StateObject.BufferSize, SocketFlags.None,
                                         new AsyncCallback(ReceiveMessage), state);
                }
            }

        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, ServerConstants.BufferOffset, byteData.Length, SocketFlags.None, new AsyncCallback(EndSend), handler);
        }

        /// <summary>
        /// Callback for ending the send procedure.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private static void EndSend(IAsyncResult asyncResult)
        {
            // Here simply end send on the socket we were transmitting
            Socket senderSocket = (Socket)asyncResult.AsyncState;
            senderSocket.EndSend(asyncResult);
        }

        // MARK - HANDLE REGISTER REQUEST

        /// <summary>
        /// Handles the register request.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="senderSocket">Sender socket.</param>
        private static void HandleRegisterRequest(Packet packet, Socket senderSocket)
        {
            //  TODO clean this bit of ugly code
            ClientType clientType = (ClientType)((int)packet.Arguments[ServerConstants.ArgumentNames.SenderType]);
            switch (clientType)
            {
                case ClientType.Agent:
                {

                        myMutex.WaitOne();
                        try
                        {
                            _currentAvailableIdForPlayers++;
                        }
                        finally
                        {
                            myMutex.ReleaseMutex();
                        }


                     int allocatedId = _currentAvailableIdForPlayers++;

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
                case ClientType.GameMaster:
                    {
                        int allocatedId = 0;

                        foreach (var clientData in MyClients)
                        {
                            if (clientData.Socket != senderSocket) continue;
                            clientData.Id = allocatedId;
                            clientData.ConnectionType = ConnectionType.Connected;
                            MyGameMasters.Add(allocatedId);
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
        /// Sends the allocated id back to the client.
        /// </summary>
        /// <returns>The index of destination in clients.</returns>
        /// <param name="destinationId">Destination identifier.</param>
        private static int GetIndexOfDestinationInClients(int destinationId)
        {
            // TODO maybe find a more beautiful solution using container methods and closures.

            for (int i = 0; i < MyClients.Count; i++)
            {
                if (MyClients[i].Id == destinationId)
                {
                    return i;
                }
            }
            return -1;
        }

        // MARK - HANDLE SEND REQUEST

        /// <summary>
        /// Handles the send request.
        /// </summary>
        /// <param name="packet">Packet.</param>
        private static void HandleSendRequest(Packet packet)
        {
            int destinationId = packet.DestinationId;

            int destinationClientIndex = GetIndexOfDestinationInClients(destinationId);

            if (destinationClientIndex != -1)
            {
                Console.WriteLine("Sending packet to "+ MyClients[destinationClientIndex].Id);

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
            String jsonString = JsonConvert.SerializeObject(packet);

            jsonString += ServerConstants.endOfPacket;

            Send(socket, jsonString);

        }

        /// <summary>
        /// Sends the allocated id back to the client.
        /// </summary>
        /// <param name="senderSocket">Sender socket.</param>
        /// <param name="allocatedId">Allocated identifier.</param>
        private static void SendIdToClient(Socket senderSocket, int allocatedId)
        {
            Packet toSend = new Packet(-1, allocatedId, RequestType.Register);

            toSend.AddArgument(ServerConstants.ArgumentNames.Id, allocatedId.ToString());
            //TODO: look for smarter way to assign gamemasters
            if (!MyGameMasters.Contains(allocatedId))
            {
                toSend.AddArgument(ServerConstants.ArgumentNames.GameMasterId, MyGameMasters[0].ToString());
            }

            String jsonString = JsonConvert.SerializeObject(toSend);

            jsonString += ServerConstants.endOfPacket;

            Send(senderSocket, jsonString);
        }

        public static void Main(string[] args)
        {
            SetupServer();
            Console.ReadLine();

            // Start server and keep console running
        }
    }
}
