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
        public FilteringLayer FilteringLayer { set; get; }
        public PhysicalAddress MacAddress { set; get; }
        public IPAddress IPAddress { set; get; }
        public FilterMatchType MatchType { set; get; }
        public AddressLocation AddressLocation { set; get; }
    }
}
