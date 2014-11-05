using System;
using System.IO;

namespace OneHit.Util
{
	public static class Logger
	{
		static readonly string logfile = "D:\\Onehit_log_" + DateTime.Now.ToFileTimeUtc() + ".txt";

		public static void WriteLine(string line)
		{
			using (StreamWriter writer = new StreamWriter(logfile, true))
			{
				writer.Write(DateTime.Now.ToLocalTime().ToString() + " : ");
				writer.WriteLine(line);
				writer.Close();
			}
		}
	}
}
