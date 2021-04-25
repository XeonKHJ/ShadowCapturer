using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ShadowDriver.DriverCommunicator
{
    public class AppRegisterContext
    {
        public int AppId { set; get; }

        private string _appName;
        public string AppName
        {
            set
            {
                if (value != null)
                {
                    if (value.Length > 20)
                    {
                        throw new Exception("App name too long");
                    }
                    _appName = value;
                }
            }
            get
            {
                return _appName;
            }
        }
        public Guid SublayerKey { set; get; }
        public SortedDictionary<int, Guid> CalloutsKey { set; get; } = new SortedDictionary<int, Guid>
        { 
            { 0, new Guid("FCCBD974-12F1-449D-915D-87F43378E2D1") }, // 网络层IPv4发送通道回调
            { 1, new Guid("684644A1-C911-4670-A446-5559FD17DF5D") }, // 链路层发送通道
            { 2, new Guid("0A85CE9B-69CE-442D-8EF4-C5CA00F5CBD9") }, // 网络层IPv4接收通道
            { 3, new Guid("1A6D0689-47F2-47C8-AF65-0F16C0FBF5F4") }, // 链路层接收通道
            { 4, new Guid("88E91326-A38B-40DF-A287-93CACAD8E8BB") }, // 网络层IPv6发送通道
            { 6, new Guid("F9B31A96-525A-494E-9E58-3BDA921896D8") }  // 网络层IPv6接收通道
        };

        /// 要注意到一般X86是小端机，ARM是大端机
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public byte[] SeralizeToByteArray()
        {
            // appid + 40 byte of name + 16 byte of sublayer key + 16 byte of callout key
            byte[] result = new byte[Common.AppRegisterContextMaxSize + 16 + 16 * CalloutsKey.Count];
            byte[] intBytes = BitConverter.GetBytes(AppId);
            byte[] nameBytes = Encoding.Unicode.GetBytes(AppName);
            byte[] sublayerKeyBytes = SublayerKey.ToByteArray();
            intBytes.CopyTo(result, 0);
            nameBytes.CopyTo(result, sizeof(int));
            sublayerKeyBytes.CopyTo(result, Common.AppRegisterContextMaxSize);
            var currentIndex = Common.AppRegisterContextMaxSize + 16;
            foreach (var guid in CalloutsKey.Values)
            {
                var guidBytes = guid.ToByteArray();
                guidBytes.CopyTo(result, currentIndex);
                currentIndex += 16;
            }

            return result;
        }
    }
}
