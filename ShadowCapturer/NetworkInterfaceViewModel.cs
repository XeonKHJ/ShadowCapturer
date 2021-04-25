using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ShadowCapturer
{
    public class NetworkInterfaceViewModel
    {
        public NetworkInterface NetworkInterface;
        public string Id { set; get; }
        public string Name { set; get; }
        public string MacAddress { set; get; }
    }
}
