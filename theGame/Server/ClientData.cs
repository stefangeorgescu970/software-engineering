using System;
using System.Net.Sockets;

namespace Server
{

    /// <summary>
    /// Client data.
    /// Class used to hold the necessary data about a client for the server to function.
    /// </summary>
    public class ClientData{

        /// <summary>
        /// The socket used to communicate with this client.
        /// </summary>
        private Socket _mySocket;


        /// <summary>
        /// The identifier of this client.
        /// </summary>
        private int _id;


        /// <summary>
        /// The type of the connection to this client.
        /// </summary>
        private ConnectionType _myConnectionType;


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Server.ClientData"/> class.
        /// </summary>
        /// <param name="socket">Socket.</param>
        public ClientData(Socket socket){
            _mySocket = socket;
            _id = -1;
            _myConnectionType = ConnectionType.PendingRequest;
        }


        /// <summary>
        /// Gets or sets the socket.
        /// </summary>
        /// <value>The socket.</value>
        public Socket Socket { get => _mySocket; set => _mySocket = value; }


        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get => _id; set => _id = value; }


        /// <summary>
        /// Gets or sets the type of the connection.
        /// </summary>
        /// <value>The type of the connection.</value>
        public ConnectionType ConnectionType { get => _myConnectionType; set => _myConnectionType = value; }
    }
}
