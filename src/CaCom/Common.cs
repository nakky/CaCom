using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace CaCom
{
    public enum ShareParam : uint
    {
        Exclusive = 0x00000001,
        Shared = 0x00000002,
        Direct = 0x00000003
    };

    public enum Protocol : uint
    {
        Undefined = 0x00000000,
        T0 = 0x00000001,
        T1 = 0x00000002,
        Raw = 0x00000004
    };

    public enum Disposition : int
    {
        Leave = 0x00000000,
        Reset = 0x00000001,
        Unpower = 0x00000002,
        Eject = 0x00000003
    };

    public enum CardState : uint
    {
        Unaware = 0x00000000,
        Empty = 0x00000010,
        Present = 0x00000020,
    };

    public enum CardEvent : uint
    {
        StateChanged = 0x00000002,
    };

    public enum Scope : uint
    {
        User = 0x00000000,
        Terminal = 0x00000001,
        System = 0x00000002
    };

    public enum ErrorCode : uint
    {
        NoError = 0x00000000,
        NoService = 0x8010001D,
        Timeout = 0x8010000A
    };

    public enum ExecStatus : uint
    {
        Success = 0,
        Warning,
        Error,
    };

    public enum CardType : byte
    {
        Unknown = 0x00,
        Iso14443a = 0x01,
        Iso14443b = 0x02,
        PicoPassB = 0x03,
        Felica = 0x04,
        NfcType1 = 0x05,
        MifareEc = 0x06,
        Iso14443a4A = 0x07,
        Iso14443b4B = 0x08,
        TypeANefDep = 0x09,
        FelicaNefDep = 0x0A,
    };

    public enum Attribute : uint
    {
        VendorIfdSerialNo = 0x00010103,
    };

    public enum TlvTag : byte
    {
        Null = 0x00,
        LockControl = 0x01,
        MemoryControl = 0x02,
        NdefMessage = 0x03,
        Proprietary = 0xfd,
        Terminator = 0xfe,
    };

    public enum TypeNameFormat : byte
    {
        Empty = 0x00,
        NfcForumWellKnownType = 0x01,
        MediaTypeAsDefineInRfc2046 = 0x02,
        AbsoluteUriAsDefineInRfc3986 = 0x03,
        NfcForumExternalType = 0x04,
        Unknown = 0x05,
        Unchanged = 0x06,
        Recerved = 0x07
    };

    public enum UriPrefix : byte
    {
        Null = 0x00,
        HttpWww = 0x01,
        HttpsWww = 0x02,
        Http = 0x03,
        Https = 0x04,
        Tel = 0x05,
        MailTo = 0x06,
        FtpAnonymous = 0x07,
        FtpFtp = 0x08,
        Ftps = 0x09,
        Sftp = 0x0a,
        Smb = 0x0b,
        Nfs = 0x0c,
        Ftp = 0x0d,
        Dav = 0x0e,
        News = 0x0f,
        Telnet = 0x10,
        Imap = 0x11,
        Rtsp = 0x12,
        Urn = 0x13,
        Pop = 0x14,
        Sip = 0x15,
        Sips = 0x16,
        Tftp = 0x17,
        Btspp = 0x18,
        Btl2cap = 0x19,
        Btgoep = 0x1a,
        Tcpobex = 0x1b,
        Irdaobex = 0x1c,
        File = 0x1d,
        UrnEpcId = 0x1e,
        UrnEpcTag = 0x1f,
        UrnEpcPat = 0x20,
        UrnEpcRaw = 0x21,
        UrnEpc = 0x22,
        UrnNfc = 0x23,
    };

    



    public static class Global
    {
        public static SynchronizationContext SyncContext { get; set; }
        public static bool UseSyncContextPost = true;

        public static string UriPrefixString(UriPrefix prefix)
        {
            switch(prefix)
            {
                case UriPrefix.Null:
                    return "";
                case UriPrefix.HttpWww:
                    return "http://www.";
                case UriPrefix.HttpsWww:
                    return "https://www.";
                case UriPrefix.Http:
                    return "http://";
                case UriPrefix.Https:
                    return "https://";
                case UriPrefix.Tel:
                    return "tel:";
                case UriPrefix.MailTo:
                    return "mailto:";
                case UriPrefix.FtpAnonymous:
                    return "ftp://anonymous:anonymous@";
                case UriPrefix.FtpFtp:
                    return "ftp://ftp.";
                case UriPrefix.Ftps:
                    return "ftps://";
                case UriPrefix.Sftp:
                    return "sftp://";
                case UriPrefix.Smb:
                    return "smb://";
                case UriPrefix.Nfs:
                    return "nfs://";
                case UriPrefix.Ftp:
                    return "ftp://";
                case UriPrefix.Dav:
                    return "dav://";
                case UriPrefix.News:
                    return "news:";
                case UriPrefix.Telnet:
                    return "telnet://";
                case UriPrefix.Imap:
                    return "imap:";
                case UriPrefix.Rtsp:
                    return "rtsp://";
                case UriPrefix.Urn:
                    return "urn:";
                case UriPrefix.Pop:
                    return "pop:";
                case UriPrefix.Sip:
                    return "sip:";
                case UriPrefix.Sips:
                    return "sips:";
                case UriPrefix.Tftp:
                    return "tftp:";
                case UriPrefix.Btspp:
                    return "btspp://";
                case UriPrefix.Btl2cap:
                    return "btl2cap://";
                case UriPrefix.Btgoep:
                    return "btgoep://";
                case UriPrefix.Tcpobex:
                    return "tcpobex://";
                case UriPrefix.Irdaobex:
                    return "irdaobex://";
                case UriPrefix.File:
                    return "file://";
                case UriPrefix.UrnEpcId:
                    return "urn:epc:id:";
                case UriPrefix.UrnEpcTag:
                    return "urn:epc:tag:";
                case UriPrefix.UrnEpcPat:
                    return "urn:epc:pat:";
                case UriPrefix.UrnEpcRaw:
                    return "urn:epc:raw:";
                case UriPrefix.UrnEpc:
                    return "urn:epc:";
                case UriPrefix.UrnNfc:
                    return "urn:nfc:";
                default:
                    return "";
            }
        }

    }
}
