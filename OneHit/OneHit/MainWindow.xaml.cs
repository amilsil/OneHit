using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using OneHit.Util;
using OneHit.ViewModel;

namespace OneHit
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        #region Clipboard viewer related methods

        private HwndSource hWndSource;
        IntPtr hWndNextViewer;

        Queue<string> _hyperlinks = new Queue<string>();

        private bool _clipboardCopyInProgress = false;

        private void InitCBViewer()
        {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            hWndSource = HwndSource.FromHwnd(wih.Handle);

            hWndNextViewer = Win32.SetClipboardViewer(hWndSource.Handle);
        }

        private void CloseCBViewer()
        {
            // remove this window from the clipboard viewer chain
            Win32.ChangeClipboardChain(hWndSource.Handle, hWndNextViewer);

            hWndNextViewer = IntPtr.Zero;
            hWndSource.RemoveHook(this.WndProc);            
        }

        private void DrawCBContent()
        {
            _hyperlinks.Clear();

            if (System.Windows.Clipboard.ContainsText())
            {
                string text = System.Windows.Clipboard.GetText();                
                bool hasHyperlinks = false;

                // Hyperlinks e.g. http://www.server.com/folder/file.aspx
                //Regex rxURL = new Regex(@"(\b(?:http|https|ftp|file)://[^\s]+)", RegexOptions.IgnoreCase);
                //Regex rxURL = new Regex(@"(\b(?:http|https|ftp|file)://([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/?)", RegexOptions.IgnoreCase);
                Regex rxURL = new Regex(@"(\b(?:http|https|ftp|file)://(?:[\da-z\.-]+)(\.([a-z\.]{2,6}))?(?:\:[\d]*)?([\%\?\=\&\d\/\w \.-]*))", RegexOptions.IgnoreCase);
                rxURL.Match(text);

                foreach (Match rm in rxURL.Matches(text))
                {
                    if (!_hyperlinks.Contains(rm.ToString()))
                    {
                        _hyperlinks.Enqueue(rm.ToString());
                        hasHyperlinks = true;
                    }
                }

                if (hasHyperlinks)
                {
                    _clipboardCopyInProgress = true;

                    this._trayIcon.BalloonTipText = string.Join("\n", _hyperlinks);
                    this._trayIcon.BalloonTipTitle = string.Format("OneHit: Click to add {0} link(s) to workspace", _hyperlinks.Count);
                    this._trayIcon.BalloonTipIcon = ToolTipIcon.Info;

                    this._trayIcon.ShowBalloonTip(10);
                }
            }
            
            //TODO: file drop support
            //else if (System.Windows.Clipboard.ContainsFileDropList())
            //{
            //    // we have a file drop list in the clipboard
            //    StringCollection files = System.Windows.Clipboard.GetFileDropList();                
            //}
        }

        void _trayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            if (_clipboardCopyInProgress)
            {
                _clipboardCopyInProgress = false;

                MainWindowViewModel vm = this.DataContext as MainWindowViewModel;
                if (vm != null)
                    vm.AddShortcutsFromClipboard(_hyperlinks);                
            }
        }

        #endregion

        #region Constants
        const string APPLICATION_NAME = "OneHit";
        #endregion

        #region Private Properties

        private NotifyIcon _trayIcon;
        private System.Windows.Forms.ContextMenu _trayMenu;

        // If the main window is open
        private bool _isOpen;
        // Exit only on _trayMenu->Exit, this is to remember if that was selected.
        private bool _readyToExit;
        // Balloon is shown only once.
        private bool _balloonShown = false;
        // If windows is shutting down, settings should be saved
        // on OnClosing() event
        private bool _isShuttingDown = false;

        private OneHit.View.QuickView quickView = null;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            _createSystemTrayMenu();
            WindowsShell.RegisterInStartup(true, APPLICATION_NAME);

            StateChanged += new EventHandler(MainWindow_StateChanged);

			// Width, Height, Top, Left from Registry settings
			_initializeSettingsFromRegistry();

            // Initialize Quick view
            InitializeQuickView();
        }
        
        #endregion

        #region Private Methods

        private void InitializeQuickView()
        {
            QuickViewViewModel vm = new QuickViewViewModel();
            quickView = new OneHit.View.QuickView();
            quickView.DataContext = vm;            
        }

        private void _createSystemTrayMenu()
        {
            _trayMenu = new System.Windows.Forms.ContextMenu();

            System.Windows.Forms.MenuItem itemShow = new System.Windows.Forms.MenuItem("Show (Ctrl+Alt+A)", OnShow);
            _trayMenu.MenuItems.Add(itemShow);

            _trayMenu.MenuItems.Add("Exit", OnExit);

            _trayIcon = new NotifyIcon();
            _trayIcon.Text = APPLICATION_NAME;

            _trayIcon.Icon = Properties.Resources.Icon;

            _trayIcon.Click += new EventHandler(OnShow);
            _trayIcon.BalloonTipClicked += new EventHandler(_trayIcon_BalloonTipClicked);

            _trayIcon.ContextMenu = _trayMenu;
            _trayIcon.Visible = true;
        }

        #endregion

        #region Public Methods

        public void ShowBalloon(string title, string tip, ToolTipIcon icon, byte timeout)
        {
            this._trayIcon.BalloonTipTitle = title;
            this._trayIcon.BalloonTipText = tip;
            this._trayIcon.BalloonTipIcon = icon;

            this._trayIcon.ShowBalloonTip(timeout);
        }

        #endregion


        #region Event Handlers

        private void OnExit(object sender, EventArgs e)
        {
            _readyToExit = true;

			System.Windows.Application.Current.Shutdown();
        }

        private void OnShow(object sender, EventArgs e)
        {
            this.Show();
        }

        /// <summary>
        /// New hide for baloon tips
        /// </summary>
        public new void Hide()
        {
            base.Hide();
            _isOpen = false;

            if (!_balloonShown)
            {
                _trayIcon.BalloonTipIcon = ToolTipIcon.Info;
                _trayIcon.BalloonTipTitle = "OneHit is still running";
                _trayIcon.BalloonTipText = "Press Ctrl+Alt+A to activate OneHit from anywhere"
                    + Environment.NewLine + "Right click on this icon to Exit";

                _trayIcon.ShowBalloonTip(5000);

                _balloonShown = true;
            }
        }

        /// <summary>
        /// New Show for initializing
        /// </summary>
        public new void Show()
        {
            base.Show();
            this.Focus();

            // Bring the window to top, and allow it to sink
            Topmost = true;
            Topmost = false;

            // Main window is open
            _isOpen = true;

            WindowState = System.Windows.WindowState.Normal;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            _isOpen = false;
            _readyToExit = false;

            Keys k = Keys.A | Keys.Control | Keys.Alt;
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            WindowsShell.RegisterHotKey(source, k);

            Keys k2 = Keys.X | Keys.Control | Keys.Alt;
            HwndSource source2 = PresentationSource.FromVisual(this) as HwndSource;
            WindowsShell.RegisterHotKey(source2, k2);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // If shuttng down, Save settings.
            if (_isShuttingDown)
            {
                _serializeSettingsToRegistry();
                CloseCBViewer();
            }

            if (!_readyToExit)
                e.Cancel = true;
            else
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayMenu.Dispose();

                // Save settings to the registry
                _serializeSettingsToRegistry();
            }

            base.OnClosing(e);

            this.Hide();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);

            // InitCBViewer();
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
        }

        #endregion

        #region Settings using Registry

        private void _initializeSettingsFromRegistry()
        {
            double _dWidth, _dHeight, _dLeft, _dTop;

            if (double.TryParse(RegistryHelper.GetSetting("Layout", "Width", null), out _dWidth))
            {
                this.Width = _dWidth;
            }

            if (double.TryParse(RegistryHelper.GetSetting("Layout", "Height", null), out _dHeight))
            {
                this.Height = _dHeight;
            }

            if (double.TryParse(RegistryHelper.GetSetting("Layout", "Left", null), out _dLeft))
            {
                this.Left = _dLeft;
                if (this.Left < 0)
                    this.Left = 50;
            }

            if (double.TryParse(RegistryHelper.GetSetting("Layout", "Top", null), out _dTop))
            {
                this.Top = _dTop;
                if (this.Top < 0)
                    this.Top = 10;
            }
        }

        private void _serializeSettingsToRegistry()
        {
            RegistryHelper.SaveSetting("Layout", "Width", this.Width.ToString());
            RegistryHelper.SaveSetting("Layout", "Height", this.Height.ToString());
            RegistryHelper.SaveSetting("Layout", "Top", this.Top.ToString());
            RegistryHelper.SaveSetting("Layout", "Left", this.Left.ToString());
        }

        #endregion

        #region Win32 Messaging

        #region Win32 Constants

        private const int HWND_BROADCAST = 0xFFFF;
        private const int WM_QUERYENDSESSION = 0x0011;
        private static readonly int WM_MY_MSG = RegisterWindowMessage("WM_MY_MSG");

        // Mutex for single instance handling. 
        static Mutex _single = new Mutex(true, "{4EABFF23-A35E-F0AB-3189-C81203BCAFF1}");

        #endregion

        #region Dll Imports

        [DllImport("user32")]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        private static extern int RegisterWindowMessage(string message);

        #endregion Dll Imports

        // Windows message loop
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // Check if this is ENDSESSION on shutdown
            if (msg == WM_QUERYENDSESSION)
            {
                _isShuttingDown = true;
            }

            // Handle messages...
            if (msg == WindowsShell.WM_HOTKEY)
            {
                if (lParam.ToInt64() == 4259843) //main window
                {
                    if (_isOpen)
                        this.Hide();
                    else
                        this.Show();
                }
                else if (lParam.ToInt64() == 5767171) // quick window
                {
                    quickView.Show();
                    quickView.Activate();
                }
            }

            // Custom message to activate window. 
            // Opening application using shortcut while it is already running causes this message. 
            // Need to activate the instance that is running.
            if (msg == WM_MY_MSG)
            {
                if ((wParam.ToInt32() == 0xCDCD) && (lParam.ToInt32() == 0xEFEF))
                {
                    if (WindowState == System.Windows.WindowState.Minimized)
                    {
                        WindowState = System.Windows.WindowState.Normal;
                    }
                    // Bring window to front.
                    bool temp = this.Topmost;
                    this.Topmost = true;
                    this.Topmost = temp;

                    this.Show();

                    // Set focus to the window.
                    Activate();
                }
            }

            // clipboard monitoring messages
            switch (msg)
            {
                case Win32.WM_CHANGECBCHAIN:
                    if (wParam == hWndNextViewer)
                    {
                        // clipboard viewer chain changed, need to fix it.
                        hWndNextViewer = lParam;
                    }
                    else if (hWndNextViewer != IntPtr.Zero)
                    {
                        // pass the message to the next viewer.
                        Win32.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    }
                    break;

                case Win32.WM_DRAWCLIPBOARD:
                    // clipboard content changed
                    this.DrawCBContent();
                    // pass the message to the next viewer.
                    Win32.SendMessage(hWndNextViewer, msg, wParam, lParam);
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion
    }
}
