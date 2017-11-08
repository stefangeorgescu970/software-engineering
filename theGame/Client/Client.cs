using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Server;

namespace Client
{
    class Client{

        

        private static Socket _mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private int _id = -1;
        private bool _isConnected;
        private static byte[] _buffer = new byte[2048];



        public Client(){
            TryConnect(5);
            if(!_mySocket.Connected) {
                //TODO handle case when the client does not manage to esablish connection to server
                _isConnected = false;
            }

        }

        private void SetId(int id) {
            _id = id;
        }

        private void TryConnect(int maximumAttempts) {
            int attempts = 0;
            while (!_mySocket.Connected && attempts < maximumAttempts){
                try{
                    System.Threading.Thread.Sleep(2000);
                    attempts++;
                    _mySocket.Connect(IPAddress.Loopback, 8080);
                    _isConnected = true;
                    _mySocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), _mySocket);

                }
                catch (SocketException){
                    Console.Clear();
                    Console.WriteLine("Connection Attempts: " + attempts);
                }
            }
            Console.Clear();
            if(_isConnected)
                Console.WriteLine("Connected");
        }

        private static void ReceiveMessage(IAsyncResult asyncResult)
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

            senderSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), senderSocket);
            // Begin receiveng again on the same socket

            // TODO handle exceptions
        }



        public String SendMessage(Packet myPacket, bool needResponse)
        {
            if (_isConnected){
                String jsonString = JsonConvert.SerializeObject(myPacket);

                byte[] toSend = Encoding.ASCII.GetBytes(jsonString);

                _mySocket.Send(toSend);

                if(needResponse){
                    byte[] receivedBuffer = new byte[2048];
                    int sizeReceived = _mySocket.Receive(receivedBuffer);
                    byte[] actualData = new byte[sizeReceived];
                    Array.Copy(receivedBuffer, actualData, sizeReceived);

                    return Encoding.ASCII.GetString(actualData);
                }
            } else {
                Console.WriteLine("Client with id " + _id + " is not connected to server");
            }
            return "";
        }


        public void RenameThisLater(int destId) {
            Packet myPacket = new Packet(_id, destId, "send");
            myPacket.addArgument("Message", "Screw You!");

            SendMessage(myPacket, false);
        }

        public void RegisterToServerAndGetId(String whoAmI){

            Packet toSend = new Packet(_id, -1, "register");
            toSend.addArgument("Sender", whoAmI);
          
            String received = SendMessage(toSend, true);
            Console.WriteLine("Received: " + received);
            Packet receivedPacket = JsonConvert.DeserializeObject<Packet>(received);
            int receivedId = Int32.Parse(receivedPacket.Arguments["Id"]);
            this.SetId(receivedId);
        }


        public static void Main(string[] args)
        {

            Client myClient = new Client();
            Console.ReadLine();
            myClient.RegisterToServerAndGetId("agent");
            Console.ReadLine();
            while (true)
            {

                Console.Write("Enter destination ID: ");
                string text = Console.ReadLine();
                myClient.RenameThisLater(Int32.Parse(text));
            }
        }
    }
}
