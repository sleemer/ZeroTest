using System;
using ProtoBuf;

namespace ZeroCore
{
    [ProtoContract]
    public class ImagePacket
    {
        [ProtoMember(1)]
        public DateTime Timestamp { get; set; }
        [ProtoMember(2)]
        public byte[] Image { get; set; }
    }
}
