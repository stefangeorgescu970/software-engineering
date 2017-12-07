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

        private static readonly Socket MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        protected int Id = -1;
        private bool _isConnected;
        private static readonly byte[] Buffer = new byte[ServerConstants.BufferSize];

	    protected Client()
        {
            TryConnect(ServerConstants.MaximumNumberOfAttemtps);
            if (!MySocket.Connected)
            {
                //TODO handle case when the client does not manage to esablish connection to server
                _isConnected = false;
            }
        }

        protected void SetId(int id)
        {
            Id = id;
        }

        public int GetId()
        {
            return Id;
        }

        public void TryConnect(int maximumAttempts)
        {
            int attempts = 0;
            while (!MySocket.Connected && attempts < maximumAttempts)
            {
                try
                {
                    System.Threading.Thread.Sleep(2000); // fix for mac which sent requests really quickly, TODO - delete this in final stages
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



        public String SendPacket(Packet myPacket)
        {
            if (_isConnected)
            {
                String jsonString = JsonConvert.SerializeObject(myPacket);

                byte[] toSendArray = Encoding.ASCII.GetBytes(jsonString);

                MySocket.BeginSend(toSendArray, ServerConstants.BufferOffset, toSendArray.Length, SocketFlags.None, EndSend, MySocket);
                // We are sending synchronously, since we are going to wait for a response we don't need to complicate our lifes

            }
            else
            {
                Console.WriteLine("Client with id " + Id + " is not connected to server");
                // TODO maybe create an internal id, since here all ids will be -1 if initial connection fails
            }
            return "";
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



        public void RegisterToServerAndGetId(ClientType whoAmI)
        {
            Packet toSend = new Packet(Id, -1, RequestType.Register);

            toSend.AddArgument(ServerConstants.ArgumentNames.SenderType, whoAmI);

            SendPacket(toSend);   
        }

        public abstract void HandleReceivePacket(Packet receivedPacket);
    }
}
