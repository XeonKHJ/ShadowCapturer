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
            var selector = CustomDevice.GetDeviceSelector(Common.InterfaceGuid);
            var shadowDriverDeviceWatcher = DeviceInformation.CreateWatcher(
                selector,
                new string[] { "System.Devices.DeviceInstanceId" }
                );

            shadowDriverDeviceWatcher.Added += ShadowDriverDeviceWatcher_Added;
            shadowDriverDeviceWatcher.Removed += ShadowDriverDeviceWatcher_Removed; ;
            shadowDriverDeviceWatcher.Start();
        }

        private void ShadowDriverDeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            _shadowDevice = null;
        }

        private async void ShadowDriverDeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            System.Diagnostics.Debug.WriteLine(args.Id);
            _shadowDevice = await CustomDevice.FromIdAsync(args.Id, DeviceAccessMode.ReadWrite, DeviceSharingMode.Exclusive);
        }

        public async void StartFiltering()
        {
            var outputBuffer = new byte[sizeof(int) + 50];

            for(int i = 0; i < 20; ++i)
            {
                InqueIOCTLForFurtherNotification();
            }

            await AppRegisterContext.WriteToStreamAsync(_shadowRegisterContext, outputBuffer.AsBuffer().AsStream().AsOutputStream());
            var result = await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAppRegister, outputBuffer.AsBuffer(), null);
        }

        private async void InqueIOCTLForFurtherNotification()
        {
            var outputBuffer = new byte[sizeof(int) + 50];

            await AppRegisterContext.WriteToStreamAsync(_shadowRegisterContext, outputBuffer.AsBuffer().AsStream().AsOutputStream());
            var result = await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAppRegister, outputBuffer.AsBuffer(), null);
            
            //通知
            
            InqueIOCTLForFurtherNotification();

            return;
        }
        public async void StopFiltering()
        {
            var outputBuffer = new byte[sizeof(int) + 50];

            await AppRegisterContext.WriteToStreamAsync(_shadowRegisterContext, outputBuffer.AsBuffer().AsStream().AsOutputStream());
            var result = await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAppRegister, outputBuffer.AsBuffer(), null);
        }
        public void AddFilteringCondition(FilterCondition condition)
        {
            
        }
    }
}
