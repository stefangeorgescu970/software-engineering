using System;
namespace Server
{
    /// <summary>
    /// A place to store all constants related to the Server and Client.
    /// </summary>
    public static class ServerConstants
    {
        /// <summary>
        /// The used port.
        /// </summary>
        public const int UsedPort = 8080;

        /// <summary>
        /// The listen backlog. How many pending requests the server accepts.
        /// </summary>
        public const int ListenBacklog = 20;

        /// <summary>
        /// The size of the buffer. 
        /// Might increase depending on message formatting.
        /// </summary>
        public const int ServerBufferSize = 1024;

        public const int ClientBufferSize = 256;

        /// <summary>
        /// The first available player id.
        /// </summary>
        public const int PlayerIdPoolStart = 1;

        /// <summary>
        /// The buffer offset.
        /// </summary>
        public const int BufferOffset = 0;

        /// <summary>
        /// The maximum number of attemtps a client has to connect.
        /// </summary>
        public const int MaximumNumberOfAttemtps = 5;

        public const String endOfPacket = "<EOF>";

        public const String tempHostName = "host.server.com";

        /// <summary>
        /// Argument names to use when adding parameters.
        /// </summary>
        public static class ArgumentNames {

            /// <summary>
            /// The identifier string used.
            /// </summary>
            public const String Id = "Id";

            /// <summary>
            /// The type of the sender string used.
            /// </summary>
            public const String SenderType = "SenderType";

            // Those who implement agent and game master are free to add here whatever tags they will use.

        }

    }
}
