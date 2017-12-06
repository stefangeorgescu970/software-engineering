using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Server;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Client
{

    // State object for receiving data from remote device.
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = ServerConstants.ClientBufferSize;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }


    public abstract class Client
    {

        // ManualResetEvent instances signal completion.
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        protected int Id = -1;
        private bool _isConnected = false;


        private Socket _mySocket;


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
            if (!_isConnected)
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
            while ( !_isConnected && attempts < maximumAttempts)
            {
                try
                {

                    IPEndPoint remoteEP = new IPEndPoint(IPAddress.Loopback, ServerConstants.UsedPort);


                    // Create a TCP/IP socket.
                    Socket client = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);


                    // Connect to the remote endpoint.
                    client.BeginConnect(remoteEP,
                        new AsyncCallback(ConnectCallback), client);
                    connectDone.WaitOne();


                    Log("");
                    System.Threading.Thread.Sleep(2000); // fix for mac which sent requests really quickly
                    attempts++;
                    Log("");
                
                    _isConnected = true;
                    _mySocket = client;

                    Log("");


                    // Create the state object.
                    StateObject state = new StateObject();
                    state.workSocket = client;

                    // Begin receiving the data from the remote device.
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                                        new AsyncCallback(ReceiveMessage), state);

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



        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }



        private void ReceiveMessage(IAsyncResult asyncResult)
        {
            
            try
            {
                // Retrieve the state object and the client socket 
                // from the asynchronous state object.
                StateObject state = (StateObject)asyncResult.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.
                int bytesRead = client.EndReceive(asyncResult);

                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get the rest of the data.
                    client.BeginReceive(state.buffer, ServerConstants.BufferOffset, StateObject.BufferSize, SocketFlags.None,
                        new AsyncCallback(ReceiveMessage), state);
                }
                else
                {
                    // All the data has arrived; put it in response.
                    if (state.sb.Length > 1)
                    {
                        
                        String response = state.sb.ToString();

                        Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(response.Remove(response.IndexOf(ServerConstants.endOfPacket, StringComparison.Ordinal)));

                        HandleReceivePacket(receivedPacket);

                        StateObject newState = new StateObject();

                        client.BeginReceive(state.buffer, ServerConstants.BufferOffset, StateObject.BufferSize, SocketFlags.None,
    new AsyncCallback(ReceiveMessage), newState);

                    }
                    // Signal that all bytes have been received.
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }


        private static void Send(Socket client, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, ServerConstants.BufferOffset, byteData.Length, SocketFlags.None,
                new AsyncCallback(SendCallback), client);
            
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }





        public void SendPacket(Packet myPacket)
        {
            if (_isConnected)
            {
                Log("");
                String jsonString = JsonConvert.SerializeObject(myPacket);

                jsonString += ServerConstants.endOfPacket;

                Send(_mySocket, jsonString);

            }
            else
            {
                Console.WriteLine("Client with id " + Id + " is not connected to server");
                // TODO maybe create an internal id, since here all ids will be -1 if initial connection fails
            }
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
