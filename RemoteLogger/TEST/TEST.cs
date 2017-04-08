using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Suntabu.Log;
public class TEST : MonoBehaviour {

	// Use this for initialization
	void Start () {
#if UNITY_EDITOR

#endif
        SunLog.SetLogScriptPath("Assets/RemoteLogger/LogManager.cs");
        SunLog.d("sdf");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
