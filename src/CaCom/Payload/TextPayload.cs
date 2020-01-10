using System;
using System.Collections.Generic;
using System.Text;

namespace CaCom
{
    public class TextPayload : NfcForumWellKnownTypePayload
    {
        public string Language { get; private set; }
        public string Text { get; private set; }

        public bool IsUtf16 { get; private set; }

        public TextPayload()
        {
        }

        public TextPayload(string code, string text, bool isUtf16)
        {
            Language = code;
            Text = text;
            IsUtf16 = isUtf16;
        }

        public override byte[] GetTypeData()
        {
            return Encoding.ASCII.GetBytes("T");
        }

        public override byte[] GetId()
        {
            return null;
        }

        public override int FromBinaryData(byte[] data, int index)
        {
            int languageLength = (int)(data[0] & 0x1f);
            IsUtf16 = ((data[0] & 0x80) != 0);

            byte[] languageByte = new byte[languageLength];
            Array.Copy(data, 1, languageByte, 0, languageLength);

            Language = Encoding.ASCII.GetString(languageByte);

            int textLength = data.Length - 1 - languageLength;
            byte[] textByte = new byte[textLength];
            Array.Copy(data, 1 + languageLength, textByte, 0, textLength);

            if (!IsUtf16)
            {
                Text = Encoding.UTF8.GetString(textByte);
            }
            else
            {
                Text = Encoding.Unicode.GetString(textByte);
            }

            return 1 + languageLength + textLength;
        }

        public override byte[] ToBinaryData()
        {
            byte[] dataBin = null;

            if (!IsUtf16)
            {
                dataBin = Encoding.UTF8.GetBytes(Text);
            }
            else
            {
                dataBin = Encoding.Unicode.GetBytes(Text);
            }

            byte[] langByte = Encoding.ASCII.GetBytes(Language);

            byte[] ret = new byte[1 + langByte.Length + dataBin.Length];

            int index = 0;

            ret[index] = (byte)langByte.Length;
            if (IsUtf16) ret[index] |= 0x80;

            index++;

            Array.Copy(langByte, 0, ret, index, langByte.Length);
            index += langByte.Length;

            Array.Copy(dataBin, 0, ret, index, dataBin.Length);

            return ret;
        }
    }
}
