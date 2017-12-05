﻿using System;
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

        private static void Log(string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Console.WriteLine("{0}_{1}({2}): {3}", Path.GetFileName(file), member, line, text);
        }


        public static ManualResetEvent allDone = new ManualResetEvent(false);



        /// <summary>
        /// The current available identifier for players.
        /// </summary>
        private static int _currentAvailableIdForPlayers = ServerConstants.PlayerIdPoolStart;
        /// <summary>
        /// List of clients.
        /// </summary>
        private static readonly List<ClientData> MyClients = new List<ClientData>(); // Updated list of clients to serve



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
            IPHostEntry ipHostInfo = Dns.GetHostEntry(""); // local
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, ServerConstants.UsedPort);


            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


            try{
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while(true) {
                    allDone.Reset();

                    Console.WriteLine("Waiting for connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptConnection), listener);

                    allDone.WaitOne();
                }


            } catch (Exception e) {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }


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


            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, ServerConstants.BufferOffset, StateObject.BufferSize, SocketFlags.None,
                new AsyncCallback(ReceiveMessage), state);



            Log("");
            ClientData newClient = new ClientData(handler);
            // Create new cliend data entity for further reference.

            Log("");
            MyClients.Add(newClient);

            Console.WriteLine("Client Connected!");


            Log("");

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
                if ( eofIndex > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content);


                    Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(content.Remove(eofIndex));

                    if (receivedPacket.RequestType == RequestType.Register)
                    {
                        Log("");
                        HandleRegisterRequest(receivedPacket, handler);
                    }
                    else if (receivedPacket.RequestType == RequestType.Send)
                    {
                        Log("");
                        HandleSendRequest(receivedPacket);
                    }

                    StateObject newState = new StateObject();

                    handler.BeginReceive(state.buffer, ServerConstants.BufferOffset, StateObject.BufferSize, SocketFlags.None,
                     new AsyncCallback(ReceiveMessage), newState);
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
            Log("");
            Packet toSend = new Packet(-1, allocatedId, RequestType.Send);

            Log("");
            toSend.AddArgument(ServerConstants.ArgumentNames.Id, allocatedId.ToString());

            Log("");
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
