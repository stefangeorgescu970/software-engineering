using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Server
{
    class MainServer
    {
        // TODO change this to singleton rather than using static fields.


        /// <summary>
        /// The current available identifier for players.
        /// </summary>
        private static int _currentAvailableIdForPlayers = ServerConstants.PlayerIdPoolStart;

        /// <summary>
        /// The buffer in which messages will arrive.
        /// </summary>
        private static byte[] _buffer = new byte[ServerConstants.BufferSize];


        /// <summary>
        /// List of clients.
        /// </summary>
        private static List<ClientData> _myClients = new List<ClientData>(); // Updated list of clients to serve


        /// <summary>
        /// The server socket. Main communication end-point.
        /// </summary>
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        /// <summary>
        /// Setups the server.
        /// Must always be called. 
        /// </summary>
        private static void SetupServer(){
            Console.WriteLine("Setting up server...");
            // For now, using the console as log

            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, ServerConstants.UsedPort));
            // Make the socket listen on all ip addresses available

            _serverSocket.Listen(ServerConstants.ListenBacklog);
            // Explained under ServerConstants

            _serverSocket.BeginAccept(new AsyncCallback(AcceptConnection), null);
            // Accepting connections, with callback function AcceptConnection

            Console.WriteLine("Awaiting connection...");
        }


        /// <summary>
        /// Accepts the connection.
        /// Gets called when a new client connects to the server
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private static void AcceptConnection(IAsyncResult asyncResult){
            // Called when a new connection is established, with an async result.

            Socket newSocket = _serverSocket.EndAccept(asyncResult);
            // Get the socket from which we got connection, and end accepting.

            ClientData newClient = new ClientData(newSocket);
            // Create new cliend data entity for further reference.

            _myClients.Add(newClient);

            Console.WriteLine("Client Connected!");

            newSocket.BeginReceive(_buffer, ServerConstants.BufferOffset, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), newSocket);
            // Begin receiving on that socket, things that will be received will be put in buffer, and ReceiveMessage is the callback of a received message
            // We pass newSocket as objectState so we have access to it in the body of the callback

            _serverSocket.BeginAccept(new AsyncCallback(AcceptConnection), null);
            // We need to begin accepting again in order to allow more than one connection
            // We begin accepting again.
        }


        /// <summary>
        /// Receives the message.
        /// Gets called when a new client sends something to the server
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private static void ReceiveMessage(IAsyncResult asyncResult) {
            //TODO handle register requests and sending messages
            // How? Messages "player" and "gamemaster" will signify registering to 
            // server, gm gets id 0, player gets first id avaialbe. Change the state
            // Of the two clients here and their ids, for further use in coms.

            Socket senderSocket = (Socket)asyncResult.AsyncState; 
            // passed as argument to begin receive, so we know who send the message

            int sizeOfReceivedData = senderSocket.EndReceive(asyncResult);

            byte[] temporaryBuffer = new byte[sizeOfReceivedData];
            Array.Copy(_buffer, temporaryBuffer, sizeOfReceivedData);
            // Truncate the data so we do not deal with unnecessary null cells.

            string receivedData = Encoding.ASCII.GetString(temporaryBuffer);

            Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(receivedData);

            Console.WriteLine("I got: " + receivedData);

            // Handle the received packet.
            if(receivedPacket.RequestType == RequestType.REGISTER) {
                handleRegisterRequest(receivedPacket, senderSocket);
            } else if (receivedPacket.RequestType == RequestType.SEND) {
                handleSendRequest(receivedPacket);
            }
           
            senderSocket.BeginReceive(_buffer, ServerConstants.BufferOffset, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), senderSocket);
            // Begin receiveng again on the same socket

            // TODO handle exceptions
        }


        /// <summary>
        /// Callback for ending the send procedure.
        /// </summary>
        /// <param name="asyncResult">Async result.</param>
        private static void EndSend(IAsyncResult asyncResult){
            // Here simply end send on the socket we were transmitting
            Socket senderSocket = (Socket)asyncResult.AsyncState;
            senderSocket.EndSend(asyncResult);
        }

        /// <summary>
        /// Handles the register request.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="senderSocket">Sender socket.</param>
        private static void handleRegisterRequest(Packet packet, Socket senderSocket)
        {
            //  TODO clean this bit of ugly code

            String clientType = packet.Arguments[ServerConstants.ArgumentNames.SenderType];

            if (clientType == "agent")
            {
                int allocatedId = MainServer._currentAvailableIdForPlayers++;

                foreach (var clientData in _myClients)
                {
                    if (clientData.Socket == senderSocket)
                    {
                        clientData.Id = allocatedId;
                        clientData.ConnectionType = ConnectionType.CONNECTED;
                        break;
                    }
                }

                sendIdToClient(senderSocket, allocatedId);
            }

            else if (clientType == "gamemaster")
            {
                int allocatedId = 0;

                foreach (var clientData in _myClients)
                {
                    if (clientData.Socket == senderSocket)
                    {
                        clientData.Id = allocatedId;
                        clientData.ConnectionType = ConnectionType.CONNECTED;
                        break;
                    }
                }

                sendIdToClient(senderSocket, allocatedId);

            }
            else
            {
                Console.WriteLine("Invalid request received, do nothing.");
            }
        }


        /// <summary>
        /// Gets the index of destination in clients.
        /// </summary>
        /// <returns>The index of destination in clients.</returns>
        /// <param name="destinationId">Destination identifier.</param>
        private static int getIndexOfDestinationInClients(int destinationId) {

            // TODO maybe find a more beautiful solution using container methods and closures.

            for (int i = 0; i < _myClients.Count; i++) {
                if(_myClients[i].Id == destinationId) {
                    return i;
                }
            }
            return -1;
        }


        /// <summary>
        /// Handles the send request.
        /// </summary>
        /// <param name="packet">Packet.</param>
        private static void handleSendRequest(Packet packet)
        {
            int destinationId = packet.DestinationId;

            int destinationClientIndex = getIndexOfDestinationInClients(destinationId);

            if(destinationClientIndex != -1) {
                Socket destinationSocket = _myClients[destinationClientIndex].Socket;
                forwardToClient(packet, destinationSocket);

            } else {
                Console.WriteLine("Packet received, but destination not available.");
            }

        }


        /// <summary>
        /// Forwards a given packet to the client via it's socket.
        /// </summary>
        /// <param name="packet">Packet.</param>
        /// <param name="socket">Socket.</param>
        private static void forwardToClient(Packet packet, Socket socket){
            String jsonString = JsonConvert.SerializeObject(packet);

            byte[] send = Encoding.ASCII.GetBytes(jsonString);

            socket.BeginSend(send, ServerConstants.BufferOffset, send.Length, SocketFlags.None, new AsyncCallback(EndSend), socket);
            // Send response to request OR forward the message to the proper socket if communication. Call EndSend when send is done

            socket.BeginReceive(_buffer, ServerConstants.BufferOffset, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
            // Begin receiveng again on the same socket

        }


        /// <summary>
        /// Sends the allocated id back to the client.
        /// </summary>
        /// <param name="senderSocket">Sender socket.</param>
        /// <param name="allocatedId">Allocated identifier.</param>
        private static void sendIdToClient(Socket senderSocket, int allocatedId)
        {
            Packet toSend = new Packet(-1, allocatedId, RequestType.SEND);
            toSend.AddArgument(ServerConstants.ArgumentNames.Id, allocatedId.ToString());

            String jsonString = JsonConvert.SerializeObject(toSend);

            byte[] send = Encoding.ASCII.GetBytes(jsonString);

            senderSocket.BeginSend(send, ServerConstants.BufferOffset, send.Length, SocketFlags.None, new AsyncCallback(EndSend), senderSocket);
            // Send response to request OR forward the message to the proper socket if communication. Call EndSend when send is done

            senderSocket.BeginReceive(_buffer, ServerConstants.BufferOffset, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), senderSocket);
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
