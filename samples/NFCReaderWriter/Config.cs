using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NFCReaderWriter
{
    [Serializable]
    public class Config
    {
        public Config()
        {
        }

        public int UserMemoryPage = 0;
        public int UpdateMemorySize = 16;
    }
}
