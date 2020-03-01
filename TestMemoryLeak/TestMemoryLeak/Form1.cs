using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestMemoryLeak
{
    public partial class Form1 : Form
    {
        private BackgroundWorker workerDetectBattery;
        private BackgroundWorker _worker;
        public Form1()
        {
            InitializeComponent();

            workerDetectBattery = new BackgroundWorker();
            workerDetectBattery.DoWork += new DoWorkEventHandler(DoWorkDetectBattery);
            workerDetectBattery.RunWorkerAsync();

            this._worker = new BackgroundWorker();
            this._worker.DoWork += this.DoWork1;
            this._worker.RunWorkerAsync();

        }
        public static byte[] ReadFile(string pathFile)
        {
            byte[] documentBytes = null;
            if (File.Exists(pathFile))
            {
                documentBytes = File.ReadAllBytes(pathFile);
            }
            return documentBytes;
        }
        private void DoWork1(object sender, DoWorkEventArgs e)
        {
            var projectPath = System.Reflection.Assembly.GetEntryAssembly().Location;
            string filePath = Path.Combine(Path.GetDirectoryName(projectPath), "Resources");
            byte[] data = ReadFile(Path.Combine(filePath, ""));
            using (var reader = new MemoryStream(data))
            {
                reader.Position = 0;
            }
        }
        private void DoWorkDetectBattery(object sender, DoWorkEventArgs e)
        {
            var query = new WqlEventQuery();
            var scope = new ManagementScope("root\\CIMV2");
            ManagementEventWatcher managementEventWatcher;
            query.EventClassName = "Win32_PowerManagementEvent";
            managementEventWatcher = new ManagementEventWatcher(scope, query);
            managementEventWatcher.EventArrived += new EventArrivedEventHandler(SystemEvents_PowerModeChanged);
            managementEventWatcher.Start();
        }

        private void SystemEvents_PowerModeChanged(object sender, EventArrivedEventArgs e)
        {
            string batteryStatus = SystemInformation.PowerStatus.PowerLineStatus.ToString();
            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
            {

            }
        }

        public static void GetAppUninstall()
        {
            //WriteToFile("GetAppUninstall");
            List<string> listAppFromUninstallFolder = new List<string>();
            List<string> listAppFromRegistry = new List<string>();

            string appFude = SearchAppInRegistry("test");
            string pathYayoikaikei = SearchAppInRegistry("test");


            if (!string.IsNullOrEmpty(appFude))
            {
                listAppFromUninstallFolder.Add(appFude);
            }

            if (!string.IsNullOrEmpty(pathYayoikaikei))
            {
                listAppFromUninstallFolder.Add(pathYayoikaikei);
            }

            List<string> listApp = GetListApplicationInRegistryInstall();
            foreach (var item in listApp)
            {
                listAppFromRegistry.Add(item);
            }
            var listAppUninstall = listAppFromRegistry.Except(listAppFromUninstallFolder).ToList();
            var listAppInstall = listAppFromUninstallFolder.Except(listAppFromRegistry).ToList();
            if (listAppUninstall.Count > 0)
            {
                foreach (var item in listAppUninstall)
                {
                    DeleteKey(item);

                    //show dialog
                    //string procPath = @"D:\IO-DATA\97_SourceCode\Kee@p\Kee@p\bin\Debug\Kee@p.exe";
                    string procPath = SearchAppInstallPath("");

                    string screenName = "DL003Notification.xaml," + item;
                    string agrument = screenName.Replace(" ", "");
                    ProcessStarter proc = new ProcessStarter(procPath, agrument);
                    List<int> userSession = proc.GetAllSessionData();
                    proc.RunForAllUser(userSession);
                }

            }
            if (listAppInstall.Count > 0)
            {
                foreach (var item in listAppInstall)
                {
                    AddKey(item);
                    //show dialog
                    //string procPath = @"D:\IO-DATA\97_SourceCode\Kee@p\Kee@p\bin\Debug\Kee@p.exe";
                    string procPath = SearchAppInstallPath("");

                    string screenName = "DL002Notification.xaml," + item;
                    string agrument = screenName.Replace(" ", "");
                    ProcessStarter proc = new ProcessStarter(procPath, agrument);
                    List<int> userSession = proc.GetAllSessionData();
                    proc.RunForAllUser(userSession);
                }
            }
        }
        public static void AddKey(string item)
        {
            //WriteToFile("Add key");
            string currentUserSID = ReadRegistry.GetLoggedOnUserSID();
            string appkeyPath = currentUserSID + "";
            RegistryKey appkey = Registry.Users.OpenSubKey(appkeyPath);
            if (appkey != null)
            {
                string subkeyPath = currentUserSID + "" + "";
                if (string.IsNullOrEmpty(item))
                {
                    RegistryKey key = Registry.Users.CreateSubKey(subkeyPath, true);

                }
                if (!string.IsNullOrEmpty(item))
                {
                    Registry.Users.CreateSubKey(currentUserSID + "" + "" + @"\" + item, true);
                }
                appkey.Close();
            }
        }
        public static string SearchAppInRegistry(string appName)
        {
            string displayname = "";
            //RegistryKey appBackupUninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", false);

            if (Environment.Is64BitOperatingSystem)
            {
                RegistryKey rb64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey subKeyRegistry = rb64.OpenSubKey("");
                int countSubkey = subKeyRegistry.SubKeyCount;
                string[] listItem = new string[countSubkey];
                listItem = subKeyRegistry.GetSubKeyNames();
                foreach (var item in listItem)
                {
                    if (item != null)
                    {
                        if (rb64.OpenSubKey("" + @"\" + item) != null && rb64.OpenSubKey("" + @"\" + item).GetValue("DisplayName") != null)
                        {
                            if (rb64.OpenSubKey("" + @"\" + item).GetValue("DisplayName").ToString().Contains(appName))
                            {
                                displayname = rb64.OpenSubKey("" + @"\" + item).GetValue("DisplayName").ToString();
                            }
                        }
                    }
                }
            }
            else
            {
                //appBackupUninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", false);
                RegistryKey rb32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey subKeyRegistry = rb32.OpenSubKey("");
                int countSubkey = subKeyRegistry.SubKeyCount;
                string[] listItem = new string[countSubkey];
                listItem = subKeyRegistry.GetSubKeyNames();
                foreach (var item in listItem)
                {
                    if (item != null)
                    {
                        if (rb32.OpenSubKey("" + @"\" + item) != null && rb32.OpenSubKey("" + @"\" + item).GetValue("DisplayName") != null)
                        {
                            if (rb32.OpenSubKey("" + @"\" + item).GetValue("DisplayName").ToString().Contains(appName))
                            {
                                displayname = rb32.OpenSubKey("" + @"\" + item).GetValue("DisplayName").ToString();
                            }
                        }
                    }
                }
            }

            return displayname;
        }

        public static string GetVolumeBackupByGUID(string guid)
        {
            String volume = "";
            ManagementObjectSearcher ms = new ManagementObjectSearcher("Select * from Win32_Volume where ( DriveType=2 OR DriveType=3 ) AND FileSystem='NTFS'");
            foreach (ManagementObject mo in ms.Get())
            {
                string deviceID = mo["DeviceID"].ToString();
                if (deviceID.Equals(guid))
                {
                    string driveLetter = mo["DriveLetter"].ToString();
                    UInt32 driveType = (UInt32)mo["DriveType"];
                    string volumeLabel = mo["Label"] != null ? mo["Label"].ToString() : "";
                    UInt64 capacity = (UInt64)mo["Capacity"];
                    UInt64 freeSpace = (UInt64)mo["FreeSpace"];
                    string name = mo["Name"].ToString();
                    string Caption = mo["Caption"].ToString();
                    volume = "";
                    break;
                }
            }
            return volume;
        }
        public static List<string> GetLocalUserAccounts()
        {
            List<string> lstAcount = new List<string>();
            SelectQuery query = new SelectQuery("SELECT * FROM Win32_UserProfile WHERE Loaded = True");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject sid in searcher.Get())
            {
                if (sid.GetPropertyValue("Special").Equals(false))
                {
                    if (!(new SecurityIdentifier(sid["SID"].ToString()).Translate(typeof(NTAccount)).ToString()).Contains("NT SERVICE"))
                    {
                        lstAcount.Add(new SecurityIdentifier(sid["SID"].ToString()).Translate(typeof(NTAccount)).ToString());
                        //WriteToFile("List user SID: " + new SecurityIdentifier(sid["SID"].ToString()).Translate(typeof(NTAccount)).ToString());
                        //WriteToFile("List user SID: " + new SecurityIdentifier(sid["SID"].ToString()));
                        //WriteToFile("List user Special: " + sid["Special"]);
                    }
                }
            }

            return lstAcount;
        }
        public void CheckAppInstalled(object sender, DoWorkEventArgs e)
        {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM RegistryKeyChangeEvent WHERE " +
               "Hive = 'HKEY_LOCAL_MACHINE'" +
              @"AND KeyPath = 'SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall'");


            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(CheckApp);
            insertWatcher.Start();
        }
        private void CheckApp(object sender, EventArrivedEventArgs e)
        {

        }
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_Volume' AND ( TargetInstance.DriveType=2 OR TargetInstance.DriveType=3 ) AND TargetInstance.FileSystem='NTFS'");

            ManagementEventWatcher insertWatcher = new ManagementEventWatcher(insertQuery);
            insertWatcher.EventArrived += new EventArrivedEventHandler(DeviceInsertedEvent);
            insertWatcher.Start();

            WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_Volume' AND ( TargetInstance.DriveType=2 OR TargetInstance.DriveType=3 ) AND TargetInstance.FileSystem='NTFS'");
            ManagementEventWatcher removeWatcher = new ManagementEventWatcher(removeQuery);
            removeWatcher.EventArrived += new EventArrivedEventHandler(DeviceRemovedEvent);
            removeWatcher.Start();
        }

        private void DeviceInsertedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                Console.WriteLine(property.Name + " = " + property.Value);
                if (property.Name.Equals("DeviceID"))
                {
                    foreach (var userSID in GetLocalUserAccounts())
                    {
                       
                    }
                }
            }
        }

        private void DeviceRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            foreach (var property in instance.Properties)
            {
                Console.WriteLine(property.Name + " = " + property.Value);
                if (property.Name.Equals("DeviceID"))
                {

                }
            }
        }

        public static List<ManagementObject> GetLocalUserPath()
        {
            List<ManagementObject> lstAcount = new List<ManagementObject>();
            SelectQuery query = new SelectQuery("SELECT * FROM Win32_UserProfile WHERE Loaded = True");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject sid in searcher.Get())
            {
                if (sid.GetPropertyValue("Special").Equals(false))
                {
                    if (!(new SecurityIdentifier(sid["SID"].ToString()).Translate(typeof(NTAccount)).ToString()).Contains("NT SERVICE"))
                    {
                        lstAcount.Add(sid);
                    }
                }
            }

            return lstAcount;
        }
        public static List<string> GetListApplicationInRegistryInstall()
        {
            List<string> lApp = new List<string>();
            string currentUserSID = ReadRegistry.GetLoggedOnUserSID();
            string appkeyPath = currentUserSID + "";
            RegistryKey appkey = Registry.Users.OpenSubKey(appkeyPath);
            {
                if (appkey != null)
                {
                    string subkeyPath = currentUserSID + "";
                    RegistryKey key = Registry.Users.CreateSubKey(subkeyPath, true);
                    string[] appBackup = key.GetSubKeyNames();
                    foreach (var itemApp in appBackup)
                    {
                        lApp.Add(itemApp);
                    }
                    key.Close();
                    appkey.Close();
                    return lApp;
                }
            }
            return null;
        }
        public static void DeleteKey(string item)
        {
            //WriteToFile("Delete" + item);

            if (!string.IsNullOrEmpty(item))
            {
                string currentUserSID = ReadRegistry.GetLoggedOnUserSID();
                string subkeyPath = currentUserSID + "" + "";
                string subkeyPathListApp = currentUserSID + "" + "";
                RegistryKey key = Registry.Users.CreateSubKey(subkeyPath, true);
                RegistryKey keyListApp = Registry.Users.CreateSubKey(subkeyPathListApp, true);
                //Delete key in ListAppInstalled
                foreach (var subkey in key.GetSubKeyNames())
                {
                    if (subkey.Equals(item))
                    {
                        key.DeleteSubKeyTree(subkey);
                    }
                }

                //Delete key in ListApp
                foreach (var subkey in keyListApp.GetSubKeyNames())
                {
                    if (subkey.Equals(item))
                    {
                        keyListApp.DeleteSubKeyTree(subkey);
                    }
                }
            }
        }
        public static string SearchAppInstallPath(string appName)
        {
            string pathInstalled = "";
            RegistryKey appBackupUninstallKey;
            if (Environment.Is64BitOperatingSystem)
            {
                appBackupUninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\App Paths\" + appName);
            }
            else
            {
                appBackupUninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + appName);

            }
            if (appBackupUninstallKey != null)
            {
                pathInstalled = appBackupUninstallKey.GetValue("Path").ToString();
                appBackupUninstallKey.Close();
            }
            return pathInstalled;
        }

        public static string SearchAppInRegistry1(string appName)
        {
            string displayname = "";
            RegistryKey appBackupUninstallKey;
            RegistryKey pathAppMarketing;
            if (Environment.Is64BitOperatingSystem)
            {
                appBackupUninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall", false);
            }
            else
            {
                appBackupUninstallKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall", false);

            }
            int countSubkey = appBackupUninstallKey.SubKeyCount;
            string[] listItem = new string[countSubkey];
            listItem = appBackupUninstallKey.GetSubKeyNames();
            foreach (var item in listItem)
            {
                if (item != null)
                {
                    if (appBackupUninstallKey.OpenSubKey(item) != null && appBackupUninstallKey.OpenSubKey(item).GetValue("DisplayName") != null)
                    {
                        if (appBackupUninstallKey.OpenSubKey(item).GetValue("DisplayName").ToString().Contains(appName))
                        {
                            displayname = appBackupUninstallKey.OpenSubKey(item).GetValue("DisplayName").ToString();
                            if ("".Equals(appName))
                            {
                                if (Environment.Is64BitOperatingSystem)
                                {
                                    pathAppMarketing = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\App Paths\");
                                }
                                else
                                {
                                    pathAppMarketing = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\");

                                }
                                if (pathAppMarketing == null)
                                {
                                    displayname = "";
                                }
                                else
                                {
                                    string pathApp = pathAppMarketing.GetValue("Path").ToString();
                                    if (new DirectoryInfo(pathApp).Exists)
                                    {
                                        displayname = appBackupUninstallKey.OpenSubKey(item).GetValue("DisplayName").ToString();
                                    }
                                    else
                                    {
                                        displayname = "";
                                    }
                                    pathAppMarketing.Close();
                                }
                            }
                            else if ("".Equals(appName))
                            {
                                if (Environment.Is64BitOperatingSystem)
                                {
                                    pathAppMarketing = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\App Paths\");
                                }
                                else
                                {
                                    pathAppMarketing = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\");

                                }
                                if (pathAppMarketing == null)
                                {
                                    displayname = "";
                                }
                                else
                                {
                                    string pathApp = pathAppMarketing.GetValue("Path").ToString();
                                    if (new DirectoryInfo(pathApp).Exists)
                                    {
                                        displayname = appBackupUninstallKey.OpenSubKey(item).GetValue("DisplayName").ToString();
                                    }
                                    else
                                    {
                                        displayname = "";
                                    }
                                    pathAppMarketing.Close();
                                }
                            }
                        }
                    }
                }
            }
            if (appBackupUninstallKey != null)
            {
                appBackupUninstallKey.Close();
            }
            return displayname;
        }
       
    }  
    
}
