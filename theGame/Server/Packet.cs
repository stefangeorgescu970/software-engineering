using System;
using Server;
using System.Collections.Generic;

namespace Server
{
    /// <summary>
    /// Packet
    /// Class used for a packet that is passed around from client to server.
    /// </summary>
    public class Packet
    {
        /// <summary>
        /// The sender identifier.
        /// An integer number representing the id that hass been previously allocated by the server to the entity.
        /// </summary>
        int _senderId;


        /// <summary>
        /// The destination identifier.
        /// An integer number representing the id of the destination of the packet.
        /// We have the following possible values:
        /// -1 is reserved for packets that the server must handle.
        /// 0 is the id reserved for the game master.
        /// from 1 up are ids of players.
        /// </summary>
        int _destinationId;


        /// <summary>
        /// The type of the request.
        /// </summary>
        RequestType _requestType;


        /// <summary>
        /// The arguments of the request. Will hold the message body of the packet. 
        /// It is up to the implementation of the agent and the game master to decide how this will be used.
        /// Subject to change if problems when implementing.
        /// </summary>
        Dictionary<String, dynamic> _arguments = new Dictionary<String, dynamic>();


        /// <summary>
        /// Initializes a new instance of the <see cref="T:Server.Packet"/> class.
        /// </summary>
        /// <param name="senderId">Sender identifier.</param>
        /// <param name="destinationId">Destination identifier.</param>
        /// <param name="requestType">Request type.</param>
        public Packet(int senderId, int destinationId, RequestType requestType)
        {
            _senderId = senderId;
            _destinationId = destinationId;
            _requestType = requestType;
        }


        /// <summary>
        /// Gets or sets the sender identifier.
        /// </summary>
        /// <value>The sender identifier.</value>
        public int SenderId { get => _senderId; set => _senderId = value; }


        /// <summary>
        /// Gets or sets the destination identifier.
        /// </summary>
        /// <value>The destination identifier.</value>
        public int DestinationId { get => _destinationId; set => _destinationId = value; }


        /// <summary>
        /// Gets or sets the type of the request.
        /// </summary>
        /// <value>The type of the request.</value>
        public RequestType RequestType { get => _requestType; set => _requestType = value; }


        /// <summary>
        /// Gets or sets the arguments.
        /// </summary>
        /// <value>The arguments.</value>
        public Dictionary<String, dynamic> Arguments { get => _arguments; set => _arguments = value; }


        /// <summary>
        /// Adds one argument argument.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public void AddArgument(String key, dynamic value)
        {
            _arguments.Add(key, value);
        }
    }
}
