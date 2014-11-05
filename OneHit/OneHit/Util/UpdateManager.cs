using System;
using System.IO;
using System.Windows;
using OneHit.Model;

namespace OneHit.Util
{
    class UpdateManager
    {
        /// <summary>
        /// Version types
        /// </summary>
        private enum VersionType
        {
            MainVersion,
            UpdaterVersion
        }

        /// <summary>
        /// Check if an update is available for the Updater
        /// If so, Update it
        /// </summary>
        /// <returns>Updater update status</returns>
        internal bool UpdateUpdater()
        {
            if (IsUpdaterUpdateAvailable())
            {               
                try
                {
                    // Update the Updater
                    File.Copy(Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION + ApplicationContext.UPDATER_PROCESS_NAME),
                        Path.Combine(ApplicationContext.USER_APPLICATION_DATA_PATH, ApplicationContext.UPDATER_PROCESS_NAME), 
                        true);

                    // Copy the version file
                    File.Copy(Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION + ApplicationContext.UPDATER_VERSION_FILE),
                        Path.Combine(ApplicationContext.USER_APPLICATION_DATA_PATH, ApplicationContext.UPDATER_VERSION_FILE), 
                        true);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                }               
            }
            return true;
        }

        /// <summary>
        /// If the Remote update location is available.
        /// Which is normally a remote share.
        /// </summary>
        /// <returns></returns>
        private bool RemoteUpdatePathExists()
        {
            return Directory.Exists(ApplicationContext.REMOTE_UPDATE_LOCATION);
        }

        /// <summary>
        /// If the main executable OneHit.exe is available in the remote location.
        /// </summary>
        /// <returns></returns>
        private bool RemoteMainExecutableExists()
        {
            if (!RemoteUpdatePathExists())
            {
                return false;
            }

            return File.Exists(Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION, ApplicationContext.APPLICATION_PROCESS_NAME));
        }

        /// <summary>
        /// If a new update of the OneHitUpdater.exe is available.
        /// </summary>
        /// <returns></returns>
        internal bool IsUpdaterUpdateAvailable()
        {
            if (!RemoteUpdaterExecutableExists())
            {
                return false;
            }

            string localVersion = LoadLocalVersion(VersionType.UpdaterVersion);
            string remoteVersion = LoadRemoteVersion(VersionType.UpdaterVersion);

            if (!string.IsNullOrEmpty(remoteVersion))
            {
                // If only remote version available, update
                if (string.IsNullOrEmpty(localVersion))
                {
                    return true;
                }

                // If local version is not equal to remote version, Update
                // Allows rollback
                else if (!localVersion.Equals(remoteVersion))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If the remote OneHitUpdater.exe is available.
        /// </summary>
        /// <returns></returns>
        private bool RemoteUpdaterExecutableExists()
        {
            if (!RemoteUpdatePathExists())
            {
                return false;
            }

            return File.Exists(Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION + ApplicationContext.UPDATER_PROCESS_NAME));
        }

        /// <summary>
        /// If a update of OneHit.exe is available.
        /// </summary>
        /// <returns></returns>
        public bool IsMainUpdateAvailable()
        {
            if (!RemoteMainExecutableExists())
            {
                return false;
            }

            string localVersion = LoadLocalVersion(VersionType.MainVersion);
            string remoteVersion = LoadRemoteVersion(VersionType.MainVersion);            

            if (!string.IsNullOrEmpty(remoteVersion))
            {
                if (remoteVersion.Equals(localVersion))
                {
                    return false;
                }

                // If only remote version available, update
                if (string.IsNullOrEmpty(localVersion))
                {
                    return true;
                }

                // If local version is not equal to remote version, Update
                // Allows rollback
                else if (!localVersion.Equals(remoteVersion))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Load the version of the current executables
        /// by reading from the version.txt for OneHit.exe and updaterversion.txt for OneHitUpdater.exe
        /// </summary>
        /// <param name="versionType"></param>
        /// <returns></returns>
        private string LoadLocalVersion(VersionType versionType)
        {
            string localVersionFileUrl;
            if (versionType == VersionType.MainVersion)
            {
                localVersionFileUrl = Path.Combine(ApplicationContext.USER_APPLICATION_DATA_PATH, ApplicationContext.APPLICATION_VERSION_FILE);
            }
            else 
            {
                localVersionFileUrl = Path.Combine(ApplicationContext.USER_APPLICATION_DATA_PATH, ApplicationContext.UPDATER_VERSION_FILE);
            }

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

        /// <summary>
        /// Load the version available in the remote update site.
        /// </summary>
        /// <param name="versionType"></param>
        /// <returns></returns>
        private string LoadRemoteVersion(VersionType versionType)
        {
            if (!RemoteUpdatePathExists())
                return null;

            string remoteVersionFileUrl;

            if (versionType == VersionType.MainVersion)
            {
                remoteVersionFileUrl = Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION, ApplicationContext.APPLICATION_VERSION_FILE);
            }
            else
            {
                remoteVersionFileUrl = Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION, ApplicationContext.UPDATER_VERSION_FILE);

            }

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
