using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CortexAccess;
using System.Threading;
using System.Collections;
using System.IO;

namespace MotionLogger
{
    class Program
    {
        const string Username = "your_username";
        const string Password = "your_password";

        const string OutFilePath = @"MotionLogger.csv";
        private static FileStream OutFileStream;

        static void Main(string[] args)
        {
            Console.WriteLine("MOTION LOGGER");
            Console.WriteLine("Please wear Headset with good signal!!!");

            // Delete Output file if existed
            if (File.Exists(OutFilePath))
            {
                File.Delete(OutFilePath);
            }
            OutFileStream = new FileStream(OutFilePath, FileMode.Append, FileAccess.Write);

            Process p = new Process();

            // Register Event
            p.OnMotionDataReceived += OnMotionDataReceived;
            p.SessionCtr.OnSubcribeMotionOK += OnMotionDataReceived;

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
                p.Authorize();
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
            if (p.IsCreateSession)
            {
                // Subcribe Motion data
                p.SubcribeData("mot");
                Thread.Sleep(5000);
            }

            Console.WriteLine("Press Enter to exit");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }

            // Unsubcribe stream
            p.UnSubcribeData("mot");
            Thread.Sleep(3000);
            // Close Out Stream
            OutFileStream.Dispose();
        }

        // Write Header and Data to File
        private static void WriteDataToFile(ArrayList data)
        {
            int i = 0;
            for (; i < data.Count - 1; i++)
            {
                byte[] val = Encoding.UTF8.GetBytes(data[i].ToString() + ", ");
                if (OutFileStream != null)
                    OutFileStream.Write(val, 0, val.Length);
                else
                    break;
            }
            // Last element
            byte[] lastVal = Encoding.UTF8.GetBytes(data[i].ToString() + "\n");
            if (OutFileStream != null)
                OutFileStream.Write(lastVal, 0, lastVal.Length);
        }

        private static void OnMotionDataReceived(object sender, ArrayList motionData)
        {
            WriteDataToFile(motionData);
        }
    }
}
