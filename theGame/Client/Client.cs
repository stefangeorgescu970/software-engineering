using System;
using System.Net.Sockets;
using System.Net;

namespace Client
{
    class Client{

        private static Socket _mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private static void LoopConnect(){


            int attempts = 0;

            while (!_mySocket.Connected){

                try
                {
                    attempts++;
                    _mySocket.Connect(IPAddress.Loopback, 8080);

                }
                catch (SocketException) 
                {
                    Console.WriteLine("Connection Attempts: " + attempts);
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }


        public static void Main(string[] args)
        {
            LoopConnect();

            Console.ReadLine();
        }
    }
}
