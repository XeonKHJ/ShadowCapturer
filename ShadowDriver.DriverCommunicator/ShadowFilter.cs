using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Custom;
using Windows.Devices.Enumeration;

namespace ShadowDriver.DriverCommunicator
{
    public class ShadowFilter
    {
        private AppRegisterContext _shadowRegisterContext;
        private CustomDevice _shadowDevice;
        public ShadowFilter(int appId, string appName)
        {
            _shadowRegisterContext = new AppRegisterContext()
            {
                AppId = appId,
                AppName = appName
            };
        }
        public bool IsFilterReady { private set; get; } = false;
        private void ShadowDriverDeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _shadowDevice = null;
        }

        private async void ShadowDriverDeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            System.Diagnostics.Debug.WriteLine(args.Id);
            _shadowDevice = await CustomDevice.FromIdAsync(args.Id, DeviceAccessMode.ReadWrite, DeviceSharingMode.Exclusive);

            IsFilterReady = true;
            FilterReady?.Invoke();
        }

        public void StartFilterWatcher()
        {
            var selector = CustomDevice.GetDeviceSelector(Common.InterfaceGuid);
            var shadowDriverDeviceWatcher = DeviceInformation.CreateWatcher(
                selector,
                new string[] { "System.Devices.DeviceInstanceId" }
                );

            shadowDriverDeviceWatcher.Added += ShadowDriverDeviceWatcher_Added;
            shadowDriverDeviceWatcher.Removed += ShadowDriverDeviceWatcher_Removed; ;
            shadowDriverDeviceWatcher.Start();
        }
        public async void RegisterAppToDevice()
        {
            var contextBytes = _shadowRegisterContext.SeralizeToByteArray();
            byte[] outputBuffer = new byte[sizeof(int)];
            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAddCondition, contextBytes.AsBuffer(), outputBuffer.AsBuffer());
            var status = BitConverter.ToInt32(outputBuffer, 0);
            if (status != 0)
            {
                throw new Exception("Add Condition Error");
            }
        }
        public async void AddFilteringCondition(FilterCondition condition)
        {
            byte[] encodedNetLayer = BitConverter.GetBytes((int)condition.FilteringLayer);
            byte[] encodedMatchType = BitConverter.GetBytes((int)condition.MatchType);
            byte[] encodedAddressLocation = BitConverter.GetBytes((int)condition.AddressLocation);
            byte[] filteringAddress = null;
            switch (condition.FilteringLayer)
            {
                case FilteringLayer.LinkLayer:
                    filteringAddress = condition.MacAddress.GetAddressBytes();
                    break;
                case FilteringLayer.NetworkLayer:
                    filteringAddress = condition.IPAddress.GetAddressBytes();
                    break;
            }
            byte[] inputBuffer = new byte[Common.AppRegisterContextMaxSize + encodedNetLayer.Length + encodedMatchType.Length + encodedAddressLocation.Length + filteringAddress.Length];
            var conditionBeginIndex = Common.AppRegisterContextMaxSize;
            var contextBytes = _shadowRegisterContext.SeralizeToByteArray();
            contextBytes.CopyTo(inputBuffer, 0);
            encodedNetLayer.CopyTo(inputBuffer, conditionBeginIndex);
            encodedMatchType.CopyTo(inputBuffer, conditionBeginIndex + encodedNetLayer.Length);
            encodedAddressLocation.CopyTo(inputBuffer, conditionBeginIndex + encodedNetLayer.Length + encodedMatchType.Length);
            filteringAddress.CopyTo(inputBuffer, conditionBeginIndex + encodedNetLayer.Length + encodedMatchType.Length + encodedAddressLocation.Length);

            byte[] outputBuffer = new byte[sizeof(int)];

            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAddCondition, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());

            var status = BitConverter.ToInt32(outputBuffer, 0);
            if (status != 0)
            {
                throw new Exception("Add Condition Error");
            }
        }
        public async void StartFiltering()
        {
            var inputBuffer = new byte[Common.AppRegisterContextMaxSize];
            var outputBuffer = new byte[sizeof(int)];

            for (int i = 0; i < 20; ++i)
            {
                InqueIOCTLForFurtherNotification();
            }

            var contextBytes = _shadowRegisterContext.SeralizeToByteArray();
            contextBytes.CopyTo(inputBuffer, 0);
            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAppRegister, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());

            int status = BitConverter.ToInt32(outputBuffer, 0);

            if (status != 0)
            {
                throw new Exception("Filtering Start Error!");
            }
        }

        private async void InqueIOCTLForFurtherNotification()
        {
            bool isQueueingContinue = true;
            var inputBuffer = new byte[Common.AppRegisterContextMaxSize];
            var outputBuffer = new byte[2000];

            var contextBytes = _shadowRegisterContext.SeralizeToByteArray();
            contextBytes.CopyTo(inputBuffer, 0);
            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverQueueNotification, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());

            int status = BitConverter.ToInt32(outputBuffer, sizeof(int));

            if (status != 0)
            {
                isQueueingContinue = false;
                if (status != 1)
                {
                    HandleError(status, "Packet Recevied Error!");
                }
            }

            //通知
            if (isQueueingContinue)
            {
                InqueIOCTLForFurtherNotification();
            }

            return;
        }

        public async Task<int> CheckQueuedIOCTLCounts()
        {
            var inputBuffer = new byte[Common.AppRegisterContextMaxSize];
            var outputBuffer = new byte[2 * sizeof(int)];
            var contextBytes = _shadowRegisterContext.SeralizeToByteArray();
            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverGetQueueInfo, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());
            var status = BitConverter.ToInt32(outputBuffer, 0);
            if(status != 0)
            {
                HandleError(status);
            }
            var result = BitConverter.ToInt32(outputBuffer, sizeof(int));
            return result;
        }
        public async void StopFiltering()
        {
            var inputBuffer = new byte[Common.AppRegisterContextMaxSize];
            var outputBuffer = new byte[sizeof(int)];

            var contextBytes = _shadowRegisterContext.SeralizeToByteArray();
            contextBytes.CopyTo(inputBuffer, 0);
            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAppRegister, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());

            int status = BitConverter.ToInt32(outputBuffer, sizeof(int));
            if (status != 0)
            {
                throw new Exception("Stop filtering error");
            }
        }

        private static void HandleError(int errorCode, string appendMessage = "")
        {
            if(errorCode != 0)
            {
                throw new Exception(string.Format("{0}: {1}", errorCode, appendMessage));
            }
        }
        public delegate void PacketReceivedEventHandler(byte[] buffer);
        public event PacketReceivedEventHandler PacketReceived;

        public delegate void FilterReadyEventHandler();
        public event FilterReadyEventHandler FilterReady;
    }
}
