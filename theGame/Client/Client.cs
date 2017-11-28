using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Server;
using System.Runtime.CompilerServices;

namespace Client
{
    public abstract class Client
    {

        private static readonly Socket MySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        protected int Id = -1;
        private bool _isConnected;
        private static readonly byte[] Buffer = new byte[ServerConstants.BufferSize];

        private static void Log(string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Console.WriteLine("{0}_{1}({2}): {3}", Path.GetFileName(file), member, line, text);
        }
        protected Client()
        {
            Log("");
            TryConnect(ServerConstants.MaximumNumberOfAttemtps);
            if (!MySocket.Connected)
            {
                //TODO handle case when the client does not manage to esablish connection to server
                _isConnected = false;
            }
        }

        protected void SetId(int id)
        {
            Log("");
            Id = id;
        }

        public void TryConnect(int maximumAttempts)
        {
            Log("");
            int attempts = 0;
            while (!MySocket.Connected && attempts < maximumAttempts)
            {
                try
                {
                    Log("");
                    System.Threading.Thread.Sleep(2000); // fix for mac which sent requests really quickly
                    attempts++;
                    Log("");
                    MySocket.Connect(IPAddress.Loopback, ServerConstants.UsedPort);
                    _isConnected = true;
                    Log("");
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
            // Here do something given the data received. Now just send back response and probably wrap this in a function in the future
            Log("");
            senderSocket.BeginReceive(Buffer, ServerConstants.BufferOffset, Buffer.Length, SocketFlags.None, ReceiveMessage, senderSocket);
            // Begin receiveng again on the same socket

            Log("");
            HandleReceivePacket(receivedPacket);
            // TODO handle exceptions
        }

        public String SendPacket(Packet myPacket, bool needResponse)
        {
            if (_isConnected)
            {
                Log("");
                String jsonString = JsonConvert.SerializeObject(myPacket);

                Log("");
                byte[] toSend = Encoding.ASCII.GetBytes(jsonString);

                Log("");
                MySocket.Send(toSend);
                // We are sending synchronously, since we are going to wait for a response we don't need to complicate our lifes

                if (needResponse)
                {
                    Log("");
                    byte[] receivedBuffer = new byte[ServerConstants.BufferSize];
                    Log("");
                    int sizeReceived = MySocket.Receive(receivedBuffer);
                    Log("");
                    byte[] actualData = new byte[sizeReceived];
                    Log("");
                    Array.Copy(receivedBuffer, actualData, sizeReceived);

                    Log("");
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

        public void RegisterToServerAndGetId(ClientType whoAmI)
        {
            Log("");
            Packet toSend = new Packet(Id, -1, RequestType.Register);

            Log("");
            toSend.AddArgument(ServerConstants.ArgumentNames.SenderType, whoAmI);

            Log("");
            String received = SendPacket(toSend, true);

            Log("");
            Console.WriteLine("Received: " + received);

            Log("");
            Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(received);

            Log("");
            int receivedId = Int32.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]);

            Log("");
            SetId(receivedId);
        }

        public abstract void HandleReceivePacket(Packet receivedPacket);
    }
}
