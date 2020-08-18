using System.Runtime.InteropServices;
using UnityEngine;

public class CrossPlatformScreenDPI : MonoBehaviour {

	// Use this for initialization
	
	// Update is called once per frame
	public float GetScreenDPI() {
		float dpi=96f;
#if UNITY_WEBGL && !UNITY_EDITOR
		double dpiFromJavascript=GetDPI();
		dpi=(float)dpiFromJavascript*96f;
		Debug.Log("try to call DPI from javascript:"+dpi);
#else
		dpi=Screen.dpi;
#endif
		return dpi;
	}
	void Update(){
		if (Input.GetKeyDown(KeyCode.R))
		{
			Debug.Log("screen width="+Screen.width+",height="+Screen.height+",dpi="+GetScreenDPI());
	
		}
	}
	[DllImport("__Internal")]
	private static extern double GetDPI();
}
