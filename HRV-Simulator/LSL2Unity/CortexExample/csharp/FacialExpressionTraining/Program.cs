using CortexAccess;
using System;
using System.Threading;

namespace FacialExpressionTraining
{
    class Program
    {
        const string Username = "your_username";
        const string Password = "your_password";
        const string ProfileName = "profileName";

        static void Main(string[] args)
        {
            Console.WriteLine("FACIAL EXPRESSION TRAINING");
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
            //p.QuerryDetectionInfo("facialExpression");
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
            p.StartFE("neutral");
            Thread.Sleep(10000);
            p.AcceptFE();
            Thread.Sleep(2000);

            // Training blink
            //p.StartFE("blink"); // Currently, can not train blink action
            //Thread.Sleep(10000);
            //p.AcceptFE();
            //Thread.Sleep(2000);

            // Training smile
            Console.WriteLine("\n###### Train SMILE Action");
            p.StartFE("smile");
            Thread.Sleep(10000);
            p.AcceptFE();
            Thread.Sleep(2000);

            // Save profile
            p.SaveProfile();
            Thread.Sleep(5000);

            // Upload Profile
            //p.UploadProfile();
            //Thread.Sleep(3000);

            // Subcribe fac event -> show training result
            p.SubcribeData("fac");
            Thread.Sleep(5000);
        }
    }
}
