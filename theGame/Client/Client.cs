using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
    class Client{

        private static Socket _mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static void loopConnect(){


            int attempts = 0;

            while (!_mySocket.Connected){

                try{
                    
                    System.Threading.Thread.Sleep(2000);
                    attempts++;
                    _mySocket.Connect(IPAddress.Loopback, 8080);


                }
                catch (SocketException) {
                    Console.Clear();
                    Console.WriteLine("Connection Attempts: " + attempts);

                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }


        private static void sendLoop(){

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
            loopConnect();

            sendLoop();

            Console.ReadLine();
        }
    }
}
