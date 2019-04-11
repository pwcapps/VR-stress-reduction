using CortexAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MentalCommandTraining
{
    class Program
    {
        const string Username = "your_username";
        const string Password = "your_password";
        const string ProfileName = "profileName";

        static void Main(string[] args)
        {
            Console.WriteLine("MENTAL COMMAND TRAINING");
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
                // Subcribe sys event
                p.SubcribeData("sys");
                Thread.Sleep(5000);
            }

            // Query Profile
            p.QuerryProfiles();
            Thread.Sleep(5000);

            // get Detection Information
            //p.QuerryDetectionInfo("mentalCommand");
            //Thread.Sleep(2000); //wait for get detection information

            // Check Profile existed
            // Then load an existed profile or create a new Profile
            if (p.IsProfilesExisted(ProfileName))
                p.LoadProfile(ProfileName);
            else
                p.CreateProfile(ProfileName);
            Thread.Sleep(2000);

            // Training neutral
            Console.WriteLine("\n###### Train NEUTRAL Action");
            p.StartCmd("neutral");
            Thread.Sleep(10000);
            p.AcceptCmd();
            Thread.Sleep(2000);
            // Training push
            Console.WriteLine("\n###### Train PUSH Action");
            p.StartCmd("push");
            Thread.Sleep(10000);
            p.AcceptCmd();
            Thread.Sleep(2000);

            // Training pull
            Console.WriteLine("\n###### Train PULL Action");
            p.StartCmd("pull");
            Thread.Sleep(10000);
            p.AcceptCmd();
            Thread.Sleep(2000);

            // Save profile
            p.SaveProfile();
            Thread.Sleep(3000);

            // Upload profile
            p.UploadProfile();
            Thread.Sleep(3000);

            // Subcribe com event -> show training result
            p.SubcribeData("com");
            Thread.Sleep(5000);

        }
    }
}
