using System;
namespace Server
{
    public enum ConnectionType
    {
        CONNECTED, PENDING_REQUEST, DISCONNECTED
    }


    /*
     * A Connected entity is one that has established communication with the server.
     * A Pending_Request entity is one that has a socket connection, but has not yet forwarded a request to join the game.
     * A Disconnected entity is one that was previously connected but stopped responding on the socket. 
     * Normally a client shouldn't wait too much in pending
     */ 
}
