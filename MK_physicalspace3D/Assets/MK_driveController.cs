// To create vehicle for curved surface navigation experiment
// I got the code snippet from Unity form thread
// https://forum.unity.com/threads/how-in-the-world-were-the-physics-in-f-zero-x-done.204747/?_ga=2.122614923.1056452554.1590671745-1716890399.1557168250
// 2020.06.02 MK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MK_driveController : MonoBehaviour {
	
	public GUIText logText1;
	public float speed=1.0f;
	public float rotateSpeed=3.0f;
	
	public float distCollide=0.1f;
	public float colliderOffset=0.0f;
	public Vector3 curNormal;
	CharacterController Controller;
	// Use this for initialization
   
    /*Auto adjust to track surface parameters*/
    public float hover_height = 3f;     //Distance to keep from the ground
    public float height_smooth = 10f;   //How fast the ship will readjust to "hover_height"
    public float pitch_smooth = 5f;     //How fast the ship will adjust its rotation to match track normal
    public float drawLineLength=2f;
	public float rotateThres=10f;
    /*We will use all this stuff later*/
    private float smooth_y;
    private float current_speed;

	void Start () {
		Controller = gameObject.GetComponent<CharacterController>();
		curNormal=Vector3.up;
		transform.rotation=Quaternion.identity;

	}
	
	// Update is called once per frame
	void Update () {
		// Rotate around y - axis
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);
       		Debug.DrawLine(transform.position,transform.position+transform.forward*drawLineLength,Color.red);
		if (Input.GetKey(KeyCode.W))
		{	transform.Translate(Vector3.forward*speed*Time.deltaTime);
		}
		if (Input.GetKey(KeyCode.S))
		{	transform.Translate(-Vector3.forward*speed*Time.deltaTime);
		}	
      //We want to save our current transform.up vector so we can smoothly change it later
        Vector3 prev_up = transform.up;
        //Now we set all angles to zero except for the Y which corresponds to the Yaw
        float yaw=transform.rotation.eulerAngles.y;
	//	transform.rotation = Quaternion.Euler(0, yaw, 0);
   
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -prev_up, out hit))
        {  
            Debug.DrawLine (transform.position, hit.point,Color.red);
           
            //Here are the meat and potatoes: first we calculate the new up vector for the ship using lerp so that it is smoothed
            
			//if (Vector3.Angle(prev_up, hit.normal)>rotateThres){// rotate only when the angular difference is greater than rotateThres
			// I put this condition to prevent the character to stop at the intersection of different surface normal and swing infinitely
			
			Vector3 desired_up = Vector3.Lerp (prev_up, hit.normal, Time.deltaTime * pitch_smooth);
            //Then we get the angle that we have to rotate in quaternion format
            Quaternion tilt = Quaternion.FromToRotation(transform.up, desired_up);
            //Now we apply it to the ship with the quaternion product property
            transform.rotation = tilt * transform.rotation;
           
            //Smoothly adjust our height
            smooth_y = Mathf.Lerp (smooth_y, hover_height - hit.distance, Time.deltaTime * height_smooth);
            transform.localPosition += prev_up * smooth_y;
			//}
		}
	//	logText1.text=prev_up.ToString("F3")+" , "+hit.normal.ToString("F3");
		


	/*
		float distForward = Mathf.Infinity;
		RaycastHit hitForward;
		if (Physics.SphereCast(transform.position, 0.25f, -transform.up + transform.forward, out hitForward, 5))
		{
			distForward = hitForward.distance;
		}
		float distDown = Mathf.Infinity;
		RaycastHit hitDown;
		if (Physics.SphereCast(transform.position, 0.25f, -transform.up, out hitDown, 5))
		{
			distDown = hitDown.distance;
		}
		float distBack = Mathf.Infinity;
		RaycastHit hitBack;
		if (Physics.SphereCast(transform.position, 0.25f, -transform.up + -transform.forward, out hitBack, 5))
		{
			distBack = hitBack.distance;
		}
		
		if (distForward < distDown && distForward < distBack)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, hitForward.normal), hitForward.normal), Time.deltaTime * 5.0f);
		}
		else if (distDown < distForward && distDown < distBack)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, hitDown.normal), hitDown.normal), Time.deltaTime * 5.0f);
		}
		else if (distBack < distForward && distBack < distDown)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(Vector3.Cross(transform.right, hitBack.normal), hitBack.normal), Time.deltaTime * 5.0f);
		}
		
		rb.AddForce(-transform.up * Time.deltaTime * gravitySpeed);
	*/	


        /*Finally we move the ship forward according to the speed we calculated before*/
    /*    if (Input.GetKey(KeyCode.W))
			transform.position += transform.forward * (current_speed * Time.deltaTime);
		if (Input.GetKey(KeyCode.S))
			transform.position -= transform.forward * (current_speed * Time.deltaTime);
	*/
		
   
	}
}
