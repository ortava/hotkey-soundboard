using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace hotkey_soundboard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// 
    /// The main functionality of the program is defined here:
    /// 1.) Methods for individual event functionality on the MainWindow controls.
    /// 2.) Supplementary methods that help deal with the user's profile data by
    ///     loading commands and registering global hotkeys.
    /// 
    /// Note: This is a long file. During the course of this project, I've become
    ///       more accustomed to the design pattern of MVC (Model-View-Controller)
    ///       and  MVVM (Model-View-ViewModel). Neither of these patterns were used
    ///       to great extent in this project, but if they had been used, this file
    ///       would be much smaller, easier to maintain, and easier to reuse. To move
    ///       this project into an MVVM format would be a great way to improve it.
    /// </summary> 

    public partial class MainWindow : Window
    {
        const int numberOfHotkeys = 36;     // the number of hotkeys a profile can have

        List<ProfileModel> profiles = new List<ProfileModel>();         // list of profiles store in the database
        List<CommandModel> commands = new List<CommandModel>();         // list of active commands (linked to a specific profile by ID)
        List<GlobalHotkey> globalHotkeys = new List<GlobalHotkey>();    // list of active global hotkey (linked to a specific profile by ID)

        int profileIndex = 0;               // keeps track of the active profile (helpful to keep track of the corresponding command/global_hotkey ID)
        bool bHotkeyInputDisabled = false;  // keeps track of when the a hotkey textbox can receive further input
        bool bHotkeyInProgress = false;     // keeps track of when the user is in the process of making a hotkey
        bool bIsInactive = false;           // keeps track of whether the "Deactivate Hotkeys" checkbox is checked or not. If bIsInactive = true, global hotkeys will not register.
        object lastSender = null;           // keeps track of the last control sender

        private MediaPlayer player = new MediaPlayer(); // the mediaplayer used to play audio when the user executes a hotkey command

        public MainWindow()
        {
            InitializeComponent();

            player.Volume = 0.40;
            player.Play();                  // play nothing on startup so the user can edit their volume control with needing to play audio

            LoadProfiles();                 // load all stored profiles into the profiles list and create an item in the listbox for each
            LoadGlobalHotkeys(0);           // load the initial profile's related global hotkeys and register them
            LoadCommandControls(0);         // load the ininital profile's related command string data and insert into controls on the window
        }

        // load all profiles from the database into the profiles list and adds appropriate items to the profile listbox
        public void LoadProfiles()
        {
            profiles = DataAccess.LoadProfiles();
            for (int i = 0; i < profiles.Count; i++)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = profiles[i].Name;
                item.FontSize = 18;
                item.Padding = new Thickness(5, 10, 0, 10);
                item.PreviewMouseDoubleClick += list_PreviewMouseDoubleClick;

                listboxProfile.Items.Add(item);
            }
        }

        // load and register global hotkeys
        private void LoadGlobalHotkeys(int profileIndex)
        {
            globalHotkeys = DataAccess.LoadGlobalHotkeys(profiles[profileIndex]);
            for (int i = 0; i < globalHotkeys.Count; i++)
            {
                globalHotkeys[i].ConvertToKey(globalHotkeys[i].Virtual_Key_Code, globalHotkeys[i].Modifiers);
                globalHotkeys[i].Action = OnHotkeyPlaySound;
                globalHotkeys[i].Register(bIsInactive);
            }
        }

        // load Command data into commands list and insert data into respective controls
        public void LoadCommandControls(int profileIndex)
        {
            commands = DataAccess.LoadCommands(profiles[profileIndex]);

            InsertCommands();
        }

        // insert all Command data into respective controls on page
        private void InsertCommands()
        {
            InsertHotkeys();        // insert Hotkeys
            InsertNames();          // ...Names
            InsertFilePaths();      // ...and File_Paths into their respective controls
        }

        // insert Hotkey values retrieved from database into respective controls
        private void InsertHotkeys()
        {
            int i = 0;
            foreach (Control txtControl in CommandGrid.Children.OfType<TextBox>())      // loop through textboxes in CommandGrid
            {
                if (txtControl.Name == ("Hotkey" + (i + 1).ToString()))                 // if this control's name matches "Hotkey1", "Hotkey2", ...
                {
                    txtControl.SetValue(TextBox.TextProperty, commands[i].Hotkey);      // set the Text property of this textbox to matching value from the database
                    i++;
                }
            }
        }

        // insert Name values retrieved from database into respective controls
        private void InsertNames()
        {
            int i = 0;
            foreach (Control txtControl in CommandGrid.Children.OfType<TextBox>())      // loop through textboxes in CommandGrid
            {
                if (txtControl.Name == ("Name" + (i + 1).ToString()))                   // if this control's name matches "Name1", "Name2", ...
                {
                    txtControl.SetValue(TextBox.TextProperty, commands[i].Name);        // set the Text property of this textbox to matching value from the database
                    i++;
                }
            }
        }

        // insert File_Path values retrieved from database into respective controls
        private void InsertFilePaths()
        {
            int i = 0;
            foreach (var txtFileControl in CommandGrid.Children.OfType<TextBlock>())        // loop through textblocks in CommandGrid
            {
                if (txtFileControl.Name == ("File" + (i + 1).ToString()))                   // if this control's name matches "File1", "File2", ...
                {
                    txtFileControl.SetValue(TextBlock.TextProperty, commands[i].File_Path); // set the Text property of this textblock to matching value from the database
                    i++;
                }
            }
        }

        // global hotkeys call this method to play their respective sound files
        private void OnHotkeyPlaySound(GlobalHotkey hotkey)
        {
            // check if the user meant to create a new hotkey command and not execute an existing one
            if (bHotkeyInProgress)
            {
                FinishHotkey(lastSender, hotkey.Key);
                lastSender = null;
                return;
            }

            int commandIndex = (hotkey.Id - 1) - (numberOfHotkeys * (profiles[profileIndex].Id - 1));

            // play the sound file 
            player.Stop();         // stop any previous sounds still playing
            if (!string.IsNullOrEmpty(commands[commandIndex].File_Path))
            {
                player.Open(new Uri(commands[commandIndex].File_Path));
                player.Play();
            }
        }

        // returns the Id of a given control
        public int GetControlId(object sender)
        {
            int Id = 0;
            if (sender is Control control)
            {
                string strId = control.Name.Substring(control.Name.Length - 2); // get the last 2 character in the control's name ("Hotkey10" gives strId='10', "Hotkey11" gives char='11', etc...)
                                                                                // ...note that this only accounts for 2-digit numbers 
                                                                                // ...(controls can have names like "Hotkey99", but not "Hotkey100", "Hotkey 101", and so on)

                if (!strId.Any(ch => char.IsLetter(ch)))                        // if there are no letters in the string (this string can be converted to a number)
                {
                    Id = int.Parse(strId);                                      // convert the string to an int so we can correctly identify the related command
                    return Id;                                                  // return Id
                }
                else                                                            // if this is a name with a single digit Id
                    strId = control.Name.Substring(control.Name.Length - 1);        // get the last character in the control's name ("Hotkey1" gives strId='1', "Hotkey2" gives strId='2', etc...)
                Id = int.Parse(strId);                                          // convert the string to an int
            }                                                                   // return Id

            if (sender is TextBlock tblock)
            {
                string strId = tblock.Name.Substring(tblock.Name.Length - 2);

                if (!strId.Any(ch => char.IsLetter(ch)))
                {
                    Id = int.Parse(strId);
                    return Id;
                }
                else
                    strId = tblock.Name.Substring(tblock.Name.Length - 1);
                Id = int.Parse(strId);
            }

            return Id;
        }

        /// ####################################################################
        /// CONTROL METHODS BELOW
        /// ####################################################################

