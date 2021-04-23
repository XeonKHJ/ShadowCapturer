using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace ShadowDriver.DriverCommunicator
{
    public class FilterCondition
    {
        public PhysicalAddress MacAddress { set; get; }
        public IPAddress IPAddress { set; get; }
    }
}
