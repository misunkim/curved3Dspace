using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selfRotate : MonoBehaviour {
	public float rotateSpeed=90;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if(Input.GetKey(KeyCode.UpArrow))
            transform.Rotate(-rotateSpeed*Time.deltaTime, 0,0);
        if (Input.GetKey(KeyCode.DownArrow))
            transform.Rotate(rotateSpeed * Time.deltaTime, 0, 0);

    }
}
