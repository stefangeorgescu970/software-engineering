using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Server;

namespace Client
{
    public abstract class Client
    {

        /// <summary>
        /// Communication socket.
        /// </summary>
        private static readonly Socket MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        /// <summary>
        /// Ininital ID
        /// </summary>
        protected int Id = -1;
        /// <summary>
        /// Indicator of connection status.
        /// </summary>
        private bool _isConnected;
        /// <summary>
        /// Buffer for message sending/receiving.
        /// </summary>
        private static readonly byte[] Buffer = new byte[ServerConstants.BufferSize];

        /// <summary>
        /// Client constructor. Creates new Client instance and tries to connect to the server.
        /// </summary>
	    protected Client()
        {
            TryConnect(ServerConstants.MaximumNumberOfAttemtps);
            if (!MySocket.Connected)
            {
                //TODO handle case when the client does not manage to esablish connection to server
                _isConnected = false;
            }
        }

        /// <summary>
        /// Sets received ID of a client.
        /// </summary>
        /// <param name="id">Received ID</param>
        protected void SetId(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Attempts connection to the server.
        /// </summary>
        /// <param name="maximumAttempts">Maximum number of connection attempts.</param>
        public void TryConnect(int maximumAttempts)
        {
            int attempts = 0;
            while (!MySocket.Connected && attempts < maximumAttempts)
            {
                try
                {
                    System.Threading.Thread.Sleep(2000); // fix for mac which sent requests really quickly
                    attempts++;
                    MySocket.Connect(IPAddress.Loopback, ServerConstants.UsedPort);
                    _isConnected = true;
                    MySocket.BeginReceive(Buffer, ServerConstants.BufferOffset, Buffer.Length, SocketFlags.None, ReceiveMessage, MySocket);
                    // Listen for messages, which will go to buffer

                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection Attempts: " + attempts);
                }
            }

            Console.Clear();
            if (_isConnected)
                Console.WriteLine("Connected");

        }

        /// <summary>
        /// Receives message from the socket. 
        /// </summary>
        /// <param name="asyncResult">Result of asynchronous Receive(). </param>
        private void ReceiveMessage(IAsyncResult asyncResult)
        {

            Socket senderSocket = (Socket)asyncResult.AsyncState;
            // passed as argument to begin receive, so we know who send the message

            int sizeOfReceivedData = senderSocket.EndReceive(asyncResult);

            byte[] temporaryBuffer = new byte[sizeOfReceivedData];
            Array.Copy(Buffer, temporaryBuffer, sizeOfReceivedData);
            // Truncate the data so we do not deal with unnecessary null cells.

            string receivedData = Encoding.ASCII.GetString(temporaryBuffer);

            Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(receivedData);

            Console.WriteLine("I got: " + receivedData);
            // Here do something given the data received. Now just send back response and probably wrap this in a function in the future

            senderSocket.BeginReceive(Buffer, ServerConstants.BufferOffset, Buffer.Length, SocketFlags.None, ReceiveMessage, senderSocket);
            // Begin receiveng again on the same socket


            HandleReceivePacket(receivedPacket);
            // TODO handle exceptions
        }

        /// <summary>
        /// Sends packet to the server.
        /// </summary>
        /// <param name="myPacket">Packet to send</param>
        /// <param name="needResponse">Indicator whether sender needs response or not.</param>
        /// <returns></returns>
        public String SendPacket(Packet myPacket, bool needResponse)
        {
            if (_isConnected)
            {
                String jsonString = JsonConvert.SerializeObject(myPacket);

                byte[] toSend = Encoding.ASCII.GetBytes(jsonString);

                MySocket.Send(toSend);
                // We are sending synchronously, since we are going to wait for a response we don't need to complicate our lifes

                if (needResponse)
                {
                    byte[] receivedBuffer = new byte[ServerConstants.BufferSize];
                    int sizeReceived = MySocket.Receive(receivedBuffer);
                    byte[] actualData = new byte[sizeReceived];
                    Array.Copy(receivedBuffer, actualData, sizeReceived);

                    return Encoding.ASCII.GetString(actualData);
                    // Return the data that we 
                }
            }
            else
            {
                Console.WriteLine("Client with id " + Id + " is not connected to server");
                // TODO maybe create an internal id, since here all ids will be -1 if initial connection fails
            }
            return "";
        }

        /// <summary>
        /// Registers client to the server and gets connection ID. 
        /// </summary>
        /// <param name="whoAmI">Client type.</param>
        public void RegisterToServerAndGetId(ClientType whoAmI)
        {
            Packet toSend = new Packet(Id, -1, RequestType.Register);

            toSend.AddArgument(ServerConstants.ArgumentNames.SenderType, whoAmI);

            String received = SendPacket(toSend, true);

            Console.WriteLine("Received: " + received);

            Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(received);

            int receivedId = Int32.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]);

            SetId(receivedId);
        }

        /// <summary>
        /// Abstract method for handling received packet.
        /// </summary>
        /// <param name="receivedPacket"></param>
        public abstract void HandleReceivePacket(Packet receivedPacket);
    }
}
