using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CaCom;

namespace NFCTestConsole
{
    class Program
    {
        static NfcController controller = new NfcController();

        static void Main(string[] args)
        {

            controller.OnConnected += (success, readers) =>
            {
                if (success) Console.WriteLine("Success Connected.");
                else Console.WriteLine("Success Failured.");

                foreach (var reader in readers)
                {
                    Console.WriteLine(reader.Name);
                }
            };

            controller.OnDisconnected += () =>
            {
                Console.WriteLine("Disconnected.");
                Console.WriteLine("Press any key..");
            };

            controller.OnEnterCard += (card, r) =>
            {
                Console.WriteLine("Enter:" + card.Id + ":" + card.Type + " on " + r.Name + " : " + r.SerialNumber);
                return Disposition.Leave;
            };

            controller.OnLeaveCard += (card, r) =>
            {
                if(card != null) Console.WriteLine("Leave:" + card.Id + " on " + r.Name + " : " + r.SerialNumber);
                else Console.WriteLine("Initial card empty");
            };

            controller.ConnectService();


            Console.WriteLine("Press any key if you want to Disconnect Service...");
            Console.ReadLine();

            controller.DisconnectService();

            Console.ReadLine();

        }
    }
}
