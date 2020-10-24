using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace soundboard_hotkeys
{
    public class DataAccess
    {
        /// CommandModel Data ///

        // returns a list of objects containing hotkey command data from the command table in ProfileDB
        public static List<CommandModel> LoadCommands()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<CommandModel>("SELECT * FROM command", new DynamicParameters());
                return output.ToList();
            }
        }

        // save all command data from a list to command table in the database
        public static void SaveCommands(List<CommandModel> commands)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                for (int i = 0; i < commands.Count; i++)
                {
                    // update row where id is the same as commands[i].Id
                    cnn.Execute("UPDATE command SET (hotkey, name, file_path) = (@Hotkey, @Name, @File_Path) WHERE id = @Id", commands[i]);
                }
            }
        }

        // save all command data from a singular command to the related row in the command table in the database
        public static void SaveCommand(CommandModel command)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("Update command SET (hotkey, name, file_path) = (@Hotkey, @Name, @File_Path) WHERE id = @Id", command);
            }
        }

        /// GlobalHotkey Data ///

        // returns a list of objects containing global hotkey data from the global_hotkey table in ProfileDB
        public static List<GlobalHotkey> LoadGlobalHotkeys()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<GlobalHotkey>("SELECT * FROM global_hotkey", new DynamicParameters());
                return output.ToList();
            }
        }

        // save all global hotkey data from a list to global_hotkey table in the database
        public static void SaveGlobalHotkeys(List<GlobalHotkey> globalHotkeys)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                for (int i = 0; i < globalHotkeys.Count; i++)
                {
                    // update row where id is the same as globalHotkeys[i].Id
                    cnn.Execute("UPDATE global_hotkey SET (virtual_key_code, modifiers) = (@Virtual_Key_Code, @Modifiers) WHERE id = @Id", globalHotkeys[i]);
                }
            }
        }

        // save all global hotkey data from a singular global hotkey to the related row in the global_hotkey table in the database
        public static void SaveGlobalHotkey(GlobalHotkey globalHotkey)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("Update global_hotkey SET (virtual_key_code, modifiers) = (@Virtual_Key_Code, @Modifiers) WHERE id = @Id", globalHotkey);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////

        // returns the connection string required to access database
        private static string LoadConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }
    }
}
