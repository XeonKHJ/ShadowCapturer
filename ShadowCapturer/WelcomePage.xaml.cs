using ShadowDriver.DriverCommunicator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class WelcomePage : Page
    {
        public WelcomePage()
        {
            this.InitializeComponent();

            var nics = NetworkInterface.GetAllNetworkInterfaces();
            foreach(var nic in nics)
            {
                NetworkInterfaceViewModels.Add(new NetworkInterfaceViewModel
                {
                    Id = nic.Id,
                    MacAddress = nic.GetPhysicalAddress().ToString(),
                    Name = nic.Name,
                    NetworkInterface = nic
                }) ;
            }
            _shadowFilter = new ShadowFilter(App.AppRegisterContext.AppId, App.AppRegisterContext.AppName);
            
        }

        private ShadowFilter _shadowFilter;
        public ObservableCollection<NetworkInterfaceViewModel> NetworkInterfaceViewModels { get; } = new ObservableCollection<NetworkInterfaceViewModel>();

        private void NicsView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var nicViewModel = (NetworkInterfaceViewModel)e.ClickedItem;
            if(nicViewModel != null)
            {
                this.Frame.Navigate(typeof(MainPage), nicViewModel.NetworkInterface.GetPhysicalAddress());
            }
        }
    }
}
