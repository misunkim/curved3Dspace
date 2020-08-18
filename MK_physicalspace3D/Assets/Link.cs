using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class Link : MonoBehaviour 
{

	public void OpenLinkJSPlugin()
	{
	//	#if !UNITY_EDITOR
	//	openWindow("http://vm-mkim-1.cbs.mpg.de/ethicsDocu/infoSheet_space3D.pdf");
		openWindow("http://wwwuser.gwdg.de/~misun.kim01/infoSheet_space3D.pdf");
	///	#endif
	}

	[DllImport("__Internal")]
	private static extern void openWindow(string url);

}