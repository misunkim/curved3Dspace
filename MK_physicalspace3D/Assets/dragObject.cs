using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class dragObject : MonoBehaviour{

    private Vector3 mOffset;
    private float mZCoord;
	public Text textTopleft;
	void Start(){
		Debug.Log("start dragObject camera.main="+Camera.main.name);
    
	}
	void OnMouseUp(){//when release the mouse 
		textTopleft.text="";
	}
    void OnMouseDrag() //while mouse was dragged over this object
    {
	//		transform.position = GetMouseAsWorldPoint() + mOffset;
	//	transform.position=new Vector3(GetMouseAsWorldPoint().x+mOffset.x, transform.position.y,GetMouseAsWorldPoint().z+mOffset.z);
		textTopleft.text="on drag";
		Ray rayTmp = Camera.main.ScreenPointToRay (Input.mousePosition);
					//Debug.DrawRay (rayTmp.origin, rayTmp.direction * 10, Color.yellow);
		RaycastHit hit;
		bool didHit = Physics.Raycast (rayTmp, out hit, Mathf.Infinity, LayerMask.GetMask ("MK_layer1"));
		if (didHit) {
			Debug.Log ("layer1 obj click:"+ hit.point);
			transform.position = hit.point;
		}			
	}

}