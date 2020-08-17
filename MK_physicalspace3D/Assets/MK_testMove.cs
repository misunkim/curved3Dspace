using UnityEngine;
using System.Collections;

// This script moves the character controller forward
// and sideways based on the arrow keys.
// It also jumps when pressing space.
// Make sure to attach a character controller to the same game object.
// It is recommended that you make only one call to Move or SimpleMove per frame.
// copied from Unity websitee manual script
// 2020.05.28 MK
public class MK_testMove : MonoBehaviour
{
    CharacterController characterController;

    public float speed=4.0f;
	public float rotateSpeed=3.0f;
	public float drawLineLength=2f;


    private Vector3 moveDirection = Vector3.zero;



    void Update()
    {
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);
        Debug.DrawLine(transform.position,transform.position+transform.forward*drawLineLength,Color.red);
		Debug.DrawLine(transform.position,transform.position-transform.up*drawLineLength,Color.red);
		if (Input.GetKey(KeyCode.W))
		{	transform.Translate(Vector3.forward*speed*Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{	transform.Translate(-Vector3.forward*speed*Time.deltaTime);
		}	
    }
}