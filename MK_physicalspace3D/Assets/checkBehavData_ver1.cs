using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;

public class checkBehavData_ver1 : MonoBehaviour {
    private JSONNode taskparam;

    // Use this for initialization
    void Start () {
        Debug.Log("start of checkBehavData_ver1.cs");
        TextAsset jsonTextAsset = Resources.Load<TextAsset>("behResult_ver06");
        string the_JSON_string = jsonTextAsset.text;

        taskparam = JSON.Parse(the_JSON_string);
        Debug.Log(taskparam["subId"]);
    }

    // Update is called once per frame
    void Update () {
		
	}

    Vector3[] json2vector3(JSONNode tmpnode)
    {
        Vector3[] newarray = new Vector3[tmpnode.Count];
        for (var i = 0; i < tmpnode.Count; i++) newarray[i] = tmpnode[i];
        return newarray;
    }
}
