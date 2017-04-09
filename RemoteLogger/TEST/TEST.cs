using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Suntabu.Log;
public class TEST : MonoBehaviour {

	// Use this for initialization
	void Start () {
#if UNITY_EDITOR

#endif
        //SunLog.SetLogScriptPath("Assets/RemoteLogger/LogManager.cs");
        //SunLog.SetLogScriptPath(@"Assets\RemoteLogger\LogModule.cs");
        //SunLog.d("sdf");
        C3();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    void C1()
    {
        SunLog.d("sdf");

        SunLog.d("sdf1");

        SunLog.d("sdf2");


        SunLog.d("sdf3");
    }

    void C2()
    {
        C1();
    }

    void C3()
    {
        C2();
    }
}
