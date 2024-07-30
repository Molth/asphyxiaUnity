//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

using static KCP.KCPBASIC;

namespace asphyxia
{
    /// <summary>
    ///     Settings
    /// </summary>
    public static class Settings
    {
        /// <summary>
        ///     Socket buffer size
        /// </summary>
        public const int SOCKET_BUFFER_SIZE = 2048;

        /// <summary>
        ///     Peer ping interval
        /// </summary>
        public const int PEER_PING_INTERVAL = 500;

        /// <summary>
        ///     Peer receive timeout
        /// </summary>
        public const int PEER_RECEIVE_TIMEOUT = 5000;

        /// <summary>
        ///     Kcp message size
        /// </summary>
        public const int KCP_MESSAGE_SIZE = 2048;

        /// <summary>
        ///     Reversed size
        /// </summary>
        public const int KCP_REVERSED_SIZE = 3;

        /// <summary>
        ///     Kcp flush buffer size
        /// </summary>
        public const int KCP_FLUSH_BUFFER_SIZE = KCP_REVERSED_SIZE + (KCP_MAXIMUM_TRANSMISSION_UNIT + (int)OVERHEAD) * 3;

        /// <summary>
        ///     Kcp maximum transmission unit
        /// </summary>
        public const int KCP_MAXIMUM_TRANSMISSION_UNIT = (int)MTU_DEF - KCP_REVERSED_SIZE;

        /// <summary>
        ///     Kcp window size
        /// </summary>
        public const int KCP_WINDOW_SIZE = 1024;

        /// <summary>
        ///     Kcp no delay
        /// </summary>
        public const int KCP_NO_DELAY = 1;

        /// <summary>
        ///     Kcp flush interval
        /// </summary>
        public const int KCP_FLUSH_INTERVAL = 1;

        /// <summary>
        ///     Kcp fast resend trigger count
        /// </summary>
        public const int KCP_FAST_RESEND = 0;

        /// <summary>
        ///     Kcp no congestion window
        /// </summary>
        public const int KCP_NO_CONGESTION_WINDOW = 1;
    }
}