using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Custom;

namespace ShadowCapturer
{
    static class IOCTLs
    {
        public static IOControlCode IOCTLShadowDriverStartWfp = new IOControlCode(0x00000012, 0x909, IOControlAccessMode.ReadWrite, IOControlBufferingMethod.DirectInput);
        public static IOControlCode IOCTLShadowDriverRequirePacketInfo = new IOControlCode(0x00000012, 0x910, IOControlAccessMode.Any, IOControlBufferingMethod.DirectInput);
        public static IOControlCode IOCTLShadowDriverGetDriverVersion = new IOControlCode(0x00000012, 0x922, IOControlAccessMode.Any, IOControlBufferingMethod.Buffered);
        public static IOControlCode IOCTLShadowDriverDequeueNotification = new IOControlCode(0x00000012, 0x911, IOControlAccessMode.Any, IOControlBufferingMethod.Buffered);
        public static IOControlCode IOCTLShadowDriverQueueNotification = new IOControlCode(0x00000012, 0x921, IOControlAccessMode.Any, IOControlBufferingMethod.Buffered);
        //#define IOCTL_SHADOWDRIVER_APP_REGISTER					CTL_CODE(FILE_DEVICE_NETWORK, 0x901, METHOD_BUFFERED, FILE_ANY_ACCESS)
        public static IOControlCode IOCTLShadowDriverAppRegister = new IOControlCode(0x00000012, 0x901, IOControlAccessMode.Any, IOControlBufferingMethod.Buffered);

        //#define IOCTL_SHADOWDRIVER_APP_DEREGISTER                 CTL_CODE(FILE_DEVICE_NETWORK, 0x902, METHOD_BUFFERED, FILE_ANY_ACCESS)
        public static IOControlCode IOCTLShadowDriverAppDeregister = new IOControlCode(0x00000012, 0x902, IOControlAccessMode.Any, IOControlBufferingMethod.Buffered);

        //#define IOCTL_SHADOWDRIVER_GET_QUEUE_INFO               CTL_CODE(FILE_DEVICE_NETWORK, 0x923, METHOD_BUFFERED, FILE_ANY_ACCESS)
        public static IOControlCode IOCTLShadowDriverGetQueueInfo = new IOControlCode(0x00000012, 0x923, IOControlAccessMode.Any, IOControlBufferingMethod.Buffered);
    }
}
