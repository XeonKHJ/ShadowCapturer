using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;

namespace ShadowCapturer.NetPacketParser
{
    public class IPv4Packet
    {
        public IPv4Packet(byte[] payload)
        {
            int currentIndex = 0;
            var versionAndHeaderLengthField = payload.Skip(currentIndex).Take(1).ToArray();
            Version = versionAndHeaderLengthField[0] & 0xF0;
            HeaderLength = (versionAndHeaderLengthField[0] & 0x0F) * 5;
            currentIndex += 1;
            // GEt services field;
            // Not implemented.
            currentIndex += 1;
            var totalLengthField = payload.Skip(currentIndex).Take(2).ToArray();
            TotalLength = (int)(((uint)totalLengthField[0] << 8) + (uint)totalLengthField[1]);
            currentIndex += 2;
            //Get identification
            currentIndex += 2;
            //Get flags and fragment offset;
            currentIndex += 2;

            //Get time to live
            currentIndex += 1;

            //GetProtocal
            var protocalField = payload[currentIndex];
            Protocal = (IPProtocals)protocalField;
            currentIndex += 1;

            //Get checksum.
            var checkSumField = payload.Skip(currentIndex).Take(2).ToArray();
            CheckSum = (int)(((uint)checkSumField[0] << 8) + (uint)checkSumField[1]);
            currentIndex += 2;

            //Get Address.
            SourceAddress = new IPAddress(payload.Skip(currentIndex).Take(4).ToArray());
            currentIndex += 4;
            DestinationAddress = new IPAddress(payload.Skip(currentIndex).Take(4).ToArray());
            currentIndex += 4;
        }
        public IPAddress SourceAddress { private set; get; }
        public IPAddress DestinationAddress { private set; get; }
        public int Version { private set; get; }
        public int HeaderLength { private set; get; }
        public int TotalLength { private set; get; }
        
        public IPProtocals Protocal { private set; get; }
        public int CheckSum { private set; get; }
    }
}
