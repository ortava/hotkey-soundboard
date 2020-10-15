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


        // returns the connection string required to access database
        private static string LoadConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }
    }
}
