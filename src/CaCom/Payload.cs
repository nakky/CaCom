using System;
using System.Collections.Generic;
using System.Text;

namespace CaCom
{
    public abstract class Payload
    {
        public abstract TypeNameFormat GetTypNameFormat();

        public abstract byte[] GetTypeData();
        public abstract byte[] GetId();

        public abstract int FromBinaryData(byte[] data, int index);
        public abstract byte[] ToBinaryData();
    }

    public abstract class NfcForumWellKnownTypePayload : Payload
    {
        public override TypeNameFormat GetTypNameFormat() { return TypeNameFormat.NfcForumWellKnownType; }
    }
}
