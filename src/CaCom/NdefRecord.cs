using System;
using System.Collections.Generic;
using System.Text;

namespace CaCom
{
    public class NdefRecord
    {
        static Dictionary<TypeNameFormat, IRecordConverter> converterMap = new Dictionary<TypeNameFormat, IRecordConverter>();
        static bool Initialized = false;

        public static void Initialize()
        {
            converterMap.Add(TypeNameFormat.NfcForumWellKnownType, new WellKnownTypeConverter());
            Initialized = true;
        }

        public byte[] Data { get; set; }

        public bool IsMessageBegin { get; set; }
        public bool IsMessageEnd { get; set; }
        public bool IsChunked { get; set; }
        public bool IsShort { get; set; }
        public bool hasIdLength { get; set; }
        public TypeNameFormat TypeNameFormat { get; set; }

        public byte TypeLength { get; set; }
        
        public int PayloadLength { get; set; }

        public Payload Payload { get; set; }

        public byte IdLength { get; set; }

        public NdefRecord()
        {
            if(!Initialized)
            {
                Initialize();
            }
        }

        public NdefRecord(Payload payload, bool isChancked = false)
        {
            if (!Initialized)
            {
                Initialize();
            }

            IsChunked = isChancked;

            TypeNameFormat = payload.GetTypNameFormat();

            Payload = payload;

            byte[] type = Payload.GetTypeData();
            TypeLength = (byte)type.Length;

            byte[] idata = Payload.GetId();
            if(idata != null)
            {
                IdLength = (byte)idata.Length;
                hasIdLength = true;
            }
            else
            {
                IdLength = 0;
                hasIdLength = false;
            }
           

            byte[] pdata = Payload.ToBinaryData();
            PayloadLength = pdata.Length;

            if (PayloadLength > 0xff) IsShort = false;
            else IsShort = true;
        }

        public int FromBinaryData(byte[] data, int index)
        {
            int origIndex = index;

            byte b = 0;

            b = data[index];
            index++;

            TypeNameFormat = (TypeNameFormat)(b & 0x07);
            if ((b & 0x08) != 0) hasIdLength = true;
            if ((b & 0x10) != 0) IsShort = true;
            if ((b & 0x20) != 0) IsChunked = true;
            if ((b & 0x40) != 0) IsMessageEnd = true;
            if ((b & 0x80) != 0) IsMessageBegin = true;

            TypeLength = data[index];
            index++;

            if (IsShort)
            {
                PayloadLength = (int)data[index];
                index++;
            }
            else
            {
                PayloadLength = (int)data[index];
                index++;
                PayloadLength += (int)(data[index] << 8);
                index++;
                PayloadLength += (int)(data[index] << 16);
                index++;
                PayloadLength += (int)(data[index] << 24);
                index++;
            }

            if (hasIdLength)
            {
                IdLength = data[index];
                index++;
            }

            byte[] typeData = new byte[TypeLength];
            Array.Copy(data, index, typeData, 0, TypeLength);
            index += TypeLength;

            byte[] idData = null;

            if (hasIdLength)
            {
                idData = new byte[IdLength];
                Array.Copy(data, index, idData, 0, IdLength);
                index += IdLength;
            }

            byte[] payloadData = new byte[PayloadLength];
            Array.Copy(data, index, payloadData, 0, PayloadLength);
            index += PayloadLength;

            if (converterMap.ContainsKey(TypeNameFormat))
            {
                var converter = converterMap[TypeNameFormat];
                converter.Generate(this, typeData, idData, payloadData);
            }
            else
            {
                throw new NotSupportedException("[" + TypeNameFormat + "] is not supported");
            }


            return index - origIndex;
        }


        public byte[] ToBinaryData()
        {
            if (Payload == null) throw new NFCException("Payload data is not found.");

            int length = 1 + 1; //Flags and Type Length

            if (IsShort) length += 1;
            else length += 4;

            if(hasIdLength) length += 1; //ID Length byte

            length += TypeLength;

            if(hasIdLength) length += IdLength;

            length += PayloadLength;

            byte[] type = Payload.GetTypeData();
            byte[] id = Payload.GetId();
            byte[] pdata = Payload.ToBinaryData();

            byte[] ret = new byte[length];

            int index = 0;

            byte frag = 0;
            frag |= (byte)TypeNameFormat;
            if (IdLength != 0) frag |= 0x08;
            if(IsShort) frag |= 0x10;
            if (IsChunked) frag |= 0x20;
            if (IsMessageEnd) frag |= 0x40;
            if (IsMessageBegin) frag |= 0x80;

            ret[index] = frag;
            index++;

            ret[index] = TypeLength;
            index++;

            if (IsShort)
            {
                ret[index] = (byte)PayloadLength;
                index++;
            }
            else
            {
                byte[] plByte = BitConverter.GetBytes(PayloadLength);
                Array.Copy(plByte, 0, ret, index, plByte.Length);
                index += plByte.Length;
            }

            if(hasIdLength)
            {
                ret[index] = IdLength;
                index++;
            }

            Array.Copy(type, 0, ret, index, TypeLength);
            index += TypeLength;

            if (hasIdLength)
            {
                Array.Copy(id, 0, ret, index, IdLength);
                index += IdLength;
            }


            Array.Copy(pdata, 0, ret, index, PayloadLength);

            return ret;
        }

    }
}
