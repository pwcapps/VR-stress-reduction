# VR Stress Reduction Project

This is a Unity VR project aimed at reducing stress for users with stress-related health concerns, including chronic pains, anxiety, and high blood pressure. Using real-time bio-signals and guided imagery techniques, the VR system will allow users to explore a calming, forest scene while modulating scene lighting color in accordance with the users’ stress levels. Current components of guided imagery therapy are adapted to each individual. We aim to establish a generalized color-modulation model that will be effective in relieving stress for a broad range of users.

This project is an interdisciplinary collaboration between our student group from Dr. Jian Chen's CSE 5542 class and the Technology for Mental Health research lab at the Ohio State University.


## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development.


### Prerequisites

- Matlab
- Unity


### Running the sample data

- Clone the project into a local directory

```
git clone https://github.com/pwcapps/VR-stress-reduction.git
```

- Open the project directory in Unity

- Go to the HRV-Simulator folder and open MatlabOutputStreamTemp.m in Matlab

- Run the script in Matlab. This script will simulate HRV biodata when the heart rate sensor hardware is not available

- At the same time in Unity, go to Assets/Scenes/ForestNew to open and run the forest scene

Done! In the Unity editor, you should be able to look around the scene by dragging your mouse and observe color changes in the scene's ambient lighting. Be sure to turn up the audio to hear the forest sounds.


### Submodules

- HRV Simulator from https://github.com/PokerBox/HRV-Simulator