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
        public int Id { set; get; }
        public string Name { set; get; }
        public string MacAddress { set; get; }

        public override string ToString()
        {
            return string.Format("{0} {1}", NetworkInterface.Name, NetworkInterface.GetPhysicalAddress().ToString());
        }
    }
}