/// ###############################
/// TEXTBOX METHODS
/// ###############################

        // defines behavior when a hotkey textbox comes into focus of a Hotkey textbox
        // selects all text in the hotkey textbox
        private void Hotkey_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            // selects (highlights) all text in the textbox
            Dispatcher.BeginInvoke(DispatcherPriority.Input,
                new Action(delegate () {
                    tboxSender.SelectAll();
                }));
        }

        // defines behavior when a hotkey textbox loses focus of a Hotkey textbox
        // resets hotkey to last valid hotkey
        private void Hotkey_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            int Id = GetControlId(sender);

            if (tboxSender.Text == "")     // if there is no valid hotkey in the textbox
                tboxSender.Text = commands[Id - 1].Hotkey;              // insert the most recently saved valid hotkey into the textbox

            bHotkeyInputDisabled = false;  // the hotkey textbox has lost focus, so the user may have moved on to another hotkey
            bHotkeyInProgress = false;
        }

        // defines behavior when the user's key is released on a Hotkey textbox
        // facilitates user input of valid hotkeys
        private void Hotkey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);
            bHotkeyInProgress = false;
            int Id = GetControlId(tboxSender);


            if (Keyboard.Modifiers.Equals(ModifierKeys.None))   // if there are no more modifier keys being held down
            {
                tboxSender.Text = commands[Id - 1].Hotkey;      // insert the most recently saved valid hotkey
                bHotkeyInputDisabled = false;                   // the hotkey textbox is ready to take more input
                e.Handled = true;                               
                return;
            }
            else                                                // if modifier keys are still being pressed
            tboxSender.Text = commands[Id - 1].Hotkey;          
            bHotkeyInputDisabled = true;                        // the hotkey textbox is not ready to take more input
            e.Handled = true;
        }

        // defines behavior when the user's key is pressed down on a Hotkey textbox
        // facilitates user input of valid hotkeys, then saves the hotkey when a valid hotkey has been entered
        private void Hotkey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // if this key is invalid for a hotkey
            if (key == Key.LWin || key == Key.RWin
                || key == Key.CapsLock || key == Key.Escape
                || key == Key.Back || key == Key.Delete
                || key == Key.Space || key == Key.System
                || key == Key.Return || key == Key.Enter
                || bHotkeyInputDisabled)    // or the hotkey textbox is not ready for input
            {
                e.Handled = true;                       // the key has been processed
                return;                                 // exit the function
            }

            if (e.IsRepeat)                             // stop key from repeating upon being held down
            {
                e.Handled = true;                       // the key has been processed
                return;                                 // exit the function
            }

            // if the hotkey isn't full of modifier keys (CTRL, ALT, SHIFT, TAB, etc.) yet, we can add another
            if (!HasMaxModifiers(tboxSender, e))
            {
                if (key == Key.LeftCtrl || key == Key.RightCtrl)            // if a CTRL key has been pressed
                {
                    if (HasOneModifier(tboxSender, e))                      // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "CTRL + ")                   // and CTRL was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            bHotkeyInProgress = false;
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        
                        if (tboxSender.Text == "SHIFT + ")
                        {
                            tboxSender.Text = "CTRL + SHIFT + ";
                            bHotkeyInProgress = true;                       // hotkey is being created
                            lastSender = tboxSender;                        // store the last textbox sender
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }

                        if (tboxSender.Text == "ALT + ")
                        {
                            tboxSender.Text = "CTRL + ALT + ";
                            bHotkeyInProgress = true;                       // hotkey is being created
                            lastSender = tboxSender;                        // store the last textbox sender
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                    }
                    else                                                    // if no modifiers have been pressed yet
                    tboxSender.Text = "CTRL + ";                            // replace the text in the textbox with "CTRL + "
                    bHotkeyInProgress = true;                               // hotkey is being created
                    lastSender = tboxSender;                                // store the last textbox sender
                    e.Handled = true;                                       // the key has been processed
                    return;                                                 // exit the function

                }

                if (key == Key.LeftAlt || key == Key.RightAlt)              // if an ALT key has been pressed
                {
                    if (HasOneModifier(tboxSender, e))                      // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "ALT + " && key != Key.System)       // and ALT was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            bHotkeyInProgress = false;
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        
                        if (tboxSender.Text == "CTRL + " && key != Key.System)
                        {
                            tboxSender.Text += "ALT + ";
                            bHotkeyInProgress = true;                       // hotkey is being created
                            lastSender = tboxSender;                        // store the last textbox sender
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        
                        if (tboxSender.Text == "SHIFT + " && key != Key.System)
                        {
                            tboxSender.Text = "ALT + SHIFT + ";             
                            bHotkeyInProgress = true;                       // hotkey is being created
                            lastSender = tboxSender;                        // store the last textbox sender
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                    }
                    else                                                    // if no modifiers have been pressed yet
                    tboxSender.Text = "ALT + ";                             // replace the text in the textbox with "ALT + "
                    bHotkeyInProgress = true;
                    lastSender = tboxSender;
                    e.Handled = true;                                       // the key has been processed
                    return;                                                 // exit the function
                }

                if (key == Key.LeftShift || key == Key.RightShift)          // if a SHIFT key has been pressed
                {
                    if (HasOneModifier(tboxSender, e))                      // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "SHIFT + ")                  // and SHIFT was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            bHotkeyInProgress = false;
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }

                        if (tboxSender.Text == "CTRL + ")
                        {
                            tboxSender.Text += "SHIFT + ";                  // append "SHIFT + " 
                            bHotkeyInProgress = true;                       // hotkey is being created
                            lastSender = tboxSender;                        // store the last textbox sender
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }           
                        
                        if (tboxSender.Text == "ALT + ")
                        {
                            tboxSender.Text = "ALT + SHIFT + ";              
                            bHotkeyInProgress = true;                       // hotkey is being created
                            lastSender = tboxSender;                        // store the last textbox sender
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                    }
                    else                                                    // if no modifiers have been pressed yet
                    tboxSender.Text = "SHIFT + ";                           // replace the text in the textbox with "SHIFT + "
                    bHotkeyInProgress = true;
                    lastSender = tboxSender;
                    e.Handled = true;                                       // the key has been processed
                    return;                                                 // exit the function
                }
            }

            if (key != Key.LeftCtrl && key != Key.RightCtrl                 // if a non-modifer key was pressed
                && key != Key.LeftShift && key != Key.RightShift            // ...
                && key != Key.LeftAlt && key != Key.RightAlt                // ...
                && key != Key.Tab && key != Key.System                      // ...
                && bHotkeyInProgress)                                       // and it's ready to be finished
            {
                FinishHotkey(tboxSender, key);                              // finish the hotkey
                e.Handled = true;
            }
        }

        // completes a hotkey command with the final key and stores it
        private void FinishHotkey(object sender, Key key)
        {
            TextBox tboxSender = (sender as TextBox);

            switch (key)                                // begin checking which key was was pressed
            {                                           // assign the appropriate character to the given key
                case Key.D0:
                    tboxSender.Text += "0";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.D1:
                    tboxSender.Text += "1";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.D2:
                    tboxSender.Text += "2";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.D3:
                    tboxSender.Text += "3";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.D4:
                    tboxSender.Text += "4";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.D5:
                    tboxSender.Text += "5";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.D6:
                    tboxSender.Text += "6";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.D7:
                    tboxSender.Text += "7";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.D8:
                    tboxSender.Text += "8";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.D9:
                    tboxSender.Text += "9";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.OemMinus:
                    tboxSender.Text += "-";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.OemPlus:
                    tboxSender.Text += "=";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.OemComma:
                    tboxSender.Text += ",";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.OemPeriod:
                    tboxSender.Text += ".";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.OemQuestion:
                    tboxSender.Text += "/";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.Oem1:
                    tboxSender.Text += ";";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.OemQuotes:
                    tboxSender.Text += "'";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.OemOpenBrackets:
                    tboxSender.Text += "[";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.Oem6:
                    tboxSender.Text += "]";
                    StoreHotkey(tboxSender, key);
                    break;
                case Key.Oem5:
                    tboxSender.Text += "'\'";
                    StoreHotkey(tboxSender, key);
                    break;
                default:                                // default is a non-special key, like the alphabetical keys
                    tboxSender.Text += key.ToString();
                    StoreHotkey(tboxSender, key);
                    break;
            }

            // hotkey is complete
            bHotkeyInProgress = false;
        }

        // stores and registers a completed hotkey
        private void StoreHotkey(object sender, Key e) //, string lastKey)
        {
            KeyModifier modifiers = (KeyModifier)Keyboard.Modifiers;
            TextBox tboxSender = (sender as TextBox);
            int Id = GetControlId(tboxSender);
            int commandIndex = Id - 1;
            string temp = tboxSender.Text;

            for (int i = 0; i < commands.Count; i++)
            {
                if (temp == commands[i].Hotkey && commands[commandIndex].Id != commands[i].Id)  // if this command already exists
                {                                                                               // ... swap them
                    // store data in temporary variables
                    string oldKeyStr = commands[commandIndex].Hotkey;
                    string oldName = commands[commandIndex].Name;
                    Key oldKey = globalHotkeys[commandIndex].Key;
                    KeyModifier oldKeyModifiers = globalHotkeys[commandIndex].Key_Modifiers;

                    // swap the data from this command and the old command
                    commands[commandIndex].Hotkey = commands[i].Hotkey;
                    commands[commandIndex].Name = commands[i].Name;
                    globalHotkeys[commandIndex].Key = e;
                    globalHotkeys[commandIndex].Key_Modifiers = modifiers;
                    globalHotkeys[commandIndex].Action = OnHotkeyPlaySound;

                    commands[i].Hotkey = oldKeyStr;
                    commands[i].Name = oldName;
                    globalHotkeys[i].Key = oldKey;
                    globalHotkeys[i].Key_Modifiers = oldKeyModifiers;
                    globalHotkeys[i].Action = OnHotkeyPlaySound;

                    // unregister the two old global hotkeys, then register with the newly stored data
                    globalHotkeys[i].Unregister();
                    globalHotkeys[commandIndex].Unregister();
                    globalHotkeys[i].Register(bIsInactive);
                    globalHotkeys[commandIndex].Register(bIsInactive);

                    // save all changes to the database
                    DataAccess.SaveGlobalHotkey(globalHotkeys[i]);
                    DataAccess.SaveCommand(commands[i]);
                    DataAccess.SaveGlobalHotkey(globalHotkeys[commandIndex]);
                    DataAccess.SaveCommand(commands[commandIndex]);

                    // find the old command's hotkey textbox and display the new hotkey (i + 1 gives the Hotkey textbox's Id)
                    foreach (TextBox tBox in CommandGrid.Children.OfType<TextBox>().Where(tbox => tbox.Name == "Hotkey" + (i + 1).ToString()))
                    {
                        tBox.Text = commands[i].Hotkey;
                    }

                    return;
                }
            }

            // store data
            globalHotkeys[commandIndex].Key = e;
            globalHotkeys[commandIndex].Key_Modifiers = modifiers;
            globalHotkeys[commandIndex].Action = OnHotkeyPlaySound;
            commands[commandIndex].Hotkey = tboxSender.Text;

            globalHotkeys[commandIndex].Unregister();                    // unregister old hotkey
            globalHotkeys[commandIndex].Register(bIsInactive);           // register the newly stored hotkey

            DataAccess.SaveGlobalHotkey(globalHotkeys[commandIndex]);    // save global hotkey to the database
            DataAccess.SaveCommand(commands[commandIndex]);               // save command info to the database
        }

        // returns true if the hotkey already has the maximum number of valid modifier keys
        private bool HasMaxModifiers(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            List<string> prefixes = new List<string>(new string[]
            {
                "CTRL + SHIFT + ",
                "CTRL + ALT + ",
                "ALT + SHIFT + ",
                "SHIFT + CTRL + ",
                "SHIFT + ALT + ",
                "ALT + CTRL + ",
            });
            
            for (int i = 0; i < prefixes.Count; i++)
            {
                if (tboxSender.Text == prefixes[i])
                return true;
            }
            
            return false;
        }

        // returns true if the hotkey has only one valid modifier key
        private bool HasOneModifier(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            List<string> initialPrefixes = new List<string>(new string[]
            {
                "CTRL + ",
                "SHIFT + ",
                "ALT + ",
            });

            for (int i = 0; i < initialPrefixes.Count; i++)
            {
                if (tboxSender.Text == initialPrefixes[i])
                    return true;
            }

            return false;
        }

        // defines behavior for when a key is pressed down on a Name textbox
        // save custom name by pressing ENTER in the Name textbox
        private void Name_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            lblInfo.Content = "Press ENTER to save.";   // reset info label content

            // save content of Name textbox upon Enter key press
            if (e.Key == Key.Enter)
            {
                int Id = GetControlId(tboxSender);
                commands[Id - 1].Name = tboxSender.Text;
                DataAccess.SaveCommand(commands[Id - 1]);

                lblInfo.Content = "Name saved!";        // inform user that the name has been saved
            }
        }

        // defines behavior upon a Name textbox gaining focus
        // lets the user know that they need to press the ENTER key to save a custom name
        private void Name_GotFocus(object sender, RoutedEventArgs e)
        {
            lblInfo.Content = "Press ENTER to save.";
        }

        // defines behavior upon losing focus of a Name textbox
        // resets the info label
        private void Name_LostFocus(object sender, RoutedEventArgs e)
        {
            lblInfo.Content = "";
        }

