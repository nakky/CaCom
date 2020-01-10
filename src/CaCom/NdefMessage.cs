using System;
using System.Collections.Generic;
using System.Text;

namespace CaCom
{
    public class NdefMessage
    {
        public List<NdefRecord> Records { get; private set; }


        public TlvTag Tag { get; set; }

        public byte Length { get; private set; }

        public NdefMessage(TlvTag tag = TlvTag.NdefMessage)
        {
            Tag = tag;
            Records = new List<NdefRecord>();
        }

        public NdefMessage(byte[] binaryData)
        {
            Records = new List<NdefRecord>();
            FromBinaryData(binaryData);
        }

        public void AddRecord(NdefRecord record)
        {
            Records.Add(record);

            for (int i = 0; i < Records.Count; i++) {
                if (i == 0) Records[i].IsMessageBegin = true;
                else Records[0].IsMessageBegin = false;

                if (i == Records.Count - 1) Records[i].IsMessageEnd = true;
                else Records[0].IsMessageEnd = false;
            }
        }

        public bool FromBinaryData(byte[] data)
        {
            if (data == null || data.Length <= 1) return false;

            Tag = (TlvTag)data[0];

            Length = data[1];

            int currentindex = 2;
            while (true)
            {
                if (currentindex >= data.Length - 1) break;
                else if (data[currentindex] == 0xfe) break;

                NdefRecord record = new NdefRecord();
                currentindex += record.FromBinaryData(data, currentindex);
                Records.Add(record);
            }

            return true;
        }

        public byte[] ToBinaryData()
        {
            if (Records == null || Records.Count == 0) return null;

            List<byte[]> data = new List<byte[]>();
            byte length = 0;

            for (int i = 0; i < Records.Count; i++)
            {
                byte[] d = Records[i].ToBinaryData();
                data.Add(d);
                length += (byte)d.Length;
            }

            byte[] binaryData = new byte[2 + length + 1];
            binaryData[0] = (byte)Tag;
            binaryData[1] = length;

            int currentIndex = 2;
            for (int i = 0; i < data.Count; i++)
            {
                Array.Copy(data[i], 0, binaryData, currentIndex, data[i].Length);
                currentIndex += data[i].Length;
            }

            binaryData[binaryData.Length - 1] = (byte)0xfe;

            return binaryData;
        }

    }
}
