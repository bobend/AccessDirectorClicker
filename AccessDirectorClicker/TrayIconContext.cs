using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace AccessDirectorClicker
{
    internal class TrayIconContext : ApplicationContext
    { 
        private readonly NotifyIcon _trayIcon;
        private readonly Assembly _assembly;
        private readonly Timer _timer;

        public TrayIconContext(string[] args)
        {
            Timer StartTimer(EventHandler tickAction, TimeSpan interval)
            {
                var timer = new Timer()
                {
                    Enabled = true,
                    Interval = (int) interval.TotalMilliseconds,
                };
                timer.Tick += tickAction;
                return timer;
            }

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
                _timer = StartTimer(Exit, TimeSpan.FromSeconds(1));
            }
            else
            {
                _timer = 
                    args.Select(a => a.ToLower()).Contains("kill") ? 
                        StartTimer(Kill, TimeSpan.FromSeconds(1)) :
                        StartTimer(Tick, TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(5));
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

            if (!res)
            {
                MessageBox.Show("Failed", "Access Director failed to kill processes", MessageBoxButtons.OK);
            }
            else
            {
                Exit(sender, e);
            }
        }
    }
}
