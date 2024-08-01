//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

namespace asphyxia
{
    /// <summary>
    ///     Peer state
    /// </summary>
    public enum PeerState
    {
        /// <summary>
        ///     None
        /// </summary>
        None,

        /// <summary>
        ///     Connecting
        /// </summary>
        Connecting,

        /// <summary>
        ///     Connect acknowledging
        /// </summary>
        ConnectAcknowledging,

        /// <summary>
        ///     Connect establishing
        /// </summary>
        ConnectEstablishing,

        /// <summary>
        ///     Connected
        /// </summary>
        Connected,

        /// <summary>
        ///     Disconnecting
        /// </summary>
        Disconnecting,

        /// <summary>
        ///     Disconnected
        /// </summary>
        Disconnected
    }
}