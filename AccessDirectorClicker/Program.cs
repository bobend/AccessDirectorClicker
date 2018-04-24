using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Automation;

namespace AccessDirectorClicker
{
    static class Program
    {
        private static readonly string[] ProcessToKill = new[] {"AccessDirectorFramework", "AccessDirectorTray"};
        public static IEnumerable<AutomationElement> EnumNotificationIcons()
        {
            return AutomationElement.RootElement.Find("User Promoted Notification Area").EnumChildButtons();
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Finding Access Director tray icon...");
            var found = false;
            foreach (var icon in EnumNotificationIcons())
            {
                var name = (string) icon.GetCurrentPropertyValue(AutomationElement.NameProperty);
                Console.WriteLine(name);
                if (!name.StartsWith("Access Director")) continue;
                Console.WriteLine("Click!");
                icon.InvokeButton();
                found = true;
                break;
            }

            if (!found)
            {
                Console.WriteLine("Access Director not found.");
            }
            else
            {
                if (IsAdministrator())
                {
                    foreach (var s in ProcessToKill)
                    {
                        foreach (var process in Process.GetProcessesByName(s))
                        {
                            Console.Write($"Found {s} with id {process.Id}, killing it...");
                            process.Kill();
                            Console.WriteLine("Done");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Not administrator, can not kill Access Director");
                }
            }
            // Console.ReadKey();
        }
    }
}
