using System;
using System.Collections.Generic;
using System.Text;

namespace CaCom
{
    public class ApduCommand
    {
        public byte[] Data { get; private set; }

        public Exception Exception { get; internal set; }

        public byte Sw1 { get; internal set; }
        public byte Sw2 { get; internal set; }

        public ApduCommand(byte[] data)
        {
            Data = data;
        }

    }
}
