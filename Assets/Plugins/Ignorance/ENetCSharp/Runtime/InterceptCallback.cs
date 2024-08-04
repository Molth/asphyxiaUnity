using System;

namespace ENet
{
    public delegate int InterceptCallback(ref Event @event, ref Address address, IntPtr receivedData, int receivedDataLength);
}