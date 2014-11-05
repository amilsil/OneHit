using System.Reflection;
using System.IO;
using System;

namespace OneHit.Model
{
    /// <summary>
    /// Contains application level properties.
    /// </summary>
	public static class ApplicationContext
	{
		public const string APPLICATION_NAME = "OneHit";
		public const string APPLICATION_PROCESS_NAME = "OneHit.exe";
		public const string APPLICATION_VERSION_FILE = "version.txt";

		public const string UPDATER_PROCESS_NAME = "OneHitUpdater.exe";
		public const string UPDATER_VERSION_FILE = "updaterversion.txt";

		public const string PRIMARY_DATA_FILENAME = "OneHitShortcuts.xml";
		public const string TEMPLATE_DIRECTORY_NAME = "Templates";
        public const string LIVENOTES_DIRECTORY_NAME = "Livenotes";
        public const string REMOTE_UPDATE_LOCATION = @"\\amiklk0\Shared\OneHit\updates\";

		public static readonly string LOCAL_APPLICATION_DIRECTORY;
		public static readonly string USER_APPLICATION_DATA_PATH;
		
		public static string LOCAL_PRIMARY_DATA_FILE
		{
            get { return USER_APPLICATION_DATA_PATH + PRIMARY_DATA_FILENAME; }
		}

		static ApplicationContext()
		{
			LOCAL_APPLICATION_DIRECTORY 
				= Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

			USER_APPLICATION_DATA_PATH 
				= Environment.GetFolderPath(Environment.SpecialFolder.Personal)	
					+ Path.DirectorySeparatorChar 
					+ APPLICATION_NAME
					+ Path.DirectorySeparatorChar;   
		}
	}
}
