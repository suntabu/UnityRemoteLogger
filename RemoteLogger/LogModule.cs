using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Suntabu;
 
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Suntabu.Log
{


	public class LogModule
	{
		public const string FormatVerbose = "I [{0}]: {1}";
		public const string FormatInfo = "I [{0}]: {1}";
		public const string FormatWarn = "W [{0}]: {1}";
		public const string FormatErr = "Err [{0}]: {1}";
		public const string FormatEx = "Ex [{0}]: {1} - Message: {2}  StackTrace: {3}";


		private static string mPath;

		private static string GetPath (string moduleName)
		{
			if (string.IsNullOrEmpty (mPath)) {
				mPath = Path.Combine (LogManager.Instance.LogPath, moduleName);

				if (!Directory.Exists (mPath)) {
					Directory.CreateDirectory (mPath);
				}

			}
			return mPath;
		}

		private static string mFilePath;

		public static string GetFilePath (string moduleName)
		{
			if (string.IsNullOrEmpty (mFilePath)) {
				mFilePath = GetPath (moduleName) + "/" + moduleName + "_" + DateTime.Now.ToString ("yyyy_MM_dd") + "_pid" + Process.GetCurrentProcess ().Id + ".log";
			}

			return mFilePath;
		}



		public static void Log (string moduleName, object msg, Loglevels level = Loglevels.All)
		{
			if (LogManager.Instance.Config.Level <= level) {
				string stringformat = string.Empty;
				Action<object> action = UnityEngine.Debug.Log;
				switch (level) {
					case Loglevels.All:
						stringformat = FormatVerbose;
						break;
					case Loglevels.Information:
						stringformat = FormatInfo;
						break;
					case Loglevels.Warning:
						stringformat = FormatWarn;
						break;
					case Loglevels.Error:
						stringformat = FormatErr;
						action = UnityEngine.Debug.LogError;
						break;
					case Loglevels.Exception:
						stringformat = FormatEx;
						break;
					case Loglevels.None:

						break;
				}

				var content = string.Format (stringformat, level,  msg);

				if (LogManager.Instance.Config.IsLogConsoleEnable) {
					action (content);
				}

				if (LogManager.Instance.Config.IsLogFileEnable) {
					try {
						File.AppendAllText (GetFilePath (moduleName), "\n" + content + "\n\r", Encoding.UTF8);
					} catch (Exception e) {
						Debug.Log (string.Format ("Write to log failed :{0}\t EXCEPTION：{1}", mFilePath, e.Message));
					}
				}

			}


		}



 
	}

}