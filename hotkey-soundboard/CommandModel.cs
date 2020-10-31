

namespace hotkey_soundboard
{
    /// <summary>
    /// Provides a model for command data.
    /// 
    /// These are string representations of commands.
    /// </summary>

    public class CommandModel
    {
        public int Id { get; set; }
        public string Hotkey { get; set; }
        public string File_Path { get; set; }
        public string Name { get; set; }
        
    }
}