// ###############################
// TEXTBLOCK METHODS
// ###############################

        // defines behavior for when a file is dragged over a File textblock
        private void File_PreviewDragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;        // keeps track of when drops are enabled on the textblock
            string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];    // to hold filename(s) 
                                                                                            // (by design, only one file should be allowed to be dropped)
            foreach (string filename in filenames)
            {
                string temp = Path.GetExtension(filename).ToLowerInvariant();               // store the lowercase version of the filename extension
                if (temp.Equals(".mp3") || temp.Equals(".wav") || temp.Equals(".aac") || temp.Equals(".wma") || temp.Equals(".m4a"))   // if it is a valid extension
                {
                    e.Effects = DragDropEffects.Copy;                                       // copy filename
                    e.Handled = true;                                                       // action complete
                    dropEnabled = true;                                                     // allow drop
                }
                else                                                                        // if not a valid file extension
                    dropEnabled = false;                                                    // don't allow drop
            }

            if (!dropEnabled)                                                               // if drop isn't allowed
            {
                e.Effects = DragDropEffects.None;                                           // don't do anything
                e.Handled = true;                                                           // action complete
            }
        }

        // defines behavior upon dropping a file on a File textblock 
        // copies filename into File textblock, shortened name into Name textbox, and saves command
        private void File_PreviewDrop(object sender, DragEventArgs e)
        {
            object filename = e.Data.GetData(DataFormats.FileDrop);                         // get the filename
            if (sender is TextBlock tblockSender)
            {
                tblockSender.Text = string.Format("{0}", ((string[])filename)[0]);          // insert filename into textblock

                // get id of command
                int Id = GetControlId(tblockSender);
                int commandIndex = Id - 1;

                commands[commandIndex].File_Path = tblockSender.Text;                             // put filename into command list

                string shortFileName = Path.GetFileName(tblockSender.Text);                       // get shortened filename
                if (shortFileName.Length > 24)                                                    // if the shortened filename is too many characters
                    commands[commandIndex].Name = shortFileName.Remove(24, (shortFileName.Length - 24));  // cut off the rest of the excess characters and store in command list
                else 
                    commands[commandIndex].Name = shortFileName;                                  // otherwise just store the name in command list

                // find the Name textbox with the corresponding id
                foreach (TextBox txtboxControl in CommandGrid.Children.OfType<TextBox>().Where(tbox => tbox.Name == "Name" + Id.ToString()))
                {
                    txtboxControl.Text = commands[commandIndex].Name;                             // put the shortened filename into the Name textbox
                }

                DataAccess.SaveCommand(commands[commandIndex]);                                   // save this command (filename and name)
            }
        }

        // defines behavior when the left mouse button released on a File textblock 
        // opens file dialog box, copies filename to File textblock, shortened name to Name textbox, and saves command
        private void File_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBlock tblockSender = (sender as TextBlock);

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog       // opens new file dialog
            {
                DefaultExt = ".mp3",                                                         // default extensions are .mp3 files
                Filter = "MP3 Files (*.mp3)|*.mp3|WAV Files (*.wav)|*.wav|AAC Files (*.aac)|*.aac|WMA Files (*.wma)|*.wma|M4A Files (*.m4a)|*.m4a"  // other allowed file extensions
            };

            Nullable<bool> result = dialog.ShowDialog();                                     // keeps track of when dialog is shown

            if (result == true)
            {
                string filename = dialog.FileName;
                tblockSender.Text = filename;

                int Id = GetControlId(tblockSender);
                int commandIndex = Id - 1;

                commands[commandIndex].File_Path = tblockSender.Text;                        // store filename in command list

                string shortFileName = dialog.SafeFileName;                                  // get shortened filename

                if (shortFileName.Length > 24)                                               // if shortened filename is too long
                    commands[commandIndex].Name = shortFileName.Remove(24);                  // cut off excess characters and store in command list
                else
                    commands[commandIndex].Name = shortFileName;                             // otherwise, store whole name in command list

                foreach (TextBox txtboxControl in CommandGrid.Children.OfType<TextBox>().Where(tbox => tbox.Name == "Name" + Id.ToString()))
                {
                    txtboxControl.Text = commands[commandIndex].Name;                        // put shortened name into corresponding Name textbox
                }

                DataAccess.SaveCommand(commands[commandIndex]);                              // save command (filename and name)
            }
        }

        // defines behavior upon mouse entering (and hovering) over File textblocks 
        // shows full filepath in textblock
        private void File_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock tblockSender = (sender as TextBlock);

            if (tblockSender.Text != "")
            tblockSender.ToolTip = tblockSender.Text;
        }

