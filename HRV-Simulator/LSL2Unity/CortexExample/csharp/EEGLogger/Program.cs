using System;
using CortexAccess;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Text;

namespace EEGLogger
{
    class Program
    {
        const string Username = "spisak13";
        const string Password = "Boc123ece";
        const string LicenseId = "0faeaab2-2d30-4ab7-8bc3-514b3f704717";
        const int DebitNumber = 2; // default number of debit

        const string OutFilePath = @"EEGLogger.csv";
        private static FileStream OutFileStream;

        static void Main(string[] args)
        {
            Console.WriteLine("EEG LOGGER");
            Console.WriteLine("Please wear Headset with good signal!!!");

            // Delete Output file if existed
            if (File.Exists(OutFilePath))
            {
                File.Delete(OutFilePath);
            }
            OutFileStream = new FileStream(OutFilePath, FileMode.Append, FileAccess.Write);


            Process p = new Process();

            // Register Event
            p.OnEEGDataReceived += OnEEGDataReceived;
            p.SessionCtr.OnSubcribeEEGOK += OnEEGDataReceived;
            //p.OnPerfDataReceived += OnPerfDataReceived;
            //p.SessionCtr.OnSubcribeMetOK += OnPerfDataReceived;

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
            if (p.IsCreateSession)
            {
                // Subcribe EEG data
                p.SubcribeData("eeg"); //eeg / met
                Thread.Sleep(5000); //5000 - 50000
            }
            
            Console.WriteLine("Press Enter to exit");
            while (Console.ReadKey().Key != ConsoleKey.Enter) { }

            // Unsubcribe stream
            p.UnSubcribeData("eeg"); //eeg / met
            Thread.Sleep(3000); //3000 - 50000
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

        private static void OnEEGDataReceived(object sender, ArrayList eegData)
        {
           WriteDataToFile(eegData);
        }

       // private static void OnPerfDataReceived(object sender, ArrayList perfData)
       // {
       //     WriteDataToFile(perfData);
       //}

        // TODO: For subcrible All stream
        //private static void OnMotionDataReceived(object sender, ArrayList eegData)
        //{
        //    //WriteDataToFile(eegData);
        //}
        //private static void OnDevDataReceived(object sender, ArrayList eegData)
        //{
        //    //WriteDataToFile(eegData);
        //}
        //private static void OnMetDataReceived(object sender, ArrayList eegData)
        //{
        //    //WriteDataToFile(eegData);
        //}

    }
}
