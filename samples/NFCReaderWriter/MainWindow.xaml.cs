﻿using System;
using System.Collections.Generic;

using System.Windows;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using CaCom;

namespace NFCReaderWriter
{
    public enum Tab : int
    {
        Text = 0,
        Uri = 1
    };

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        NfcController controller = new NfcController();

        List<Reader> readerList;

        Config config;

        const string configFileName = "./config.data";

        public MainWindow()
        {
            InitializeComponent();

            // Global.UseSyncContextPost = true;

            DeserializeConfig();

            controller.OnConnected += (success, readers) =>
            {
                if (success) AddLog("Success Connected.");
                else AddLog("Success Failured.");

                foreach (var reader in readers)
                {
                    AddLog(reader.Name);
                }
                readerList = readers;

                AddLog("-------");
            };

            controller.OnDisconnected += () =>
            {
                readerList = null;

                AddLog("Disconnected.");
                AddLog("-------");
            };

            Reader targetReader = null;

            controller.OnEnterCard += (card, r) =>
            {
                AddLog("Enter:" + card.Id + ":" + card.Type + " on " + r.Name + " : " + r.SerialNumber) ;

                if(targetReader == null)
                {
                    labelIDm.Content = card.Id;
                    labelType.Content = card.Type;

                    buttonRead.IsEnabled = true;
                    buttonWrite.IsEnabled = true;
                    buttonSend.IsEnabled = true;

                    targetReader = r;
                }
                AddLog("-------");

                return Disposition.Leave;
            };

            controller.OnLeaveCard += (card, r) =>
            {
                if (card != null) AddLog("Leave:" + card.Id + " on " + r.Name + " : " + r.SerialNumber);

                if (targetReader == r)
                {
                    labelIDm.Content = "";
                    labelType.Content = "";

                    buttonRead.IsEnabled = false;
                    buttonWrite.IsEnabled = false;
                    buttonSend.IsEnabled = false;

                    targetReader = null;
                }
                AddLog("-------");
            };

            controller.OnAPDUCommand += (success, reader, command, data) =>
            {
                if (success != ExecStatus.Error)
                {
                    AddLog("Success APDU Command:");
                    string datastring = BitConverter.ToString(data);
                    AddLog("Data:" + datastring);

                }
                else
                {
                    AddLog("Failure APDU Command:(" + command.Sw1 + "," + command.Sw2 + ")");
                    string reason = "";
                    if (command.Exception != null) reason = command.Exception.Message;
                    AddLog("Reason:" + reason);
                }
                AddLog("-------");
            };


            controller.OnWriteNdefMessage += (success, reader, message) =>
            {
                if (success)
                {
                    AddLog("Success WriteNdefMessage:");
                }
                else
                {
                    AddLog("Failure WriteNdefMessage:");
                }
                AddLog("-------");
            };

            controller.OnReadNdefMessage += (success, reader, message) =>
            {
                if (success)
                {
                    if (message.Records.Count == 0)
                    {
                        AddLog("Record is invalid.");
                        AddLog("-------");
                        return;
                    }

                    if (message.Records[0].TypeNameFormat != TypeNameFormat.NfcForumWellKnownType)
                    {
                        AddLog("Record is not support format.");
                        AddLog("-------");
                        return;
                    }

                    var payload = message.Records[0].Payload;

                    if (payload.GetType() == typeof(TextPayload))
                    {
                        tabNdefData.SelectedIndex = (int)Tab.Text;
                        TextPayload tp = (TextPayload)payload;
                        textTextData.Text = tp.Text;
                    }
                    else if (payload.GetType() == typeof(UriPayload))
                    {
                        tabNdefData.SelectedIndex = (int)Tab.Uri;
                        UriPayload up = (UriPayload)payload;

                        comboUriData.SelectedIndex = (int)up.Prefix;
                        textUriData.Text = up.Data;

                    }
                    else
                    {
                        AddLog("Payload is not support format.");
                        AddLog("-------");
                        return;
                    }

                    AddLog("Success to Read Ndef Data");
                }
                else
                {
                    AddLog("Failure ReadNdefMessage:");
                }
                AddLog("-------");
            };

            controller.ConnectService();

            foreach (UriPrefix value in Enum.GetValues(typeof(UriPrefix)))
            {
                string name = Enum.GetName(typeof(UriPrefix), value);
                comboUriData.Items.Add(name);
            }

            tabNdefData.SelectedIndex = (int)Tab.Text;

        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            controller.DisconnectService();
        }

        private void SerializeConfig()
        {
            try
            {
                int userMemory = int.Parse(textUserMemory.Text);
                int updateSize = int.Parse(textUpdateSize.Text);

                if (userMemory < 0) throw new ArgumentException();
                if (updateSize <= 0) throw new ArgumentException();

                config.UserMemoryPage = userMemory;
                config.UpdateMemorySize = updateSize;

                using (FileStream fs = new FileStream(configFileName, FileMode.OpenOrCreate))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(fs, config);
                    fs.Close();
                }
            }
            catch
            {
                textUserMemory.Text = config.UserMemoryPage.ToString();
                textUpdateSize.Text = config.UpdateMemorySize.ToString();

                Console.WriteLine("Can not serialize config data.");
            }

        }

        private void DeserializeConfig()
        {
            try
            {
                using (FileStream fs = new FileStream(configFileName, FileMode.Open))
                {
                    BinaryFormatter f = new BinaryFormatter();
                    config = (Config)f.Deserialize(fs);
                    fs.Close();
                }

                textUserMemory.Text = config.UserMemoryPage.ToString();
                textUpdateSize.Text = config.UpdateMemorySize.ToString();
            }
            catch
            {
                config = new Config();
                SerializeConfig();
            }

        }

        void AddLog(string text)
        {
            textLog.Text += text;
            textLog.Text += "\n";
            textLog.ScrollToEnd();
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            textLog.Text = "";
        }

        private void OnWriteClick(object sender, RoutedEventArgs e)
        {
            SerializeConfig();

            NdefMessage message = new NdefMessage();

            Payload payload;

            if (tabNdefData.SelectedIndex == (int)Tab.Text)
            {
                payload = new TextPayload("en", textTextData.Text, false);
            }
            else
            {
                payload = new UriPayload((UriPrefix)comboUriData.SelectedIndex, textUriData.Text);
            }

            message.AddRecord(new NdefRecord(payload));


            controller.WriteNdefMessage(readerList[0], message, (byte)config.UserMemoryPage, (byte)config.UpdateMemorySize);

        }

        private void OnReadClick(object sender, RoutedEventArgs e)
        {

            NdefMessage message = new NdefMessage();
            controller.ReadNdefMessage(readerList[0], message, (byte)config.UserMemoryPage);
        }

        private void OnSendClick(object sender, RoutedEventArgs e)
        {
            string sendText = textAPDU.Text;
            string[] sendArray = sendText.Split('-');

            if (sendArray.Length < 4)
            {
                AddLog("Invalid Command");
                AddLog("-------");
                return;
            }

            byte[] data = new byte[sendArray.Length];

            for (int i = 0 ; i < sendArray.Length ; i++)
            {
                data[i] = Convert.ToByte(sendArray[i], 16);
            }

            string datastring = BitConverter.ToString(data);
            AddLog("Send APDU Command:" + datastring);

            ApduCommand command = new ApduCommand(data);

            controller.SendAPDUCommand(readerList[0], command);

            if ((bool)checkResetAPDUC.IsChecked) textAPDU.Text = "";
        }

    }
}
