using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Suntabu;
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

        private string moduleName;
        private StringBuilder mStringBuilder;
        private StreamWriter mStreamWriter;
        private string mFilePath;
        private string mPath
        {
            get
            {
                return Path.Combine(LogManager.Instance.LogPath,
                    moduleName);
            }
        }

        public string FilePath
        {
            get { return mFilePath; }
        }

        public LogModule(string moduleName)
        {
            this.moduleName = moduleName;
            mStringBuilder = new StringBuilder();

            if (!Directory.Exists(mPath))
            {
                Directory.CreateDirectory(mPath);
            }
            mFilePath = mPath + "/" +moduleName+"_"+ DateTime.Now.ToString("yyyy_MM_dd") +"_pid"+ Process.GetCurrentProcess().Id+".log";
            //mStreamWriter = File.AppendText(filePath);
        }



        public void Log(string msg, Loglevels level = Loglevels.All)
        {
            if (LogManager.Instance.Config.Level <= level)
            {
                string stringformat = string.Empty;
                switch (level)
                {
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
                        break;
                    case Loglevels.Exception:
                        stringformat = FormatEx;
                        break;
                    case Loglevels.None:

                        break;
                }

                var content = string.Format(stringformat, DateTime.Now.ToShortTimeString() + ":" + level.ToString(), msg);
                mStringBuilder.Append(content);
                if (LogManager.Instance.Config.IsLogConsoleEnable)
                {
                    UnityEngine.Debug.Log(content);
                }

                if (LogManager.Instance.Config.IsLogFileEnable)
                {
                    try
                    {

                        File.AppendAllText(mFilePath, content, Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(string.Format("Write to log failed :{0}\t EXCEPTION：{1}", mFilePath, e.Message));
                    }
                }



            }


        }
    }

}