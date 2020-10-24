using System;
using System.Collections.Generic;
using System.Text;

namespace soundboard_hotkeys
{
    public class CommandModel
    {
        public int Id { get; set; }
        public string Hotkey { get; set; }
        public string File_Path { get; set; }
        public string Name { get; set; }
        
    }
}
