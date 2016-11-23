using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Inlycat;
using UnityEngine;

namespace Suntabu.Log
{

    public class SunLog
    {
        public static void Log(string msg)
        {
            LogManager.Instance.Log("-app-", msg);
        }

        public static void Log(string module, string msg)
        {
            LogManager.Instance.Log(module, msg);
        }
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

        public Dictionary<string, LogModule> LogModuleDic
        {
            get { return mModuleDic; }
        }

        private Dictionary<string, LogModule> mModuleDic;
        private string mLogPath;

        private LogConfig _config;
        private LogManager()
        {
            mModuleDic = new Dictionary<string, LogModule>();
            _config = ReadConfig();
            if (_config == null)
            {
                _config = new LogConfig()
                {
                    IsAllEnable = true,
                    IsLogFileEnable = true,
                    IsLogConsoleEnable = true,
                    Level = Loglevels.All,
                    ModuleDic = new Dictionary<string, bool>(),
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

 

        public void Log(string moduleName, string msg)
        {
            if (_config.IsAllEnable)
            {
                if (mModuleDic.ContainsKey(moduleName))
                {
                    mModuleDic[moduleName].Log(msg);
                }
                else
                {
                    var module = new LogModule(moduleName);
                    mModuleDic.Add(moduleName, module);
                    module.Log(msg);
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
