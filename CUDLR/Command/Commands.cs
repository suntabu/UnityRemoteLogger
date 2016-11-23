using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Net;
using CUDLR;
using Suntabu.Log;
using Console = CUDLR.Console;

/**
 * Example console commands for getting information about GameObjects
 */
public static class GameObjectCommands {

  [CUDLR.Command("object list", "lists all the game objects in the scene")]
  public static void ListGameObjects() {
    UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
    foreach (UnityEngine.Object obj in objects) {
      CUDLR.Console.Log(obj.name);
    }
  }

  [CUDLR.Command("object print", "lists properties of the object")]
  public static void PrintGameObject(string[] args) {
    if (args.Length < 1) {
      CUDLR.Console.Log( "expected : object print <Object Name>" );
      return;
    }

    GameObject obj = GameObject.Find( args[0] );
    if (obj == null) {
      CUDLR.Console.Log("GameObject not found : "+args[0]);
    } else {
      CUDLR.Console.Log("Game Object : "+obj.name);
      foreach (Component component in obj.GetComponents(typeof(Component))) {
       CUDLR.Console.Log("  Component : "+component.GetType());
        foreach (FieldInfo f in component.GetType().GetFields()) {
          CUDLR.Console.Log("    "+f.Name+" : "+f.GetValue(component));
        }
      }
    }
  }
}



/**
 * Example console route for getting information about GameObjects
 *
 */
public static class GameObjectRoutes {

  [CUDLR.Route("^/object/list.json$", @"(GET|HEAD)", true)]
  public static void ListGameObjects(CUDLR.RequestContext context) {
    string json = "[";
    UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
    foreach (UnityEngine.Object obj in objects) {
      // FIXME object names need to be escaped.. use minijson or similar
      json += string.Format("\"{0}\", ", obj.name);
    }
    json = json.TrimEnd(new char[]{',', ' '}) + "]";

    context.Response.WriteString(json, "application/json");
  }
}


public static class LogMethods
{
    //TODO:
    [Command("evaluate", "evaluate an expression ", true)]
    public static void Evaluate()
    {

    }

    [Command("lm", "list log module")]
    public static void ListModule()
    {
        var keys = LogManager.Instance.LogModuleDic.Keys;
        var names = string.Empty;
        foreach (var key in keys)
        {
            names +=  key+ "\n";
        }
        CUDLR.Console.Log(names);
    }

    [Command("pull", "pull a module log",false)]
    public static void PullModuleLog(RequestContext context,string[] args)
    {
        if (args.Length < 1)
        {
            CUDLR.Console.Log("expected : pull module <module name>");
            return;
        }

        var path =  (LogManager.Instance.LogModuleDic[args[0]].FilePath);
        if (context != null)
        {
                context.Response.AddHeader("Content-disposition", string.Format("attachment; filename={0}", Path.GetFileName(path)));
                context.Response.StatusCode = (int)HttpStatusCode.OK;
                context.Response.StatusDescription = "OK";
                context.Response.WriteString("log/pull?file="+args[0]);
        }

        
    }

    [Route("^/log/pull$")]
    public static void Pull(RequestContext context)
    {
        string module = Uri.UnescapeDataString(context.Request.QueryString.Get("file"));
        var path = LogManager.Instance.LogModuleDic[module].FilePath;
        context.Response.WriteFile(path, "application/octet-stream", true);
        Console.Log("downloading... " + path);
    }
}
