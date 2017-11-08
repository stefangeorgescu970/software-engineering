using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class MainServer
    {
        private int currentAvailableIdForPlayers = 1;


        private static byte[] _buffer = new byte[2048];


        private static List<ClientData> _myClients = new List<ClientData>(); // Updated list of clients to serve

        private static List<Socket> _clientSockets = new List<Socket>(); // List of clients to serve

        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static void SetupServer(){
            Console.WriteLine("Setting up server...");
            // For now, using the console as log

            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, ServerConstants.UsedPort));
            // Make the socket listen on all ip addresses available

            _serverSocket.Listen(ServerConstants.ListenBacklog);
            // Explained under ServerConstants

            _serverSocket.BeginAccept(new AsyncCallback(AcceptConnection), null);
            // Accepting connections, with callback function AcceptConnection
        }

        private static void AcceptConnection(IAsyncResult asyncResult){
            // Called when a new connection is established, with an async result

            Socket newSocket = _serverSocket.EndAccept(asyncResult);
            // Get the socket from which we got connection, and end accepting

            ClientData newClient = new ClientData(newSocket);
            // Create new cliend data entity for further reference

            _clientSockets.Add(newSocket);
            _myClients.Add(newClient);

            Console.WriteLine("Client Connected!");

            newSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), newSocket);
            // Begin receiving on that socket, things that will be received will be put in buffer, and ReceiveMessage is the callback of a received message
            // We pass newSocket as objectState so we have access to it in the body of the callback

            _serverSocket.BeginAccept(new AsyncCallback(AcceptConnection), null);
            // We need to begin accepting again in order to allow more than one connection
            // We begin accepting again.
        }


        private static void ReceiveMessage(IAsyncResult asyncResult) {
            //TODO handle register requests and sending messages
            // How? Messages "player" and "gameMaster" will signify registering to 
            // server, gm gets id0, player gets first id avaialbe. Change the state
            // Of the two clients here and their ids, for further use in coms.

            Socket senderSocket = (Socket)asyncResult.AsyncState; 
            // passed as argument to begin receive, so we know who send the message

            int sizeOfReceivedData = senderSocket.EndReceive(asyncResult);
            byte[] temporaryBuffer = new byte[sizeOfReceivedData];
            Array.Copy(_buffer, temporaryBuffer, sizeOfReceivedData);
            // Truncate the data so we do not deal with unnecessary null cells.

            string receivedData = Encoding.ASCII.GetString(temporaryBuffer);
            Console.WriteLine("I got: " + receivedData);
            // Here do something given the data received. Now just send back response and probably wrap this in a function in the future


            byte[] toSend = Encoding.ASCII.GetBytes("Here is your response.");
            senderSocket.BeginSend(toSend, 0, toSend.Length, SocketFlags.None, new AsyncCallback(EndSend), senderSocket);
            // Send response to request OR forward the message to the proper socket if communication. Call EndSend when send is done

            senderSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), senderSocket);
            // Begin receiveng again on the same socket

            // TODO handle exceptions
        }


        private static void EndSend(IAsyncResult asyncResult){
            // Here simply end send on the socket we were transmitting
            Socket senderSocket = (Socket)asyncResult.AsyncState;
            senderSocket.EndSend(asyncResult);
        }


        public static void Main(string[] args)
        {
            SetupServer();
            Console.ReadLine();

            // Start server and keep console running
        }

    }
}
