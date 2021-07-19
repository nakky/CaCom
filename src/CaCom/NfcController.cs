using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace CaCom
{
    public class NfcController
    {
        IntPtr context = IntPtr.Zero;

        IntPtr pciProcessT0 = IntPtr.Zero;
        IntPtr pciProcessT1 = IntPtr.Zero;
        IntPtr pciProcessRaw = IntPtr.Zero;

        public bool IsConnected { get; private set; }


        List<Reader> readers;
        SCARD_READERSTATE[] readerStates;

        public int IntervalMilliSec { get; set; }

        const int DefaultInterval = 100;

        object pcscApiLock = new object();

        #region Eventhandler


        public delegate void ConnectedHandler(bool success, List<Reader> readers);
        public ConnectedHandler OnConnected { get; set; }

        public delegate void DisconnectedHandler();
        public DisconnectedHandler OnDisconnected { get; set; }

        public delegate Disposition EnterCardHandler(Card card, Reader reader);
        public EnterCardHandler OnEnterCard { get; set; }

        public delegate void LeaveCardHandler(Card card, Reader reader);
        public LeaveCardHandler OnLeaveCard { get; set; }

        public delegate void APDUCommandHandler(ExecStatus status, Reader reader, ApduCommand command, byte[] receive);
        public APDUCommandHandler OnAPDUCommand { get; set; }

        public delegate void ReadNdefMessageHandler(bool success, Reader reader, NdefMessage message);
        public ReadNdefMessageHandler OnReadNdefMessage { get; set; }

        public delegate void WriteNdefMessageHandler(bool success, Reader reader, NdefMessage message);
        public WriteNdefMessageHandler OnWriteNdefMessage { get; set; }

        #endregion

        #region EntryPoint

        [DllImport("winscard.dll")]
        static extern uint SCardEstablishContext(uint dwScope, IntPtr pvReserved1, IntPtr pvReserved2, out IntPtr phContext);

        [DllImport("winscard.dll", EntryPoint = "SCardListReadersW", CharSet = CharSet.Unicode)]
        static extern uint SCardListReaders(
          IntPtr hContext,
          byte[] mszGroups,
          byte[] mszReaders,
          ref UInt32 pcchReaders
          );

        [DllImport("WinScard.dll")]
        static extern uint SCardReleaseContext(IntPtr phContext);

        [DllImport("winscard.dll", EntryPoint = "SCardConnectW", CharSet = CharSet.Unicode)]
        static extern uint SCardConnect(
             IntPtr hContext,
             string szReader,
             uint dwShareMode,
             uint dwPreferredProtocols,
             ref IntPtr phCard,
             ref IntPtr pdwActiveProtocol);

        [DllImport("WinScard.dll")]
        static extern uint SCardDisconnect(IntPtr hCard, int Disposition);

        [StructLayout(LayoutKind.Sequential)]
        internal class SCARD_IO_REQUEST
        {
            internal uint dwProtocol;
            internal int cbPciLength;
            public SCARD_IO_REQUEST()
            {
                dwProtocol = 0;
            }
        }

        [DllImport("winscard.dll")]
        static extern uint SCardTransmit(IntPtr hCard, IntPtr pioSendRequest, byte[] SendBuff, int SendBuffLen, SCARD_IO_REQUEST pioRecvRequest,
                byte[] RecvBuff, ref int RecvBuffLen);

        [DllImport("winscard.dll")]
        static extern uint SCardControl(IntPtr hCard, int controlCode, byte[] inBuffer, int inBufferLen, byte[] outBuffer, int outBufferLen, ref int bytesReturned);

        [DllImport("winscard.dll", SetLastError = true)]
        static extern Int32 SCardGetAttrib(
           IntPtr hCard,            // Reference value returned from SCardConnect
           UInt32 dwAttrId,         // Identifier for the attribute to get
           byte[] pbAttr,           // Pointer to a buffer that receives the attribute
           ref IntPtr pcbAttrLen    // Length of pbAttr in bytes
        );

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct SCARD_READERSTATE
        {
            /// <summary>
            /// Reader
            /// </summary>
            internal string szReader;
            /// <summary>
            /// User Data
            /// </summary>
            internal IntPtr pvUserData;
            /// <summary>
            /// Current State
            /// </summary>
            internal UInt32 dwCurrentState;
            /// <summary>
            /// Event State/ New State
            /// </summary>
            internal UInt32 dwEventState;
            /// <summary>
            /// ATR Length
            /// </summary>
            internal UInt32 cbAtr;
            /// <summary>
            /// Card ATR
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
            internal byte[] rgbAtr;
        }

        [DllImport("winscard.dll", EntryPoint = "SCardGetStatusChangeW", CharSet = CharSet.Unicode)]
        static extern uint SCardGetStatusChange(IntPtr hContext, int dwTimeout, [In, Out] SCARD_READERSTATE[] rgReaderStates, int cReaders);

        [DllImport("kernel32.dll", SetLastError = true)]
        extern static IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        extern static void FreeLibrary(IntPtr handle);

        [DllImport("kernel32.dll")]
        extern static IntPtr GetProcAddress(IntPtr handle, string procName);

        #endregion

        public NfcController()
        {
            IntPtr dllHandle = LoadLibrary("Winscard.dll");
            pciProcessT0 = GetProcAddress(dllHandle, "g_rgSCardT0Pci");
            pciProcessT1 = GetProcAddress(dllHandle, "g_rgSCardT1Pci");
            pciProcessRaw = GetProcAddress(dllHandle, "g_rgSCardRawPci");
            FreeLibrary(dllHandle);

            IntervalMilliSec = DefaultInterval;


        }

        #region ConnectDisconnect

        public async Task ConnectService(Scope scope = Scope.User)
        {
            if (Global.UseSyncContextPost && Global.SyncContext == null)
                Global.SyncContext = SynchronizationContext.Current;

            await Task.Run(() =>
            {
                ErrorCode ret;
                
                lock (pcscApiLock)
                {
                    ret = (ErrorCode)SCardEstablishContext((uint)scope, IntPtr.Zero, IntPtr.Zero, out context);
                }
               
                if (ret != ErrorCode.NoError)
                {
                    throw new NotSupportedException("No Service:" + ret);
                }
            });

            if (context != IntPtr.Zero)
            {
                readers = GetReaders();

                IsConnected = true;

                InitReaderState(readers);

                if (Global.SyncContext != null)
                {
                    Global.SyncContext.Post((state) => {
                        List<Reader> rs = (List<Reader>)state;
                        if (OnConnected != null) OnConnected(true, rs);
                    }, readers);
                }
                else
                {
                    if (OnConnected != null) OnConnected(true, readers);
                }

                while (IsConnected)
                {
                    try
                    {
                        Poll(IntervalMilliSec);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Poll Error:" + e.Message);
                    }


                    await Task.Delay(IntervalMilliSec);
                }

                if (Global.SyncContext != null)
                {
                    Global.SyncContext.Post((state) => {
                        if (OnDisconnected != null) OnDisconnected();
                    }, null);
                }
                else
                {
                    if (OnDisconnected != null) OnDisconnected();
                }


                context = IntPtr.Zero;
                readers = null;
                readerStates = null;


            }
            else
            {
                if (Global.SyncContext != null)
                {
                    Global.SyncContext.Post((state) => {
                        if (OnConnected != null) OnConnected(false, new List<Reader>());
                    }, null);
                }
                else
                {
                    if (OnConnected != null) OnConnected(false, new List<Reader>());
                }
            }
        }

        public async Task DisconnectService()
        {
            if (context != IntPtr.Zero)
            {
                await Task.Run(() =>
                {
                    lock (pcscApiLock)
                    {
                        SCardReleaseContext(context);
                    }
                });
            }

            IsConnected = false;
        }

        #endregion

        #region BasicFunctions

        List<Reader> GetReaders()
        {
            uint pcchReaders = 0;

            List<Reader> readers = new List<Reader>();

            ErrorCode ret;
            lock (pcscApiLock)
            {
                ret = (ErrorCode)SCardListReaders(context, null, null, ref pcchReaders);
            }

            if (ret != ErrorCode.NoError)
            {
                throw new NFCException("Can not get readcers info:" + ret);
            }

            byte[] mszReaders = new byte[pcchReaders * 2];

            lock (pcscApiLock)
            {
                ret = (ErrorCode)SCardListReaders(context, null, mszReaders, ref pcchReaders);
            }

            if (ret != ErrorCode.NoError)
            {
                throw new NFCException("Can not get readcers info:" + ret);
            }

            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            string readerNameMultiString = unicodeEncoding.GetString(mszReaders);

            int len = (int)pcchReaders;
            char nullchar = (char)0;


            if (len > 0)
            {
                while (readerNameMultiString[0] != nullchar)
                {
                    int nullindex = readerNameMultiString.IndexOf(nullchar);   // Get null end character.
                    string readerName = readerNameMultiString.Substring(0, nullindex);
                    readers.Add(new Reader(readerName));
                    len = len - (readerName.Length + 1);
                    readerNameMultiString = readerNameMultiString.Substring(nullindex + 1, len);
                }
            }
            return readers;
        }

        void InitReaderState(List<Reader> readers)
        {
            readerStates = new SCARD_READERSTATE[readers.Count];

            for (int i = 0 ; i < readers.Count; i++) {
                readerStates[i].dwCurrentState = (uint)CardState.Unaware;
                readerStates[i].szReader = readers[i].Name;
            }

            ErrorCode ret;
            lock (pcscApiLock)
            {
                ret = (ErrorCode)SCardGetStatusChange(context, 100, readerStates, readerStates.Length);
            }

            if (ret != ErrorCode.NoError)
            {
                throw new NFCException("Can not initialize card state;" + ret);
            }

            byte[] pbAttr = new byte[255];
            IntPtr pcbAttrLen = new IntPtr(pbAttr.Length);

            ASCIIEncoding asciiEncoding = new ASCIIEncoding();

            for (int ri = 0; ri < readerStates.Length; ri++)
            {
                IntPtr hCard = ConnectCard(readers[ri], ShareParam.Direct, Protocol.Undefined);

                lock (pcscApiLock)
                {
                    ret = (ErrorCode)SCardGetAttrib(hCard, (uint)Attribute.VendorIfdSerialNo, pbAttr, ref pcbAttrLen);
                }

                if (ret != ErrorCode.NoError)
                {
                    throw new NFCException("Can not get card serial;" + ret);
                }

                string serialNumber = asciiEncoding.GetString(pbAttr, 0, (int)pcbAttrLen - 1);
                readers[ri].SerialNumber = serialNumber;

                DisconnectCard(hCard, Disposition.Leave);
            }
        }

        void Poll(int timeoutMilliSec)
        {
            ErrorCode ret;
            lock (pcscApiLock)
            {
                ret = (ErrorCode)SCardGetStatusChange(context, timeoutMilliSec, readerStates, readerStates.Length);
            }

            switch (ret)
            {
                case ErrorCode.NoError:
                    break;
                case ErrorCode.Timeout:
                    throw new TimeoutException();
                default:
                    throw new NFCException("Can not get card state");
            }

            for (int ri = 0; ri < readerStates.Length; ri++)
            {
                uint eventState = readerStates[ri].dwEventState;
                if ((eventState & (uint)CardEvent.StateChanged) != 0)
                {

                    uint changedStateMask = (uint)eventState ^ readerStates[ri].dwCurrentState;
                    if ((changedStateMask & ((uint)CardState.Empty | (uint)CardState.Present)) != 0)
                    {

                        if ((eventState & (uint)CardState.Present) != 0)
                        {
                            if (readers[ri].State != CardState.Present)
                            {
                                IntPtr hCard;
                                lock (readers[ri].ApduLock)
                                {
                                    hCard = ConnectCard(readers[ri], readers[ri].ShareParam, readers[ri].Protocol);
                                    readers[ri].CurrentCard = GetCardInfo(hCard, readers[ri]);

                                    Disposition disposition = Disposition.Leave;

                                    if (Global.SyncContext != null)
                                    {
                                        Global.SyncContext.Post((state) => {
                                            Reader r = (Reader)state;
                                            if (OnEnterCard != null) OnEnterCard(r.CurrentCard, r);
                                        }, readers[ri]);
                                    }
                                    else
                                    {
                                        if (OnEnterCard != null) OnEnterCard(readers[ri].CurrentCard, readers[ri]);
                                    }

                                    DisconnectCard(hCard, disposition);

                                    readers[ri].State = CardState.Present;

                                }
                            }



                        }
                        if ((eventState & (uint)CardState.Empty) != 0 && readers[ri].CurrentCard != null)
                        {
                            if (readers[ri].State != CardState.Empty)
                            {
                                if (Global.SyncContext != null)
                                {
                                    Global.SyncContext.Post((state) => {
                                        Reader r = (Reader)state;
                                        if (OnLeaveCard != null) OnLeaveCard(r.CurrentCard, r);
                                        r.CurrentCard = null;
                                    }, readers[ri]);
                                }
                                else
                                {
                                    if (OnLeaveCard != null) OnLeaveCard(readers[ri].CurrentCard, readers[ri]);
                                }

                                readers[ri].State = CardState.Empty;
                            }
                        }
                    }


                }
            }

        }


        Card GetCardInfo(IntPtr hCard, Reader reader)
        {

            byte sw1, sw2;

            Card card = new Card();

            byte[] sendBuffer;
            byte[] recvBuffer;

            byte maxRecvDataLen;

            //Get Id
            maxRecvDataLen = 64;

            sendBuffer = new byte[] { 0xff, 0xca, 0x00, 0x00, 0 };
            recvBuffer = new byte[maxRecvDataLen + 2];

            int recvLength = TransmitToCard(hCard, sendBuffer, recvBuffer, reader.Protocol);

            sw1 = recvBuffer[recvLength - 2];
            sw2 = recvBuffer[recvLength - 1];

            if (sw1 != 0x90 || sw2 != 0x00)
            {
                throw new APDUException(sw1, sw2);
            }

            string cardId = BitConverter.ToString(recvBuffer, 0, recvLength - 2).Replace("-", "");

            card.Id = cardId;

            //Get Card Type
            maxRecvDataLen = 64;

            sendBuffer = new byte[] { 0xff, 0xca, 0xf3, 0x00, 0 };
            recvBuffer = new byte[maxRecvDataLen + 2];

            recvLength = TransmitToCard(hCard, sendBuffer, recvBuffer, reader.Protocol);

            sw1 = recvBuffer[recvLength - 2];
            sw2 = recvBuffer[recvLength - 1];

            if (sw1 != 0x90 || sw2 != 0x00)
            {
                throw new APDUException(sw1, sw2);
            }

            CardType type = (CardType)recvBuffer[0];

            card.Type = type;

            return card;
        }

        internal IntPtr ConnectCard(Reader reader, ShareParam shared, Protocol protocol)
        {
            IntPtr hCard = IntPtr.Zero;

            IntPtr activeProtocol = IntPtr.Zero;

            ErrorCode ret;
            lock (pcscApiLock)
            {
                ret = (ErrorCode)SCardConnect(context, reader.Name, (uint)shared, (uint)protocol, ref hCard, ref activeProtocol);
            }

            if (ret != ErrorCode.NoError)
            {
                throw new NFCException("Can not connect card:" + ret);
            }

            return hCard;
        }

        internal void DisconnectCard(IntPtr hCard, Disposition reaction)
        {
            ErrorCode ret;
            lock (pcscApiLock)
            {
                ret = (ErrorCode)SCardDisconnect(hCard, (int)reaction);
            }

            if (ret != ErrorCode.NoError)
            {
                throw new NFCException("Can not disconnect card:" + ret);
            }
        }


        internal int TransmitToCard(IntPtr hCard, byte[] sendBuffer, byte[] recvBuffer, Protocol protocol)
        {
            SCARD_IO_REQUEST ioRecv = new SCARD_IO_REQUEST();
            ioRecv.cbPciLength = 255;

            int pcbRecvLength = recvBuffer.Length;
            int cbSendLength = sendBuffer.Length;

            IntPtr process = pciProcessT0;
            if (protocol == Protocol.T1) process = pciProcessT1;
            else if (protocol == Protocol.Raw) process = pciProcessRaw;

            ErrorCode ret;
            lock (pcscApiLock)
            {
                ret = (ErrorCode)SCardTransmit(hCard, process, sendBuffer, cbSendLength, ioRecv, recvBuffer, ref pcbRecvLength);
            }
            if (ret != ErrorCode.NoError)
            {
                throw new NFCException("Can not transmit to card:" + ret);
            }

            return pcbRecvLength;
        }

        #endregion

        #region ReadWrite
         
        public async Task SendAPDUCommand(Reader reader, ApduCommand command, APDUCommandHandler handler)
        {
            await Task.Run(() =>
            {
                byte sw1 = 0;
                byte sw2 = 0;

                if (reader.CurrentCard == null || !IsConnected)
                {
                    if (Global.SyncContext != null)
                    {
                        Global.SyncContext.Post((state) =>
                        {
                            ValueTuple<Reader, ApduCommand, byte[]> t = (ValueTuple<Reader, ApduCommand, byte[]>)state;
                            if (handler != null) handler(ExecStatus.Error, t.Item1, t.Item2, t.Item3);
                        }, new ValueTuple<Reader, ApduCommand, byte[]>(reader, command, null));
                    }
                    else
                    {
                        if (OnAPDUCommand != null) handler(ExecStatus.Error, reader, command, null);
                    }
                }
                else
                {
                    try
                    {

                        byte maxRecvDataLen;

                        maxRecvDataLen = 64;

                        byte[] recvBuffer;

                        IntPtr hCard;
                        int recvLength;
                        byte[] data;

                        lock (reader.ApduLock)
                        {
                            hCard = ConnectCard(reader, reader.ShareParam, reader.Protocol);
                            recvBuffer = new byte[maxRecvDataLen + 2];
                            recvLength = TransmitToCard(hCard, command.Data, recvBuffer, reader.Protocol);
                        
                            sw1 = recvBuffer[recvLength - 2];
                            sw2 = recvBuffer[recvLength - 1];

                            command.Sw1 = sw1;
                            command.Sw2 = sw2;

                            if (sw1 != 0x90 && sw1 != 0x62 && sw1 != 0x63)
                            {
                                throw new APDUException(sw1, sw2);
                            }

                            data = new byte[recvLength - 2];
                            Array.Copy(recvBuffer, data, recvLength - 2);

                            DisconnectCard(hCard, Disposition.Leave);

                        }

                        if (Global.SyncContext != null)
                        {
                            Global.SyncContext.Post((state) =>
                            {
                                ValueTuple<Reader, ApduCommand, byte[]> t = (ValueTuple<Reader, ApduCommand, byte[]>)state;
                                if (handler != null)
                                {
                                    if (command.Sw1 == 0x90) handler(ExecStatus.Success, t.Item1, t.Item2, t.Item3);
                                    else handler(ExecStatus.Warning, t.Item1, t.Item2, t.Item3);
                                }
                            }, new ValueTuple<Reader, ApduCommand, byte[]>(reader, command, data));
                        }
                        else
                        {
                            if (handler != null)
                            {
                                if(command.Sw1 == 0x90) handler(ExecStatus.Success, reader, command, data);
                                else handler(ExecStatus.Warning, reader, command, data);
                            }
                        }

                    }
                    catch(Exception e)
                    {
                        command.Exception = e;

                        Console.WriteLine("Can not send Apdu command:" + e.Message);

                        if (Global.SyncContext != null)
                        {
                            Global.SyncContext.Post((state) =>
                            {
                                ValueTuple<Reader, ApduCommand, byte[]> t = (ValueTuple<Reader, ApduCommand, byte[]>)state;
                                if (handler != null) handler(ExecStatus.Error, t.Item1, t.Item2, t.Item3);
                            }, new ValueTuple<Reader, ApduCommand, byte[]>(reader, command, null));
                        }
                        else
                        {
                            if (handler != null) handler(ExecStatus.Error, reader, command, null);
                        }
                    }
                }

            });
        }

        public async Task SendAPDUCommand(Reader reader, ApduCommand command)
        {
            try
            {
                await SendAPDUCommand(reader, command, OnAPDUCommand);
            }
            catch(Exception e)
            {
                Console.WriteLine("Can not send Apdu command:" + e.Message);
            }
            
        }

        public async Task WriteNdefMessage(Reader reader, NdefMessage message, byte userMemoryPage = 4, byte writeUnitSize = 4)
        {
            byte[] binary = message.ToBinaryData();
            int length = binary.Length;

            ApduCommand command;
            int currentIndex = 0;
            int numWrite = 0;

            while (length > 0)
            {
                byte[] com = new byte[6 + writeUnitSize];

                com[0] = 0xff;
                com[1] = 0xd6;
                com[2] = 0x00;

                com[3] = (byte)(userMemoryPage + numWrite);
                com[4] = writeUnitSize;

                int l = writeUnitSize;

                if (length < l) l = (byte)length;
                Array.Copy(binary, currentIndex, com, 5, l);

                command = new ApduCommand(com);

                ExecStatus success = ExecStatus.Error;

                await SendAPDUCommand(reader, command, (s, r, c, rec) =>
                {
                    success = s;
                });

                if (success == ExecStatus.Error)
                {
                    if (Global.SyncContext != null)
                    {
                        Global.SyncContext.Post((state) =>
                        {
                            ValueTuple<Reader, NdefMessage> t = (ValueTuple<Reader, NdefMessage>)state;
                            if (OnWriteNdefMessage != null) OnWriteNdefMessage(false, t.Item1, t.Item2);
                        }, new ValueTuple<Reader, NdefMessage>(reader, message));
                    }
                    else
                    {
                        if (OnWriteNdefMessage != null) OnWriteNdefMessage(false, reader, message);
                    }
                    return;
                }


                length -= writeUnitSize;
                currentIndex += writeUnitSize;
                numWrite++;
            }

            if (Global.SyncContext != null)
            {
                Global.SyncContext.Post((state) =>
                {
                    ValueTuple<Reader, NdefMessage> t = (ValueTuple<Reader, NdefMessage>)state;
                    if (OnWriteNdefMessage != null) OnWriteNdefMessage(true, t.Item1, t.Item2);
                }, new ValueTuple<Reader, NdefMessage>(reader, message));
            }
            else
            {
                if (OnWriteNdefMessage != null) OnWriteNdefMessage(true, reader, message);
            }
        }

        public async Task ReadNdefMessage(Reader reader, NdefMessage message, byte userMemoryPage = 4)
        {
            byte[] com = { 0xff, 0xb0, 0x00, userMemoryPage, 0x00 };

            ApduCommand command = new ApduCommand(com);

            List<byte[]> receivedByte = new List<byte[]>();
            ExecStatus success = ExecStatus.Error;
            byte length = 0;
            int index = 0;

            await SendAPDUCommand(reader, command, (s, r, c, rec) =>
            {
                if (s == ExecStatus.Success) receivedByte.Add(rec);
                success = s;
            });

            if (success != ExecStatus.Success || receivedByte[index][0] != 0x03)
            {

                if (Global.SyncContext != null)
                {
                    Global.SyncContext.Post((state) =>
                    {
                        ValueTuple<Reader, NdefMessage> t = (ValueTuple<Reader, NdefMessage>)state;
                        if (OnReadNdefMessage != null) OnReadNdefMessage(false, t.Item1, t.Item2);
                    }, new ValueTuple<Reader, NdefMessage>(reader, message));
                }
                else
                {
                    if (OnReadNdefMessage != null) OnReadNdefMessage(false, reader, message);
                }
                return;
            }

            length = (byte)(receivedByte[index][1] + 2);

            int currentLength = length - 16;
            int numRead = 1;

            while (currentLength > 0)
            {
                com[3] += 0x04;
                command = new ApduCommand(com);

                success = ExecStatus.Error;

                await SendAPDUCommand(reader, command, (s, r, c, rec) =>
                {
                    if (s == ExecStatus.Success) receivedByte.Add(rec);
                    success = s;
                    numRead++;
                });

                if (success != ExecStatus.Success)
                {
                    if (Global.SyncContext != null)
                    {
                        Global.SyncContext.Post((state) =>
                        {
                            ValueTuple<Reader, NdefMessage> t = (ValueTuple<Reader, NdefMessage>)state;
                            if (OnReadNdefMessage != null) OnReadNdefMessage(false, t.Item1, t.Item2);
                        }, new ValueTuple<Reader, NdefMessage>(reader, message));
                    }
                    else
                    {
                        if (OnReadNdefMessage != null) OnReadNdefMessage(false, reader, message);
                    }
                    return;
                }

                currentLength -= 16;
            }

            byte[] binary = new byte[numRead * 16];

            for (int i = 0; i < receivedByte.Count; i++)
            {
                Array.Copy(receivedByte[i], 0, binary, 16 * i, 16);
            }

            message.FromBinaryData(binary);

            if (Global.SyncContext != null)
            {
                Global.SyncContext.Post((state) =>
                {
                    ValueTuple<Reader, NdefMessage> t = (ValueTuple<Reader, NdefMessage>)state;
                    if (OnReadNdefMessage != null) OnReadNdefMessage(true, t.Item1, t.Item2);
                }, new ValueTuple<Reader, NdefMessage>(reader, message));
            }
            else
            {
                if (OnReadNdefMessage != null) OnReadNdefMessage(true, reader, message);
            }


        }

         
        #endregion
    
    }

}
