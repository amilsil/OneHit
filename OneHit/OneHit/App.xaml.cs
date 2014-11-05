using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using OneHit.Util;
using OneHit.Util.ResourceUpdate;
using OneHit.ViewModel;

namespace OneHit
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        #region Dll Imports
        private const int HWND_BROADCAST = 0xFFFF;

        private static readonly int WM_MY_MSG = RegisterWindowMessage("WM_MY_MSG");

        [DllImport("user32")]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        private static extern int RegisterWindowMessage(string message);
        #endregion Dll Imports

        #region Constants (and wanna be's)
        static Mutex _single = new Mutex(true, "{4EABFF23-A35E-F0AB-3189-C81203BCAFF1}");
        #endregion

        #region Event Handlers

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // See if an instance is already running...
            if (_single.WaitOne(TimeSpan.Zero, true))
            {
                try
                {
                    // Launch from installer
                    // Ensure installer doesn't wait until application shuts down.
                    if (e.Args.Length == 1 && e.Args[0] == "INSTALLER")
                    {
                        Process.Start(Assembly.GetExecutingAssembly().Location, "INSTALLED");
                        System.Windows.Application.Current.Shutdown();
                        return;
                    }

                    // If activated from the updater, don't check for updates 
                    bool update = true;
                    if (e.Args.Length > 0 && e.Args[0] == "UPDATER")
                    {
                        update = false;
                    }
					if (e.Args.Length > 0 && e.Args[0] == "INSTALLED")
					{
						update = false;
					}

					if (update)
                    {
                        // Check for updates
                        UpdateManager updateManager = new UpdateManager();
                        if (updateManager.IsUpdaterUpdateAvailable())
                        {
                            updateManager.UpdateUpdater();
                        }

                        if (updateManager.IsMainUpdateAvailable())
                        {
                            // Call the updater exe
                            try
                            {
                                Process.Start("OneHitUpdater.exe");
                                System.Windows.Application.Current.Shutdown();
                                return;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }

                    MainWindowViewModel viewModel = new MainWindowViewModel();
                    MainWindow mainWindow = new MainWindow();
                    mainWindow.DataContext = viewModel;
                    mainWindow.Cards.Items.SortDescriptions.Add(
                        new System.ComponentModel.SortDescription(
                            "Label",
                            System.ComponentModel.ListSortDirection.Ascending
                            )
                        );

                    mainWindow.Show();

                    ResourceUpdateManager.StartResourceUpdateDaemon();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    // handle exception accordingly
                }
                finally
                {
                    _single.ReleaseMutex();
                }
            }
            else
            {
                // An instance is running.
                // Bring existing instance to top and activate it.
                PostMessage(
                    (IntPtr)HWND_BROADCAST,
                    WM_MY_MSG,
                    new IntPtr(0xCDCD),
                    new IntPtr(0xEFEF));

                // Exit from this execution
                Application.Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            ResourceUpdateManager.StopResourceUpdateDaemon();
        }

        #endregion
    }
}
