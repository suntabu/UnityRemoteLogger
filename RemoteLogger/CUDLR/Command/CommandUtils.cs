using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text;

public static class CommandUtils
{
	public static string PrintHierarchyTree<T> (string path, string prefix = "\t") where T : Transform
	{
		var sb = new StringBuilder ();
		T[] objects = string.IsNullOrEmpty (path) ? UnityEngine.Object.FindObjectsOfType<T> ().Where (t => t.parent == null).ToArray ()
			: UnityEngine.Object.FindObjectsOfType<T> ().Where (t => t.GetAbsolutePath () == path).ToArray ();
		foreach (T obj in objects) {
			sb.AppendLine (prefix + obj.name);
			if (typeof(T) == typeof(Transform))
				PrintTransform (sb, obj, prefix + "\t");
			else if (typeof(T) == typeof(RectTransform)) {
				var rectTransform = obj as RectTransform;
				PrintRectTransform (sb, rectTransform, prefix + "\t");
			}
			if (obj.childCount != 0) {
				for (int i = 0; i < obj.childCount; i++) {
					var child = obj.GetChild (i);
					sb.Append (PrintHierarchyTree<T> (obj.GetAbsolutePath () + "/" + child.name, prefix + "-\t"));	
				}
			}
		}
		return sb.ToString ();
	}


	public static void PrintTransform (StringBuilder sb, Transform transform, string prefix)
	{
		var tp = prefix + "\t";
		tp = tp.Replace ("-", " ");
		Debug.Log (tp);
		sb.AppendLine (tp + transform.localPosition);
		sb.AppendLine (tp + transform.localRotation.eulerAngles);
		sb.AppendLine (tp + transform.localScale);
	}

	public static void PrintRectTransform (StringBuilder sb, RectTransform transform, string prefix)
	{
		var tp = prefix + "\t";
		tp = tp.Replace ("-", " ");
		Debug.Log (tp);
		sb.AppendLine (tp + transform.rect.size);
		sb.AppendLine (tp + transform.anchorMin);
		sb.AppendLine (tp + transform.anchorMax);
		sb.AppendLine (tp + transform.anchoredPosition);
		sb.AppendLine (tp + transform.localScale);
	}


	public static string GetAbsolutePath (this Transform transform)
	{
		string path = transform.name;
		if (transform.parent != null) {
			path = transform.parent.GetAbsolutePath () + "/" + path;
		}
		return path;
	}

}

