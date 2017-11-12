﻿using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Server;

namespace Client
{
    public abstract class Client
    {

        private static readonly Socket _mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private int _id = -1;
        private bool _isConnected;
        private static readonly byte[] _buffer = new byte[ServerConstants.BufferSize];

	    protected Client()
        {
            TryConnect(ServerConstants.MaximumNumberOfAttemtps);
            if (!_mySocket.Connected)
            {
                //TODO handle case when the client does not manage to esablish connection to server
                _isConnected = false;
            }
        }

        private void SetId(int id)
        {
            _id = id;
        }

        public void TryConnect(int maximumAttempts)
        {
            int attempts = 0;
            while (!_mySocket.Connected && attempts < maximumAttempts)
            {
                try
                {
                    System.Threading.Thread.Sleep(2000); // fix for mac which sent requests really quickly
                    attempts++;
                    _mySocket.Connect(IPAddress.Loopback, ServerConstants.UsedPort);
                    _isConnected = true;
                    _mySocket.BeginReceive(_buffer, ServerConstants.BufferOffset, _buffer.Length, SocketFlags.None, ReceiveMessage, _mySocket);
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
            Array.Copy(_buffer, temporaryBuffer, sizeOfReceivedData);
            // Truncate the data so we do not deal with unnecessary null cells.

            string receivedData = Encoding.ASCII.GetString(temporaryBuffer);

            Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(receivedData);

            Console.WriteLine("I got: " + receivedData);
            // Here do something given the data received. Now just send back response and probably wrap this in a function in the future

            senderSocket.BeginReceive(_buffer, ServerConstants.BufferOffset, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), senderSocket);
            // Begin receiveng again on the same socket


            HandleReceivePacket(receivedPacket);
            // TODO handle exceptions
        }

        public String SendPacket(Packet myPacket, bool needResponse)
        {
            if (_isConnected)
            {
                String jsonString = JsonConvert.SerializeObject(myPacket);

                byte[] toSend = Encoding.ASCII.GetBytes(jsonString);

                _mySocket.Send(toSend);
                // We are sending synchronously, since we are going to wait for a response we don't need to complicate our lifes

                if (needResponse)
                {
                    byte[] receivedBuffer = new byte[ServerConstants.BufferSize];
                    int sizeReceived = _mySocket.Receive(receivedBuffer);
                    byte[] actualData = new byte[sizeReceived];
                    Array.Copy(receivedBuffer, actualData, sizeReceived);

                    return Encoding.ASCII.GetString(actualData);
                    // Return the data that we 
                }
            }
            else
            {
                Console.WriteLine("Client with id " + _id + " is not connected to server");
                // TODO maybe create an internal id, since here all ids will be -1 if initial connection fails
            }
            return "";
        }

        public void RegisterToServerAndGetId(ClientType whoAmI)
        {
            Packet toSend = new Packet(_id, -1, RequestType.Register);

            toSend.AddArgument(ServerConstants.ArgumentNames.SenderType, whoAmI);

            String received = SendPacket(toSend, true);

            Console.WriteLine("Received: " + received);

            Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(received);

            int receivedId = Int32.Parse(receivedPacket.Arguments[ServerConstants.ArgumentNames.Id]);

            SetId(receivedId);
        }

        public abstract void HandleReceivePacket(Packet receivedPacket);
    }
}
