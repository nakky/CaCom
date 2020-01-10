using System;
using System.Collections.Generic;
using System.Text;

namespace CaCom
{
    public class WellKnownTypeConverter : IRecordConverter
    {
        public void Generate(NdefRecord record, byte[] type, byte[] id, byte[] payload)
        {
            if (type.Length == 0) throw new  NFCException("Param type is invalid.");

            string typeStr = Encoding.ASCII.GetString(type);

            switch(typeStr)
            {
                case "T":
                    {
                        var p = new TextPayload();
                        p.FromBinaryData(payload, 0);
                        record.Payload = p;
                    }
                    break;
                case "U":
                    {
                        var p = new UriPayload();
                        p.FromBinaryData(payload, 0);
                        record.Payload = p;
                    }
                    break;
                default:
                    {
                        throw new NotSupportedException("This type is not supported");
                    }
            }
        }
    }
}
