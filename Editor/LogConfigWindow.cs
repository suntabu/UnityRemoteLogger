using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using Suntabu;
using UnityEditor;

namespace Suntabu.Log
{


    public class LogConfigWindow : EditorWindow
    {
        [MenuItem("Suntabu/Log Config")]
        public static void InitWindow()
        {
            EditorWindow.GetWindow<LogConfigWindow>();
        }

        private LogConfig _config;
        private string[] _levelNames;
        private LogConfigWindow()
        {
            _config = LogManager.Instance.Config;
            _levelNames = new[]
             {
             Loglevels.All.ToString(),
             Loglevels.Information.ToString(),
             Loglevels.Warning.ToString(),
             Loglevels.Error.ToString(),
             Loglevels.Exception.ToString(),
             Loglevels.None.ToString(),
         };
        }



        void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            GUI.color = Color.red;
            _config.IsAllEnable = GUILayout.Toggle(_config.IsAllEnable, "是否启用日志");
            _config.IsLogConsoleEnable = GUILayout.Toggle(_config.IsLogConsoleEnable, "是否启用控制日志功能");
            _config.IsLogFileEnable = GUILayout.Toggle(_config.IsLogFileEnable, "是否启用文件日志功能");

            GUI.color = Color.white;
            _config.Level = (Loglevels)Enum.Parse(typeof(Loglevels), _levelNames[GUILayout.Toolbar((int)_config.Level, _levelNames)]);

            EditorGUILayout.BeginHorizontal();
            //EditorGUILayout.BeginVertical();
            //for (int i = 0; i < _config.DevDic.Keys.Count; i++)
            //{
            //    var key = _config.DevDic.Keys.ToList()[i];

            //    var dev = _config.DevDic[key];

            //    dev = GUILayout.Toggle(dev, key);

            //    _config.DevDic[key] = dev;
            //}
            //EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            for (int i = 0; i < _config.ModuleDic.Keys.Count; i++)
            {
                var key = _config.ModuleDic.Keys.ToList()[i];

                var dev = _config.ModuleDic[key];

                dev = GUILayout.Toggle(dev, key);

                _config.ModuleDic[key] = dev;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Open log folder"))
            {
                string path = LogManager.Instance.LogPath;
                Debug.Log(path);
                
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);//创建新路径  
                }
                System.Diagnostics.Process.Start(  path);
            }
        }
    }

}