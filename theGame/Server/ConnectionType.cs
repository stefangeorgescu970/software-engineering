namespace Server
{
    /// <summary>
    /// Connection type.
    /// CONNECTED - connection has been established with the server and server has allocated id for entity.
    /// PENDING-REQUEST socket connection has been established, request to join has not yet been forwarded.
    /// DISCONNECTED - entity with which connection has been lost.
    /// </summary>
    public enum ConnectionType
    {
        CONNECTED, PENDING_REQUEST, DISCONNECTED
    }
}
