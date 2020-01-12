# CaCom
NFC Module to Control NFC Reader/Writer.

## Overview

CaCom is simple NFC device controller to read/write information from/to NFC card, tag and so on.   
In this module, APDU (Application Protocol Data Unit) commands are mainly used in row level. NFC has complecated specificatios and includes many types (Mifare, Felica...), so you can controll many devices if you use APDU commands but it is complecated. In CaCom, you can also use functions to read/write NDEF (NFC Data Exchange Format) message.

## Target Reader/Writer Devices

### Windows
Target is devices which conform to Windows PC/SC.
 - Ex. Sony RC-S380

### Mac, Linux
Not supported.

## Code Flow

A NFC Device is controlled by a service of operation system directly, so applications in user land need to communicate with the service.

Code flow is as follows.

 0. Register callbacks.
 1. Connect to the service.
 2. Transfer data.
 3. Disconnect the service.

CaCom has callback architecture, so you need to register callback functions before transferring data. 

In step 2, you can use APDU commands and read/write API.

## Quick Start

Declear "using" directive if you need.
```csharp
using CaCom;
```
You need to register callbacks.
```csharp
NfcController controller = new NfcController();
controller.OnConnected += (success, readers) => {...};
controller.OnDisconnected += () => {...};
controller.OnEnterCard += (card, reader) => {...};
controller.OnLeaveCard += (card, reader) => {...};
controller.OnAPDUCommand += (success, reader, command, data) => {...};
controller.OnWriteNdefMessage += (success, reader, message) => {...};
controller.OnReadNdefMessage += (success, reader, message) => {...};
```
After that, just connect to the NFC service.
```csharp
controller.ConnectService();
```

When you quit an application, you need to disconnect the NFC service.
```csharp
controller.DisconnectService();
```

Transmitting examples are as follows.
 - Send APDU
```csharp
byte[] data = { 0xff, 0xb0, 0x00, 0x00, 0x00 };
ApduCommand command = new ApduCommand(data);
controller.SendAPDUCommand(reader, command);
```
 - Write NDEF Message
```csharp
NdefMessage message = new NdefMessage();
Payload payload = new TextPayload("en", "Hello World.", false);
message.AddRecord(new NdefRecord(payload));
controller.WriteNdefMessage(reader, message, 0, 16);
```
 - Read NDEF Message
```csharp
NdefMessage message = new NdefMessage();
controller.ReadNdefMessage(reader, message, 0);
```

## Examples
This repository includes a simple reader/writer application. "NfcReaderWriter" can control NFC devices.

### Target Chipset
In NFC, there are many target chipset. These chipsets has each memory layout and updating rules. 

Examples as follows.

 - NTAG213
   User Mmoery Page : page 0x04 - page 0x27
   Update Unit Size : 4byte
   
 - Felica lite-s
   User Mmoery Page : page 0x00 - page 0x0D
   Update Unit Size : 16byte

In NfcReaderWriter App, "User Mmoery Page" and " Update Unit Size" can be set.
