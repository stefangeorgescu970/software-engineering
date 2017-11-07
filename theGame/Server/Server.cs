using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class MainServer
    {
        private static byte[] _buffer = new byte[2048];


        private static List<Socket> _clientSockets = new List<Socket>(); // List of clients to serve
        // TODO create class for client info, since we'll have to filter by ip

        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static void setupServer(){
            Console.WriteLine("Setting up server...");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, ServerConstants.UsedPort));
            _serverSocket.Listen(ServerConstants.ListenBacklog);
            _serverSocket.BeginAccept(new AsyncCallback(acceptConnection), null);
        }

        private static void acceptConnection(IAsyncResult asyncResult){
            Socket newSocket = _serverSocket.EndAccept(asyncResult);
            _clientSockets.Add(newSocket);
            Console.WriteLine("Client Connected!");
            newSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(receiveMessage), newSocket);
            _serverSocket.BeginAccept(new AsyncCallback(acceptConnection), null);
            // We need to begin accepting again in order to allow more than one connection
        }


        private static void receiveMessage(IAsyncResult asyncResult) {
            Socket senderSocket = (Socket)asyncResult.AsyncState; // passed as argument to begin receive

            int sizeOfReceivedData = senderSocket.EndReceive(asyncResult);
            byte[] temporaryBuffer = new byte[sizeOfReceivedData];

            Array.Copy(_buffer, temporaryBuffer, sizeOfReceivedData);

            string receivedData = Encoding.ASCII.GetString(temporaryBuffer);
            Console.WriteLine("I got: " + receivedData);

            //Here do something given the data received. Now just send back response and probably wrap this in a function in the future


            byte[] toSend = Encoding.ASCII.GetBytes("Here is your response.");
            senderSocket.BeginSend(toSend, 0, toSend.Length, SocketFlags.None, new AsyncCallback(endSend), senderSocket);
            senderSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(receiveMessage), senderSocket);
        }


        private static void endSend(IAsyncResult asyncResult){
            Socket senderSocket = (Socket)asyncResult.AsyncState;
            senderSocket.EndSend(asyncResult);
        }


        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            setupServer();
            Console.ReadLine();
        }

    }
}
