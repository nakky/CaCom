using System;
using System.Collections.Generic;
using System.Text;

namespace CaCom
{
    interface IRecordConverter
    {
        void Generate(NdefRecord record, byte[] type, byte[] id, byte[] payload);
    }
}
