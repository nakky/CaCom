using System;
using System.Collections.Generic;
using System.Text;

namespace CaCom
{
    public class Reader
    {
        public string Name { get; private set; }
        public int Index { get; private set; }

        public Card CurrentCard { get; internal set; }

        public ShareParam ShareParam { get; set; }
        public Protocol Protocol { get; set; }

        public CardState State { get; internal set; }

        public string SerialNumber { get; internal set; }

        public Reader(string name)
        {
            Name = name;

            string[] token = name.Split(' ');
            string[] number = token[token.Length - 1].Split('.');
            Index = int.Parse(number[number.Length - 1]);

            ShareParam = ShareParam.Shared;
            Protocol = Protocol.T1;
            State = CardState.Unaware;
        }
    }
}
