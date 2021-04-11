using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Custom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ShadowCapturer
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class IOCTLTestPage : Page
    {
        public IOCTLTestPage()
        {
            this.InitializeComponent();

            var selector = CustomDevice.GetDeviceSelector(App.DeviceInterfaceGuid);

            // Create a device watcher to look for instances of the fx2 device interface
            var shadowDriverDeviceWatcher = Windows.Devices.Enumeration.DeviceInformation.CreateWatcher(
                            selector,
                            new string[] { "System.Devices.DeviceInstanceId" }
                            );

            shadowDriverDeviceWatcher.Added += ShadowDriverDeviceWatcher_Added;
            shadowDriverDeviceWatcher.Removed += ShadowDriverDeviceWatcher_Removed; ;
            shadowDriverDeviceWatcher.Start();
        }
        public IOCTLTestViewModel ViewModel { get; } = new IOCTLTestViewModel()
        {
            DeviceConnectStatus = "Disconnected",
            AppRegisterStatus = "Unchecked"
        };
        public CustomDevice ShadowDriverDevice { set; get; } = null;
        private void ShadowDriverDeviceWatcher_Removed(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformationUpdate args)
        {
            ShadowDriverDevice = null;
            ViewModel.DeviceConnectStatus = "Disconnected";
        }

        private async void ShadowDriverDeviceWatcher_Added(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformation args)
        {
            ShadowDriverDevice = await CustomDevice.FromIdAsync(args.Id, DeviceAccessMode.ReadWrite, DeviceSharingMode.Exclusive);
            if(ShadowDriverDevice != null)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ViewModel.DeviceConnectStatus = "Connected";
                });
            }
        }

        private async void RegisterAppButton_Click(object sender, RoutedEventArgs e)
        {
            var outputBuffer = new byte[sizeof(int) + 50];
            
            await AppRegisterContext.WriteToStreamAsync(App.AppRegisterContext, outputBuffer.AsBuffer().AsStream().AsOutputStream());
            var status = await ShadowDriverDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAppRegister, outputBuffer.AsBuffer(), null);

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.AppRegisterStatus = "Registered";
            });
        }

        private async void CheckQueuedIoctlCountButton_Click(object sender, RoutedEventArgs e)
        {
            var inputBuffer = new byte[sizeof(int) + 50];
            var outputBuffer = new byte[sizeof(int)];
            await AppRegisterContext.WriteToStreamAsync(App.AppRegisterContext, inputBuffer.AsBuffer().AsStream().AsOutputStream());
            var status = await ShadowDriverDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverGetQueueInfo, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());
            
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.QueuedIOCTLCount = BitConverter.ToInt32(outputBuffer, 0); ;
            });
        }

        private async void QueueIoctlButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShadowDriverDevice != null)
            {
                var outputBuffer = new byte[sizeof(int) + 50];
                await AppRegisterContext.WriteToStreamAsync(App.AppRegisterContext, outputBuffer.AsBuffer().AsStream().AsOutputStream());
                var status = await ShadowDriverDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverQueueNotification, outputBuffer.AsBuffer(), null);
            }
        }

        private async void DequeueIoctlButton_Click(object sender, RoutedEventArgs e)
        {
            if (ShadowDriverDevice != null)
            {
                var outputBuffer = new byte[sizeof(int) + 50];
                await AppRegisterContext.WriteToStreamAsync(App.AppRegisterContext, outputBuffer.AsBuffer().AsStream().AsOutputStream());
                var status = await ShadowDriverDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverDequeueNotification, outputBuffer.AsBuffer(), null);
            }
        }

        private async void DeregisterAppButton_Click(object sender, RoutedEventArgs e)
        {
            var outputBuffer = new byte[sizeof(int) + 50];

            await AppRegisterContext.WriteToStreamAsync(App.AppRegisterContext, outputBuffer.AsBuffer().AsStream().AsOutputStream());
            var status = await ShadowDriverDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAppDeregister, outputBuffer.AsBuffer(), null);

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.AppRegisterStatus = "Unregistered";
            });
        }
    }
}
