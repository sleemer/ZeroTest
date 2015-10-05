using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroTransport
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