/// ###############################
/// BUTTON METHODS
/// ###############################

        // defines behavior upon clicking a clear button 
        // clears Name textboxes and File textblocks corresponding to the row of the clicked Clear button, then saves the cleared command
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Button btnSender = (sender as Button);

            int Id = GetControlId(btnSender);
            int controlIndex = Id - 1;            

            foreach (TextBox txtboxControl in CommandGrid.Children.OfType<TextBox>().Where(tbox => tbox.Name == "Name" + Id.ToString()))
            {
                txtboxControl.Text = "";
                commands[controlIndex].Name = txtboxControl.Text;
            }

            foreach (TextBlock txtblockControl in CommandGrid.Children.OfType<TextBlock>().Where(tblock => tblock.Name == "File" + Id.ToString()))
            {
                txtblockControl.Text = "";
                commands[controlIndex].File_Path = txtblockControl.Text;
            }

            DataAccess.SaveCommand(commands[controlIndex]);
        }

        // defines behavior upon clicking the Clear All button 
        // clears all Name textboxes and File textblock on the CommandGrid, then saves the cleared commands
        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            foreach (TextBox txtboxControl in CommandGrid.Children.OfType<TextBox>())
            {
                if (txtboxControl.Name == "Name" + (i+1).ToString())
                {
                    txtboxControl.Text = "";
                    commands[i].Name = txtboxControl.Text;
                    i++;
                }             
            }

            i = 0;
            foreach (TextBlock txtblockControl in CommandGrid.Children.OfType<TextBlock>())
            {
                if (txtblockControl.Name == "File" + (i+1).ToString())
                {
                    txtblockControl.Text = "";
                    commands[i].File_Path = txtblockControl.Text;
                    i++;
                }  
            }

            DataAccess.SaveCommands(commands);      // save all commands (no need to save global hotkeys because the hotkeys don't change)
        }

        // defines behavior upon clicking the Add Profile button 
        // inserts a new profile into the database (including corresponding commands/globalhotkeys) and stores the new profile in the profiles list
        private void btnAddProfile_Click(object sender, RoutedEventArgs e)
        {
            // add new listbox item
            ListBoxItem item = new ListBoxItem();
            item.Content = "New Profile";
            item.FontSize = 18;
            item.Padding = new Thickness(5, 10, 0, 10);
            item.PreviewMouseDoubleClick += list_PreviewMouseDoubleClick;

            listboxProfile.Items.Add(item);

            // insert new profile into the database
            DataAccess.InsertProfile(item.Content.ToString(), commands, globalHotkeys);

            // load the new profile from the database
            ProfileModel newProfile = DataAccess.LoadNewProfile();

            // store the new profile in the profiles list
            profiles.Add(newProfile);
        }

        // defines behavior upon clicking the Delete button
        // deletes the selected profiles and its related commands/global hotkeys
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (profiles.Count == 1)
                return;

            DataAccess.DeleteProfile(profiles[listboxProfile.SelectedIndex]);   // delete selected profile and related data from database
            profiles.RemoveAt(listboxProfile.SelectedIndex);                    // remove the selected profile from the active profiles list

            if (profileIndex == listboxProfile.SelectedIndex)                   // if the list being deleted is the currently active profile
            {
                player.Stop();                                                  // stop the currently playing audio
                profileIndex = listboxProfile.SelectedIndex - 1;                // set a new profile index

                for (int i = 0; i < globalHotkeys.Count; i++)          // dispose of old global hotkeys
                {
                    globalHotkeys[i].Dispose();
                }

                LoadCommandControls(profileIndex);                     // load the new commands into the commands list
                LoadGlobalHotkeys(profileIndex);                       // load the new global hotkeys into the globalHotkeys list
            }

            if (profileIndex > listboxProfile.SelectedIndex)           // if the profile being deleted is below the currently active profile
            {
                profileIndex = profileIndex - 1;                       // adjust the profileIndex accordingly (reduce it by one)
            }

            listboxProfile.Items.Remove(listboxProfile.SelectedItem);  // remove the listbox item associated with the deleted profile
        }

