namespace ENet
{
    public class Callbacks
    {
        private ENetCallbacks nativeCallbacks;

        internal ENetCallbacks NativeData
        {
            get => this.nativeCallbacks;
            set => this.nativeCallbacks = value;
        }

        public Callbacks(AllocCallback allocCallback, FreeCallback freeCallback, NoMemoryCallback noMemoryCallback)
        {
            this.nativeCallbacks.malloc = allocCallback;
            this.nativeCallbacks.free = freeCallback;
            this.nativeCallbacks.noMemory = noMemoryCallback;
        }
    }
}