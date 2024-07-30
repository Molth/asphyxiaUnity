//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

namespace asphyxia
{
    /// <summary>
    ///     Header
    /// </summary>
    internal enum Header
    {
        /// <summary>
        ///     None
        /// </summary>
        None = 0,

        /// <summary>
        ///     Ping
        /// </summary>
        Ping = 1,

        /// <summary>
        ///     Connect
        /// </summary>
        Connect = 2,

        /// <summary>
        ///     Connect acknowledge
        /// </summary>
        ConnectAcknowledge = 4,

        /// <summary>
        ///     Connect establish
        /// </summary>
        ConnectEstablish = 8,

        /// <summary>
        ///     Data
        /// </summary>
        Data = 16,

        /// <summary>
        ///     Disconnect
        /// </summary>
        Disconnect = 32,

        /// <summary>
        ///     Disconnect acknowledge
        /// </summary>
        DisconnectAcknowledge = 64
    }
}