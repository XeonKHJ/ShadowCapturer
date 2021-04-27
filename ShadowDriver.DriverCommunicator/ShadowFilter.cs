﻿using System;
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
            _shadowDevice = await CustomDevice.FromIdAsync(args.Id, DeviceAccessMode.ReadWrite, DeviceSharingMode.Shared);

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
        public async Task RegisterAppToDeviceAsync()
        {
            var contextBytes = _shadowRegisterContext.SeralizeToByteArray();
            byte[] outputBuffer = new byte[sizeof(int)];
            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAppRegister, contextBytes.AsBuffer(), outputBuffer.AsBuffer());
            var status = BitConverter.ToInt32(outputBuffer, 0);
            if (status != 0)
            {
                HandleError(status);
            }
        }
        public async Task AddFilteringConditionAsync(FilterCondition condition)
        {
            byte[] encodedNetLayer = BitConverter.GetBytes((int)condition.FilteringLayer);
            byte[] encodedMatchType = BitConverter.GetBytes((int)condition.MatchType);
            byte[] encodedAddressLocation = BitConverter.GetBytes((int)condition.AddressLocation);
            byte[] encodedDirection = BitConverter.GetBytes((int)condition.PacketDirection);
            byte[] encodedAddressFamily = new byte[sizeof(int)];
            byte[] filteringAddressAndMask = new byte[32];
            switch (condition.FilteringLayer)
            {
                case FilteringLayer.LinkLayer:
                    condition.MacAddress.GetAddressBytes().CopyTo(filteringAddressAndMask, 0);
                    break;
                case FilteringLayer.NetworkLayer:
                    condition.IPAddress.GetAddressBytes().CopyTo(filteringAddressAndMask, 0);
                    switch(condition.IPAddress.AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            if(BitConverter.IsLittleEndian)
                            {
                                Array.Reverse(filteringAddressAndMask, 0, 4);
                            }
                            condition.IPMask.GetAddressBytes().CopyTo(filteringAddressAndMask, 4);
                            encodedAddressFamily = BitConverter.GetBytes((int)IpAddrFamily.IPv4);
                            break;
                        case System.Net.Sockets.AddressFamily.InterNetworkV6:
                            condition.IPMask.GetAddressBytes().CopyTo(filteringAddressAndMask, 16);
                            encodedAddressFamily = BitConverter.GetBytes((int)IpAddrFamily.IPv4);
                            break;
                    }
                    break;
            }

            byte[] inputBuffer = new byte[6 * sizeof(int) + filteringAddressAndMask.Length];
            
            var contextBytes = _shadowRegisterContext.SeralizeAppIdToByteArray();

            var conditionBeginIndex = 0;
            contextBytes.CopyTo(inputBuffer, conditionBeginIndex);
            conditionBeginIndex += contextBytes.Length;
            encodedNetLayer.CopyTo(inputBuffer, conditionBeginIndex);
            conditionBeginIndex += encodedNetLayer.Length;
            encodedMatchType.CopyTo(inputBuffer, conditionBeginIndex);
            conditionBeginIndex += encodedMatchType.Length;
            encodedAddressLocation.CopyTo(inputBuffer, conditionBeginIndex);
            conditionBeginIndex += encodedAddressLocation.Length;
            encodedDirection.CopyTo(inputBuffer, conditionBeginIndex);
            conditionBeginIndex += encodedDirection.Length;
            encodedAddressFamily.CopyTo(inputBuffer, conditionBeginIndex);
            conditionBeginIndex += encodedAddressFamily.Length;
            filteringAddressAndMask.CopyTo(inputBuffer, conditionBeginIndex);

            byte[] outputBuffer = new byte[sizeof(int)];

            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAddCondition, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());

            var status = BitConverter.ToInt32(outputBuffer, 0);
            if (status != 0)
            {
                throw new Exception("Add Condition Error");
            }
        }
        public async Task StartFiltering()
        {
            var outputBuffer = new byte[sizeof(int)];
            _isQueueingContinue = true;
            for (int i = 0; i < 20; ++i)
            {
                InqueueIOCTLForFurtherNotification();
            }

            var inputBuffer = _shadowRegisterContext.SeralizeAppIdToByteArray();

            try
            {
                await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverStartFiltering, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());
            }
            catch(Exception exception)
            {
                throw exception;
            }

            int status = BitConverter.ToInt32(outputBuffer, 0);

            if (status != 0)
            {
                throw new Exception("Filtering Start Error!");
            }
            _isFilteringStarted = true;
        }

        public async Task DeregisterAppFromDeviceAsync()
        {
            _isQueueingContinue = false;
            _isFilteringStarted = false;
            var contextBytes = _shadowRegisterContext.SeralizeAppIdToByteArray();
            byte[] outputBuffer = new byte[sizeof(int)];
            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverAppDeregister, contextBytes.AsBuffer(), outputBuffer.AsBuffer());
            var status = BitConverter.ToInt32(outputBuffer, 0);
            if (status != 0)
            {
                HandleError(status);
            }
        }

        private bool _isQueueingContinue = false;
        private async void InqueueIOCTLForFurtherNotification()
        {
            var outputBuffer = new byte[2000];

            var inputBuffer = _shadowRegisterContext.SeralizeAppIdToByteArray();
            while(_isQueueingContinue)
            {
                try
                {
                    await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverQueueNotification, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());
                }
                catch(NullReferenceException exception)
                {
                    _isQueueingContinue = false;
                    break;
                }
                catch(Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception.Message);
                }

                int status = BitConverter.ToInt32(outputBuffer, 0);

                if (status != 0)
                {
                    _isQueueingContinue = false;
                    if (status != 1)
                    {
                        HandleError(status, "Packet Recevied Error!");
                    }
                }

                var packetSize = BitConverter.ToInt64(outputBuffer, sizeof(int));
                byte[] packetBuffer = new byte[packetSize];
                Array.Copy(outputBuffer, sizeof(int) + sizeof(long), packetBuffer, 0, packetSize);

                if(_isFilteringStarted)
                {
                    PacketReceived?.Invoke(packetBuffer);
                }

            }
            return;
        }

        public async Task<int> CheckQueuedIOCTLCounts()
        {
            var outputBuffer = new byte[2 * sizeof(int)];
            var inputBuffer = _shadowRegisterContext.SeralizeAppIdToByteArray();
            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverGetQueueInfo, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());
            var status = BitConverter.ToInt32(outputBuffer, 0);
            if(status != 0)
            {
                HandleError(status);
            }
            var result = BitConverter.ToInt32(outputBuffer, sizeof(int));
            return result;
        }

        bool _isFilteringStarted = false;
        public async Task StopFilteringAsync()
        {
            _isFilteringStarted = false;
            var outputBuffer = new byte[sizeof(int)];
            _isQueueingContinue = false;
            var inputBuffer = _shadowRegisterContext.SeralizeAppIdToByteArray();
            await _shadowDevice.SendIOControlAsync(IOCTLs.IOCTLShadowDriverStopFiltering, inputBuffer.AsBuffer(), outputBuffer.AsBuffer());

            int status = BitConverter.ToInt32(outputBuffer, 0);
            if (status != 0)
            {
                HandleError(status, "Stop Filtering error");
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
