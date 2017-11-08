using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
    class Client{

        

        private static Socket _mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private int _id;


        public Client(){
            TryConnect(5);
            if(!_mySocket.Connected) {
                //TODO handle case when the client does not manage to esablish connection to server
            }

        }

        private void SetId(int id) {
            _id = id;
        }

        public void SendMessage(String message){
            byte[] toSend = Encoding.ASCII.GetBytes(message);
            _mySocket.Send(toSend);
        }


        private void TryConnect(int maximumAttempts) {
            int attempts = 0;
            while (!_mySocket.Connected && attempts < maximumAttempts){
                try{
                    System.Threading.Thread.Sleep(2000);
                    attempts++;
                    _mySocket.Connect(IPAddress.Loopback, 8080);
                }
                catch (SocketException){
                    Console.Clear();
                    Console.WriteLine("Connection Attempts: " + attempts);
                }
            }
            Console.Clear();
            Console.WriteLine("Connected");
        }

        private void RegisterToServerAndGetId(String whoAmI){
            SendMessage(whoAmI);

            byte[] receivedBuffer = new byte[2048];
            int sizeReceived = _mySocket.Receive(receivedBuffer);

            byte[] actualData = new byte[sizeReceived];
            Array.Copy(receivedBuffer, actualData, sizeReceived);

            Console.WriteLine("Received: " + Encoding.ASCII.GetString(actualData));

            // TODO get id and save it. Game master should get id 0, players from 1 up.

        }

        //TODO implement receive message

        private void SendLoop(){
            while(true) {
                Console.Write("Enter Message: ");
                string text = Console.ReadLine();

                byte[] toSend = Encoding.ASCII.GetBytes(text);
                _mySocket.Send(toSend);

                byte[] receivedBuffer = new byte[2048];
                int sizeReceived = _mySocket.Receive(receivedBuffer);

                byte[] actualData = new byte[sizeReceived];
                Array.Copy(receivedBuffer, actualData, sizeReceived);

                Console.WriteLine("Received: " + Encoding.ASCII.GetString(actualData));
            }
        }



        public static void Main(string[] args)
        {

            Client myClient = new Client();
            myClient.SendLoop();
            Console.ReadLine();
        }
    }
}
