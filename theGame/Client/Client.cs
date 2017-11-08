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


        public void RegisterToServerAndGetId(String whoAmI){

          

            Packet toSend = new Packet(_id, -1, "register");

            toSend.addArgument("Sender", whoAmI);
           
            String received = SendMessage(toSend, true);
            // TODO parse received JSON? 

            Console.WriteLine("Received: " + received);

            // TODO get id and save it. Game master should get id 0, players from 1 up 
        }

        //TODO implement send message with  argument await response or not

        private void SendLoop(){
            while (true)
            {
                Console.Write("Enter Message: ");
                string text = Console.ReadLine();

                RegisterToServerAndGetId(text);
            }
        }



        public static void Main(string[] args)
        {

            Client myClient = new Client();
            Console.ReadLine();
            myClient.RegisterToServerAndGetId("client");
            Console.ReadLine();
        }
    }
}
