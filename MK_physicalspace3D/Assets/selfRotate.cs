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
	  transform.Rotate(0, rotateSpeed*Time.deltaTime, 0);
      
	}
}
