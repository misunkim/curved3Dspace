using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MK_urlTest : MonoBehaviour {
	public Text showText;

    private string _url = "http://leatonm.net/wp-content/uploads/2017/candlepin/getdate.php"; //change this to your own
    private string _timeData;
    private string _currentTime;
    private string _currentDate;
	// Use this for initialization
	void Start () {
		var parameters=URLParameters.GetSearchParameters();
		string site;
		Debug.Log("full URL="+URLParameters.Href);
		if (parameters.TryGetValue("site", out site))
		{
			showText.text=site;// use "site" here
			Debug.Log("site="+site);
		}
		if (parameters.TryGetValue("PID", out site))
		{
			showText.text=site;// use "site" here
			Debug.Log("PID="+site);
		}
		else
		{
			// no parameter with name "site" found
			showText.text="can't extract the URL, full URL is "+URLParameters.Href;

		}

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{	Debug.Log("Alpha1 pressed");
			StartCoroutine(getTime());}
	}
	public IEnumerator getTime()
    {
        Debug.Log ("connecting to php");
        WWW www = new WWW (_url);
        yield return www;
        if (www.error != null) {
            Debug.Log ("Error");
        } else {
            Debug.Log ("got the php information");
        }
        _timeData = www.text;
        string[] words = _timeData.Split('/');    
        //timerTestLabel.text = www.text;
        Debug.Log ("The date is : "+words[0]);
        Debug.Log ("The time is : "+words[1]);
 
        //setting current time
        _currentDate = words[0];
        _currentTime = words[1];
    }
}
