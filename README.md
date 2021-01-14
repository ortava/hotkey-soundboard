# Hotkey Soundboard

### [Click to watch a demo video.](https://youtu.be/OXj9n1p-LVs)
## [![Watch a demo video.](https://img.youtube.com/vi/OXj9n1p-LVs/maxresdefault.jpg)](https://youtu.be/OXj9n1p-LVs)
## [Download Hotkey Soundboard with Google Drive.](https://drive.google.com/drive/folders/16mEw-v5kDrADmK2NVHTNYyMQLwlPNJNw?usp=sharing)

### About

Hotkey Soundboard is a windows desktop application that lets you play audio files from anywhere on your computer 
with custom global hotkeys.

Create profiles with different themes and purposes. Each profile is a set of customizable hotkeys that can be tied
to an audio file located on your computer. When the hotkey is pressed, the audio will play even if Hotkey Soundboard
is minimized.

### Using Profiles

* **Create Profiles:** Press the Add Profile button to create a new profile.
* **Switch Profiles:** Double click on a profile to switch to it and play a different set of sounds.
* **Rename Profiles:** Right-click a profile and select Rename to give a custom name to your profile.
* **Delete Profiles:** Select a profile and click the Delete button to delete the selected profile.

### Entering Custom Hotkeys
* When entering a new hotkey, use the CTRL, SHIFT, and ALT, as modifier keys.
* **Note:** Due to the hotkeys being global, it is advised to use hotkeys that are
not commonly used for other purposes (such as CTRL + C for copying). If you
want to temporarily disable this program's global hotkeys, make sure the
Disable Hotkeys checkbox is checked. Your hotkeys will no longer play the
linked audio file, and any conflicting hotkeys in other programs will work
as normal. Uncheck the checkbox to reenable your custom commands.

## ______________________________________________________

#### Developer Notes
This is my 2nd major personal project. 

It's a desktop program developed in C# using .NET Core.
WPF (Windows Presentation Foundation) is used for the GUI (this is why it can only be run on Windows computers).
SQLite and Dapper are used to create and manage the relational database that stores all profile and hotkey data.
A lot of these tools were new to me, but I'm glad with what I was able to do and I believe the program accomplishes its goal.
It provides for an entertaining audio environment anywhere on your desktop, particularly useful during recording or streaming.

For future reference, there are multiple ways I could expand on this project:
* **More Settings** (Play multiple sounds at once, Pause on hotkey repress, Hide window when minimized)
* **Pause Toggle** Make a toggle that lets repeated hotkey presses pause the currently playing audio file instead 
of replaying it from the start.
* **Convert to an MVVM design** I learned a lot about design patterns during the course of this project.
MVC (Model-View-Controller) and MVVM (Model-View-ViewModel) were frequently referenced, MVVM especially because this
is a C# WPF project. There is a clear issue with the majority of my code being in one file, and using a
solid design pattern like MVVM would make my code much cleaner and easy to follow.
