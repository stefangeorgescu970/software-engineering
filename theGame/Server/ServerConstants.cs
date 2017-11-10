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
        public const int BufferSize = 2048;

        public const int PlayerIdPoolStart = 1;

        public const int BufferOffset = 0;

        public const int MaximumNumberOfAttemtps = 5;

        /// <summary>
        /// Argument names to use when adding parameters.
        /// </summary>
        public static class ArgumentNames {

            public const String Id = "Id";

            public const String SenderType = "SenderType";

        }

    }
}
