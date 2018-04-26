using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Windows.Automation;

namespace AccessDirectorClicker
{
    internal static class AccessDirectorUtils
    {
        private static readonly string[] ProcessToKill = new[] {"AccessDirectorFramework", "AccessDirectorTray"};
        private static IEnumerable<AutomationElement> EnumNotificationIcons()
        {
            return AutomationElement.RootElement.Find("User Promoted Notification Area").EnumChildButtons();
        }

        private static uint STANDARD_RIGHTS_READ = 0x00020000;
        private static uint TOKEN_QUERY = 0x0008;
        private static uint TOKEN_READ = (STANDARD_RIGHTS_READ | TOKEN_QUERY);

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength);
        public enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            TokenElevationType,
            TokenLinkedToken,
            TokenElevation,
            TokenHasRestrictions,
            TokenAccessInformation,
            TokenVirtualizationAllowed,
            TokenVirtualizationEnabled,
            TokenIntegrityLevel,
            TokenUIAccess,
            TokenMandatoryPolicy,
            TokenLogonSid,
            MaxTokenInfoClass
        }

        public enum TOKEN_ELEVATION_TYPE
        {
            TokenElevationTypeDefault = 1,
            TokenElevationTypeFull,
            TokenElevationTypeLimited
        }

        public static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool ClickAccessDirector()
        {
            Debug.WriteLine("Looking for Access Director Tray icon...");
            try
            {
                foreach (var icon in EnumNotificationIcons())
                {
                    var name = (string) icon.GetCurrentPropertyValue(AutomationElement.NameProperty);
                    Debug.WriteLine(name);
                    if (!name.StartsWith("Access Director") || name.Contains("Clicker"))
                    {
                        continue;
                    }
                    Debug.WriteLine("Found and clicked!");
                    icon.InvokeButton();
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            Debug.WriteLine("Didn't find Access Director");
            return false;
        }

        public static bool KillAccessDirector()
        {
            if (IsAdministrator())
            {
                foreach (var s in ProcessToKill)
                {
                    foreach (var process in Process.GetProcessesByName(s))
                    {
                        Debug.Write($"Found {s} with id {process.Id}, killing it...");
                        process.Kill();
                        Debug.WriteLine("Done");
                    }
                }
                return true;
            }
            Debug.WriteLine("Not administrator, can not kill Access Director");
            return false;
        }

        public static TOKEN_ELEVATION_TYPE ReadElevationLevel()
        {
            IntPtr tokenHandle;
            if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_READ, out tokenHandle))
            {
                throw new ApplicationException("Could not get process token.  Win32 Error Code: " + Marshal.GetLastWin32Error());
            }

            var elevationResult = TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault;

            var elevationResultSize = Marshal.SizeOf((int)elevationResult);
            var returnedSize = (uint) 0;
            var elevationTypePtr = Marshal.AllocHGlobal(elevationResultSize);

            var success = GetTokenInformation(tokenHandle, TOKEN_INFORMATION_CLASS.TokenElevationType, elevationTypePtr, (uint)elevationResultSize, out returnedSize);
            if (success)
            {
                elevationResult = (TOKEN_ELEVATION_TYPE)Marshal.ReadInt32(elevationTypePtr);
                //bool isProcessAdmin = elevationResult == TOKEN_ELEVATION_TYPE.TokenElevationTypeFull;
                Debug.WriteLine(elevationResult.ToString());
                return elevationResult;
            }
            else
            {
                throw new ApplicationException("Unable to determine the current elevation.");
            }
        }

    }
}
