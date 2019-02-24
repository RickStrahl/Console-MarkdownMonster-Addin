using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;

namespace ConsoleAddin
{
    public class ConsoleAddin : MarkdownMonster.AddIns.MarkdownMonsterAddin
    {
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();


            // Id - should match output folder name. REMOVE 'Addin' from the Id
            Id = "ConsoleAddin";

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
            catch { }

            // if you don't want to display config or main menu item clear handler
            //menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items            
            this.MenuItems.Add(menuItem);
        }

        IntPtr ConsoleHwnd = IntPtr.Zero;        
        Process ConsoleProcess;
        ConsoleBox ConsoleRectangle;

        public override void OnExecute(object sender)
        {
            if (ConsoleHwnd == IntPtr.Zero)
            {
                // re-read settings in case they were changed
                ConsoleAddinConfiguration.Current.Read();
                
                string args = null;
                var path = Model.ActiveDocument?.Filename;
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetDirectoryName(path);
                    args = string.Format(ConsoleAddinConfiguration.Current.TerminalArguments, path);
                }
                ConsoleProcess = Process.Start(ConsoleAddinConfiguration.Current.TerminalExecutable, args);

                while (true)
                {
                    ConsoleHwnd = ConsoleProcess.MainWindowHandle;
                    if (ConsoleHwnd.ToInt32() > 0)
                        break;
                }

                PositionConsole(true);

                Model.Window.SizeChanged += Window_SizeChanged;
                Model.Window.LocationChanged += Window_LocationChanged;
                Model.Window.Activated += Window_LocationChanged;
            }
            else
            {
                Model.Window.SizeChanged -= Window_SizeChanged;
                Model.Window.SizeChanged -= Window_SizeChanged;
                Model.Window.Activated -= Window_LocationChanged;

                if (ConsoleProcess != null)
                    ConsoleProcess.Kill();

                ConsoleHwnd = IntPtr.Zero;
                
            }

        }


        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Model.Window.Dispatcher.InvokeAsync(() => PositionConsole());
            Debug.WriteLine("Location Changed");            
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Model.Window.Dispatcher.InvokeAsync(() => PositionConsole());
            Debug.WriteLine("Size Changed");
        }

        void PositionConsole(bool initial = false)
        {
            if (ConsoleHwnd == IntPtr.Zero)
                return;

            var hwnd = Model.Window.Hwnd;
            GetWindowRect(new HandleRef(Model.Window, hwnd), out RECT rectangle);

            GetWindowRect(new HandleRef(Model.Window,ConsoleHwnd), out RECT consoleRect);

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
                ConsoleRectangle.X , ConsoleRectangle.Y, ConsoleRectangle.Width, ConsoleRectangle.Height, SetWindowPosFlags.DoNotActivate);
        }

        public override void OnExecuteConfiguration(object sender)
        {
            Model.Window.OpenTab(Path.Combine(Model.Configuration.CommonFolder, "ConsoleAddin.json"));           
        }

        public override bool OnCanExecute(object sender)
        {
            return true;
        }


        #region Interop
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
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
        #endregion


    }
}
