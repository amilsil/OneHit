using System;
using System.IO;
using OneHit.DataAccess;
using OneHit.Model;
using System.Collections.Generic;

namespace OneHit.Util.ResourceUpdate
{
    // Represents an updatable resource. 
    // Ex. Template, Note.
	internal abstract class UpdatableResource
	{
		protected string FolderName = string.Empty;

		internal UpdatableResource(string _folderName)
		{
			FolderName = _folderName;
		}

        internal bool Update(List<string> updatedFiles)
        {
            bool updated = false;

            if (_isRemoteUpdateLocationAccessible())
            {
                foreach (string _sRemoteFile in Directory.GetFiles(Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION, FolderName)))
                {
                    if (IsNewVersion(_sRemoteFile))
                    {
                        UpdateResourceFile(_sRemoteFile);
						updated = true;
                        // record this updated (to show as a note)
                        updatedFiles.Add(Path.GetFileName(_sRemoteFile));
                    }
                }
            }

			return updated;
        }

		protected virtual void UpdateResourceFile(string sfile)
		{
            FileSystemUtility.DownloadRemoteFile(
                sfile,
                Path.Combine(ApplicationContext.USER_APPLICATION_DATA_PATH, FolderName)
                );
		}

		/// <summary>
		/// if the newer version of a file exists in the remote update repository
		/// </summary>
		/// <param name="resourceFileName">File name, without path</param>
		/// <returns>if a newer version exists</returns>
		protected virtual bool IsNewVersion(string remoteResourceFileUri)
		{
            string fileName = Path.GetFileName(remoteResourceFileUri);
            string localFile = Path.Combine(ApplicationContext.USER_APPLICATION_DATA_PATH, 
                                FolderName, fileName);

            if (File.Exists(remoteResourceFileUri))
			{
				if (!File.Exists(localFile))
				{
					// if a new file				
					return true;
				}
				else
				{
					// else, new version
                    return File.GetLastWriteTime(remoteResourceFileUri) > File.GetLastWriteTime(localFile);
				}
			}
			
			return false;
		}

		private bool _isRemoteUpdateLocationAccessible()
		{
			return FileSystemUtility.IsReadable(
                Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION, FolderName)
                );
		}
	}
}
