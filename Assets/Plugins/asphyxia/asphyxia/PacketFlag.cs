//------------------------------------------------------------
// あなたたちを許すことはできません
// Copyright © 2024 怨靈. All rights reserved.
//------------------------------------------------------------

#if UNITY_2021_3_OR_NEWER || GODOT
using System;
#endif

namespace asphyxia
{
    /// <summary>
    ///     Packet flag
    /// </summary>
    [Flags]
    public enum PacketFlag
    {
        None = 0,
        NoAllocate = 1,
        Unreliable = 2,
        Sequenced = 4,
        Reliable = 8
    }
}