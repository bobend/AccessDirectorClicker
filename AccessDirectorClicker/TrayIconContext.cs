using System;
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
            // Initialize Tray Icon
            _trayIcon = new NotifyIcon()
            {
                Icon = new System.Drawing.Icon(_assembly.GetManifestResourceStream("AccessDirectorClicker.Icon.ico") ?? throw new InvalidOperationException()),
                ContextMenu = new ContextMenu(new [] {
                    new MenuItem("&Kill", Kill)
                    {
                        Enabled = AccessDirectorUtils.IsAdministrator(),
                    },
                    new MenuItem("&Elvation", Elvation),
                    new MenuItem("&About", About),
                    new MenuItem("E&xit", Exit),
                    
                }),
                Visible = true,
                Text = "Access Director Clicker",
                ContextMenuStrip = new ContextMenuStrip {}

            };
            _timer = new Timer()
            {
                Enabled = true,
                Interval = (int) TimeSpan.FromSeconds(5).TotalMilliseconds + 10,
                
            };
            
            Tick(null, null);
            _timer.Tick += Tick;
            _timer.Start();
        }

        void Tick(object sender, EventArgs e)
        {
            //Debug.WriteLine("tick");
            if (AccessDirectorUtils.ReadElevationLevel() ==
                AccessDirectorUtils.TOKEN_ELEVATION_TYPE.TokenElevationTypeDefault)
            {
                AccessDirectorUtils.ClickAccessDirector();
            }
        }
        void Exit(object sender, EventArgs e)
        {
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

        void Elvation(object sender, EventArgs e)
        {
            MessageBox.Show(AccessDirectorUtils.ReadElevationLevel().ToString(), "Elevation", MessageBoxButtons.OK);
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
