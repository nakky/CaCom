using System;
using System.Collections.Generic;
using System.Text;

namespace CaCom
{
    public class UriPayload : NfcForumWellKnownTypePayload
    {
        public UriPrefix Prefix { get; private set; }
        public string Data { get; private set; }

        public UriPayload()
        {
        }

        public UriPayload(UriPrefix prefix, string data)
        {
            Prefix = prefix;
            Data = data;
        }

        public override byte[] GetTypeData()
        {
            return Encoding.ASCII.GetBytes("U");
        }

        public override byte[] GetId()
        {
            return null;
        }

        public override int FromBinaryData(byte[] data, int index)
        {
            Prefix = (UriPrefix)data[0];

            int dataLength = data.Length - 1;
            byte[] dataByte = new byte[dataLength];
            Array.Copy(data, 1, dataByte, 0, dataLength);

            Data = Encoding.ASCII.GetString(dataByte);

            return 1 + dataLength;

        }

        public override byte[] ToBinaryData()
        {
            byte[] dataBin = Encoding.ASCII.GetBytes(Data);
            byte[] ret = new byte[1 + dataBin.Length];

            ret[0] = (byte)Prefix;
            Array.Copy(dataBin, 0, ret, 1, dataBin.Length);

            return ret;
        }
    }
}
