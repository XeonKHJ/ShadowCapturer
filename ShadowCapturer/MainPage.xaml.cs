using ShadowDriver.DriverCommunicator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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

            _filter = new ShadowFilter(App.AppRegisterContext.AppId, App.AppRegisterContext.AppName);
            _filter.PacketReceived += Filter_PacketReceived;
        }

        
        private void Filter_PacketReceived(byte[] buffer)
        {
            var netPacketViewModel = new NetPacketViewModel();
            for (int i = 0; i < 20; ++i)
            {
                netPacketViewModel.Content += buffer[i].ToString("X4");
            }

            NetPacketViewModels.Add(netPacketViewModel);
        }

        ShadowFilter _filter;
        public ObservableCollection<NetPacketViewModel> NetPacketViewModels { get; } = new ObservableCollection<NetPacketViewModel>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            PhysicalAddress macAddress = (PhysicalAddress)(e.Parameter);
            FilterCondition filterCondition = new FilterCondition
            {
                AddressLocation = AddressLocation.Local,
                FilteringLayer = FilteringLayer.LinkLayer,
                MacAddress = macAddress,
                MatchType = FilterMatchType.Equal
            };

            _filter.AddFilteringCondition(filterCondition);
            _filter.StartFiltering();
        }
    }
}
