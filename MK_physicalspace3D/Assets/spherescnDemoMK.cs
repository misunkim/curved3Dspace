using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spherescnDemoMK : MonoBehaviour {
	public 	Transform character;
	// Use this for initialization
	void Start () {
		Debug.Log("start of spherescnDemoMK.cs");
	}
	
	// Update is called once per frame
	void Update () {
		Vector3[] initialLoc=new Vector3[8];
		initialLoc[1]=new Vector3(207f,3.4f,16.4f);//sphere hole
		initialLoc[2]=new Vector3(258f,15.7f,16.4f);//sphere dome
		initialLoc[3]=new Vector3(207f,1.5f,-27.5f);//circle flat
		initialLoc[4]=new Vector3(254f,28f,-36.3f);//full sphere
		initialLoc[5]=new Vector3(237f,6.6f,-41.5f);//cylinder
		initialLoc[6]=new Vector3(131.5f,23.8f,-1.9f);//full sphere with cross wall
		initialLoc[7]=new Vector3(229, 8.6f, -94.1f);//circle with dome

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{	character.position=initialLoc[1];Debug.Log("press 1 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{	character.position=initialLoc[2]; character.eulerAngles=new Vector3(0,0,0);Debug.Log("press 2 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{	character.position=initialLoc[3];character.eulerAngles=new Vector3(0,0,0);Debug.Log("press 3 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{	character.position=initialLoc[4];Debug.Log("press 4 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{	character.position=initialLoc[5];Debug.Log("press 5 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha6))
		{	character.position=initialLoc[6];character.eulerAngles = new Vector3 (-4, 0, 5);Debug.Log("press 6 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha7))
		{	character.position=initialLoc[7];character.eulerAngles = new Vector3 (0, 0, 0);Debug.Log("press 7 MK");
		}

		if (Input.GetKeyDown(KeyCode.Alpha9))
		{	RenderSettings.fogMode=FogMode.ExponentialSquared;
			RenderSettings.fogDensity=0.3f;
			RenderSettings.fog=true; //fog on
		}
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{	RenderSettings.fog=false;//fog off
		}	
	}
}
