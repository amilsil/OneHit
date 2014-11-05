using System;
using System.IO;
using OneHit.Model;
using System.Net;

namespace OneHit.DataAccess
{
	public static class FileSystemUtility
	{
        private static bool _createIfNotExists(string destinationFolder)
        {
            if (!Directory.Exists(destinationFolder))
            {
                try
                {
                    Directory.CreateDirectory(destinationFolder);
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return true;
        }

		/// <summary>
		/// Download a file from the remote shared repository
		/// </summary>
		public static bool DownloadRemoteFile(string sourceFileName, string destinationFolder)
		{
            if (_createIfNotExists(destinationFolder) && IsWritable(destinationFolder))
            {
                string fileName = Path.GetFileName(sourceFileName);
                string localFileUri = Path.Combine(destinationFolder, fileName);
                try
                {
                    if (File.Exists(localFileUri))
                    {
                        File.Delete(localFileUri);
                    }
                    
                    File.Copy(sourceFileName, Path.Combine(destinationFolder, fileName));                    
                    return true;
                }
                catch (Exception)
                {                    
                }
            }

            return false;            
		}

        /// <summary>
        /// Download remote files of an extension. 
        /// </summary>
        /// <param name="extension">extension to filter for</param>
        /// <param name="sourceFolder"></param>
        /// <param name="destinationFolder"></param>
		public static void DownloadRemoteFilesOfType(
            string extension, string sourceFolder, string destinationFolder)
		{
            if (IsWritable(destinationFolder))
            {
                foreach (string file in Directory.GetFiles(sourceFolder, "." + extension))
                {
                    DownloadRemoteFile(file, destinationFolder);
                }
            }
		}

		public static string GetQualifiedRemotePath(string folderName)
		{
			return Path.Combine(ApplicationContext.REMOTE_UPDATE_LOCATION, folderName);
		}

		public static bool IsReadable(string folderPath)
		{
            return Directory.Exists(folderPath);			
		}

		public static bool IsWritable(string folderPath)
		{
			try
			{
				// Attempt to get a list of security permissions from the folder. 
				// This will raise an exception if the path is read only or 
				// do not have access to view the permissions. 
				System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(folderPath);
				return true;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}

		public static bool IsUrlAvailable(string url)
		{
		   try
		   {
		       HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
		       
		        using (HttpWebResponse rsp = (HttpWebResponse)req.GetResponse())
		        {
		            if (rsp.StatusCode == HttpStatusCode.OK)
		            {
		                return true;
		            }
		        }
		    }
		    catch (WebException)
		    {
		        // Eat it because all we want to do is return false
		    }
		
		    // Otherwise
		    return false;
		}
	}
}
