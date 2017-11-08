using System;
using System.Collections.Generic;

namespace Server
{
    public class Packet
    {
        int _senderId;
        int _destinationId;
        String _opType;
        Dictionary<String, String> _arguments = new Dictionary<string, string>();


        public Packet(int senderId, int destinationId, String opType)
        {
            SenderId = senderId;
            DestinationId = destinationId;
            OpType = opType;
        }

        public int SenderId { get => _senderId; set => _senderId = value; }
        public int DestinationId { get => _destinationId; set => _destinationId = value; }
        public string OpType { get => _opType; set => _opType = value; }
        public Dictionary<string, string> Arguments { get => _arguments; set => _arguments = value; }

        public void addArgument(String key, String value)
        {
            Arguments.Add(key, value);
        }





    }
}
