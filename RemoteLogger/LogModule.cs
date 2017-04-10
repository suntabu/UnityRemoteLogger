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

        private static string GetPath(string moduleName)
        {
            if (string.IsNullOrEmpty(mPath))
            {
                mPath = Path.Combine(LogManager.Instance.LogPath, moduleName);

                if (!Directory.Exists(mPath))
                {
                    Directory.CreateDirectory(mPath);
                }

            }
            return mPath;
        }

        private static string mFilePath;

        public static string GetFilePath(string moduleName)
        {
            if (string.IsNullOrEmpty(mFilePath))
            {
                mFilePath = GetPath(moduleName) + "/" + moduleName + "_" + DateTime.Now.ToString("yyyy_MM_dd") + "_pid" + Process.GetCurrentProcess().Id + ".log";
            }

            return mFilePath;
        }



        public static void Log(string moduleName, object msg, Loglevels level = Loglevels.All)
        {
            try
            {

                if (LogManager.Instance.Config.Level <= level)
                {
                    string stringformat = string.Empty;
                    Action<object> action = UnityEngine.Debug.Log;
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
                            action = UnityEngine.Debug.LogError;
                            break;
                        case Loglevels.Exception:
                            stringformat = FormatEx;
                            break;
                        case Loglevels.None:

                            break;
                    }

                    StackTrace stackTrace = new StackTrace(true);
                    // FIXME call stack count!
                    var stackFrame = stackTrace.GetFrame(5);
#if UNITY_EDITOR
                    SunLog.AddStackFrame(stackFrame);
#endif
                    string stackInfo = Path.GetFileName(stackFrame.GetFileName()) + ":" + stackFrame.GetMethod().Name + "() @ L" + stackFrame.GetFileLineNumber();
                    string timeInfo = Time.frameCount + "F , " + DateTime.Now.Millisecond + "ms";
                    string stackInfoColor = "#990032";
                    string messageColor = "#B803D0";

                    var content = string.Empty;
                    string logContent = string.Format("[{0}][{1}] -> {2}", timeInfo, stackInfo, msg);
                    content = string.Format(stringformat, level, logContent);


                    if (LogManager.Instance.Config.IsLogFileEnable)
                    {
                        CUDLR.Console.Log(content);
                        try
                        {
                            File.AppendAllText(GetFilePath(moduleName), "\n" + content + "\n\r", Encoding.UTF8);
                        }
                        catch (Exception e)
                        {
                            Debug.Log(string.Format("Write to log failed :{0}\t EXCEPTION：{1}", mFilePath, e.Message));
                        }
                    }

#if UNITY_EDITOR
                    string editorContent = string.Format("[{0}][<color={1}>{2}</color>] --> <color={3}>{4}</color>", timeInfo, stackInfoColor, stackInfo, messageColor, msg);
                    content = string.Format(stringformat, level, editorContent);
#endif
                    if (LogManager.Instance.Config.IsLogConsoleEnable)
                    {
                        action(content);
                    }



                }

            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Failed :{0}\t EXCEPTION：{1}", mFilePath, e.Message));
            }
        }




    }

}