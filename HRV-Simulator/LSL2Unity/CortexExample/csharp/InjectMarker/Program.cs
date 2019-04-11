using CortexAccess;
using System;
using System.Threading;

namespace RecordData
{
    class Program
    {
        const string Username = "your_username";
        const string Password = "your_password";
        const string LicenseId = "your_license";
        const int DebitNumber = 2; // default number of debit

        static void Main(string[] args)
        {
            Console.WriteLine("INJECT MARKER DEMO");
            Console.WriteLine("Please wear Headset with good signal!!!");

            Process p = new Process();
            Thread.Sleep(10000); //wait for querrying user login, query headset
            if (String.IsNullOrEmpty(p.GetUserLogin()))
            {
                p.Login(Username, Password);
                Thread.Sleep(5000); //wait for logining
            }
            // Show username login
            Console.WriteLine("Username :" + p.GetUserLogin());

            if (p.AccessCtr.IsLogin)
            {
                // Send Authorize
                p.Authorize(LicenseId, DebitNumber);
                Thread.Sleep(5000); //wait for authorizing
            }
            if (!p.IsHeadsetConnected())
            {
                p.QueryHeadset();
                Thread.Sleep(10000); //wait for querying headset and create session
            }
            if (!p.IsCreateSession)
            {
                p.CreateSession();
                Thread.Sleep(5000);
            }

            Console.WriteLine("Press a certain Key to inject marker");
            Console.WriteLine("Press Q to querry session and quit");
            Console.WriteLine("Press Esc to quit");
            Console.WriteLine("Ignore Tab, Enter, Spacebar and Backspace key");

            int valueMaker = 1;
            ConsoleKeyInfo keyInfo;
            while (true)
            {
                keyInfo = Console.ReadKey(true);
                Console.WriteLine(keyInfo.KeyChar.ToString() + " has pressed");
                if (keyInfo.Key == ConsoleKey.Q)
                {
                    // Querry Sessions before quit
                    p.QuerySession();
                    Thread.Sleep(10000);
                    break;
                }
                else if (keyInfo.Key == ConsoleKey.Tab) continue;
                else if (keyInfo.Key == ConsoleKey.Backspace) continue;
                else if (keyInfo.Key == ConsoleKey.Enter) continue;
                else if (keyInfo.Key == ConsoleKey.Spacebar) continue;
                else if (keyInfo.Key == ConsoleKey.Escape)
                {
                    break;
                }
                else
                {
                    // Inject marker
                    p.InjectMarker(keyInfo.KeyChar.ToString(), valueMaker, "USB", Utils.GetEpochTimeNow());
                    Thread.Sleep(2000);
                    valueMaker++;
                }
            }
            Console.WriteLine("End of Program");

        }
    }
}
