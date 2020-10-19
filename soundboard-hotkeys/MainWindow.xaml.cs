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
using System.Windows.Threading; // maybe remove

namespace soundboard_hotkeys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary> 
   
    public partial class MainWindow : Window
    {
        List<CommandModel> commands = new List<CommandModel>();
        bool bHotkeyIsComplete = false;

        public MainWindow()
        {
            InitializeComponent();

            LoadCommandControls();
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

            char charId = tboxSender.Name[tboxSender.Name.Length - 1];  // get the last character in the control's name ("Hotkey1" gives char='1', "Hotkey2" gives char='2', etc...)
            int Id = int.Parse(charId.ToString());                      // convert the character to an int so we can correctly identify the related command

            if (tboxSender.Text == "")                                  // if there is no hotkey in the textbox
                tboxSender.Text = commands[Id - 1].Hotkey;              // insert the most recently saved valid hotkey into the textbox

            bHotkeyIsComplete = false;                                  // the hotkey textbox has lost focus, so the user may have moved on to another hotkey
        }

        // defines behavior when the user's key is released on a Hotkey textbox
        private void Hotkey_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            char charId = tboxSender.Name[tboxSender.Name.Length - 1];  // get the last character in the control's name ("Hotkey1" gives char='1', "Hotkey2" gives char='2', etc...) 
            int Id = int.Parse(charId.ToString());                      // convert the character to an int so we can correctly identify the related command

            /*
              || tboxSender.Text == "SHIFT + " || tboxSender.Text "CTRL + "
              || tboxSender.Text == "TAB + " || tboxSender.Text == "ALT + "
            */

            if (HasMaxPrefixes(sender, e) || tboxSender.Text == "")     // if the hotkey textbox is not complete
            {
                tboxSender.Text = commands[Id - 1].Hotkey;              // insert the most recently saved valid hotkey
                bHotkeyIsComplete = false;                              // hotkey is not complete
            }
        }

        // defines behavoir when the user's key is pressed down on a Hotkey textbox
        private void Hotkey_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            // if this key is an invalid for a hotkey
            if (e.Key == Key.LWin || e.Key == Key.RWin          
                || e.Key == Key.CapsLock || e.Key == Key.Escape 
                || e.Key == Key.Back || e.Key == Key.Delete 
                || e.Key == Key.Space || e.Key == Key.System)   // ALT gets recognized as System
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
            }
            
            // if the hotkey isn't full of modifier keys (CTRL, ALT, SHIFT, TAB, etc.) yet, we can add another
            if (!HasMaxPrefixes(sender, e))
            {
                if ((e.Key == Key.LeftCtrl) || (e.Key == Key.RightCtrl))    // if a CTRL key has been pressed
                {
                    if (HasOnePrefix(sender, e))                            // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "CTRL + ")                   // and CTRL was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        else                                                // if CTRL wasn't pressed yet
                        tboxSender.Text += "CTRL + ";                       // append "CTRL + " 
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                    }
                    else                                                    // if no modifiers have been pressed yet
                        tboxSender.Text = "CTRL + ";                        // replace the text in the textbox with "CTRL + "
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                }

                /// THE ALT KEY IS BEING RECOGNIZED AS Key.System, THUS THE BELOW IF STATEMENT IS INOPERATIVE ///
                if ((e.Key == Key.LeftAlt) || (e.Key == Key.RightAlt))      // if an ALT key has been pressed
                {
                    if (HasOnePrefix(sender, e))                            // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "ALT + ")                    // and ALT was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        else                                                // if ALT wasn't pressed yet
                        tboxSender.Text += "ALT + ";                        // append "ALT + " 
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                    }
                    else                                                    // if no modifiers have been pressed yet
                        tboxSender.Text = "ALT + ";                         // replace the text in the textbox with "ALT + "
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                }
                /////////////////////////////////////////////////////////////////////////////////////////////

                if ((e.Key == Key.LeftShift) || (e.Key == Key.RightShift))  // if a SHIFT key has been pressed
                {
                    if (HasOnePrefix(sender, e))                            // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "SHIFT + ")                  // and SHIFT was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        else                                                // if SHIFT wasn't pressed yet
                        tboxSender.Text += "SHIFT + ";                      // append "SHIFT + " 
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                    }
                    else                                                    // if no modifiers have been pressed yet
                        tboxSender.Text = "SHIFT + ";                       // replace the text in the textbox with "SHIFT + "
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                }

                if (e.Key == Key.Tab)                                       // if a TAB key has been pressed
                {
                    if (HasOnePrefix(sender, e))                            // if there's only one modifier in the hotkey
                    {
                        if (tboxSender.Text == "TAB + ")                    // and TAB was already pressed
                        {
                            tboxSender.Text = "";                           // reset the hotkey because we can't have the same modifer key twice
                            e.Handled = true;                               // the key has been processed
                            return;                                         // exit the function
                        }
                        else                                                // if TAB wasn't pressed yet
                        tboxSender.Text += "TAB + ";                        // append "TAB + " 
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                    }
                    else                                                    // if no modifiers have been pressed yet
                        tboxSender.Text = "TAB + ";                         // replace the text in the textbox with "TAB + "
                        e.Handled = true;                                   // the key has been processed
                        return;                                             // exit the function
                }
            }

            string lastKey;                                                 // string to hold the text for the last key in the hotkey
            if (tboxSender.Text == "")                                      // if the text is blank (this handles non-modifer keys being pressed first)
            {
                e.Handled = true;                                           // the key has been processed
                return;                                                     // exit the function
            }
            else if (e.Key != Key.LeftCtrl && e.Key != Key.RightCtrl        // if a non-modifer key was pressed
                && e.Key != Key.LeftShift && e.Key != Key.RightShift        // ...
                && e.Key != Key.LeftAlt && e.Key != Key.RightAlt            // ...
                && e.Key != Key.Tab && e.Key != Key.System                  // ...
                && (HasOnePrefix(sender, e) || HasMaxPrefixes(sender, e)))  // and the hotkey already has at least one modifier
            {
                switch (e.Key)                                              // begin checking which key was was pressed
                {                                                           // assign the appropriate character to the given key
                    case Key.D0:
                        lastKey = "0";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.D1:
                        lastKey = "1";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.D2:
                        lastKey = "2";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.D3:
                        lastKey = "3";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.D4:
                        lastKey = "4";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.D5:
                        lastKey = "5";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.D6:
                        lastKey = "6";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.D7:
                        lastKey = "7";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.D8:
                        lastKey = "8";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.D9:
                        lastKey = "9";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.OemMinus:
                        lastKey = "-";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.OemPlus:
                        lastKey = "=";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.OemComma:
                        lastKey = ",";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.OemPeriod:
                        lastKey = ".";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.OemQuestion:
                        lastKey = "/";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.Oem1:
                        lastKey = ";";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.OemQuotes:
                        lastKey = "'";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.OemOpenBrackets:
                        lastKey = "[";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.Oem6:
                        lastKey = "]";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    case Key.Oem5:
                        lastKey = "'\'";
                        FinishHotkey(sender, e, lastKey);
                        break;
                    default:                                // default is a non-special key, like the alphabetical keys
                        lastKey = e.Key.ToString();
                        FinishHotkey(sender, e, lastKey);
                        break;
                }
            }
        }

        // returns true if the hotkey already has the maximum number of valid modifier keys
        private bool HasMaxPrefixes(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            List<string> prefixes = new List<string>(new string[]
            {
                "CTRL + SHIFT + ",
                "CTRL + ALT + ",
                "CTRL + TAB + ",
                "SHIFT + CTRL + ",
                "SHIFT + ALT + ",
                "SHIFT + TAB + ",
                "ALT + CTRL + ",
                "ALT + SHIFT + ",
                "ALT + TAB + ",
                "TAB + CTRL + ",
                "TAB + SHIFT + ",
                "TAB + ALT + ",
            });
            
            for (int i = 0; i < prefixes.Count; i++)
            {
                if (tboxSender.Text == prefixes[i])
                return true;
            }
            
            return false;
        }

        // returns true if the hotkey has only one valid modifier key
        private bool HasOnePrefix(object sender, KeyEventArgs e)
        {
            TextBox tboxSender = (sender as TextBox);

            List<string> initialPrefixes = new List<string>(new string[]
            {
                "CTRL + ",
                "SHIFT + ",
                "ALT + ",
                "TAB + ",
            });

            for (int i = 0; i < initialPrefixes.Count; i++)
            {
                if (tboxSender.Text == initialPrefixes[i])
                    return true;
            }

            return false;
        }

        // appends the last key on the hotkey
        private void FinishHotkey(object sender, KeyEventArgs e, string lastKey)
        {
            TextBox tboxSender = (sender as TextBox);

            string temp = tboxSender.Text += lastKey;
            char charId = tboxSender.Name[tboxSender.Name.Length - 1];
            int Id = int.Parse(charId.ToString());

            if (HotkeyExists(temp))
            {
                tboxSender.Text = commands[Id - 1].Hotkey;
                bHotkeyIsComplete = false;
                e.Handled = true;
                return;
            }
            else
                tboxSender.Text = temp;
                bHotkeyIsComplete = true;
                e.Handled = true;
            
                commands[Id - 1].Hotkey = tboxSender.Text;
                DataAccess.SaveCommand(commands[Id - 1]);
        }

        // checks if the hotkey already exists (can't have two of the same hotkey)
        private bool HotkeyExists(string hotkey)
        {
            for (int i = 0; i < commands.Count; i++)
            {
                if (hotkey == commands[i].Hotkey)
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
                char charId = tboxSender.Name[tboxSender.Name.Length - 1];
                int Id = int.Parse(charId.ToString());
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
            if (sender is TextBlock tblock)
            {
                tblock.Text = string.Format("{0}", ((string[])filename)[0]);                // insert filename into textblock

                // get id of command
                char charId = tblock.Name[tblock.Name.Length - 1];
                int Id = int.Parse(charId.ToString());
                commands[Id - 1].File_Path = tblock.Text;                                   // put filename into command list

                string shortFileName = Path.GetFileName(tblock.Text);                       // get shortened filename
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

                char charId = tblockSender.Name[tblockSender.Name.Length - 1];
                int Id = int.Parse(charId.ToString());
                commands[Id - 1].File_Path = tblockSender.Text;                              // store filename in command list

                string shortFileName = dialog.SafeFileName;                                  // get shortened filename

                if (shortFileName.Length > 24)                                               // if shortened filename is too long
                    commands[Id - 1].Name = shortFileName.Remove(24, shortFileName.Length);  // cut off excess characters and store in command list
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

            char charId = btnSender.Name[btnSender.Name.Length - 1];
            int Id = int.Parse(charId.ToString());
            int commandIndex = Id - 1;            

            foreach (TextBox txtboxControl in CommandGrid.Children.OfType<TextBox>().Where(tbox => tbox.Name == "Name" + Id.ToString()))
            {
                txtboxControl.Text = "";
                commands[commandIndex].Name = txtboxControl.Text;
            }

            foreach (TextBlock txtblockControl in CommandGrid.Children.OfType<TextBlock>().Where(tblock => tblock.Name == "File" + Id.ToString()))
            {
                txtblockControl.Text = "";
                commands[commandIndex].File_Path = txtblockControl.Text;
            }

            DataAccess.SaveCommand(commands[commandIndex]);
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
    }




}
