using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using Westwind.Utilities;

namespace ConsoleAddin
{
    public class ConsoleAddin : MarkdownMonster.AddIns.MarkdownMonsterAddin
    {
        ConsoleAddinConfiguration Configuration { get; set; }

        public override async Task OnApplicationStart()
        {
            await base.OnApplicationStart();


            // Id - should match output folder name. REMOVE 'Addin' from the Id
            Id = "Console";

            // a descriptive name - shows up on labels and tooltips for components
            // REMOVE 'Addin' from the Name
            Name = "Console Addin";


            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = Name,

                // if an icon is specified it shows on the toolbar
                // if not the add-in only shows in the add-ins menu
                FontawesomeIcon = FontAwesomeIcon.Bullhorn
            };

            try
            {
                menuItem.IconImageSource = new ImageSourceConverter()
                    .ConvertFromString("pack://application:,,,/ConsoleAddin;component/icon_22.png") as ImageSource;
            }
            catch
            {
            }

            // if you don't want to display config or main menu item clear handler
            //menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items            
            this.MenuItems.Add(menuItem);
            
        }


        public override async Task OnExecuteConfiguration(object sender)
        {
            await Model.Window.OpenTab(Path.Combine(Model.Configuration.CommonFolder, "ConsoleAddin.json"));
        }

        public override bool OnCanExecute(object sender)
        {
            return true;
        }


        public override Task OnApplicationShutdown()
        {
            ReleaseConsole();
            return Task.CompletedTask;
        }


        IntPtr ConsoleHwnd = IntPtr.Zero;
        Process ConsoleProcess;
        ConsoleBox ConsoleRectangle;

        public override Task OnExecute(object sender)
        {
            this.Configuration = ConsoleAddinConfiguration.Current;

            if (ConsoleHwnd == IntPtr.Zero)            
                CreateConsole();            
            else
                ReleaseConsole(false); // allow re-opening if it was closed

            return Task.CompletedTask;
        }

        

        void CreateConsole()
        {
            // re-read settings in case they were changed
            ConsoleAddinConfiguration.Current.Read();

            string args = null;
            var path = Model.ActiveDocument?.Filename;
            if (!string.IsNullOrEmpty(path))
            {
                path = Path.GetDirectoryName(path);
                args = ConsoleAddinConfiguration.Current.TerminalArguments.Replace("{0}", path);
            }

            var startInfo = new ProcessStartInfo()
            {
                FileName = ConsoleAddinConfiguration.Current.TerminalExecutable,
                Arguments = args,

            };
            ConsoleProcess = Process.Start(startInfo);

            while (true)
            {
                ConsoleHwnd = ConsoleProcess.MainWindowHandle;
                if (ConsoleHwnd.ToInt32() > 0)
                    break;
            }

            RemoveWindowHeader(ConsoleHwnd);

            PositionConsole(true);

            Model.Window.SizeChanged += Window_SizeChanged;
            Model.Window.LocationChanged += Window_LocationChanged;
            Model.Window.Activated += Window_LocationChanged;
        }

        void ReleaseConsole(bool force = true)
        {
            Model.Window.SizeChanged -= Window_SizeChanged;
            Model.Window.SizeChanged -= Window_SizeChanged;
            Model.Window.Activated -= Window_LocationChanged;

            if (!force && (ConsoleProcess == null || ConsoleProcess.HasExited))
            {
                ConsoleHwnd = IntPtr.Zero;
                ConsoleProcess = null;

                // start a new one since the old one was manually closed/killed
                CreateConsole();
                return;
            }

            if (ConsoleProcess != null)
            {
                try
                {
                    ConsoleProcess.Kill();
                }
                catch
                {
                }

                ConsoleProcess = null;
            }
            ConsoleHwnd = IntPtr.Zero;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Model.Window.Dispatcher.InvokeAsync(() => PositionConsole());
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Model.Window.Dispatcher.InvokeAsync(() => PositionConsole());
        }

        void PositionConsole(bool initial = false)
        {
            if (ConsoleHwnd == IntPtr.Zero)
                return;

            var hwnd = Model.Window.Hwnd;
            GetWindowRect(new HandleRef(Model.Window, hwnd), out RECT rectangle);

            GetWindowRect(new HandleRef(Model.Window, ConsoleHwnd), out RECT consoleRect);

            if (initial || ConsoleRectangle == null)
            {
                ConsoleRectangle = new ConsoleBox()
                {
                    Height = ConsoleAddinConfiguration.Current.InitialHeight
                };
            }
            else
            {
                // keep existing height
                ConsoleRectangle.Height = consoleRect.Bottom - consoleRect.Top;
            }


            // Set the window's position.
            ConsoleRectangle.Width = rectangle.Right - rectangle.Left;
            ConsoleRectangle.X = rectangle.Left;
            ConsoleRectangle.Y = rectangle.Bottom + 2;
            SetWindowPos(ConsoleHwnd, Model.Window.Hwnd,
                ConsoleRectangle.X, ConsoleRectangle.Y, ConsoleRectangle.Width, ConsoleRectangle.Height,
                SetWindowPosFlags.DoNotActivate);
        }



        #region Interop

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left; // x position of upper-left corner
            public int Top; // y position of upper-left corner
            public int Right; // x position of lower-right corner
            public int Bottom; // y position of lower-right corner
        }



        // Define the SetWindowPos API function.
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd,
            IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            SetWindowPosFlags uFlags);

        // Define the SetWindowPosFlags enumeration.
        [Flags()]
        private enum SetWindowPosFlags : uint
        {
            SynchronousWindowPosition = 0x4000,
            DeferErase = 0x2000,
            DrawFrame = 0x0020,
            FrameChanged = 0x0020,
            HideWindow = 0x0080,
            DoNotActivate = 0x0010,
            DoNotCopyBits = 0x0100,
            IgnoreMove = 0x0002,
            DoNotChangeOwnerZOrder = 0x0200,
            DoNotRedraw = 0x0008,
            DoNotReposition = 0x0200,
            DoNotSendChangingEvent = 0x0400,
            IgnoreResize = 0x0001,
            IgnoreZOrder = 0x0004,
            ShowWindow = 0x0040,
        }

        [DllImport("user32.dll")]
        static extern bool DestroyWindow(IntPtr hWnd);

        public class ConsoleBox
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }


        //Sets a window to be a child window of another window
        [DllImport("USER32.DLL")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        //Sets window attributes
        [DllImport("USER32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //Gets window attributes
        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);


        //assorted constants needed
        public static int GWL_STYLE = -16;
        public static int WS_BORDER = 0x00800000; //window with border
        public static int WS_DLGFRAME = 0x00400000; //window with double border but no title
        public static int WS_CAPTION = WS_BORDER | WS_DLGFRAME; //window with a title bar

        public void RemoveWindowHeader(IntPtr hwnd)
        {
            // don't stript from ConEmu - it does funky window nesting and it doesn't work to remove header
            if (!Configuration.StripWindowHeader ||
                StringUtils.Contains(Configuration.TerminalExecutable,
                                     "ConEmu", StringComparison.InvariantCultureIgnoreCase))
                return;

            int style = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, (style & ~WS_CAPTION));
        }

        #endregion
    }
}
