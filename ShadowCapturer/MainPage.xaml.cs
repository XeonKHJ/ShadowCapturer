using ShadowDriver.Core;
using ShadowDriver.Core.Common;
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

            _filter = new ShadowFilter(App.RandomAppIdGenerator.Next(), App.AppRegisterContext.AppName);
            _filter.FilterReady += Filter_FilterReady;
            _filter.PacketReceived += Filter_PacketReceived;
        }

        private List<FilterCondition> _conditions = new List<FilterCondition>();
        private async void Filter_FilterReady()
        {
            await _filter.RegisterAppToDeviceAsync();
            foreach (var condition in _conditions)
            {
                await _filter.AddFilteringConditionAsync(condition);
            }
            await _filter.StartFilteringAsync();
        }

        private async void Filter_PacketReceived(byte[] buffer)
        {
            var netPacketViewModel = new NetPacketViewModel();
            for (int i = sizeof(int); i < buffer.Length; ++i)
            {
                netPacketViewModel.Content += buffer[i].ToString("X4");
            }

            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                NetPacketViewModels.Add(netPacketViewModel);
            });
        }

        ShadowFilter _filter;
        public ObservableCollection<NetPacketViewModel> NetPacketViewModels { get; } = new ObservableCollection<NetPacketViewModel>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            PhysicalAddress macAddress = (PhysicalAddress)(e.Parameter);
            FilterCondition filterConditionOut = new FilterCondition
            {
                AddressLocation = AddressLocation.Local,
                FilteringLayer = FilteringLayer.LinkLayer,
                MacAddress = macAddress,
                MatchType = FilterMatchType.Equal,
                PacketDirection = NetPacketDirection.Out
            };
            var filterConditionIn = new FilterCondition
            {
                AddressLocation = AddressLocation.Local,
                FilteringLayer = FilteringLayer.LinkLayer,
                MacAddress = macAddress,
                MatchType = FilterMatchType.Equal,
                PacketDirection = NetPacketDirection.In
            };

            _conditions.Add(filterConditionOut);
            _conditions.Add(filterConditionIn);

            _filter.StartFilterWatcher();
        }
    }
}
