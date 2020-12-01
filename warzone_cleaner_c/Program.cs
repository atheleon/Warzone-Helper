using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.AccessControl;
using System.Diagnostics;


namespace warzone_cleaner_c
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Task application = new Task(closeApplication);
            Task files = new Task(cleanFiles);
            Task registry = new Task(cleanRegistry);
            Task guid = new Task(spoofGuid);

            Console.WriteLine("Warzone shadow ban helper");
            application.Start();
            application.Wait();
            files.Start();
            files.Wait();
            registry.Start();
            registry.Wait();
            guid.Start();
            guid.Wait();
            Console.WriteLine("\nPlease restart your system.");
            Console.WriteLine("After restarting, reinstall/repair battle.net, blizzard, warzone.");
            Console.WriteLine("Make sure to login to a fresh account- otherwise you will need to do this again.");
            Console.WriteLine("Everytime you start a new account after one has been banned,");
            Console.WriteLine("run this application again. Press any key to close.");
            Console.ReadKey();
        }

        private static void cleanFiles()
        {
            string gameDirectory = findGameDirectory();

            if(gameDirectory == null)
            {
                Console.WriteLine("No game directory found.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            string[] directoryPaths = { 
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Local\\Battle.net", 
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Local\\Blizzard Entertainment",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\AppData\\Roaming\\Battle.net",
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"\\Documents\\Call of Duty Modern Warfare",
                $"C:\\ProgramData\\Battle.net",
                $"C:\\ProgramData\\Blizzard Entertainment"
            };

            foreach(string directoryPath in directoryPaths)
            {
                DirectoryInfo directory = new DirectoryInfo(directoryPath);
                if (directory.Exists) directory.Delete(true);
            }

            string[] filePaths = {
                gameDirectory + $"\\main\\data0.dcache",
                gameDirectory + $"\\main\\data1.dcache",
                gameDirectory + $"\\main\\toc0.dcache",
                gameDirectory + $"\\main\\toc1.dcache",
                gameDirectory + $"\\Data\\data\\shmem",
                gameDirectory + $"\\main\\recipes\\cmr_hist"
            };

            foreach (string filePath in filePaths)
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }

            Console.WriteLine("Cleaned files.");
        }

        private static void cleanRegistry()
        {
            string WOW6432Node = @"SOFTWARE\WOW6432Node";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(WOW6432Node, true)) if (key != null) foreach (string subkey in key.GetSubKeyNames()) if (subkey == "Blizzard Entertainment") key.DeleteSubKeyTree(subkey);

            string Blizzard = @"Software\Blizzard Entertainment";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(Blizzard, true)) if (key != null) foreach (string subkey in key.GetSubKeyNames()) if (subkey == "Battle.net") key.DeleteSubKeyTree(subkey);

            Console.WriteLine("Cleaned registry.");
        }

        private static void spoofGuid()
        {
            Guid guid = Guid.NewGuid();

            string GUID = @"SYSTEM\CurrentControlSet\Control\IDConfigDB\Hardware Profiles\0001";
            RegistryKey subkey = Registry.LocalMachine.OpenSubKey(GUID, true);
            if (subkey != null)
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

            Console.WriteLine("GUID spoofed to " + guid);
        }

        private static string findGameDirectory()
        {
            RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default);
            foreach (string subKey in baseKey.GetSubKeyNames())
            {
                if(subKey.Contains("S-1-5-21-") && !subKey.Contains("Classes"))
                {
                    RegistryKey games = baseKey.OpenSubKey(subKey + @"\System\GameConfigStore\Children");
                    foreach(string game in games.GetSubKeyNames())
                    {
                        RegistryKey temp = games.OpenSubKey(game);
                        if(Equals(temp.GetValue("TitleId"), "1787008472"))
                        {
                            string ExePath = temp.GetValue("MatchedExeFullPath").ToString();
                            return ExePath.Substring(0, ExePath.LastIndexOf("\\"));
                        }
                    }
                }
            }
            return null;
        }

        private static void closeApplication()
        {
            string[] processNames = { "Agent", "Battle.net" };

            foreach(string processName in processNames)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                foreach(Process process in processes) process.Kill();
            }
            Console.WriteLine("Closed Battle.net applications.");
        }
    }
}
