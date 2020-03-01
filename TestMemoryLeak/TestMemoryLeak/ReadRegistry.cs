using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TestMemoryLeak
{
    public class ReadRegistry
    {
        enum TokenInformationClass
        {
            TokenOwner = 4,
        }

        struct TokenOwner
        {
            public IntPtr Owner;
        }

        [DllImport("advapi32.dll", EntryPoint = "GetTokenInformation", SetLastError = true)]
        static extern bool GetTokenInformation(
            IntPtr tokenHandle,
            TokenInformationClass tokenInformationClass,
            IntPtr tokenInformation,
            int tokenInformationLength,
            out int ReturnLength);

        [DllImport("kernel32.dll")]
        private static extern UInt32 WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSQueryUserToken(UInt32 sessionId, out IntPtr Token);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool ConvertSidToStringSid(IntPtr sid, [In, Out, MarshalAs(UnmanagedType.LPTStr)] ref string pStringSid);

        public static string GetLoggedOnUserSID()
        {
            IntPtr tokenOwnerPtr;
            int tokenSize;
            IntPtr hToken;

            // Get a token from the logged on session
            // !!! this line will only work within the SYSTEM session !!!
            WTSQueryUserToken(WTSGetActiveConsoleSessionId(), out hToken);

            // Get the size required to host a SID
            GetTokenInformation(hToken, TokenInformationClass.TokenOwner, IntPtr.Zero, 0, out tokenSize);
            tokenOwnerPtr = Marshal.AllocHGlobal(tokenSize);

            // Get the SID structure within the TokenOwner class
            GetTokenInformation(hToken, TokenInformationClass.TokenOwner, tokenOwnerPtr, tokenSize, out tokenSize);
            TokenOwner tokenOwner = (TokenOwner)Marshal.PtrToStructure(tokenOwnerPtr, typeof(TokenOwner));

            // Convert the SID into a string
            string strSID = "";
            ConvertSidToStringSid(tokenOwner.Owner, ref strSID);
            Marshal.FreeHGlobal(tokenOwnerPtr);
            return strSID;
        }
    }
}
