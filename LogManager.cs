using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Inlycat;
using UnityEngine;

namespace Suntabu
{

    public class SunLog
    {
        public static void Log(string msg)
        {
            LogManager.Instance.Log(msg);
        }
    }

    /// <summary>
    /// TODO:
    /// 1. use editor to display each module's log file 
    /// 2. use editor to config each module's opening state
    /// 3. in log editor, we can add new module(even developer's name) to debug 
    /// 
    /// 
    /// </summary>
    internal class LogManager : Singleton<LogManager>
    {
        private StringBuilder mSb;
        private bool mEnable = true;

        private LogManager()
        {
            mSb = new StringBuilder(1000);
        }

        public void Log(string msg)
        {
            string log = string.Format("{0} : {1}", DateTime.Now.ToLongTimeString(), msg);
            mSb.Append(log);
            if (mEnable)
            {
                Debug.Log(log);
            }
        }

    }


}
