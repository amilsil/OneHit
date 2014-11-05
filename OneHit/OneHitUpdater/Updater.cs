using System.IO;
using System.Windows.Forms;
using System;
using System.Diagnostics;
using System.Reflection;

namespace OneHitUpdater
{
    internal class Updater
    {
        private const string UPDATE_LOCATION = @"\\amiklk0\Shared\OneHit\updates\";        
        private const string MAIN_VERSION_FILE = @"version.txt";
        private const string MAIN_EXECUTABLE = "OneHit.exe";

        private static readonly string LOCAL_DIRECTORY;

        static Updater()
        {
            LOCAL_DIRECTORY = 
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
        }

        public bool Update()
        {
            Process[] oneHitProcesses = Process.GetProcessesByName(MAIN_EXECUTABLE);

            // Wait for the OneHit process to stop
            //MessageBox.Show(oneHitProcesses.Length.ToString());
            Process process = (oneHitProcesses.Length > 0) ? oneHitProcesses[0] : null;

            if (process != null)
            {
                process.WaitForExit(5000);
                if (process.HasExited == false)
                {
                    if (process.Responding)
                        process.CloseMainWindow();
                    else
                        process.Kill();
                }
            }

            string localVersion = LoadLocalVersion();
            string remoteVersion = LoadRemoteVersion();

            if (!string.IsNullOrEmpty(remoteVersion) && !remoteVersion.Equals(localVersion))
            {
                try
                {
                    File.Copy(UPDATE_LOCATION + MAIN_EXECUTABLE,
                        MAIN_EXECUTABLE,
                        true);

                    File.Copy(UPDATE_LOCATION + MAIN_VERSION_FILE,
                        MAIN_VERSION_FILE,
                        true);

                    return true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }
            }
            
            return false;
        }

        private bool RemoteUpdatePathExists()
        {
            return Directory.Exists(UPDATE_LOCATION);
        }

        private string LoadLocalVersion()
        {
            string localVersionFileUrl = MAIN_VERSION_FILE;            

            string fileContent = null;
            if (File.Exists(localVersionFileUrl))
            {
                using (StreamReader sr = new StreamReader(localVersionFileUrl))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        fileContent += line;
                    }
                }
            }

            return fileContent;
        }

        private string LoadRemoteVersion()
        {
            if (!RemoteUpdatePathExists())
                return null;

            string remoteVersionFileUrl = UPDATE_LOCATION + @"\" + MAIN_VERSION_FILE;            

            string fileContent = null;
            if (File.Exists(remoteVersionFileUrl))
            {
                using (StreamReader sr = new StreamReader(remoteVersionFileUrl))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        fileContent += line;
                    }
                }
            }

            return fileContent;
        }
    }
}
