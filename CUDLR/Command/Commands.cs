using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Net;
using CUDLR;
using Suntabu.Log;
using Console = CUDLR.Console;

/**
 * Example console commands for getting information about GameObjects
 */
public static class GameObjectCommands
{

	[CUDLR.Command ("ls", "lists all the game objects in the scene")]
	public static void ListGameObjects (RequestContext context, string[] args)
	{
		if (args == null || args.Length == 0) {
			CUDLR.Console.Log ("  Hierarchy TREE:");
			UnityEngine.GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject> ().Where (t => t.transform.parent == null).ToArray ();
			foreach (UnityEngine.Object obj in objects) {
				CUDLR.Console.Log ("\t" + obj.name);
			}
		} else if (args.Length == 1) {
			var path = args [0];
			UnityEngine.GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject> ().Where (t => t.transform.GetAbsolutePath () == path).ToArray ();
			if (objects.Length >= 1) {
				for (int i = 0; i < objects.Length; i++) {
					var go = objects [i];
					//Console.Log("\t" + go.transform.GetAbsolutePath());
					for (int j = 0; j < go.transform.childCount; j++) {
						var child = go.transform.GetChild (j);
						CUDLR.Console.Log ("\t\t" + child.GetAbsolutePath ());
					}

				}
			} else {
				Console.Log ("can not find :" + args [0]);
			}

		}
	}

	[CUDLR.Command ("ls2d", "lists all rect transforms in the scene")]
	public static void ListRectTransforms (RequestContext context, string[] args)
	{
		if (args == null || args.Length == 0) {
			CUDLR.Console.Log ("  Hierarchy TREE:");
			CUDLR.Console.Log ("\t" + CommandUtils.PrintHierarchyTree<RectTransform> (null, "\t"));
		} else if (args.Length == 1) {
			var path = args [0];
			CUDLR.Console.Log ("\t" + CommandUtils.PrintHierarchyTree<RectTransform> (path, "\t"));

		}
	}

	[CUDLR.Command ("lp", "lists properties of the object")]
	public static void PrintGameObject (RequestContext context, string[] args)
	{
		if (args.Length < 1) {
			CUDLR.Console.Log ("expected : object print <Object Name> <Component Name> <Field Name>");
			return;
		}

		UnityEngine.GameObject[] objects = UnityEngine.Object.FindObjectsOfType<GameObject> ().Where (t => t.transform.GetAbsolutePath () == args [0]).ToArray ();

		if (objects.Length == 0) {
			CUDLR.Console.Log ("GameObject not found : " + args [0]);
		} else {

			for (int i = 0; i < objects.Length; i++) {
				GameObject obj = objects [i];
				CUDLR.Console.Log ("Game Object : " + obj.name);
				foreach (Component component in obj.GetComponents(typeof(Component))) {
					var active = component.gameObject.active ? "Enable" :
                        "Disable";
					CUDLR.Console.Log ("  Component : " + component.GetType () + " (" + active + ")");
					foreach (FieldInfo f in component.GetType().GetFields()) {
						CUDLR.Console.Log ("    " + f.Name + " : " + f.GetValue (component));
					}
				}
			}
		}
	}


	[CUDLR.Command ("ln", "list name ---print by call toString() for a Object with given name ")]
	public static void PrintNameObject (RequestContext context, string[] args)
	{
		if (args.Length < 1) {
			CUDLR.Console.Log ("expected : object print <Type Name> <Object Name>");
			return;
		}
		Type typeName = Type.GetType (args [0]);
		string objName = args [1];

		UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsOfType (typeName).Where (t => t.name == objName).ToArray ();
        
		if (objects.Length == 0) {
			CUDLR.Console.Log ("Object not found : " + args [0]);
		} else {

			for (int i = 0; i < objects.Length; i++) {
				UnityEngine.Object obj = objects [i];
				if (obj) {
					MethodInfo[] methodInfos = typeName.GetMethods ();
					for (int j = 0; j < methodInfos.Length; j++) {
						var methodInfo = methodInfos [j];
						if (methodInfo.Name.ToLower () == "tostring") {
							CUDLR.Console.Log ("\t---> " + methodInfo.Invoke (obj, null));
						}
					}
                    
                     
				}
			}
		}
	}

	[Command ("pause", "pause the game")]
	public static void Pause ()
	{
		Time.timeScale = 0;
		Console.Log ("\tdone!");
	}

	[Command ("play", "play the game")]
	public static void Play ()
	{
		Time.timeScale = 1;
		Console.Log ("\tdone!");
	}
}



/**
 * Example console route for getting information about GameObjects
 *
 */
public static class GameObjectRoutes
{

	[CUDLR.Route ("^/object/list.json$", @"(GET|HEAD)", true)]
	public static void ListGameObjects (CUDLR.RequestContext context)
	{
		string json = "[";
		UnityEngine.Object[] objects = UnityEngine.Object.FindObjectsOfType (typeof(GameObject));
		foreach (UnityEngine.Object obj in objects) {
			// FIXME object names need to be escaped.. use minijson or similar
			json += string.Format ("\"{0}\", ", obj.name);
		}
		json = json.TrimEnd (new char[] { ',', ' ' }) + "]";

		context.Response.WriteString (json, "application/json");
	}
}


public static class LogMethods
{
	//TODO:
	[Command ("evaluate", "evaluate an expression ", true)]
	public static void Evaluate ()
	{

	}

	[Command ("lm", "list log module")]
	public static void ListModule ()
	{
		var keys = LogManager.Instance.LogModuleNames;
		var names = string.Empty;
		foreach (var key in keys) {
			names += key + "\n";
		}
		CUDLR.Console.Log (names);
	}

	[Command ("pull", "pull a module log", false)]
	public static void PullModuleLog (RequestContext context, string[] args)
	{
		if (args.Length < 1) {
			CUDLR.Console.Log ("expected : pull module <module name>");
			return;
		}

		var path = LogManager.Instance.GetModuleLogFilePath (args [0]);
		if (context != null) {
			context.Response.AddHeader ("Content-disposition", string.Format ("attachment; filename={0}", Path.GetFileName (path)));
			context.Response.StatusCode = (int)HttpStatusCode.OK;
			context.Response.StatusDescription = "OK";
			context.Response.WriteString ("log/pull?file=" + args [0]);
		}


	}

	[Route ("^/log/pull$")]
	public static void Pull (RequestContext context)
	{
		string module = Uri.UnescapeDataString (context.Request.QueryString.Get ("file"));
		var path = LogManager.Instance.GetModuleLogFilePath (module);
		context.Response.WriteFile (path, "application/octet-stream", true);
		Console.Log ("downloading... " + path);
	}
}
