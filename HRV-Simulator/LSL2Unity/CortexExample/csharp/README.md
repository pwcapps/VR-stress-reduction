# Cortex C# Examples
These examples show how to call the Cortex APIs from C# which describe at [Cortex Document](https://emotiv.github.io/cortex-docs/)

## Getting Started
These instructions will get you a copy of the project up and running on your local machine for development.
### Prerequisites
* You might have [Visual Studio](https://www.visualstudio.com/) with C# supported (msvc14 or higher recommended).
* The Cortex have to be running on your machine such as service. You can get the Cortex from (https://www.emotiv.com/developer/). In addition, the Cortex also is installed when EmotivPRO is installed.
* Get a client id and a client secret from emotiv.com. You must connect to your Emotiv account on emotiv.com and create a Cortex app. If you don't have a EmotivID, you can [register here](https://id.emotivcloud.com/eoidc/account/registration/).
* We have updated our Terms of Use, Privacy Policy and EULA to comply with GDPR. Please login via CortexUI to read and accept our latest policies in order to proceed using the following examples.  

### How to compile
<!-- how to compile  -->
1. Open CortexExamples.sln by Visual Studio IDE
2. Use Nuget Package Manager to install _Newtonsoft.Json, SuperSocket.ClientEngine.Core, WebSocket4Net_ for CortexAccess project
3. Put your login information and your client id and client secret to source. More detail please see below.
4. You can compile and run the examples directly from the IDE.

### Code structure
<!-- Code structure :overview about projects, classes in CortexAccess project and other examples-->
This section describe structure overview, core classes and examples. The C# Cortex examples contain 2 parts. Firstly, CortexAcess project is core and responsible for processing all requests and responses such as a middle layer between user and cortex. Secondly, examples, each examples as a project which reference to CortexAccess project.
<!-- Structure overview -->
#### Structure overview
The class diagram is described as below
<p align="center">
  <img width="460" height="300" src="Resources/Images/classDiagram.png">
</p>

* CortexClient: Responsible for sending requests to Cortex and receiving responses, warning, data from Cortex.
* Process: Process request / response messages and route to corresponding controller for handling.
  * AccessController: Responsible for login, authenticate and refresh token. You need put ClientId and ClientSecret to AccessController.cs
  * HeadsetController: Responsible for headset query, check headset connection. The controller will trigger a query headset to detect headset available or not each 10 seconds.
  * SessionController: Responsible for session creation, update, data recording and inject marker. A Session is created when headset connected and closed when headset disconnected.
  * TrainingController: Responsible for profile creation, update, load, save and training.
* Examples: Each example is a project have main class. Create a new Instance of Process and make examples.

#### Examples
**1. EEGLogger**
* This example opens a session with the first Emotiv headset it can find, and save eeg data to EEGLogger.csv file until Enter key pressed. 
* To run this example, you need put your Emotiv ID and license in Program.cs.
* The basic work-flow: Login -> Authen(Authenticate), Query Headset -> Create Session -> Subscribe EEG data

**2. MotionLogger**
* This example opens a session with the first Emotiv headset it can find, and save motion data to MotionLogger.csv file until Enter key pressed.
* To run this example, you need put your Emotiv ID in Program.cs. Do not need license to retrieve motion data.
* The basic work-flow: Login -> Authen, Query Headset -> Create Session -> Subscribe Motion data 

**3. MentalCommandTraining**
* This example opens a session with the first Emotiv headset it can find, load existed profile/ create a new profile then ask you to train **mental command** actions.
* To run this example, you need put your Emotiv ID and Profile Name in Program.cs. Do not need a license here.
* The basic work-flow: Login -> Authen, Query Headset, Query Detection -> Create Session -> Subscribe _sys_ data -> Load/Create profile -> Start training actions -> Accept/Reject training result -> Save profile -> Subscribe _com_ data.

**4. FacialExpressionTraining**
* This example opens a session with the first Emotiv headset it can find, load existed profile/ create a new profile then ask you to train **facial expression** actions.
* To run this example, you need put your Emotiv ID and Profile Name in Program.cs. Do not need a license here.
* The basic work-flow: Login -> Authen, Query Headset, Query Detection -> Create Session -> Subscribe _sys_ data -> Load/Create profile -> Start training actions -> Accept/Reject training result -> Save profile -> Subscribe _fac_ data.

**5. InjectMarkers**
* This example opens a session with the first Emotiv headset it can find, and inject markers to data stream.
* To run this example, you need put your Emotiv ID and license in Program.cs.
* Following guideline shown on console. Press a certain key to set a  label of marker into injectmarker function. The program ignores Tab, Enter, Spacebar and Backspace Key.
* The basic work-flow: Login -> Authen, Query Headset -> Create Session -> Inject Markers -> Quit.

**6. RecordData**
* This example opens a session with the first Emotiv headset it can find, start record and inject markers then stop record.
* To run this example, you need put your Emotiv ID and license in Program.cs.
* There are default record information (record Name, record subject, record Note), you can replace by yourself.
* The basic work-flow: Login -> Authen, Query Headset -> Create Session -> Start Record -> Inject marker (receive A, B, C key for markers) -> Stop Record.

### Notes
* You have to logout before logging other account.
* You need put a valid license to get EEG data, Performance metrics data
* Currently, the examples use Sleep() for waiting responses from Cortex. But not elegant, you can implement more events for responsive handling.
* Sometimes, you might get a error. Please re-run the example again. If the issue have not been resolved, Please contact us. 
* You also can develop some similar examples, kind of FacialExpressionTraining, Subscribe other streams such as: fac, dev, pow

* TODO: Error handling for each case, basic UI for examples.

### References
1. https://www.newtonsoft.com/json
2. http://www.supersocket.net/
3. http://websocket4net.codeplex.com/
4. https://emotiv.github.io/cortex-docs/
5. C# Coding Standards for .NET - Lance Hunt
