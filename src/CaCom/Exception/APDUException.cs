using System;
using System.Runtime.Serialization;

namespace CaCom
{
    [Serializable()]
    class APDUException : Exception
    {
        public Byte Sw1 { get; private set; }
        public Byte Sw2 { get; private set; }

        public APDUException(byte sw1, byte sw2)
            :base()
        {
            Sw1 = sw1; Sw2 = sw2;
        }

        public override string Message
        {
            get
            {
                return base.Message + ErrorDescription;
            }
        }

        public string ErrorDescription
        {
            get
            {
                switch (Sw1)
                {
                    case 0x90:
                        {
                            switch(Sw2)
                            {
                                case 0x00:
                                    {
                                        return "No error.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x62:
                        {
                            switch (Sw2)
                            {
                                case 0x81:
                                    {
                                        return "Part of returned data may be corrupted.";
                                    }
                                case 0x82:
                                    {
                                        return "End of file/record reached before reading Le bytes.";
                                    }
                                case 0x83:
                                    {
                                        return "Warning selected file deactivated.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")" ;
                                    }
                            }
                        }
                    case 0x63:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "No information given (NV-Ram changed).";
                                    }
                                case 0x81:
                                    {
                                        return "File filled up by the last write. Loading/updating is not allowed.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x64:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "No information given (NV-Ram not changed).";
                                    }
                                case 0x01:
                                    {
                                        return "Command timeout. Immediate response required by the card.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x65:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "No information given.";
                                    }
                                case 0x01:
                                    {
                                        return "Write error. Memory failure. There have been problems in writing or reading the EEPROM. Other hardware problems may also bring this error.";
                                    }
                                case 0x81:
                                    {
                                        return "Memory failure.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x66:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "Error while receiving (timeout).";
                                    }
                                case 0x01:
                                    {
                                        return "Error while receiving (character parity error).";
                                    }
                                case 0x02:
                                    {
                                        return "Wrong checksum.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x67:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "Wrong length.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x68:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "No information given (The request function is not supported by the card).";
                                    }
                                case 0x81:
                                    {
                                        return "Logical channel not supported.";
                                    }
                                case 0x82:
                                    {
                                        return "Secure messaging not supported.";
                                    }
                                case 0x83:
                                    {
                                        return "Last command of the chain expected.";
                                    }
                                case 0x84:
                                    {
                                        return "Command chaining not supported.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x69:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "No information given (Command not allowed).";
                                    }
                                case 0x01:
                                    {
                                        return "Command not accepted (inactive state).";
                                    }
                                case 0x81:
                                    {
                                        return "Command incompatible with file structure.";
                                    }
                                case 0x82:
                                    {
                                        return "Security condition not satisfied.";
                                    }
                                case 0x83:
                                    {
                                        return "Authentication method blocked.";
                                    }
                                case 0x84:
                                    {
                                        return "Referenced data reversibly blocked (invalidated).";
                                    }
                                case 0x85:
                                    {
                                        return "Conditions of use not satisfied.";
                                    }
                                case 0x86:
                                    {
                                        return "Command not allowed (no current EF).";
                                    }
                                case 0x87:
                                    {
                                        return "Expected secure messaging (SM) object missing.";
                                    }
                                case 0x88:
                                    {
                                        return "Incorrect secure messaging (SM) data object.";
                                    }
                                case 0x96:
                                    {
                                        return "Data must be updated again.";
                                    }
                                case 0xe1:
                                    {
                                        return "POL1 of the currently Enabled Profile prevents this action.";
                                    }
                                case 0xf0:
                                    {
                                        return "Permission Denied.";
                                    }
                                case 0xf1:
                                    {
                                        return "Permission Denied – Missing Privilege.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x6a:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "No information given (Bytes P1 and/or P2 are incorrect).";
                                    }
                                case 0x80:
                                    {
                                        return "The parameters in the data field are incorrect.";
                                    }
                                case 0x81:
                                    {
                                        return "Function not supported.";
                                    }
                                case 0x82:
                                    {
                                        return "File not found.";
                                    }
                                case 0x83:
                                    {
                                        return "Record not found.";
                                    }
                                case 0x84:
                                    {
                                        return "There is insufficient memory space in record or file.";
                                    }
                                case 0x85:
                                    {
                                        return "Lc inconsistent with TLV structure.";
                                    }
                                case 0x86:
                                    {
                                        return "Incorrect P1 or P2 parameter.";
                                    }
                                case 0x87:
                                    {
                                        return "Lc inconsistent with P1-P2.";
                                    }
                                case 0x88:
                                    {
                                        return "Referenced data not found.";
                                    }
                                case 0x89:
                                    {
                                        return "File already exists.";
                                    }
                                case 0x8a:
                                    {
                                        return "DF name already exists.";
                                    }
                                case 0xf0:
                                    {
                                        return "Wrong parameter value.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x6b:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "Wrong parameter(s) P1-P2.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x6c:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "Incorrect P3 length.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x6d:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "Instruction code not supported or invalid.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x6e:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "Class not supported.";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    case 0x6f:
                        {
                            switch (Sw2)
                            {
                                case 0x00:
                                    {
                                        return "Command aborted – more exact diagnosis not possible (e.g., operating system error).";
                                    }
                                default:
                                    {
                                        return "Undefined Error.(sw1=" + Sw1 + ",sw2=" + Sw2 + ")";
                                    }
                            }
                        }
                    default:
                        {
                            return "Undefined Error.";
                        }

                }
            }
        }
    }
}
