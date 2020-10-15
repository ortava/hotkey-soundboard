using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace soundboard_hotkeys
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary> 
   
    public partial class MainWindow : Window
    {
        List<CommandModel> commands = new List<CommandModel>();

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

        /*
        public List<CommandModel> InitializeCommandList()
        {
            int CommandCount = GetCommandCount();
            List<CommandModel> cList = new List<CommandModel>();

            for (int i = 0; i < CommandCount; i++)
            {
                CommandModel c = new CommandModel();
                c.Id = (i + 1);
                cList.Add(c);
            }
            return cList;
        }
        */

        /*
        // return the number of commands located in MainWindow
        public int GetCommandCount()
        {
            int CommandCount = CommandGrid.RowDefinitions.Count - 1;   // get number of Commands (subtract 1 to remove header row from count)

            return CommandCount;   // return command count
        }
        */
    }


}
