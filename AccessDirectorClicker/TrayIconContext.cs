using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;

namespace AccessDirectorClicker
{
    internal class TrayIconContext : ApplicationContext
    { 
        private readonly NotifyIcon _trayIcon;
        private readonly Assembly _assembly;
        private readonly Timer _timer;

        public TrayIconContext ()
        {
            _assembly = typeof(TrayIconContext).GetTypeInfo().Assembly;
           
            if (!AccessDirectorUtils.IsAdministrator())
            {
                while (true)
                {
                    MessageBox.Show(
                        "Not admin, click access director once manually, then click ok and the app will be restarted as administrator.");

                    var startInfo = new ProcessStartInfo(Application.ExecutablePath) {Verb = "runas"};
                    try
                    {
                        Process.Start(startInfo);
                    }
                    catch (Exception e)
                    {
                        var res = MessageBox.Show($"{e.Message}", "Error", MessageBoxButtons.RetryCancel);
                        if (res == DialogResult.Retry)
                        {
                            continue;
                        }
                    }
                    break;
                }

                _timer = new Timer()
                {
                    Enabled = true,
                    Interval = (int) TimeSpan.FromSeconds(1).TotalMilliseconds,
                };
                _timer.Tick += Exit;
            }
            else
            {
                _timer = new Timer()
                {
                    Enabled = true,
                    Interval = (int) TimeSpan.FromMinutes(5).TotalMilliseconds + 5000,
                };
                _timer.Tick += Tick;
            }

            // Initialize Tray Icon
            _trayIcon = new NotifyIcon()
            {
                Icon = new System.Drawing.Icon(_assembly.GetManifestResourceStream("AccessDirectorClicker.Icon.ico") ?? throw new InvalidOperationException()),
                ContextMenu = new ContextMenu(new [] {
                    new MenuItem("&Kill", Kill)
                    {
                        Enabled = AccessDirectorUtils.IsAdministrator(),
                    },
                    new MenuItem("&Test click automation", Tick), 
                    new MenuItem("&About", About),
                    new MenuItem("E&xit", Exit),
                    
                }),
                Visible = true,
                Text = "Access Director Clicker",
                
            };
          
            _timer.Start();
        }

        void Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("tick");
            AccessDirectorUtils.ClickAccessDirector();
        }
        void Exit(object sender, EventArgs e)
        {
            
            _timer?.Stop();
            _timer?.Dispose();
            // Hide tray icon, otherwise it will remain shown until user mouses over it
            _trayIcon.Visible = false;
            Application.Exit();
        }
      
        void About(object sender, EventArgs e)
        {
            var aboutText = string.Empty;
            var aboutCaption = string.Empty;

            if (Attribute.IsDefined(_assembly, typeof(AssemblyDescriptionAttribute)))
            {
                var a = (AssemblyDescriptionAttribute) Attribute.GetCustomAttribute(_assembly, typeof(AssemblyDescriptionAttribute));
                aboutText = a.Description;
            }

            if (Attribute.IsDefined(_assembly, typeof(AssemblyTitleAttribute)))
            {
                var a = (AssemblyTitleAttribute) Attribute.GetCustomAttribute(_assembly, typeof(AssemblyTitleAttribute));
                aboutCaption = a.Title;
            }

            MessageBox.Show(aboutText, aboutCaption, MessageBoxButtons.OK);
        }
     
        void Kill(object sender, EventArgs e)
        {
            var res = false;
            if (AccessDirectorUtils.IsAdministrator())
            {
                res = AccessDirectorUtils.KillAccessDirector();
            }
            MessageBox.Show(res ? "Killed" : "Failed", "Access Director processes killed", MessageBoxButtons.OK);
        }
    }
}
