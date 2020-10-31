using System.Windows;

namespace hotkey_soundboard
{
    /// <summary>
    /// Interaction logic for Rename.xaml
    /// 
    /// Contains some basic code to retrive user input from a textbox.
    /// </summary>
    
    public partial class RenameDialog : Window
    {
        public RenameDialog()
        {
            InitializeComponent();

            ResponseTextBox.Focus();
        }

        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
