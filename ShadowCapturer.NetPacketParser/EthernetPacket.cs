using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace ShadowCapturer.NetPacketParser
{
    public class EthernetPacket
    {
        public EthernetPacket(byte[] buffer)
        {
            int currentIndex = 0;
            DestinationAddress = new PhysicalAddress(buffer.Take(6).ToArray());
            currentIndex += 6;
            SourceAddress = new PhysicalAddress(buffer.Skip(currentIndex).Take(6).ToArray());
            currentIndex += 6;
            uint etherTypeField = (((uint)(buffer[currentIndex])) << 8) + ((uint)buffer[currentIndex+1]);
            EtherType = (EtherTypes)etherTypeField;
            currentIndex += 2;
            PacketSize = buffer.Length;
            OriginalData = buffer;
            HeaderLength = currentIndex;
        }
        public DateTime ArrivedTime { get; } = DateTime.Now;
        public int HeaderLength { private set; get; }
        public byte[] OriginalData { private set; get; }
        public int PacketSize { private set; get; }
        public PhysicalAddress SourceAddress { private set; get; } = PhysicalAddress.None;
        public PhysicalAddress DestinationAddress { private set; get; } = PhysicalAddress.None;
        public byte[] Payload { private set; get; }
        public EtherTypes EtherType { private set; get; }
    }
}
