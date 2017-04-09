using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;

namespace Suntabu.Log
{

    public class SunLog
    {
        public static void d(object msg)
        {
            Log("-app-", msg);
        }

        public static void d(string module, object msg)
        {
            Log(module, msg);
        }

        public static void e(object msg)
        {
            Log("-app-", msg, Loglevels.Error);
        }

        public static void e(string module, object msg)
        {
            Log(module, msg, Loglevels.Error);
        }

        public static void Log(string module, object msg, Loglevels level = Loglevels.All)
        {


            StackTrace stackTrace = new StackTrace(true);
            // FIXME call stack count!
            var stackFrame = stackTrace.GetFrame(2);
#if UNITY_EDITOR
            s_LogStackFrameList.Add(stackFrame);
#endif
            string stackMessageFormat = string.Format("<color={1}>{0}</color>", Path.GetFileName(stackFrame.GetFileName()) + ":" + stackFrame.GetMethod().Name + "():at line " + stackFrame.GetFileLineNumber(), "#990032");
            string timeFormat = Time.frameCount + "F , " + DateTime.Now.Millisecond + "ms";
            string objectName = string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[ {0} ][ {3} ] <color={2}>{1}</color>", timeFormat, msg, "#B803D0", stackMessageFormat);
            LogManager.Instance.Log(module, sb.ToString());
        }




#if UNITY_EDITOR
        private static int s_InstanceID;
        private static int s_Line = 84;
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

        private static string m_LogScriptPath;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        private static void Init(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                UnityEngine.Debug.LogError("The given path which leads to log script file is null or empty");
            }
            m_LogScriptPath = path;

            var scriptAsset = AssetDatabase.LoadAssetAtPath<MonoScript>(m_LogScriptPath);
            if (scriptAsset == null)
            {
                UnityEngine.Debug.LogError("The given path :" + m_LogScriptPath + "  can not be found!");
            }
            s_InstanceID = scriptAsset.GetInstanceID();

            s_LogStackFrameList.Clear();

            GetConsoleWindowListView();
        }

        static SunLog()
        {
            Init(@"Assets\RemoteLogger\LogModule.cs");
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
                    if (condition.Contains("][") && condition.Contains("</color>"))
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
            if (instanceID == s_InstanceID && line == s_Line)
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

    /// <summary>
    /// TODO:
    /// 1. use editor to display each module's log file 
    /// 2. use editor to config each module's opening state
    /// 3. in log editor, we can add new module(even developer's name) to debug 
    /// 4. in editro, use button open log directory
    /// 
    /// </summary>
    public class LogManager
    {
        private static LogManager mInstance;

        public static LogManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new LogManager();
                }
                return mInstance;
            }
        }


        public string LogPath
        {
            get
            {
                if (string.IsNullOrEmpty(mLogPath))
                {
                    mLogPath = Path.Combine(Application.persistentDataPath, "applog");
                }

                if (!Directory.Exists(mLogPath))
                {
                    Directory.CreateDirectory(mLogPath);
                }
                return mLogPath;
            }
        }

        public LogConfig Config { get { return _config; } }

        public List<string> LogModuleNames
        {
            get { return mModuleNames; }
        }


        public string GetModuleLogFilePath(string moduleName)
        {
            if (mModuleNames.Contains(moduleName))
            {
                return LogModule.GetFilePath(moduleName);
            }
            return string.Empty;
        }


        private List<string> mModuleNames;
        private string mLogPath;

        private LogConfig _config;

        private LogManager()
        {
            mModuleNames = new List<string>();
            _config = ReadConfig();
            if (_config == null)
            {
                _config = new LogConfig()
                {
                    IsAllEnable = true,
                    IsLogFileEnable = true,
                    IsLogConsoleEnable = true,
                    Level = Loglevels.All,
                };
            }

#if disablelog
        _config.IsAllEnable = false;
#endif

#if disablelogfile
        _config.IsLogFileEnable = false;
#endif

#if disablelogconsole
        _config.IsLogConsoleEnable = false;
#endif

        }



        public void Log(string moduleName, object msg, Loglevels level = Loglevels.All)
        {
            if (_config.IsAllEnable)
            {
                LogModule.Log(moduleName, msg, level);
                if (!mModuleNames.Contains(moduleName))
                {
                    mModuleNames.Add(moduleName);
                }

            }
        }


        private LogConfig ReadConfig()
        {
            //TODO:
            return null;
        }
    }

    public class LogConfig
    {
        public bool IsAllEnable;

        public bool IsLogFileEnable;

        public bool IsLogConsoleEnable;

        public Dictionary<string, bool> ModuleDic;

        public Loglevels Level;
    }




    /// <summary>
    /// Available logging levels.
    /// </summary>
    public enum Loglevels
    {
        /// <summary>
        /// All message will be logged.
        /// </summary>
        All,

        /// <summary>
        /// Only Informations and above will be logged.
        /// </summary>
        Information,

        /// <summary>
        /// Only Warnings and above will be logged.
        /// </summary>
        Warning,

        /// <summary>
        /// Only Errors and above will be logged.
        /// </summary>
        Error,

        /// <summary>
        /// Only Exceptions will be logged.
        /// </summary>
        Exception,

        /// <summary>
        /// No logging will be occur.
        /// </summary>
        None
    }



}
