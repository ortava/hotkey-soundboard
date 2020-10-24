using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace soundboard_hotkeys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary> 
   
    public partial class MainWindow : Window
    {
        List<CommandModel> commands = new List<CommandModel>();
        List<GlobalHotkey> globalHotkeys = new List<GlobalHotkey>();
        bool bHotkeyIsComplete = false;
        bool bHotkeyInProgress = false;
        object lastSender = null;           // keeps track of the last control sender

        private MediaPlayer player = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();

            player.Volume = 0.40;
            player.Play();

            LoadCommandControls();
            LoadGlobalHotkeys();
        }

        // load Command data into commands list and insert data into respective controls
        public void LoadCommandControls()
        {
            commands = DataAccess.LoadCommands();

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

        // load and register global hotkeys
        private void LoadGlobalHotkeys()
        {
            globalHotkeys = DataAccess.LoadGlobalHotkeys();
            for (int i = 0; i < globalHotkeys.Count; i++)
            {
                globalHotkeys[i].ConvertToKey(globalHotkeys[i].Virtual_Key_Code, globalHotkeys[i].Modifiers);
                globalHotkeys[i].Action = OnHotkeyPlaySound;
                globalHotkeys[i].Register();
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
            
            // play the sound file
            player.Stop();         // stop any previous sounds still playing
            if (!commands[hotkey.Id - 1].File_Path.Equals(""))
            {
                player.Open(new Uri(commands[hotkey.Id - 1].File_Path));
                player.Play();
            }
        }

        private void PauseSound(GlobalHotkey hotkey)
        {
            player.Pause();
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

        ///// CONTROL METHODS BELOW /////

        // saves all commands in the profile to the database
        private void btnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            // boolean values to keep track of whether or not an iteration of the foreach loop has finished inserting hotkey and name values before incrementing
            bool bHotkeyDone = false;
            bool bNameDone = false;

            int i = 0;
            foreach (TextBox txtboxControl in CommandGrid.Children.OfType<TextBox>())   // loop through textbox controls in CommandGrid
            {
                if (txtboxControl.Name == "Hotkey" + (i + 1).ToString())                // if this textbox is a matching Hotkey textbox
                {
                    commands[i].Hotkey = txtboxControl.Text;                            // insert its text into the CommandModel List
                    bHotkeyDone = true;                                                 // mark the Hotkey as recorded for this iteration
                }
                if (txtboxControl.Name == "Name" + (i + 1).ToString())                  // if this textbox is a matching Name textbox
                {
                    commands[i].Name = txtboxControl.Text;                              // insert its text into the CommandModel List
                    bNameDone = true;                                                   // mark the Name as recorded for this iteration
                }
                if (bHotkeyDone && bNameDone)                                           // if both Hotkey and Name textboxes have been recorded for this iteration
                {
                    i++;                                                                // increment for the next iteration
                    bHotkeyDone = false;                                                // reset Hotkey recorded status for next iteration
                    bNameDone = false;                                                  // reset Name recorded status for next iteration
                }
            }

            i = 0;  // reset increment value (no need to check for recorded status because there is only one type of textblock in CommandGrid
            foreach (TextBlock txtblockControl in CommandGrid.Children.OfType<TextBlock>()) // loop through textblock controls in CommandGrid
            {
                if (txtblockControl.Name == "File" + (i + 1).ToString())                    // if this textblock is a matching File textblock
                {
                    commands[i].File_Path = txtblockControl.Text;                           // insert its text into the CommandModel List
                    i++;                                                                    // increment for next iteration
                }
            }

            DataAccess.SaveCommands(commands);          // save the fully updated command list to the database
        }
        
        // defines behavior when a hotkey textbox comes into focus of a Hotkey textbox
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
        private void Hotkey_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            int Id = GetControlId(sender);

            if (tboxSender.Text == "")                                  // if there is no hotkey in the textbox
                tboxSender.Text = commands[Id - 1].Hotkey;              // insert the most recently saved valid hotkey into the textbox

            bHotkeyIsComplete = false;                                  // the hotkey textbox has lost focus, so the user may have moved on to another hotkey
        }

        // defines behavior when the user's key is released on a Hotkey textbox
        private void Hotkey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);
            bHotkeyInProgress = false;
            int Id = GetControlId(tboxSender);

            if (HasMaxModifiers(tboxSender, e) || HasOneModifier(tboxSender, e) || (tboxSender.Text == "")
                || bHotkeyIsComplete == false)     // if the hotkey textbox is not complete
            {
                tboxSender.Text = commands[Id - 1].Hotkey;                  // insert the most recently saved valid hotkey
                bHotkeyIsComplete = false;                                  // hotkey is not complete
                e.Handled = true;                                           // the key has been processed
                return;                                                     // exit the function
            }
        }

        // defines behavoir when the user's key is pressed down on a Hotkey textbox
        private void Hotkey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // if this key is an invalid for a hotkey
            if (key == Key.LWin || key == Key.RWin
                || key == Key.CapsLock || key == Key.Escape
                || key == Key.Back || key == Key.Delete
                || key == Key.Space || key == Key.System)
            {
                e.Handled = true;                               // the key has been processed
                return;                                         // exit the function
            }

            if (e.IsRepeat)                                     // stop key from repeating upon being held down
            {
                e.Handled = true;                               // the key has been processed
                return;                                         // exit the function
            }

            if (bHotkeyIsComplete == true)                      // if this hotkey is already complete (no need to add another key to the command)
            {
                tboxSender.Text = "";                           // reset text in textbox to ""
                bHotkeyIsComplete = false;                      // hotkey is no longer complete
                return;
            }

            // if the hotkey isn't full of modifier keys (CTRL, ALT, SHIFT, TAB, etc.) yet, we can add another
            if (!HasMaxModifiers(tboxSender, e))
            {
                if ((key == Key.LeftCtrl) || (key == Key.RightCtrl))    // if a CTRL key has been pressed
                {
                    if (HasOneModifier(tboxSender, e))                            // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "CTRL + ")                   // and CTRL was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        else                                                // if CTRL wasn't pressed yet
                        tboxSender.Text += "CTRL + ";                       // append "CTRL + " 
                    bHotkeyInProgress = true;
                    lastSender = tboxSender;
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                    }
                    else                                                    // if no modifiers have been pressed yet
                    tboxSender.Text = "CTRL + ";                            // replace the text in the textbox with "CTRL + "
                    bHotkeyInProgress = true;
                    lastSender = tboxSender;
                    e.Handled = true;                                       // the key has been processed
                    return;                                                 // exit the function

                }

                /// THE ALT KEY IS BEING RECOGNIZED AS Key.System, THUS THE BELOW IF STATEMENT IS INOPERATIVE ///
                if (key == Key.LeftAlt || key == Key.RightAlt)              // if an ALT key has been pressed
                {
                    if (HasOneModifier(tboxSender, e))                        // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "ALT + " && key != Key.System)                    // and ALT was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        else                                                // if ALT wasn't pressed yet
                        tboxSender.Text += "ALT + ";                        // append "ALT + " 
                        bHotkeyInProgress = true;
                        lastSender = tboxSender;
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                    }
                    else                                                    // if no modifiers have been pressed yet
                    tboxSender.Text = "ALT + ";                             // replace the text in the textbox with "ALT + "
                    bHotkeyInProgress = true;
                    lastSender = tboxSender;
                    e.Handled = true;                                       // the key has been processed
                    return;                                                 // exit the function
                }

                if ((key == Key.LeftShift) || (key == Key.RightShift))      // if a SHIFT key has been pressed
                {
                    if (HasOneModifier(tboxSender, e))                        // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "SHIFT + ")                  // and SHIFT was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        else                                                // if SHIFT wasn't pressed yet
                        tboxSender.Text += "SHIFT + ";                      // append "SHIFT + " 
                        bHotkeyInProgress = true;
                        lastSender = tboxSender;
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                    }
                    else                                                    // if no modifiers have been pressed yet
                    tboxSender.Text = "SHIFT + ";                           // replace the text in the textbox with "SHIFT + "
                    bHotkeyInProgress = true;
                    lastSender = tboxSender;
                    e.Handled = true;                                       // the key has been processed
                    return;                                                 // exit the function
                }
        }

        if (key != Key.LeftCtrl && key != Key.RightCtrl                     // if a non-modifer key was pressed
                && key != Key.LeftShift && key != Key.RightShift            // ...
                && key != Key.LeftAlt && key != Key.RightAlt                // ...
                && key != Key.Tab && key != Key.System                      // ...
                && bHotkeyInProgress)                                       // ...
            {
                FinishHotkey(tboxSender, key);
                e.Handled = true;
            }
        }

        // completes a hotkey command
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
            bHotkeyIsComplete = true;
        }

        // stores and registers a completed hotkey
        private void StoreHotkey(object sender, Key e) //, string lastKey)
        {
            KeyModifier modifiers = (KeyModifier)Keyboard.Modifiers;
            TextBox tboxSender = (sender as TextBox);
            int Id = GetControlId(tboxSender);
            string temp = tboxSender.Text;

            for (int i = 0; i < commands.Count; i++)
            {
                if (temp == commands[i].Hotkey && Id != commands[i].Id)     // if this command already exists
                {                                                           // ... swap them
                    // store data in temporary variables
                    string oldKeyStr = commands[Id - 1].Hotkey;
                    string oldName = commands[Id - 1].Name;
                    Key oldKey = globalHotkeys[Id - 1].Key;
                    KeyModifier oldKeyModifiers = globalHotkeys[Id - 1].Key_Modifiers;

                    // swap the data from this command and the old command
                    commands[Id - 1].Hotkey = commands[i].Hotkey;
                    commands[Id - 1].Name = commands[i].Name;
                    globalHotkeys[Id - 1].Key = e;
                    globalHotkeys[Id - 1].Key_Modifiers = modifiers;
                    globalHotkeys[Id - 1].Action = OnHotkeyPlaySound;

                    commands[i].Hotkey = oldKeyStr;
                    commands[i].Name = oldName;
                    globalHotkeys[i].Key = oldKey;
                    globalHotkeys[i].Key_Modifiers = oldKeyModifiers;
                    globalHotkeys[i].Action = OnHotkeyPlaySound;

                    // unregister the two old global hotkeys, then register with the newly stored data
                    globalHotkeys[i].Unregister();
                    globalHotkeys[Id - 1].Unregister();
                    globalHotkeys[i].Register();
                    globalHotkeys[Id - 1].Register();

                    // save all changes to the database
                    DataAccess.SaveGlobalHotkey(globalHotkeys[i]);
                    DataAccess.SaveCommand(commands[i]);
                    DataAccess.SaveGlobalHotkey(globalHotkeys[Id - 1]);
                    DataAccess.SaveCommand(commands[Id - 1]);

                    // find the old command's hotkey textbox and display the new hotkey
                    foreach (TextBox tBox in CommandGrid.Children.OfType<TextBox>().Where(tbox => tbox.Name == "Hotkey" + commands[i].Id.ToString()))
                    {
                        tBox.Text = commands[i].Hotkey;
                    }

                    return;                                         // return
                }
            }

            // store data
            globalHotkeys[Id - 1].Key = e;
            globalHotkeys[Id - 1].Key_Modifiers = modifiers;
            globalHotkeys[Id - 1].Action = OnHotkeyPlaySound;
            commands[Id - 1].Hotkey = tboxSender.Text;

            globalHotkeys[Id - 1].Unregister();                     // unregister old hotkey
            globalHotkeys[Id - 1].Register();                       // register the newly stored hotkey

            DataAccess.SaveGlobalHotkey(globalHotkeys[Id - 1]);     // save global hotkey to the database
            DataAccess.SaveCommand(commands[Id - 1]);               // save command info to the database
        }

        // returns true if the hotkey already has the maximum number of valid modifier keys
        private bool HasMaxModifiers(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            List<string> prefixes = new List<string>(new string[]
            {
                "CTRL + SHIFT + ",
                "CTRL + ALT + ",
                "SHIFT + CTRL + ",
                "SHIFT + ALT + ",
                "ALT + CTRL + ",
                "ALT + SHIFT + ",
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
        private void Name_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            // save content of Name textbox upon Enter key press
            if (e.Key == Key.Enter)
            {
                int Id = GetControlId(tboxSender);
                commands[Id - 1].Name = tboxSender.Text;
                DataAccess.SaveCommand(commands[Id - 1]);
            }
        }

        // defines behavior for when a file is dragged over a File textblock
        private void File_PreviewDragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;        // keeps track of when drops are enabled on the textblock
            string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];    // to hold filename(s) 
                                                                                            // (by design, only one file should be allowed to be dropped)
            foreach (string filename in filenames)
            {
                string temp = Path.GetExtension(filename).ToLowerInvariant();               // store the lowercase version of the filename extension
                if (temp.Equals(".mp3") || temp.Equals(".wav") || temp.Equals(".aac") || temp.Equals(".wma"))   // if it is a valid extension
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

        // defines behavior upon dropping a file on a File textblock (copies filename into File textblock, shortened name into Name textbox, and saves command)
        private void File_PreviewDrop(object sender, DragEventArgs e)
        {
            object filename = e.Data.GetData(DataFormats.FileDrop);                         // get the filename
            if (sender is TextBlock tblockSender)
            {
                tblockSender.Text = string.Format("{0}", ((string[])filename)[0]);                // insert filename into textblock

                // get id of command
                int Id = GetControlId(tblockSender);
                commands[Id - 1].File_Path = tblockSender.Text;                                   // put filename into command list

                string shortFileName = Path.GetFileName(tblockSender.Text);                       // get shortened filename
                if (shortFileName.Length > 24)                                              // if the shortened filename is too many characters
                    commands[Id - 1].Name = shortFileName.Remove(24, (shortFileName.Length - 24));  // cut off the rest of the excess characters and store in command list
                else 
                    commands[Id - 1].Name = shortFileName;                                  // otherwise just store the name in command list

                // find the Name textbox with the corresponding id
                foreach (TextBox txtboxControl in CommandGrid.Children.OfType<TextBox>().Where(tbox => tbox.Name == "Name" + Id.ToString()))
                {
                    txtboxControl.Text = commands[Id - 1].Name;                             // put the shortened filename into the Name textbox
                }

                DataAccess.SaveCommand(commands[Id - 1]);                                   // save this command (filename and name)
            }
        }

        // defines behavior when the left mouse button released on a File textblock (opens file dialog box, copies filename to File textblock, shortened name to Name textbox, and saves command)
        private void File_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBlock tblockSender = (sender as TextBlock);

            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog       // opens new file dialog
            {
                DefaultExt = ".mp3",                                                         // default extensions are .mp3 files
                Filter = "MP3 Files (*.mp3)|*.mp3|WAV Files (*.wav)|*.wav|AAC Files (*.aac)|*.aac|WMA Files (*.wma)|*.wma"  // other allowed file extensions
            };

            Nullable<bool> result = dialog.ShowDialog();                                     // keeps track of when dialog is shown

            if (result == true)
            {
                string filename = dialog.FileName;
                tblockSender.Text = filename;

                int Id = GetControlId(tblockSender);
                commands[Id - 1].File_Path = tblockSender.Text;                              // store filename in command list

                string shortFileName = dialog.SafeFileName;                                  // get shortened filename

                if (shortFileName.Length > 24)                                               // if shortened filename is too long
                    commands[Id - 1].Name = shortFileName.Remove(24);                       // cut off excess characters and store in command list
                else
                    commands[Id - 1].Name = shortFileName;                                   // otherwise, store whole name in command list

                foreach (TextBox txtboxControl in CommandGrid.Children.OfType<TextBox>().Where(tbox => tbox.Name == "Name" + Id.ToString()))
                {
                    txtboxControl.Text = commands[Id - 1].Name;                              // put shortened name into corresponding Name textbox
                }

                DataAccess.SaveCommand(commands[Id - 1]);                                    // save command (filename and name)
            }
        }

        // defines behavior upon mouse entering (and hovering) over File textblocks (shows full filename)
        private void File_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock tblockSender = (sender as TextBlock);

            if (tblockSender.Text != "")
            tblockSender.ToolTip = tblockSender.Text;
        }

        // defines behavior upon clicking a clear button (clears Name textboxes and File textblocks corresponding to the row of the clicked Clear button, then saves the cleared command)
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

        // defined behavior upon clicking the Clear All button (clears all Name textboxes and File textblock on the CommandGrid, then saves the cleared commands)
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

            DataAccess.SaveCommands(commands);
        }

        // dispose of global hotkeys upon closing window
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            for (int i = 0; i < globalHotkeys.Count; i++)
            {
                globalHotkeys[i].Dispose();
            }

            player.Close();
        }
    }




}
