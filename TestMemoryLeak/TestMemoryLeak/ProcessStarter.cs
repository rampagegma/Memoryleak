using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace TestMemoryLeak
{
    public class ProcessStarter
    {
        public static IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        public static int WTS_CURRENT_SESSION = 1;

        public const UInt32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;

        public const UInt32 STANDARD_RIGHTS_READ = 0x00020000;

        public const UInt32 TOKEN_ASSIGN_PRIMARY = 0x0001;

        public const UInt32 TOKEN_DUPLICATE = 0x0002;

        public const UInt32 TOKEN_IMPERSONATE = 0x0004;

        public const UInt32 TOKEN_QUERY = 0x0008;

        public const UInt32 TOKEN_QUERY_SOURCE = 0x0010;

        public const UInt32 TOKEN_ADJUST_PRIVILEGES = 0x0020;

        public const UInt32 TOKEN_ADJUST_GROUPS = 0x0040;

        public const UInt32 TOKEN_ADJUST_DEFAULT = 0x0080;

        public const UInt32 TOKEN_ADJUST_SESSIONID = 0x0100;

        public const UInt32 TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        public const UInt32 TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_DEFAULT |
            TOKEN_ADJUST_SESSIONID);

        public string processPath;

        public string arguments;

        public ProcessStarter(string _processPath, string _arguments)
        {
            this.processPath = _processPath;
            this.arguments = _arguments;
        }
        /// <summary>
        /// WTSEnumerateSessions
        /// This Terminal Services API call lists all local and remote sessions for a given server,
        /// including their state (e.g. connected, disconnected) and type (local, RDP). 
        /// It is the basis for the output of qwinsta.exe.
        /// </summary>
        /// <param name="hServer"></param>
        /// <param name="Reserved"></param>
        /// <param name="Version"></param>
        /// <param name="ppSessionInfo"></param>
        /// <param name="pCount"></param>
        /// <returns></returns>
        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern int WTSEnumerateSessions(
                System.IntPtr hServer,
                int Reserved,
                int Version,
                ref System.IntPtr ppSessionInfo,
                ref int pCount);


        /// <summary>
        /// CloseHandle
        /// </summary>
        /// <param name="hObject"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]

        static extern bool CloseHandle(IntPtr hObject);

        /// <summary>
        /// CreateEnvironmentBlock
        /// </summary>
        /// <param name="lpEnvironment"></param>
        /// <param name="hToken"></param>
        /// <param name="bInherit"></param>
        /// <returns></returns>
        [DllImport("userenv.dll", SetLastError = true)]
        static extern bool CreateEnvironmentBlock(out IntPtr lpEnvironment, IntPtr hToken, bool bInherit);

        /// <summary>
        /// Creates a new process, using the creditials supplied by hToken. 
        /// The application opened is running under the credentials and authority for the user supplied to LogonUser.
        /// </summary>
        /// <param name="hToken"></param>
        /// <param name="lpApplicationName"></param>
        /// <param name="lpCommandLine"></param>
        /// <param name="lpProcessAttributes"></param>
        /// <param name="lpThreadAttributes"></param>
        /// <param name="bInheritHandles"></param>
        /// <param name="dwCreationFlags"></param>
        /// <param name="lpEnvironment"></param>
        /// <param name="lpCurrentDirectory"></param>
        /// <param name="lpStartupInfo"></param>
        /// <param name="lpProcessInformation"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName,
        string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
        ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles,
        uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);


        /// <summary>
        /// The SECURITY_ATTRIBUTES structure contains the security descriptor for an object 
        /// and specifies whether the handle retrieved by specifying this structure is inheritable.
        /// This structure provides security settings for objects created by various functions, 
        /// such as CreateFile, CreatePipe, CreateProcess, or RegCreateKeyEx.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public IntPtr lpSecurityDescriptor;
            public int bInheritHandle;
        }

        /// <summary>
        /// The SECURITY_IMPERSONATION_LEVEL enumeration type contains 
        /// values that specify security impersonation levels. 
        /// Security impersonation levels govern the degree to which a server 
        /// process can act on behalf of a client process.
        /// </summary>
        public enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        /// <summary>
        /// The following enum (Create Process Flags) can be used by CreateProcess, 
        /// CreateProcessAsUser, CreateProcessWithLogonW, and CreateProcessWithTokenW.
        /// </summary>
        [Flags]
        enum CreateProcessFlags : uint
        {
            CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
            CREATE_DEFAULT_ERROR_MODE = 0x04000000,
            CREATE_NEW_CONSOLE = 0x00000010,
            CREATE_NEW_PROCESS_GROUP = 0x00000200,
            CREATE_NO_WINDOW = 0x08000000,
            CREATE_PROTECTED_PROCESS = 0x00040000,
            CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
            CREATE_SEPARATE_WOW_VDM = 0x00000800,
            CREATE_SHARED_WOW_VDM = 0x00001000,
            CREATE_SUSPENDED = 0x00000004,
            CREATE_UNICODE_ENVIRONMENT = 0x00000400,
            DEBUG_ONLY_THIS_PROCESS = 0x00000002,
            DEBUG_PROCESS = 0x00000001,
            DETACHED_PROCESS = 0x00000008,
            EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
            INHERIT_PARENT_AFFINITY = 0x00010000,
            NORMAL_PRIORITY_CLASS = 0x00000020
        }

        /// <summary>
        /// The TOKEN_TYPE enumeration type contains values 
        /// that differentiate between a primary token and an impersonation token.
        /// </summary>
        public enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation
        }

        /// <summary>
        /// The DuplicateTokenEx function creates a new access token that duplicates an existing token. 
        /// This function can create either a primary token or an impersonation token
        /// </summary>
        /// <param name="hExistingToken"></param>
        /// <param name="dwDesiredAccess"></param>
        /// <param name="tokenAttr"></param>
        /// <param name="ImpersonationLevel"></param>
        /// <param name="TokenType"></param>
        /// <param name="phNewToken"></param>
        /// <returns></returns>
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr tokenAttr,
                    SECURITY_IMPERSONATION_LEVEL ImpersonationLevel, TOKEN_TYPE TokenType, out IntPtr phNewToken);

        /// <summary>
        /// WTS_SESSION_INFO
        /// </summary>
        struct WTS_SESSION_INFO
        {
            public int SessionId;
            public string pWinStationName;
            public WTS_CONNECTSTATE_CLASS State;
        }

        /// <summary>
        /// Contains int values that indicate the connection state of a Terminal Services session.
        /// </summary>
        enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            TSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        };

        /// <summary>
        /// Retrieve the primary access token for the user associated with the specified session ID. 
        /// The caller must be running in the context of the LocalSystem account and have the SE_TCB_NAME privilege.
        /// Only highly trusted service should use this function. 
        /// The application must not leak tokens, and close the token when it has finished using it.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="Token"></param>
        /// <returns></returns>
        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSQueryUserToken(Int32 sessionId, out IntPtr Token);

        /// <summary>
        /// GetLastError
        /// </summary>
        /// <returns></returns>
        [DllImport("wininet.dll", EntryPoint = "GetLastError")]
        public static extern long GetLastError();

        /// <summary>
        /// Passed in place of STARTUPINFO to extend CreateProcess
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        /// <summary>
        /// The PROCESS_INFORMATION structure is filled in by either the CreateProcess, CreateProcessAsUser, CreateProcessWithLogonW, 
        /// or CreateProcessWithTokenW function with information about the newly created process and its primary thread.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        /// <summary>
        /// Get CurrentUser Token
        /// </summary>
        /// <returns></returns>
        public static IntPtr GetCurrentUserToken()
        {
            IntPtr currentToken = IntPtr.Zero;
            IntPtr primaryToken = IntPtr.Zero;
            IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
            int dwSessionId = 0;
            IntPtr hUserToken = IntPtr.Zero;
            IntPtr hTokenDup = IntPtr.Zero;
            IntPtr pSessionInfo = IntPtr.Zero;
            int dwCount = 0;
            WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pSessionInfo,
           ref dwCount);
            Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            Int32 current = (int)pSessionInfo;
            for (int i = 0; i < dwCount; i++)
            {
                WTS_SESSION_INFO si =
               (WTS_SESSION_INFO)Marshal.PtrToStructure((System.IntPtr)current,
               typeof(WTS_SESSION_INFO));
                if (WTS_CONNECTSTATE_CLASS.WTSActive == si.State)
                {
                    dwSessionId = si.SessionId;
                    break;
                }
                current += dataSize;
            }

            bool bRet = WTSQueryUserToken(dwSessionId, out currentToken);
            if (bRet == false)
            {
                return IntPtr.Zero;
            }
            SECURITY_ATTRIBUTES security = new SECURITY_ATTRIBUTES();
            bRet = DuplicateTokenEx(currentToken, TOKEN_ASSIGN_PRIMARY | TOKEN_ALL_ACCESS, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, out primaryToken);
            if (bRet == false)
            {
                return IntPtr.Zero;
            }
            return primaryToken;

        }
        public int GetSessionDataWithUserSID(string userSID)
        {
            int sessionId = 0;
            if (Environment.Is64BitOperatingSystem)
            {
                RegistryKey rb64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey testKey = rb64.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData");
                if (testKey != null)
                {
                    string[] valueNames = testKey.GetSubKeyNames();
                    for (int i = 0; i < valueNames.Length; i++)
                    {
                        using (RegistryKey key = testKey.OpenSubKey(valueNames[i], true))
                        {
                            string LoggedOnUserSID = key.GetValue("LoggedOnUserSID").ToString();
                            if (userSID.Equals(LoggedOnUserSID))
                            {
                                sessionId = Int32.Parse(valueNames[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                RegistryKey rb32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey testKey = rb32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData");
                if (testKey != null)
                {
                    string[] valueNames = testKey.GetSubKeyNames();
                    for (int i = 0; i < valueNames.Length; i++)
                    {
                        using (RegistryKey key = testKey.OpenSubKey(valueNames[i], true))
                        {
                            string LoggedOnUserSID = key.GetValue("LoggedOnUserSID").ToString();
                            if (userSID.Equals(LoggedOnUserSID))
                            {
                                sessionId = Int32.Parse(valueNames[i]);
                            }
                        }
                    }
                }
            }

            return sessionId;
        }


        public static IntPtr GetCurrentUserTokenBySessionID(int sessionID)
        {
            IntPtr currentToken = IntPtr.Zero;
            IntPtr primaryToken = IntPtr.Zero;
            IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;
            IntPtr hUserToken = IntPtr.Zero;
            IntPtr hTokenDup = IntPtr.Zero;
            IntPtr pSessionInfo = IntPtr.Zero;
            int dwCount = 0;
            WTSEnumerateSessions(WTS_CURRENT_SERVER_HANDLE, 0, 1, ref pSessionInfo,
           ref dwCount);
            Int32 dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            Int32 current = (int)pSessionInfo;


            bool bRet = WTSQueryUserToken(sessionID, out currentToken);
            if (bRet == false)
            {
                return IntPtr.Zero;
            }
            SECURITY_ATTRIBUTES security = new SECURITY_ATTRIBUTES();
            bRet = DuplicateTokenEx(currentToken, TOKEN_ASSIGN_PRIMARY | TOKEN_ALL_ACCESS, IntPtr.Zero, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, out primaryToken);
            if (bRet == false)
            {
                return IntPtr.Zero;
            }
            return primaryToken;

        }

        public void RunForUserID(int userSID)
        {
            //IntPtr primaryToken = GetCurrentUserToken();
            IntPtr primaryToken = GetCurrentUserTokenBySessionID(userSID);
            if (primaryToken == IntPtr.Zero)
            {
                return;
            }
            STARTUPINFO StartupInfo = new STARTUPINFO();
            PROCESS_INFORMATION processInfo_ = new PROCESS_INFORMATION();
            StartupInfo.cb = Marshal.SizeOf(StartupInfo);
            SECURITY_ATTRIBUTES Security1 = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES Security2 = new SECURITY_ATTRIBUTES();
            string command = "\"" + processPath + "\"";


            if ((arguments != null) /*&& (arguments.Length != 0)*/)
            {
                command += " " + arguments;
            }
            IntPtr lpEnvironment = IntPtr.Zero;
            bool resultEnv = CreateEnvironmentBlock(out lpEnvironment, primaryToken,
           false);
            if (resultEnv != true)
            {
                int nError = (int)GetLastError();
            }
            CreateProcessAsUser(primaryToken, null, command, ref Security1, ref
           Security2, false, (uint)CreateProcessFlags.CREATE_NO_WINDOW | (uint)CreateProcessFlags.NORMAL_PRIORITY_CLASS |
           (uint)CreateProcessFlags.CREATE_UNICODE_ENVIRONMENT, lpEnvironment, null, ref StartupInfo, out
           processInfo_);
        }

        public List<int> GetAllSessionData()
        {
            List<int> sessionId = new List<int>();
            if (Environment.Is64BitOperatingSystem)
            {
                RegistryKey rb64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                RegistryKey testKey = rb64.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData");
                if (testKey != null)
                {
                    string[] valueNames = testKey.GetSubKeyNames();
                    for (int i = 0; i < valueNames.Length; i++)
                    {
                        using (RegistryKey key = testKey.OpenSubKey(valueNames[i], true))
                        {
                            sessionId.Add(Int32.Parse(valueNames[i]));
                        }
                    }
                }
            }
            else
            {
                RegistryKey rb32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                RegistryKey testKey = rb32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Authentication\LogonUI\SessionData");
                if (testKey != null)
                {
                    string[] valueNames = testKey.GetSubKeyNames();
                    for (int i = 0; i < valueNames.Length; i++)
                    {
                        using (RegistryKey key = testKey.OpenSubKey(valueNames[i], true))
                        {
                            string LoggedOnUserSID = key.GetValue("LoggedOnUserSID").ToString();
                            sessionId.Add(Int32.Parse(valueNames[i]));
                        }
                    }
                }
            }

            return sessionId;
        }



        public void RunForAllUser(List<int> userSID)
        {
            //IntPtr primaryToken = GetCurrentUserToken();

            List<IntPtr> primaryToken = new List<IntPtr>();
            foreach (var item in userSID)
            {
                IntPtr token = GetCurrentUserTokenBySessionID(item);
                if (!(token == IntPtr.Zero))
                {
                    primaryToken.Add(token);
                }

            }
            STARTUPINFO StartupInfo = new STARTUPINFO();
            PROCESS_INFORMATION processInfo_ = new PROCESS_INFORMATION();
            StartupInfo.cb = Marshal.SizeOf(StartupInfo);
            SECURITY_ATTRIBUTES Security1 = new SECURITY_ATTRIBUTES();
            SECURITY_ATTRIBUTES Security2 = new SECURITY_ATTRIBUTES();
            string command = "\"" + processPath + "\"";


            if ((arguments != null) /*&& (arguments.Length != 0)*/)
            {
                command += " " + arguments;
            }
            IntPtr lpEnvironment = IntPtr.Zero;
            foreach (var item in primaryToken)
            {
                bool resultEnv = CreateEnvironmentBlock(out lpEnvironment, item, false);
                if (resultEnv != true)
                {
                    int nError = (int)GetLastError();
                }
                CreateProcessAsUser(item, null, command, ref Security1, ref
               Security2, false, (uint)CreateProcessFlags.CREATE_NO_WINDOW | (uint)CreateProcessFlags.NORMAL_PRIORITY_CLASS |
               (uint)CreateProcessFlags.CREATE_UNICODE_ENVIRONMENT, lpEnvironment, null, ref StartupInfo, out
               processInfo_);
            }

        }
    }
}
