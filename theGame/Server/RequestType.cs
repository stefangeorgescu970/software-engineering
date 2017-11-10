using System;
namespace Server
{

    /// <summary>
    /// The request types that can be forwarded to the server.
    /// REGISTER - one time request for registering and receiving allocate id.
    /// SEND - since server does not care about contents of messages, it will just forward them to the destination, therefore the only other operation we need is send.
    /// </summary>
    public enum RequestType
    {
        REGISTER, SEND

    }
}
