using ShadowCapturer.NetPacketParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
namespace ShadowCapturer
{
    public class NetPacketViewModel
    {
        public NetPacketViewModel(EthernetPacket packet, int index)
        {
            Index = index;
            DestinationAddress = packet.DestinationAddress.ToString();
            SourceAddress = packet.SourceAddress.ToString();
            EtherType = packet.EtherType.ToString();
            PacketSize = packet.PacketSize;
            Packet = packet;
            ArriveTime = packet.ArrivedTime;
            switch(packet.EtherType)
            {
                case EtherTypes.IPv4:
                    IPv4Packet ipv4Packet = new IPv4Packet(packet.OriginalData.Skip(packet.HeaderLength).Take(packet.PacketSize - packet.HeaderLength).ToArray());
                    DestinationAddress = ipv4Packet.DestinationAddress.ToString();
                    SourceAddress = ipv4Packet.SourceAddress.ToString();
                    EtherType = ipv4Packet.Protocal.ToString();
                    break;
            }

        }
        public DateTime ArriveTime {private set; get;}
        public EthernetPacket Packet { private set; get; }
        public int Index { set; get; }
        public string SourceAddress { set; get; }
        public string DestinationAddress { set; get; }
        public string EtherType { set; get; }
        public int PacketSize { set; get; }
        public string Content
        {
            get
            {
                string content = string.Empty;
                for (int i = sizeof(int); i < PacketSize; ++i)
                {
                    content += Packet.OriginalData[i].ToString("X4");
                }
                return content;
            }
        }
        public override string ToString()
        {
            return Content;
        }
    }
}
