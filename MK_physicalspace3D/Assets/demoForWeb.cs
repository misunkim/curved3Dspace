using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class demoForWeb : MonoBehaviour {
	int moveConstraint;
	bool rotateAllow, translateAllow;
	public Transform char2D,character,characterCamera, prop2D, prop3D, guideArrow2D, guideArrow3D;

	float rotateSpeed,translateSpeed, characterRadius,distCollide;
	public Vector3 curr_norm2D;
	public GameObject mainMenuUI;
	public Text text_top;
	public int trial;
	// Use this for initialization
	void Start () {
		rotateSpeed=90;
		translateSpeed=4;
		characterRadius = 0.25f;
		distCollide=1f;
		rotateAllow=false; translateAllow=false;
		text_top.text="";
	  
	}
	
	// Update is called once per frame
    void Update()
    {
        if (moveConstraint == 1) // update the locationon the flattened surface(2D) and then translate into 3D
        {
            if (rotateAllow)
            {
                char2D.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime, 0);
            }

            Vector3 old_2Dpos = char2D.position;
            if (translateAllow)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    char2D.Translate(Vector3.forward * translateSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    char2D.Translate(-Vector3.forward * translateSpeed * Time.deltaTime);
                }
            }
            curr_norm2D = phy2DtoNorm2D(char2D);//extract the normalised 2D coordinates and direction from arrow on 2D
            if (curr_norm2D.x < -0.98f | curr_norm2D.x > 0.98f | curr_norm2D.y < 0.02f | curr_norm2D.y > 0.98f) // if the movement makes subject outside the range of arena, move back to where they were
            {
                char2D.position = old_2Dpos;
                curr_norm2D = phy2DtoNorm2D(char2D);
            }

            norm2DtoPhy3D(curr_norm2D, character);// place character on 3D location using the normalised 2D coordinate and facing direction 
        }
		if (moveConstraint == 3) // fly movement
        {

            float rotationX = character.localEulerAngles.y + Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime;
            float rotationY = character.localEulerAngles.x - Input.GetAxis("Vertical") * rotateSpeed * Time.deltaTime;
            //  character.localEulerAngles = new Vector3(rotationY, rotationX, 0);

            if (rotateAllow)
            {
                character.Rotate(0, 1 * Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime, 0, Space.Self);
                character.Rotate(-Input.GetAxis("Vertical") * rotateSpeed * Time.deltaTime, 0, 0, Space.Self);
            }

            Vector3 p1 = character.position + character.up * -0.2f; //capsule cast bottom position, depending on the hover distance between the character center and ground
            Vector3 p2 = character.position + character.up * 0.2f; //capsule cast top position 
            Debug.DrawLine(p1, p1 + character.forward * distCollide, Color.red);
            Debug.DrawLine(p2, p2 + character.forward * distCollide, Color.red);

            RaycastHit hit;

            if (translateAllow)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    if (Physics.CapsuleCast(p1, p2, characterRadius, character.forward, out hit, distCollide))
                        Debug.Log("touch front:" + hit.collider);
                    else
                        character.Translate(Vector3.forward * translateSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    if (Physics.CapsuleCast(p1, p2, characterRadius, -character.forward, out hit, distCollide))
                        Debug.Log("touch back:" + hit.collider);
                    else
                        character.Translate(-Vector3.forward * translateSpeed * Time.deltaTime);
                }
            }
        }
        if (moveConstraint == 4)// driving with pitch rotation (very similar to simple driving, moveConstraint=1 except that different key configuratino is used and camera rotation is added)
        {
            if (rotateAllow)
            {
                char2D.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime, 0);
                float tmpPitch = characterCamera.localEulerAngles.x;

                if (Input.GetKey(KeyCode.UpArrow))
                {
                    tmpPitch = tmpPitch - rotateSpeed * 0.6f * Time.deltaTime;
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    tmpPitch = tmpPitch + rotateSpeed * 0.6f * Time.deltaTime;
                
                
                }
                // restrict the range of camera pitch to +-70deg
                tmpPitch = tmpPitch % 360;
                if (tmpPitch > 70 & tmpPitch <= 180)
                    tmpPitch = 70;
                if (tmpPitch > 180 & tmpPitch < 290)
                    tmpPitch = 290;
                characterCamera.localEulerAngles = new Vector3(tmpPitch, 0, 0);

            }


            Vector3 old_2Dpos = char2D.position;
            if (translateAllow)
            {
                if (Input.GetKey(KeyCode.W))
                {
                    char2D.Translate(Vector3.forward * translateSpeed * Time.deltaTime);
                }
                if (Input.GetKey(KeyCode.S))
                {
                    char2D.Translate(-Vector3.forward * translateSpeed * Time.deltaTime);
                }
            }
            curr_norm2D = phy2DtoNorm2D(char2D);//extract the normalised 2D coordinates and direction from arrow on 2D
            if (curr_norm2D.x < -0.98f | curr_norm2D.x > 0.98f | curr_norm2D.y < 0.02f | curr_norm2D.y > 0.98f) // if the movement makes subject outside the range of arena, move back to where they were
            {
                char2D.position = old_2Dpos;
                curr_norm2D = phy2DtoNorm2D(char2D);
            }

            norm2DtoPhy3D(curr_norm2D, character);// place character on 3D location using the normalised 2D coordinate and facing direction 

        }


    }
	public void changeMoveMode(int moveMode){
		Debug.Log("changeMode to "+moveMode);
		moveConstraint=moveMode;
		if (moveConstraint == 1 | moveConstraint == 4)
        {    characterCamera.localPosition = new Vector3(0, 4, 0); // for driving condition, I should give vertical offset to camera (otherwise camera is at th surface, and I can'see well)
			norm2DtoPhy2D(new Vector3(0, 0.5f, 45), char2D); //starting position of character
        }
		if (moveConstraint == 3)
		{    characterCamera.localPosition = new Vector3(0, 0, 0); //when flying, it is more natural when eye and center of mass are aligned
			character.position=new Vector3(-6.5f,1f,-40);
			character.eulerAngles=new Vector3(0,60,0);
		}
		mainMenuUI.SetActive(false);
    	translateAllow=true;rotateAllow=true;
		StartCoroutine(propFollowingTask());
	}
	public void toMainMenu(){
		mainMenuUI.SetActive(true);
		translateAllow=false;rotateAllow=false;
	}
	IEnumerator propFollowingTask()
    {
		Debug.Log("start propFollowingTask");
        Vector3[] pos2DList = new Vector3[9];
        //	pos2DList[1]=new Vector3(-0.5f,0.9f, 0);
        //	pos2DList[2]=new Vector3(0.5f,0.1f, 0);
        //	pos2DList[3]=new Vector3(0.7f,0.5f,0);	
		pos2DList[1]=new Vector3(0,0.5f,0);
		pos2DList[2]=new Vector3(0.5f,0.9f,0);
		pos2DList[3]=new Vector3(-0.9f,0.2f,0);
		pos2DList[4]=new Vector3(-0.7f,0.7f,0);
		pos2DList[5]=new Vector3(0.5f,0.5f,0);
		pos2DList[6]=new Vector3(0,0.2f,0);
		pos2DList[7]=new Vector3(-0.2f,0.8f,0);
		pos2DList[8]=new Vector3(-0.5f,0.1f,0);


        norm2DtoPhy2D(new Vector3(0, 0.7f, 180), char2D); //starting position of character

        text_top.text = "Find the traffic cone and move to it (rotation: arrow keys, move: W/S)";

        float inittime = Time.time;
 
        trial = 1;
        while (trial < pos2DList.Length)
        {
            norm2DtoPhy2D(pos2DList[trial], prop2D); // place the prop (e.g. traffic cone) to predefined location
            norm2DtoPhy3D(pos2DList[trial], prop3D); // place the prop (e.g. traffic cone) to predefined location
           
		    RaycastHit hit;
            Vector3 p1 = character.position; //capsule cast bottom position, depending on the hover distance between the character center and ground
            Vector3 p2 = character.position + character.up * 1; //capsule cast top position 

            Debug.DrawLine(p1, p1 + character.forward * 2, Color.red);
            Debug.DrawLine(p2, p2 + character.forward * 2, Color.blue);

            if (Physics.CapsuleCast(p1, p2, characterRadius, character.forward, out hit, distCollide))
            {
                Debug.Log("hit forward"); // if character face any wall
                if (hit.collider.name == prop3D.name)
                {//If character get sufficiently close to the prop
                    text_top.text = "good";
                    yield return new WaitForSeconds(0.5f);
                    Debug.Log("touch:" + hit.collider);
                    trial++;
                    text_top.text = "Find the cone and move to it (rotation: arrow keys, move:W/S)";
                }
            }// then move the object
        yield return null; // yield return null is absolute necessary for while loop!
        }
       text_top.text="Good job. You have found all traffic cones.";
		
    }
    Vector3 phy2DtoNorm2D(Transform obj3D)
    {
        // this scale and offset should be adjusted if I change the size of objects in Unity

        float tmpx = obj3D.position.x / 25f;
        float tmpy = (obj3D.position.z - 87.5f) / 25f;
        float ang = obj3D.eulerAngles.y;
        return new Vector3(tmpx, tmpy, ang);
    }
    void norm2DtoPhy2D(Vector3 normPos2D, Transform obj3D)
    {
        // this scale and offset should be adjusted if I change the size of objects in Unity
        float tmpx = normPos2D.x * 25f;
        float tmpy = 0.5f;
        float tmpz = normPos2D.y * 25f + 87.5f;
        obj3D.position = new Vector3(tmpx, tmpy, tmpz);
        float rotY = normPos2D.z;
        obj3D.eulerAngles = new Vector3(0, rotY, 0);
    }
    Vector3 norm2DtoPhy3D(Vector3 normPos2D, Transform obj3D)
    {
        // convert normalised 2D coordinate onto 3D coordinate in Unity
        // input should be x=(-1,1), y=(0,1)
        // when I adjust the size/offset of environment on Unity, I should adjust this part correctly
        // I also have to update "conv3Dto2D" function correctly
        // because it's not simple linear transformation matrix, I can't just use the inverse of rigic body transformatio matrix here
        float Radius = 0;

        float tmpx = 0; float tmpy = 0; float tmpz = 0;
        float rotY = normPos2D.z;
        Vector3 baseNormal = new Vector3(0, 0, 0);// base Euler angles on the surface
                //Radius = 25 / Mathf.PI*2/3;
		Radius = 5.3f;
		float sqDim=25;
		if (normPos2D.x < 0)
		{
			tmpx = normPos2D.x * sqDim + 0f;
			tmpy = 0f;
			tmpz = normPos2D.y * sqDim - 55f;

			baseNormal = new Vector3(0, 0, 0);
		}
		if (normPos2D.x >= 0)
		{
			tmpx = Mathf.Sin(normPos2D.x * Mathf.PI * 3 / 2) * Radius + 0f; //
			tmpy = -Mathf.Cos(normPos2D.x * Mathf.PI * 3 / 2) * Radius + Radius;
			tmpz = normPos2D.y * sqDim - 55f;

			baseNormal = new Vector3(0, 0, (normPos2D.x * Mathf.PI * 3 / 2) * Mathf.Rad2Deg);
		}

        Vector3 outPos3D = new Vector3(tmpx, tmpy, tmpz);

        obj3D.position = outPos3D;
        obj3D.eulerAngles = baseNormal;
        obj3D.Rotate(new Vector3(0, rotY, 0), Space.Self);

        Debug.DrawLine(obj3D.position, obj3D.position + obj3D.up * 1, Color.red);
        Debug.DrawLine(obj3D.position + obj3D.up * 1, obj3D.position + obj3D.up * 1 + obj3D.forward * 2, Color.red);

        return outPos3D;
    }
       void placeGuideArrow(Vector3 start, Vector3 target)
    {
        float currentDistSq = Mathf.Pow(start.x - target.x, 2) + Mathf.Pow(start.y - target.y, 2);
        float minDist = 0.2f;
        //text_top.text="currentDist="+Mathf.Sqrt(currentDistSq).ToString("F3");
        if (currentDistSq > minDist * minDist)
        {
            guideArrow3D.gameObject.SetActive(true);//if the character is alreay close to the target object, then don't show the guide arrow
            guideArrow2D.gameObject.SetActive(true);//if the character is alreay close to the target object, then don't show the guide arrow

            float tmpang = Mathf.Atan2(target.x - start.x, target.y - start.y);
            float guideDist = 0.05f;
            float tmpx = start.x + guideDist * Mathf.Sin(tmpang);
            float tmpy = start.y + guideDist * Mathf.Cos(tmpang);
            norm2DtoPhy3D(new Vector3(tmpx, tmpy, tmpang * Mathf.Rad2Deg), guideArrow3D);
            norm2DtoPhy2D(new Vector3(tmpx, tmpy, tmpang * Mathf.Rad2Deg), guideArrow2D);
        }
        else
        {
            guideArrow3D.gameObject.SetActive(false);//if the character is alreay close to the target object, then don't show the guide arrow
            guideArrow2D.gameObject.SetActive(false);//if the character is alreay close to the target object, then don't show the guide arrow

        }
    }

}
