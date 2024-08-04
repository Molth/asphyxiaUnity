namespace ENet
{
    internal struct ENetCallbacks
    {
        public AllocCallback malloc;
        public FreeCallback free;
        public NoMemoryCallback noMemory;
    }
}