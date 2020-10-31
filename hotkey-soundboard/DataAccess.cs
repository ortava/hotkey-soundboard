using Dapper;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace hotkey_soundboard
{
    /// <summary>
    /// Creates connections to the database and executes SQL statements to store and retrieve
    /// data from a database.
    /// 
    /// This file works with the ProfileDB database. It contains data for profiles in the 
    /// profile table, data for string representations of hotkey commands in the 
    /// command table, and data for virtually storing hotkey commands in the 
    /// global_hotkey table.
    /// </summary>

    public class DataAccess
    {
/// ###############################
/// COMMAND DATA
/// ###############################

        // returns a list of objects containing hotkey command data from the command table in ProfileDB
        public static List<CommandModel> LoadCommands(ProfileModel profile)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<CommandModel>("SELECT * FROM command WHERE profile_id = @Id", profile);
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
                cnn.Execute("UPDATE command SET (hotkey, name, file_path) = (@Hotkey, @Name, @File_Path) WHERE id = @Id", command);
            }
        }

/// ###############################
/// GLOBALHOTKEY DATA
/// ###############################

        // returns a list of objects containing global hotkey data from the global_hotkey table in ProfileDB
        public static List<GlobalHotkey> LoadGlobalHotkeys(ProfileModel profile)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<GlobalHotkey>("SELECT * FROM global_hotkey WHERE profile_id = @Id", profile);
                return output.ToList();
            }
        }

        // save all global hotkey data from a singular global hotkey to the related row in the global_hotkey table in the database
        public static void SaveGlobalHotkey(GlobalHotkey globalHotkey)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("UPDATE global_hotkey SET (virtual_key_code, modifiers) = (@Virtual_Key_Code, @Modifiers) WHERE id = @Id", globalHotkey);
            }
        }

/// ###############################
/// PROFILE DATA
/// ###############################

        // save all profile data from a singular profile to the related row in the command table in the database
        public static void SaveProfile(ProfileModel profile)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("Update profile SET (name) = (@Name) WHERE id = @Id", profile);
            }
        }

        // inserts a new profile, including all of its related data (commands and global hotkeys), into the database
        public static void InsertProfile(string profileName, List<CommandModel> commands, List<GlobalHotkey> globalHotkeys)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                const int numberOfHotkeys = 36;
                ProfileModel newProfile = new ProfileModel();
                newProfile.Name = profileName;

                cnn.Execute("INSERT INTO profile (name) VALUES (@Name)", newProfile);

                newProfile = cnn.Query<ProfileModel>("SELECT * FROM profile ORDER BY id DESC LIMIT 1", new DynamicParameters()).First();

                int ProfileID = newProfile.Id;
                for (int i = 0; i < numberOfHotkeys; i++)
                {
                    cnn.Execute($"INSERT INTO command (hotkey, profile_id) VALUES (@Hotkey, {ProfileID})", commands[i]);
                    cnn.Execute($"INSERT INTO global_hotkey (virtual_key_code, modifiers, profile_id) VALUES (@Virtual_Key_Code, @Modifiers, {ProfileID})", globalHotkeys[i]);
                }
            }
        }

        // returns a list of objects containing all profile data from the profile table in ProfileDB
        public static List<ProfileModel> LoadProfiles()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                var output = cnn.Query<ProfileModel>("SELECT * FROM profile", new DynamicParameters());
                return output.ToList();
            }
        }
        
        // returns an object containing profile data for the most recently created profile
        public static ProfileModel LoadNewProfile()
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                // gets profile with the highest ID
                ProfileModel output = cnn.Query<ProfileModel>("SELECT * FROM profile ORDER BY id DESC LIMIT 1", new DynamicParameters()).First();
                return output;
            }
        }

        // deletes a profile and all of its related data (commands/global hotkeys) from the database.
        public static void DeleteProfile(ProfileModel profile)
        {
            using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
            {
                cnn.Execute("DELETE FROM profile WHERE id = @Id", profile);
                cnn.Execute("DELETE FROM command WHERE profile_id = @Id", profile);
                cnn.Execute("DELETE FROM global_hotkey WHERE profile_id = @Id", profile);
            }
        }

/// ###############################
/// LOAD CONNECTION STRING TO DATABASE
/// ###############################

        // returns the connection string required to access database
        private static string LoadConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["Default"].ConnectionString;
        }
    }
}
