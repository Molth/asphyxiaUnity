namespace ENet
{
    public struct Event
    {
        private ENetEvent nativeEvent;

        internal ENetEvent NativeData
        {
            get => this.nativeEvent;
            set => this.nativeEvent = value;
        }

        internal Event(ENetEvent @event) => this.nativeEvent = @event;

        public EventType Type => this.nativeEvent.type;

        public Peer Peer => new Peer(this.nativeEvent.peer);

        public byte ChannelID => this.nativeEvent.channelID;

        public uint Data => this.nativeEvent.data;

        public Packet Packet => new Packet(this.nativeEvent.packet);
    }
}