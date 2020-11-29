using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Security.AccessControl;


namespace warzone_cleaner_c
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Warzone shadow ban helper");
            Thread.Sleep(1000);
            Console.WriteLine("Begining filetype cleanup...");
            Thread.Sleep(1000);
            cleanFiles();
            Console.WriteLine("Files have been cleaned... Working on registry values.");
            Thread.Sleep(1000);
            cleanRegistry();
            Thread.Sleep(1000);
            Console.Clear();
            Console.WriteLine("Please locate your game files and delete the following directories:");
            Console.WriteLine("Call of Duty Modern Warfare\\Data\\data");
            Console.WriteLine("Call of Duty Modern Warfare\\main\\recipes");
            Console.WriteLine("Hit any key once you have completed this step...");
            Console.ReadKey();
            Console.Clear();
            Console.WriteLine("Attempting to change your GUID...");
            spoofGuid();
            Thread.Sleep(5000);
            Console.WriteLine("Device GUID changed.");
            Console.Clear();
            Console.WriteLine("Please restart your system.");
            Thread.Sleep(1000);
            Console.WriteLine("After restarting, reinstall/repair battle.net, blizzard, warzone.");
            Thread.Sleep(1000);
            Console.WriteLine("Make sure to login to a fresh account- otherwise you will need to do this again.");
            Thread.Sleep(1000);
            Console.WriteLine("Everytime you start a new account after one has been banned,");
            Console.WriteLine("run this application again. Press any key to close.");
            Console.ReadKey();
        }

        private static void cleanFiles()
        {
            string battleDirectoryOne = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Local\\Battle.net"; //%localappdata%
            string battleDirectoryTwo = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Local\\Blizzard Entertainment"; //%localappdata%
            string battleDirectoryThree = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Roaming\\Battle.net"; //%appdata%
            string battleDirectoryFour = $"C:\\ProgramData\\Battle.net";
            string battleDirectoryFive = $"C:\\ProgramData\\Blizzard Entertainment";
            string battleDirectorySix = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\Documents\\Call of Duty Modern Warfare"; //userprofile documents

            DirectoryInfo directory1 = new DirectoryInfo(battleDirectoryOne); //use DirectoryInfo to delete both directories/files in one
            if (directory1.Exists) directory1.Delete(true);

            DirectoryInfo directory2 = new DirectoryInfo(battleDirectoryTwo);
            if (directory2.Exists) directory2.Delete(true);

            DirectoryInfo directory3 = new DirectoryInfo(battleDirectoryThree);
            if (directory3.Exists) directory3.Delete(true);

            DirectoryInfo directory4 = new DirectoryInfo(battleDirectoryFour);
            if (directory4.Exists) directory4.Delete(true);

            DirectoryInfo directory5 = new DirectoryInfo(battleDirectoryFive);
            if (directory5.Exists) directory5.Delete(true);

            DirectoryInfo directory6 = new DirectoryInfo(battleDirectorySix);
            if (directory6.Exists) directory6.Delete(true);
        }

        private static void cleanRegistry()
        {
            string WOW6432Node = @"SOFTWARE\WOW6432Node";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WOW6432Node, true)) if (key != null) foreach (string subkey in key.GetSubKeyNames()) if (subkey == "Blizzard Entertainment") key.DeleteSubKeyTree(subkey);

            string Blizzard = @"Software\Blizzard Entertainment";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Blizzard, true)) if (key != null) foreach (string subkey in key.GetSubKeyNames()) if (subkey == "Battle.net") key.DeleteSubKeyTree(subkey);
        }

        private static void spoofGuid()
        {
            Guid guid = Guid.NewGuid();

            string GUID = @"SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001";
            RegistryKey subkey = Registry.LocalMachine.OpenSubKey(GUID, true);
            if(subkey != null)
            {
                string user = Environment.UserDomainName + "\\" + Environment.UserName;
                RegistrySecurity rs = new RegistrySecurity();
                rs.AddAccessRule(new RegistryAccessRule(user,
                    RegistryRights.FullControl,
                    InheritanceFlags.None,
                    PropagationFlags.None,
                    AccessControlType.Allow));
                subkey.SetAccessControl(rs);
                subkey.SetValue("HwProfileGuid", "{" + guid + "}", RegistryValueKind.String);
                subkey.Close();
            }

            Console.WriteLine("GUID changed to " + guid);
        }
    }
}

//HKEY_CURRENT_USER\Software\Blizzard Entertainment\Battle.net\