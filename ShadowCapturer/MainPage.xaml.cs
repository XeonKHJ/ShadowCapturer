using ShadowCapturer.ViewModels;
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
using Windows.UI.ViewManagement;
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

            var view = ApplicationView.GetForCurrentView();
            view.Consolidated += View_Consolidated;

            _filter = new ShadowFilter(App.RandomAppIdGenerator.Next(), App.AppName);
            _filter.FilterReady += Filter_FilterReady;
            _filter.PacketReceived += Filter_PacketReceived;
        }

        private List<FilterCondition> _conditions = new List<FilterCondition>();
        private async void Filter_FilterReady()
        {
            await _filter.RegisterAppAsync();
            foreach (var condition in _conditions)
            {
                await _filter.AddConditionAsync(condition);
            }
            await _filter.StartFilteringAsync();
        }

        private int _packetIndex = 0;
        private byte[] Filter_PacketReceived(byte[] buffer, CapturedPacketArgs args)
        {
            NetPacketParser.EthernetPacket packet = new NetPacketParser.EthernetPacket(buffer);
            var netPacketViewModel = new NetPacketViewModel(packet, _packetIndex++);


            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                NetPacketViewModels.Add(netPacketViewModel);
            }).AsTask();

            return null;
        }

        ShadowFilter _filter;
        public ObservableCollection<NetPacketViewModel> NetPacketViewModels { get; } = new ObservableCollection<NetPacketViewModel>();
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if(e.Parameter != null && e.Parameter is NetworkInterface)
            {
                NetworkInterface netInterface = (NetworkInterface)(e.Parameter);
                var ipv4Properties = netInterface.GetIPProperties().GetIPv4Properties();
                FilterCondition filterConditionOut = new FilterCondition
                {
                    AddressLocation = AddressLocation.Local,
                    FilteringLayer = FilteringLayer.LinkLayer,
                    InterfaceIndex = (uint)ipv4Properties.Index,
                    MatchType = FilterMatchType.Equal,
                    PacketDirection = NetPacketDirection.Out
                };
                var filterConditionIn = new FilterCondition
                {
                    AddressLocation = AddressLocation.Local,
                    FilteringLayer = FilteringLayer.LinkLayer,
                    InterfaceIndex = (uint)ipv4Properties.Index,
                    MatchType = FilterMatchType.Equal,
                    PacketDirection = NetPacketDirection.In
                };

                _conditions.Add(filterConditionOut);
                _conditions.Add(filterConditionIn);

                _filter.StartFilterWatcher();
            }
        }

        public ObservableCollection<ByteViewModel> PacketDetailViewModel = new ObservableCollection<ByteViewModel>();
        private void View_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Deregistering app id {0}", _filter.AppId));
            _filter.DeregisterApp();
        }
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PacketDetailViewModel.Clear();
            if(PacketDetailView.SelectedItems.Count != 0 || e.AddedItems.Count != 0)
            {
                if (e.AddedItems.First() is NetPacketViewModel viewModel)
                {
                    var data = viewModel.Packet.OriginalData;
                    for (int i = 0; i < data.Length; ++i)
                    {
                        PacketDetailViewModel.Add(new ByteViewModel(data[i]));
                    }
                }
            }

        }

        private async void StartCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            await _filter.StartFilteringAsync();
        }

        private async void StopCaptureButton_Click(object sender, RoutedEventArgs e)
        {
            await _filter.StopFilteringAsync();

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                NetPacketViewModels.Clear();
            });

            _packetIndex = 0;
        }

        private async void RestartCaptureButton_Click(object sender, RoutedEventArgs e)
        {

                NetPacketViewModels.Clear();
            _packetIndex = 0;
        }
    }
}
