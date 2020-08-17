using UnityEngine;
using System.Collections;

public class MK_moveController : MonoBehaviour {
	public float speed = 1.0F;

	public float bounceback=0.2f;//Misun
	public float distCollide=0.1f;//Misun
	public float colliderOffset=0.0f;//Misun
	CharacterController Controller;
	// Use this for initialization
	void Start () {
		Controller = gameObject.GetComponent<CharacterController>();

	}
	
	// Update is called once per frame
	void Update () {
		

		Vector3 p1=transform.position+Controller.center-Vector3.up*Controller.height/2+Vector3.up*colliderOffset;
		Vector3 p2=p1+Vector3.up*Controller.height;
		RaycastHit hit;
		if (Physics.CapsuleCast(p1, p2,Controller.radius, transform.forward, out hit, distCollide))
		{	//If character approaching the collider, bounce the character back so that it doesn't penetrate the walls and etc
			// exception: target object when it has to do something. (If subject can't approach the target object, it's problematic)
			Debug.Log("hit forward");
			if (hit.collider.tag != "Respawn") {
			//	Controller.Move (hit.normal * bounceback);
				transform.Translate(hit.normal*bounceback);
				Debug.Log("touch:"+hit.collider);
			}
		}
		if (Physics.CapsuleCast(p1, p2,Controller.radius, -transform.forward, out hit, distCollide))
		{	//If character approaching the collider, bounce the character back so that it doesn't penetrate the walls and etc
			// exception: target object when it has to do something. (If subject can't approach the target object, it's problematic)
			Debug.Log("hit back");
			if (hit.collider.tag != "Respawn") {
			//	Controller.Move (hit.normal * bounceback);
				transform.Translate(hit.normal*bounceback);
				Debug.Log("touch:"+hit.collider);
			}
		}
		if (Input.GetKey (KeyCode.Escape)) {
			transform.Translate (Vector3.forward * speed*Time.deltaTime);
		}
		if (Input.GetKey (KeyCode.F1)) {
			transform.Translate (-Vector3.forward * speed*Time.deltaTime);
		}
	}
}
