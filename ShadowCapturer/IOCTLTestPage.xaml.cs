using ShadowDriver.DriverCommunicator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            _filter = new ShadowFilter(App.AppRegisterContext.AppId, App.AppRegisterContext.AppName);
            _filter.StartFilterWatcher();
            _filter.FilterReady += Filter_FilterReady;
            _filter.PacketReceived += Filter_PacketReceived;
        }

        private async void Filter_FilterReady()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.DeviceConnectStatus = "Connected";
            });
        }

        private void Filter_PacketReceived(byte[] buffer)
        {
            NetPacketViewModel netPacketViewModel = new NetPacketViewModel();
            for (int i = 0; i < 20; ++i)
            {
                netPacketViewModel.Content += buffer[i].ToString("X4");
            }
            NetPacketViewModels.Add(netPacketViewModel);
        }

        private ShadowFilter _filter;
        public IOCTLTestViewModel ViewModel { get; } = new IOCTLTestViewModel()
        {
            DeviceConnectStatus = "Disconnected",
            AppRegisterStatus = "Unchecked"
        };
        public CustomDevice ShadowDriverDevice { set; get; } = null;
        public ObservableCollection<NetPacketViewModel> NetPacketViewModels = new ObservableCollection<NetPacketViewModel>() { new NetPacketViewModel { Content = "fuck" } };
        private void ShadowDriverDeviceWatcher_Removed(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformationUpdate args)
        {
            ShadowDriverDevice = null;
            ViewModel.DeviceConnectStatus = "Disconnected";
        }

        private async void ShadowDriverDeviceWatcher_Added(Windows.Devices.Enumeration.DeviceWatcher sender, Windows.Devices.Enumeration.DeviceInformation args)
        {
            ShadowDriverDevice = await CustomDevice.FromIdAsync(args.Id, DeviceAccessMode.ReadWrite, DeviceSharingMode.Exclusive);
            if (ShadowDriverDevice != null)
            {
                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    ViewModel.DeviceConnectStatus = "Connected";
                });
            }
        }

        private async void CheckQueuedIoctlCountButton_Click(object sender, RoutedEventArgs e)
        {
            var result = await _filter.CheckQueuedIOCTLCounts();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ViewModel.QueuedIOCTLCount = result;
            });
        }

        private void StartFilterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _filter.StartFiltering();
            }
            catch(Exception exception)
            {
                DisplayException(exception);
            }
        }

        public async void DisplayException(Exception exception)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ErrorMessageBlock.Text = exception.Message;
                ErrorMessageGrid.Visibility = Visibility.Visible;
            });
        }

        private void RegisterAppButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _filter.RegisterAppToDevice();   
            }
            catch(Exception exception)
            {
                DisplayException(exception);
            }
        }

        private void DeregisterAppButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
