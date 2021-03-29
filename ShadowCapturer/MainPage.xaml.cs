using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.Custom;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace ShadowCapturer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            var selector = CustomDevice.GetDeviceSelector(InterfaceGuid);

            // Create a device watcher to look for instances of the fx2 device interface
            var shadowDriverDeviceWatcher = Windows.Devices.Enumeration.DeviceInformation.CreateWatcher(
                            selector,
                            new string[] { "System.Devices.DeviceInstanceId" }
                            );

            shadowDriverDeviceWatcher.Added += ShadowDriverDeviceWatcher_Added;
            shadowDriverDeviceWatcher.Removed += ShadowDriverDeviceWatcher_Removed; ;
            shadowDriverDeviceWatcher.Start();
        }
        static public Guid InterfaceGuid { get; } = new Guid("45f22bb7-6bc3-4545-96ed-73de89c46e7d");
        private void ShadowDriverDeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate args)
        {
            throw new NotImplementedException();
        }

        public CustomDevice ShadowDriverDevice { set; get; } = null;
        private async void ShadowDriverDeviceWatcher_Added(DeviceWatcher sender, DeviceInformation args)
        {
            System.Diagnostics.Debug.WriteLine(args.Id);
            ShadowDriverDevice = await CustomDevice.FromIdAsync(args.Id, DeviceAccessMode.ReadWrite, DeviceSharingMode.Exclusive);

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                FuckBlock.Text = "成功连接设备";
            });
        }

        public static IOControlCode IOCTLShadowDriverStartWfp = new IOControlCode(0x00000012, 0x909, IOControlAccessMode.ReadWrite, IOControlBufferingMethod.DirectInput);
        public static IOControlCode IOCTLShadowDriverRequirePacketInfo = new IOControlCode(0x00000012, 0x910, IOControlAccessMode.Any, IOControlBufferingMethod.DirectInput);
        public static IOControlCode IOCTLShadowDriverGetDriverVersion = new IOControlCode(0x00000012, 0x922, IOControlAccessMode.Any, IOControlBufferingMethod.Buffered);
        public static IOControlCode IOCTLShadowDriverRequirePacketInfoShit = new IOControlCode(0x00000012, 0x911, IOControlAccessMode.Any, IOControlBufferingMethod.Buffered);
        public static IOControlCode IOCTLShadowDriverInvertNotification = new IOControlCode(0x00000012, 0x921, IOControlAccessMode.Any, IOControlBufferingMethod.Buffered);

        private int _fuckme = 0;
        async void SendIOCTL(IOControlCode controlCode)
        {
            if(ShadowDriverDevice != null)
            {
                if(controlCode == IOCTLShadowDriverInvertNotification)
                {
                    _fuckme++;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        SendReceiveCountBlock.Text = _fuckme.ToString();
                    });
                }
                MemoryBuffer buffer = new MemoryBuffer(1000);

                var status = await ShadowDriverDevice.SendIOControlAsync(controlCode, null, null);


                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    FuckBlock.Text = controlCode.ControlCode.ToString();
                });
                if (controlCode == IOCTLShadowDriverInvertNotification)
                {
                    _fuckme--;
                    await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        SendReceiveCountBlock.Text = _fuckme.ToString();
                    });
                }


            }
        }

        private void QueueIOCTLButton_Click(object sender, RoutedEventArgs e)
        {
            SendIOCTL(IOCTLShadowDriverInvertNotification);
        }

        private void DequeueIOCTLButton_Click(object sender, RoutedEventArgs e)
        {
            SendIOCTL(IOCTLShadowDriverRequirePacketInfoShit);
        }

        private void TestIOCTLButton_Click(object sender, RoutedEventArgs e)
        {
            SendIOCTL(IOCTLShadowDriverStartWfp);
        }

        private async void GetVersionIOCTLButton_Click(object sender, RoutedEventArgs e)
        {
            byte[] outputBuffer = new byte[64];
            var status = await ShadowDriverDevice.SendIOControlAsync(IOCTLShadowDriverGetDriverVersion, null, outputBuffer.AsBuffer());

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                string outputString = Encoding.Unicode.GetString(outputBuffer);
                DriverVersionBlock.Text = Encoding.Unicode.GetString(outputBuffer);
            });
        }
    }
}
