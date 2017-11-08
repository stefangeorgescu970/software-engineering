using System;
using System.Net.Sockets;


namespace Server
{
    public class ClientData{

        private Socket _mySocket;
        private int _id;
        private ConnectionType _myConnectionType;

        public ClientData(Socket socket){
            _mySocket = socket;
            _id = -1;
            _myConnectionType = ConnectionType.PENDING_REQUEST;
        }

        public Socket GetSocket(){
            return _mySocket;
        }

        public int GetId() {
            return _id;
        }

        public void SetId(int id) {
            _id = id;
        }

        public ConnectionType GetConnectionType(){
            return _myConnectionType;
        }
    }
}