/// ###############################
/// LISTBOX METHODS
/// ###############################

        // load an existing profile by double clicking on a listbox item
        private void list_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (profileIndex == listboxProfile.SelectedIndex)          // if the selected profile is already the active profile
                return;                                                // return

            // set the new active profile index
            profileIndex = listboxProfile.SelectedIndex;

            // dispose of old global hotkeys
            for (int i = 0; i < globalHotkeys.Count; i++)
            {
                globalHotkeys[i].Dispose();
            }

            LoadCommandControls(profileIndex);                          // load the new commands into the commands list
            LoadGlobalHotkeys(profileIndex);                            // load the new global hotkeys into the globalHotkeys list
        }

        // defines behavior upon clicking the Rename context menu item in the listbox
        // allow user to rename an existing profile and save the new name to the database
        private void listMenuItem_Rename_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new RenameDialog();
            if (dialog.ShowDialog() == true)
            {
                ListBoxItem listboxItem = listboxProfile.SelectedItem as ListBoxItem;
                listboxItem.Content = dialog.ResponseText;
                profiles[listboxProfile.SelectedIndex].Name = listboxItem.Content.ToString();
                DataAccess.SaveProfile(profiles[listboxProfile.SelectedIndex]);
            }
        }

/// ###############################
/// CHECKBOX METHODS
/// ###############################

        // defines behavior upon checking the "Deactivate Hotkeys" checkbox
        // deactivates all currently registered global hotkeys and prevents new hotkeys from being registered
        private void checkInactive_Checked(object sender, RoutedEventArgs e)
        {
            bIsInactive = true;

            // dispose of all active globalhotkeys
            for (int i = 0; i < globalHotkeys.Count; i++)
            {
                globalHotkeys[i].Unregister();
            }

            player.Close();                     // ensure that the media player is closed
        }

        // defines behavior upon unchecking the "Deactivate Hotkeys" checkbox
        // activates all currently selected hotkeys and allows new registrations as normal
        private void checkInactive_Unchecked(object sender, RoutedEventArgs e)
        {
            bIsInactive = false;

            // activate all globalhotkeys
            for (int i = 0; i < globalHotkeys.Count; i++)
            {
                globalHotkeys[i].Register(bIsInactive);
            }
        }

/// ###############################
/// WINDOW METHODS
/// ###############################

        // dispose of global hotkeys and close media player upon closing window
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // dispose of all active globalhotkeys
            for (int i = 0; i < globalHotkeys.Count; i++)
            {
                globalHotkeys[i].Dispose();
            }

            player.Close();                     // ensure that the media player is closed
        }
    }

}
