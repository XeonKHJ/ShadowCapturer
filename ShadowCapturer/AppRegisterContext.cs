using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace ShadowCapturer
{
    struct AppRegisterContext
    {
        public int AppId;
        public string AppName;

        /// <summary>
        /// 要注意到一般X86是小端机，ARM是大端机
        /// </summary>
        /// <param name="context"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static async Task WriteToStreamAsync(AppRegisterContext context, IOutputStream stream)
        {
            byte[] intBytes = BitConverter.GetBytes(context.AppId);
            byte[] nameBytes = Encoding.Unicode.GetBytes(context.AppName);
            //if (BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(intBytes);
            //    for(int i = 0; i < nameBytes.Length; i+=2)
            //    {
            //        var temp = nameBytes[i];
            //        nameBytes[i] = nameBytes[i + 1];
            //        nameBytes[i + 1] = temp;
            //    }
            //}
            byte[] result = new byte[intBytes.Length + nameBytes.Length];
            intBytes.CopyTo(result, 0);
            nameBytes.CopyTo(result, intBytes.Length);
            DataWriter dataWriter = new DataWriter(stream);
            dataWriter.WriteBytes(result);
            await dataWriter.StoreAsync();
        }
    }
}
