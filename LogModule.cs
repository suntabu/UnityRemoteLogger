using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Suntabu;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
#endif
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



        public static void Log(string moduleName, string msg, Loglevels level = Loglevels.All)
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

                StackTrace stackTrace = new StackTrace(true);
                var stackFrame = stackTrace.GetFrame(3);
#if UNITY_EDITOR1
                s_LogStackFrameList.Add(stackFrame);
#endif
                string stackMessageFormat = Path.GetFileName(stackFrame.GetFileName()) + ":" + stackFrame.GetMethod().Name + "():at line " + stackFrame.GetFileLineNumber();

                var content = string.Format(stringformat, "Frame:" + Time.frameCount + ","+DateTime.Now.ToShortTimeString() + ":" + level.ToString(), msg + "\t\t" + stackMessageFormat);

                if (LogManager.Instance.Config.IsLogConsoleEnable)
                {
                    UnityEngine.Debug.Log(content);
                }

                if (LogManager.Instance.Config.IsLogFileEnable)
                {
                    try
                    {
                        File.AppendAllText(GetFilePath(moduleName), "\n" +content + "\n\r", Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(string.Format("Write to log failed :{0}\t EXCEPTION：{1}", mFilePath, e.Message));
                    }
                }

            }


        }




#if UNITY_EDITOR1
        private static int s_InstanceID;
        private static int s_Line = 97;
        private static List<StackFrame> s_LogStackFrameList = new List<StackFrame>();
        //ConsoleWindow
        private static object s_ConsoleWindow;
        private static object s_LogListView;
        private static FieldInfo s_LogListViewTotalRows;
        private static FieldInfo s_LogListViewCurrentRow;
        //LogEntry
        private static MethodInfo s_LogEntriesGetEntry;
        private static object s_LogEntry;
        //instanceId 非UnityEngine.Object的运行时 InstanceID 为零所以只能用 LogEntry.Condition 判断
        private static FieldInfo s_LogEntryInstanceId;
        private static FieldInfo s_LogEntryLine;
        private static FieldInfo s_LogEntryCondition;
        static LogModule()
        {
            s_InstanceID = AssetDatabase.LoadAssetAtPath<MonoScript>("Assets\\_MyGame\\Scripts\\utils\\Log\\LogModule.cs").GetInstanceID();
            s_LogStackFrameList.Clear();

            GetConsoleWindowListView();
        }

        private static void GetConsoleWindowListView()
        {
            if (s_LogListView == null)
            {
                Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
                Type consoleWindowType = unityEditorAssembly.GetType("UnityEditor.ConsoleWindow");
                FieldInfo fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
                s_ConsoleWindow = fieldInfo.GetValue(null);
                FieldInfo listViewFieldInfo = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
                s_LogListView = listViewFieldInfo.GetValue(s_ConsoleWindow);
                s_LogListViewTotalRows = listViewFieldInfo.FieldType.GetField("totalRows", BindingFlags.Instance | BindingFlags.Public);
                s_LogListViewCurrentRow = listViewFieldInfo.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);
                //LogEntries
                Type logEntriesType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntries");
                s_LogEntriesGetEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
                Type logEntryType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntry");
                s_LogEntry = Activator.CreateInstance(logEntryType);
                s_LogEntryInstanceId = logEntryType.GetField("instanceID", BindingFlags.Instance | BindingFlags.Public);
                s_LogEntryLine = logEntryType.GetField("line", BindingFlags.Instance | BindingFlags.Public);
                s_LogEntryCondition = logEntryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public);
            }
        }
        private static StackFrame GetListViewRowCount()
        {
            GetConsoleWindowListView();
            if (s_LogListView == null)
                return null;
            else
            {
                int totalRows = (int)s_LogListViewTotalRows.GetValue(s_LogListView);
                int row = (int)s_LogListViewCurrentRow.GetValue(s_LogListView);
                int logByThisClassCount = 0;
                for (int i = totalRows - 1; i >= row; i--)
                {
                    s_LogEntriesGetEntry.Invoke(null, new object[] { i, s_LogEntry });
                    string condition = s_LogEntryCondition.GetValue(s_LogEntry) as string;
                    //判断是否是由LoggerUtility打印的日志
                    if (condition.Contains("Frame"))
                        logByThisClassCount++;
                }

                //同步日志列表，ConsoleWindow 点击Clear 会清理
                while (s_LogStackFrameList.Count > totalRows)
                    s_LogStackFrameList.RemoveAt(0);
                if (s_LogStackFrameList.Count >= logByThisClassCount)
                    return s_LogStackFrameList[s_LogStackFrameList.Count - logByThisClassCount];
                return null;
            }
        }

        [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            if (instanceID == s_InstanceID && s_Line == line)
            {
                var stackFrame = GetListViewRowCount();
                if (stackFrame != null)
                {
                    string fileName = stackFrame.GetFileName();
                    string fileAssetPath = fileName.Substring(fileName.IndexOf("Assets"));
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(fileAssetPath), stackFrame.GetFileLineNumber());
                    return true;
                }
            }

            return false;
        }
#endif
    }

}