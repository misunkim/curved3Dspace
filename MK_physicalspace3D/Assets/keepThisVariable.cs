using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keepThisVariable : MonoBehaviour {
	public string subId;
	public int sex;
	public int age;
	public int run;
	static keepThisVariable i;
	void Awake(){
		if (!i){
			i=this;
			DontDestroyOnLoad(gameObject);
		}
		else
			Destroy(gameObject);
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
