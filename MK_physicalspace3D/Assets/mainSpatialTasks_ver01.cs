// let's implement multiple task in this one script for now
// familiarisation period (=ball picking)
// object-location memory test (treasure chest)
// distance esimtation test and so on..
// 2020.06.03 MK

using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class mainSpatialTasks_ver01 : MonoBehaviour {
	private int moveToNext=0;
	// GUI elements
	public GameObject UIcanvas;
	public Button nextButton;
	public Text text_top, text_fullscreen;
	public GameObject img_fullscreen;
	public Slider timerSlider;
	public GameObject distEstimateHolder;
	public Slider distEstSlider;
	public Image distEstCue1, distEstCue2;
	public Sprite[] cueList;


	// character related
	public Transform markerFlat1, marker3D;
	public Transform arrow2D, arrow3D;
	public Vector3[] posList;
	public Transform character, arrow;
	public Transform characterCamera;
	public float characterRadius=1.0f; // size of character for collision detection
	public Vector2 targetLoc;
	public Transform prop, prop2;
	public GameObject[] objList;
	public float distCollide;//distance for collision detection
	private string deli;
	public string subId;
	// Use this for initialization
	public int trial;
	// Environment related parameters (ideally this should be also placed somewhere else..)
	public int envType=1; //critical, whether the environment is slope or cylinder
	public float sqDim=25;
	public	float Radius=25/Mathf.PI;
		
	// below, I declared task order/parameters as public global variable for easy debugging purpose
	// I think for the readability of script, such public global variable should be minimally used
	// but unfortunately, debugging Unity is not trivial and public variables visible in the inspector view is the easiest way (at least to my knowledge)
	public float testfloat;


	public Vector3[] objLoc,startLoc;
	public int[] learnOrder;
	public string[] objName;
	private JSONNode taskparam;

	IEnumerator Start () {
		 //Get the path of the Game data folder
        string m_Path = Application.dataPath;
		Debug.Log("dataPath : " + m_Path);
		

		string the_JSON_string="";
		
	#if UNITY_WEBGL
		UnityWebRequest www=UnityWebRequest.Get(Application.dataPath+"/psub99_inputParam2.json");
		yield return www.SendWebRequest();
		if (www.isNetworkError||www.isHttpError)
			Debug.Log(www.error);
		else{
			Debug.Log(www.downloadHandler.text);
			the_JSON_string=www.downloadHandler.text;
		}
	#endif
		
		if (the_JSON_string=="") //if I fail to load the json task param from the server, find it from Resources directory
		{	TextAsset jsonTextAsset= Resources.Load<TextAsset>("psub99_inputParam");
			the_JSON_string=jsonTextAsset.text;
		}
		taskparam = JSON.Parse(the_JSON_string);
		learnOrder=json2int(taskparam["learnOrder"]);
		objName=json2string(taskparam["objName"]);
		objLoc=json2vector3(taskparam["objLoc"]);
		startLoc=json2vector3(taskparam["startLoc"]);
		
		
		GameObject obj=GameObject.Find("globalVariable");
		int run,sex,age;
		if (!obj)
		{	Debug.Log("globalVariable does not exist, so I will assume it is Misun self testing");
			subId="psub99";
			run=0; sex=1; age=30;
		}
		else{
			keepThisVariable scriptobj=obj.GetComponent("keepThisVariable") as keepThisVariable;
			subId=scriptobj.subId;
			run=scriptobj.run;
			sex=scriptobj.sex; age=scriptobj.age;
		}
		deli="\t";

		Vector2[] pos2Dlist=new Vector2[6];
		pos2Dlist[0]=new Vector2(0.0f,0.2f);
		pos2Dlist[1]=new Vector2(0.5f,0.8f);
		pos2Dlist[2]=new Vector2(0.9f,0.4f);
		pos2Dlist[3]=new Vector2(0.0f-1,0.2f);
		pos2Dlist[4]=new Vector2(0.5f-1,0.8f);
		pos2Dlist[5]=new Vector2(0.9f-1,0.4f);
		for (int i=0; i<pos2Dlist.Length;i++)
		{	Vector3 tmppos3D=conv2Dto3D(pos2Dlist[i]);
			Instantiate(prop.gameObject, tmppos3D,Quaternion.identity);
		}

		yield return null;
	
   	//	yield return propFollowingTask();
	//	yield return objectLocationLearnPhase();
	//	yield return objectLocationTestPhase();

	//	character.gameObject.SetActive(false);
	//	yield return objectDistEstimate();
	//	yield return endOfExp();
	}
	
	// Update is called once per frame
	
	void Update () {
	//	arrow2Dto3D(arrow2D);
	//	arrowControl()
	//	arrow.LookAt(prop.position);
	//	currentLocPart();
//	putMarkerOn3D();
//		putMarkerOn2D();
		Vector3[] initialLoc=new Vector3[7];
		initialLoc[0]=new Vector3(0.1f,5.04f,13.34f);//slope
		initialLoc[1]=new Vector3(282.7f,-6.7f,1.54f);//cylinder
		initialLoc[2]=new Vector3(256.3f,0.2f,-0.25f);//rect
		initialLoc[3]=new Vector3(226.6f,-9.7f,-8.2f);//sphere
		initialLoc[4]=new Vector3(188.5f,0.1f,-6.54f);//circle
		initialLoc[5]=new Vector3(273.4f,2.4f,-78.4f);// invert cylinder
/*		if (Input.GetKey(KeyCode.Alpha1))
		{	if (Input.GetKeyDown(KeyCode.Alpha2)) 
			{	Debug.Log("reset character position");
				character.position=new Vector3(0.1f,2.6f,-0.25f);
				character.rotation=Quaternion.Euler(0,0,0);
			}
		}
*/
		if (Input.GetKeyDown(KeyCode.Return)){
			string tmpstr="rand "+Random.Range(0,100);
			Debug.Log(tmpstr);
			StartCoroutine(save2file("save0717.txt", tmpstr));
		}
		if (Input.GetKeyDown(KeyCode.Alpha0))
		{	character.position=initialLoc[0];Debug.Log("press 1 MK");
		}
		
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{	character.position=initialLoc[1];Debug.Log("press 1 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{	character.position=initialLoc[2]; Debug.Log("press 2 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{	character.position=initialLoc[3];Debug.Log("press 3 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{	character.position=initialLoc[4];Debug.Log("press 4 MK");
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{	character.position=initialLoc[5];Debug.Log("press 5 MK-invert cylinder");
		}
	}
	/*
	void ArrowControl(){
		// current location, target location
		// calculate angle
		// rotate the angle
		// cast shadows on the ground
		float length1=25f;
		float hoverheight=1f;
		float radius1=7.96f;
		Vector3 charPos=character.position-character.up*hoverheight;
		
		if (charPos.x<-length1 & charPos.z<0 & charPos.z>-length1)
		{	
			float thetaChar=radius1*Mathf.Atan2(charPos.y,charPos.x+length1+radius1);
			float hChar=charPos.z;
			float thetaGoal=radius1*Mathf.Atan2(prop.position.y,prop.position.x+length1+radius1);
			float hGoal=prop.position.z;

			float distGuide=1.5f;
			float phi=Mathf.Atan2(hGoal-hChar,thetaGoal-thetaChar);
			float hGuide=hChar+distGuide*Mathf.Sin(phi);
			float thetaGuide=thetaChar+distGuide*Mathf.Cos(phi);
			float guideX=radius1*Mathf.Cos(thetaGuide/radius1)-length1-radius1;
			float guideY=radius1*Mathf.Sin(thetaGuide/radius1);
			float guideZ=hGuide;
			arrow.position=new Vector3(guideX,guideY,guideZ)+character.up*hoverheight;
			prop2.position=arrow.position;//+(arrow.position-character.position)*0.5f;
			arrow.LookAt(arrow.position+(arrow.position-character.position)*0.5f);
			//float arrowAng=Mathf.Rad2Deg*Mathf.Atan2(thetaChar*radius1-targetLoc.x, character.position.z-targetLoc.y);
			//arrow.localEulerAngles=new Vector3(90,arrowAng,0);
			Vector2 tmpvec1=new Vector2(thetaChar,hChar);
			Vector2 tmpvec2=new Vector2(thetaGoal,hGoal);
			Vector2 tmpvec3=new Vector2(thetaGuide,hGuide);
			text_top.text="vec1-vec2-vec3"+tmpvec1.ToString("F1")+","+tmpvec2.ToString("F1")+","+tmpvec3.ToString("F1")+", phi="+(phi*Mathf.Rad2Deg).ToString("F1");
		//	text_top.text="cylinder,theta="+(theta*Mathf.Rad2Deg)+", arrowAng="+arrowAng;
		}	
		if (charPos.x>-length1 & charPos.x<0 & charPos.z<0 & charPos.z>-length1)
		{	float arrowAng=Mathf.Rad2Deg*Mathf.Atan2(character.position.x-targetLoc.x, character.position.z-targetLoc.y);
			
		//	arrow.localEulerAngles=new Vector3(90, arrowAng,0);
			//arrow.LookAt(prop.position,Vector3.up);
			Vector2 tmpvec1=new Vector2(character.position.x,character.position.z);
			Vector2 tmpvec2=new Vector2(targetLoc.x,targetLoc.y);
			text_top.text="vec1-vec2"+tmpvec1.ToString("F1")+","+tmpvec2.ToString("F1")+", ang="+arrowAng.ToString("F1");
		//	text_top.text="square";
		}
			
		
	}*/
	void arrowControl(Vector2 norm2D_start, Vector2 norm2D_end, Transform guideArrow){
		// retrieve current location, target location on normalised space
		// then put the arrow between
		float guideDist=0.2f;
		Vector2 tmp2D=norm2D_start+(norm2D_end-norm2D_start)/Vector2.Distance(norm2D_end,norm2D_start)*guideDist;
		float ang=Mathf.Atan2(norm2D_end.y-norm2D_start.y, norm2D_end.x-norm2D_start.x);
		conv2Dto3D(tmp2D,ang*Mathf.Rad2Deg,guideArrow);

	}
	int currentLocPart(){
		float length1=25f; //length of square
		float radius1=7.96f; //radius of half cylinder
		int currentSection=0;
		if (character.position.x>-length1 & character.position.x<0 & character.position.z<0 & character.position.z>-length1)
		{	currentSection=1; // float square part
			text_top.text="square";
		}
		if (character.position.x<-length1 & character.position.z<0 & character.position.z>-length1)
		{	currentSection=2; // half cylinder part
			float tmpx=character.position.x+length1+radius1;
			float tmpy=character.position.y;
			float theta=Mathf.Atan2(tmpy,tmpx);
			text_top.text="cylinder,theta="+(theta*Mathf.Rad2Deg)+", h="+character.position.z;
		}		
		return currentSection;
	}
	void putMarkerOnFlattened(Vector2 normPos2D)
	{		marker3D.position=conv2Dto3D(normPos2D);		
	}
	void putMarkerOn3D(){
		float tmpx=markerFlat1.position.x/25.0f;
		float tmpy=(markerFlat1.position.z-87.5f)/25f;
		marker3D.position=conv2Dto3D(new Vector2(tmpx,tmpy));
	}
	void putMarkerOn2D(){
		Vector2 norm2D=conv3Dto2D(character.position-character.up);
		markerFlat1.position=new Vector3(norm2D.x*25,0,norm2D.y*25+87.5f);
	}
	Vector2 conv3Dto2D(Vector3 pos3D){
		// this should be adjusted when conv2Dto3D function has changed
		// this is a reciprocal function to conv2Dto3/d
		float tmpx=0;float tmpy=0;
		float sqDim=25;
		float Radius=25/Mathf.PI;
		
		switch (envType)
		{	case 1://slope
				if (pos3D.x<0)
				{	tmpx=pos3D.x/sqDim;
					tmpy=(pos3D.z-47.5f)/sqDim;
				}
				if (pos3D.x>=0)
				{	tmpx=pos3D.x/Mathf.Cos(Mathf.Deg2Rad*45)/sqDim;
					tmpy=(pos3D.z-47.5f)/sqDim;
				}				
				break;
			case 2: //cylinder
				if (pos3D.x<0)
				{	tmpx=pos3D.x/sqDim;
					tmpy=pos3D.z/sqDim;
				}
				if (pos3D.x>0.5)
				{	tmpx=Mathf.Acos(-(pos3D.x-0.5f-Radius)/Radius)/Mathf.PI;
					tmpy=pos3D.z/sqDim;
				}
				break;
		}
		return new Vector2(tmpx,tmpy);
	}

	void arrow2Dto3D(Transform arrow2D){
		float tmpx=arrow2D.position.x/25.0f; //dependent on scale of Unity
		float tmpy=(arrow2D.position.z-87.5f)/25f; //should check the offset in Unity
		Vector3 tmp1=conv2Dto3D(new Vector2(tmpx,tmpy), arrow2D.eulerAngles.y, arrow3D);

	}
	Vector3 conv2Dto3D(Vector2 normPos2D){
		return conv2Dto3D(normPos2D,0,this.transform);
	}
	Vector3 conv2Dto3D(Vector2 normPos2D,float rotY,Transform obj3D){
		// convert normalised 2D coordinate onto 3D coordinate in Unity
		// input should be x=(-1,1), y=(0,1)
		// when I adjust the size/offset of environment on Unity, I should adjust this part correctly
		// I also have to update "conv3Dto2D" function correctly
		// because it's not simple linear transformation matrix, I can't just use the inverse of rigic body transformatio matrix here
		float tmpx=0;float tmpy=0; float tmpz=0;
		
		Vector3 baseNormal=new Vector3(0,0,0);// base Euler angles on the surface
		switch (envType)
		{	case 1: // slope
				if (normPos2D.x<0){
					tmpx=normPos2D.x*sqDim+0f;
					tmpy=0f;
					tmpz=normPos2D.y*sqDim+47.5f;
					
					baseNormal=new Vector3(0,0,0);
				}
				if (normPos2D.x>=0){
					tmpx=normPos2D.x*Mathf.Cos(Mathf.Deg2Rad*45)*sqDim+0f;
					tmpy=normPos2D.x*Mathf.Sin(Mathf.Deg2Rad*45)*sqDim+0f;
					tmpz=normPos2D.y*sqDim+47.5f;
				
					baseNormal=new Vector3(0,0,45);
				}
				break;
			case 2: // half cylinder
				if (normPos2D.x<0){
					tmpx=normPos2D.x*sqDim+0f;
					tmpy=0f;
					tmpz=normPos2D.y*sqDim+0f;

					baseNormal=new Vector3(0,0,0);
				}
				if (normPos2D.x>=0){
					tmpx=-Mathf.Cos(normPos2D.x*Mathf.PI)*Radius+Radius+0.5f; //slight off set of 0.5 because step between the flat surface and half moon
					tmpy=Mathf.Sin(normPos2D.x*Mathf.PI)*Radius+0.5f;
					tmpz=normPos2D.y*sqDim+0f;

					baseNormal=new Vector3(0,0,(Mathf.PI/2-normPos2D.x*Mathf.PI)*Mathf.Rad2Deg);
				}				
				break;
			case 3: //curve2
				if (normPos2D.x<0){
					tmpx=normPos2D.x*sqDim+0f;
					tmpy=0f;
					tmpz=normPos2D.y*sqDim-40f;

					baseNormal=new Vector3(0,0,0);
				}
				if (normPos2D.x>=0& normPos2D.x<0.5f){
					tmpx=Mathf.Sin(normPos2D.x*Mathf.PI)*Radius; 
					tmpy=Radius-Mathf.Cos(normPos2D.x*Mathf.PI)*Radius;
					tmpz=normPos2D.y*sqDim-40f;

					baseNormal=new Vector3(0,0,(normPos2D.x*Mathf.PI)*Mathf.Rad2Deg);
				}				
				if (normPos2D.x>=0.5f& normPos2D.x<1f){
					tmpx=-Mathf.Cos((normPos2D.x-0.5f)*Mathf.PI)*Radius+2*Radius; 
					tmpy=Radius+Mathf.Sin((normPos2D.x-0.5f)*Mathf.PI)*Radius;
					tmpz=normPos2D.y*sqDim-40f;

					baseNormal=new Vector3(0,0,(Mathf.PI-normPos2D.x*Mathf.PI)*Mathf.Rad2Deg);
				}
				if (normPos2D.x>=1f){
					tmpx=2*Radius+(normPos2D.x-1)*sqDim;
					tmpy=2*Radius;
					tmpz=normPos2D.y*sqDim-40f;
					baseNormal=new Vector3(0,0,0);
				}				
				break;
			
		}

		Vector3 outPos3D=new Vector3(tmpx,tmpy,tmpz);
		
		obj3D.position=outPos3D;
		obj3D.eulerAngles=baseNormal;
		obj3D.Rotate(new Vector3(0,rotY,0),Space.Self);

		Debug.DrawLine(obj3D.position,obj3D.position-obj3D.up*2,Color.red);
		Debug.DrawLine(obj3D.position,obj3D.position+obj3D.forward*2,Color.red);

		return outPos3D;
	}


	IEnumerator endOfExp(){
		string runStartTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		nextButton.gameObject.SetActive(false);
		text_top.text="";
		string codeNumber="cherry";//should vary it later
		string surveycode=subId+"_"+codeNumber;
		string tmpstr="\nThis is the end of the experiment. Thanks so much for your participation!";
		tmpstr=tmpstr+"\n\nHere is the Surveycode for MTurk Workers: "+surveycode;
		tmpstr=tmpstr+"\n\nYou can now close this browser. Have a nice day!";
		text_fullscreen.text=tmpstr;	
		yield return null;
	}
	IEnumerator propFollowingTask(){
	//	Vector2[] pos2DList=json2vector2(taskparam["familiarisePropLoc"]); // load Vector3 prop locations
		Vector2[] pos2DList=new Vector2[4];
		pos2DList[1]=new Vector2(-0.5f,0.9f);
		pos2DList[2]=new Vector2(0.5f,0.1f);
		pos2DList[3]=new Vector2(0.7f,0.5f);	
		
		text_top.text="";
		img_fullscreen.SetActive(true); //put background image
		string tmptext="Familiarisation period:\nYou can rotate right/left, and look up/down with arrow key. To move forward or backward press W/D.";
		tmptext=tmptext+"\nLet's practice the movement. Move to the ball, once you get close enough to the ball, the ball will disappear and reappear somewhere else. Move to the ball again.";
		tmptext=tmptext+"\nRepeat this process until you are asked to stop (~few minutes)";
		tmptext=tmptext+"\nClick next to begin the familiarisation period";

		text_fullscreen.text=tmptext;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		nextButton.gameObject.SetActive(false);
		img_fullscreen.SetActive(false);
		text_fullscreen.text="";

		Debug.Log("start of propFollowingTask()");
		text_top.text="Find the ball and move to it";
		float inittime=Time.time;
		float timelimit=15*60;// max 15min
		
		string savefn=subId+"_familiarisation_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		string savetext="";
		trial=1;
		while (Time.time-inittime<timelimit & trial<posList.Length){
			prop.position=posList[trial]; // place the prop (e.g. traffic cone) to predefined location
			
			RaycastHit hit;
			Vector3 p1=character.position-character.up*1; //capsule cast bottom position, depending on the hover distance between the character center and ground
			Vector3 p2=character.position+character.up*1; //capsule cast top position 
			
			Debug.DrawLine(p1,p1+character.forward*2,Color.red);
			Debug.DrawLine(p2,p2+character.forward*2,Color.blue);
		
			if (Physics.CapsuleCast(p1, p2,characterRadius, character.forward, out hit, distCollide))
			{	Debug.Log("hit forward"); // if character face any wall
				if (hit.collider.tag == "Respawn") {//If character get sufficiently close to the prop
					Debug.Log("touch:"+hit.collider);
					trial++;
				}
			}// then move the object
			
			float cameraPitch=characterCamera.localEulerAngles.x;
			savetext=savetext+Time.time.ToString("F3")+deli+MK2string(character.position)+deli+MK2string(character.rotation)+deli+cameraPitch.ToString("F1")+"\n";

			if (Input.GetKeyDown(KeyCode.Escape)) break; //break condition just in case (for debugging) this should be removed before posting online in case subject accidenatlly press the wrong button
			yield return null; // repeat every 0.1s
		}
		StartCoroutine(save2file(savefn,savetext));
		// tell subjects that he completed the task and guide to the next task.. (how? full screen text? top text?)
		prop.gameObject.SetActive(false);
		text_top.text="End of the movement practice phase. Click Next to continue the experiment";
		Debug.Log("end of propFollowingTask()");
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		
	}
	
	IEnumerator objectDistEstimate(){
		text_top.text="";
		img_fullscreen.SetActive(true); //put background image
		string tmptext="In this task, you should estimate how long it would take to move from one location to other location";
		tmptext=tmptext+"\nYou will adjust the slider to report 'very close' to 'very far'.";
		tmptext=tmptext+"\nClick next to begin";
		text_fullscreen.text=tmptext;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		nextButton.gameObject.SetActive(false);
		text_fullscreen.text="";

		distEstimateHolder.SetActive(true); 
		Vector2[] distEstOrder=json2vector2(taskparam["distEstOrder"]);
		string savefnSum=subId+"_DistEst1_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		for (trial=1; trial<distEstOrder.Length; trial++){
			text_top.text="How far are these two items? Adjust the slider and press the Spacebar to confirm";
			int item1=Mathf.RoundToInt(distEstOrder[trial].x);
			int item2=Mathf.RoundToInt(distEstOrder[trial].y);

			distEstCue1.sprite=cueList[item1];
			distEstCue2.sprite=cueList[item2];
			float inittime=Time.time;
			float timelimit1=60f; // X sec to find the goal
			float RT=-1;
			while(Time.time-inittime<timelimit1 & !Input.GetKeyDown(KeyCode.Space))
			{//	timerSlider.value=(Time.time-inittime)/timelimit1;
				yield return null;
			}
			RT=Time.time-inittime;
			string savetextSum="";
			savetextSum=trial+deli+item1+deli+item2+deli+distEstSlider.value+deli+RT;
			StartCoroutine(save2file(savefnSum,savetextSum));

			distEstCue1.sprite=null;
			distEstCue2.sprite=null;
			yield return new WaitForSeconds(1);
			
		}
		text_top.text="End of distance estimation task. Click next";
		distEstimateHolder.SetActive(false);

		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
	}
	IEnumerator objectLocationTestPhase(){
		text_top.text="";
		img_fullscreen.SetActive(true); //put background image
		string tmptext="This is test phase. You should move to the location of item that you learned previously.";
		tmptext=tmptext+"\nClick next to begin";

		text_fullscreen.text=tmptext;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		nextButton.gameObject.SetActive(false);
		img_fullscreen.SetActive(false);
		text_fullscreen.text="";

		// 1. Load object identity/location first
		for (int j=1;j<objLoc.Length; j++)
		{	objList[j].transform.position=objLoc[j];
			objList[j].transform.rotation=Random.rotation;
			objList[j].SetActive(false);
		}
		string savefnSum=subId+"_ObjLoc_TestPhaseSum_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		string savefnTraj=subId+"_ObjLoc_TestPhaseTraj_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";

		float sumError=0f; //to calculate the mean distance error at the end of task
		int nCompleteTrial=0;
		for (trial=1; trial<learnOrder.Length;trial++){
			GameObject currentObj=objList[learnOrder[trial]];
			// temporarily hide the view because subject will be relocated
			character.position=startLoc[trial];
			img_fullscreen.SetActive(true);
			yield return new WaitForSeconds(2f);
			img_fullscreen.SetActive(false);

			// retriev phase
			currentObj.SetActive(false);
			text_top.text="Go to the "+objName[learnOrder[trial]]+", then press the Spacebar";
			
			float inittime=Time.time;
			float timelimit1=60f; // X sec to find the goal
			string savetextSum="";
			string savetextTraj="";
			while(Time.time-inittime<timelimit1 & !Input.GetKeyDown(KeyCode.Space))
			{	timerSlider.value=(Time.time-inittime)/timelimit1;
				float cameraPitch=characterCamera.localEulerAngles.x;
				savetextTraj=savetextTraj+trial+deli+Time.time.ToString("F3")+deli+MK2string(character.position)+deli+MK2string(character.rotation)+deli+cameraPitch.ToString("F1")+"\n";
				yield return null;
			}
			// should I give feedback during the test phase? if I want to stop further learning then I shouldn't.
			currentObj.SetActive(true);
			float distEucl=-1;
			if (Input.GetKeyDown(KeyCode.Space))
			{	distEucl=Vector3.Distance(character.position, currentObj.transform.position);
				text_top.text="Error is "+distEucl.ToString("F1");
				sumError=sumError+distEucl;
				nCompleteTrial++;
			}
			else{
				text_top.text="Timeout";
			}
			float RT=Time.time-inittime;
			savetextSum=trial+deli+MK2string(currentObj.transform.position)+deli+MK2string(character.position)+deli+distEucl.ToString("F1")+deli+RT;
			
			StartCoroutine(save2file(savefnSum,savetextSum));
			StartCoroutine(save2file(savefnTraj,savetextTraj));
			yield return  new WaitForSeconds(3f);
			currentObj.SetActive(false);
		}
		string tmpstring="End of test phase, mean error="+(sumError/nCompleteTrial).ToString("F1");
		tmpstring=tmpstring+"\nClick Next to continue the experiment";
		text_top.text=tmpstring;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
	}
	
	IEnumerator objectLocationLearnPhase(){

		text_top.text="";
		img_fullscreen.SetActive(true); //put background image
		string tmptext="In this task, you will learn the location of few items. You should remember where each item is located for the later memory test.";
		tmptext=tmptext+"\nClick next to begin";

		text_fullscreen.text=tmptext;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		nextButton.gameObject.SetActive(false);
		img_fullscreen.SetActive(false);
		text_fullscreen.text="";

		// 1. Load object identity/location first
		for (int j=1;j<objLoc.Length; j++)
		{	objList[j].transform.position=objLoc[j];
			objList[j].transform.rotation=Random.rotation;
			objList[j].SetActive(false);
		}
		
		// encoding phase
		// place one object
		// wait subject to grab the object
		// give some seconds to encode the location
		// remove the object
		// wait subject to press the button
		// show the correct location of the item
		
		// load object locationss

		string savefnSum=subId+"_ObjLoc_LearnPhaseSum_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		string savefnTraj=subId+"_ObjLoc_LearnPhaseTraj_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";

		float sumError=0f; //to calculate the mean distance error at the end of task
		int nCompleteTrial=0;
		for (trial=1; trial<learnOrder.Length;trial++){
			GameObject currentObj=objList[learnOrder[trial]];
			currentObj.SetActive(true);
			text_top.text="Move to the picture box";
			while(!Input.GetKeyDown(KeyCode.Escape))
			{	RaycastHit hit;
				Vector3 p1=character.position-character.up*1; //capsule cast bottom position, depending on the hover distance between the character center and ground
				Vector3 p2=character.position+character.up*1; //capsule cast top position 
				Debug.DrawLine (p1,p1+character.forward, Color.blue);
				Debug.DrawLine (p2,p2+character.forward, Color.blue);
				
				if (Physics.CapsuleCast(p1, p2,characterRadius, character.forward, out hit, distCollide))
				{	if (hit.collider.tag=="Respawn")
						break;
				}
				yield return null;
			}
			text_top.text="Remember this location";
			float inittime=Time.time;
			float timelimit1=5f; // give 5sec for encoding
			while(Time.time-inittime<timelimit1)
			{//	timerSlider.value=(Time.time-inittime)/timelimit1;
				yield return null;
			}
			// temporarily hide the view because subject will be relocated
			character.position=startLoc[trial];
			img_fullscreen.SetActive(true);
			yield return new WaitForSeconds(2f);
			img_fullscreen.SetActive(false);

			// retriev phase
			currentObj.SetActive(false);
			text_top.text="Go to the location and press the Spacebar";
			
			inittime=Time.time;
			timelimit1=60f; // X sec to find the goal
			string savetextSum="";
			string savetextTraj="";
			while(Time.time-inittime<timelimit1 & !Input.GetKeyDown(KeyCode.Space))
			{	timerSlider.value=(Time.time-inittime)/timelimit1;
				float cameraPitch=characterCamera.localEulerAngles.x;
				savetextTraj=savetextTraj+trial+deli+Time.time.ToString("F3")+deli+MK2string(character.position)+deli+MK2string(character.rotation)+deli+cameraPitch.ToString("F1")+"\n";
				yield return null;
			}
			// feedback phase
			currentObj.SetActive(true);
			float distEucl=-1;
			if (Input.GetKeyDown(KeyCode.Space))
			{	distEucl=Vector3.Distance(character.position, currentObj.transform.position);
				text_top.text="Error is "+distEucl.ToString("F1");
				sumError=sumError+distEucl;
				nCompleteTrial++;
			}
			else{
				text_top.text="Timeout";
			}
			float RT=Time.time-inittime;
			savetextSum=trial+deli+MK2string(currentObj.transform.position)+deli+MK2string(character.position)+deli+distEucl.ToString("F1")+deli+RT;
			
			StartCoroutine(save2file(savefnSum,savetextSum));
			StartCoroutine(save2file(savefnTraj,savetextTraj));
			yield return  new WaitForSeconds(3f);
			currentObj.SetActive(false);
		}
		string tmpstring="End of encoding phase, mean error="+(sumError/nCompleteTrial).ToString("F1");
		tmpstring=tmpstring+"\nClick Next to continue the experiment";
		text_top.text=tmpstring;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;
		// retrieval phase
	}
	
	//////////// utility function //////////////////////////
	public void ClickedNext(){
		moveToNext=1;
		Debug.Log("clickedNext()");
		EventSystem.current.SetSelectedGameObject(null);// to prevent the color of "pressed button" stay (this is related to keyboard navigation of UI. By deselect the object, I disable participants to choose different button using arrow keys)

	}
	string MK2string(Quaternion tmpvec){
		string output;
		output=tmpvec.x.ToString("F3")+deli+tmpvec.y.ToString("F3")+deli+tmpvec.z.ToString("F3")+deli+tmpvec.w.ToString("F3");
		return output;
	}string MK2string(Vector3 tmpvec){
		string output;
		output=tmpvec.x.ToString("F3")+deli+tmpvec.y.ToString("F3")+deli+tmpvec.z.ToString("F3");
		return output;
	}
	string MK2string(Vector2 tmpvec){
		string output;
		output=tmpvec.x.ToString("F3")+deli+tmpvec.y.ToString("F3");
		return output;
	}
	IEnumerator save2file(string fn,string textToWrite){
		Debug.Log("save2file");
		int whereToSave=0; // default is save to local directory
		#if UNITY_WEBGL
			whereToSave=1; // for web build, save it into server
		#endif
		//I can also manually change this behaviour if I want to test the server save behaviour in the Editor
		whereToSave=1;
		if (whereToSave==0) //local storage
		{   string saveLocalDir="./";
			StreamWriter sw=new StreamWriter(saveLocalDir+fn,true);
			sw.WriteLine(textToWrite);
			sw.Close();
		}
		if (whereToSave==1){ //Server storage
			WWWForm form= new WWWForm();
			form.AddField("fn", fn);
			form.AddField("content", textToWrite+"\n");

			//var www = UnityWebRequest.Post("http://vm-mkim-1.cbs.mpg.de/testPHP/php-example.php", form);
			var www = UnityWebRequest.Post("http://wwwuser.gwdg.de/~misun.kim01/testPHP/php-example.php", form);
			{
				yield return www.SendWebRequest();

				if (www.isNetworkError) //it only return system error (DNS, socket) not the error from server side (404/Not Found)
				{
					Debug.Log(www.error);
					text_top.text="Oops! Server problem, stop the experiment and contact the researcher";
				}
				else
				{
					Debug.Log("Form upload complete!");
					text_top.text="";
				}
			}
		}
	}

	int[] json2int(JSONNode tmpnode){
		int[] newarray=new int[tmpnode.Count];
		for (var i=0;i<tmpnode.Count;i++) newarray[i]=tmpnode[i];
		return newarray;
	}
	float[] json2float(JSONNode tmpnode){
		float[] newarray=new float[tmpnode.Count];
		for (var i=0;i<tmpnode.Count;i++) newarray[i]=tmpnode[i];
		return newarray;
	}
	Vector3[] json2vector3(JSONNode tmpnode){
		Vector3[] newarray=new Vector3[tmpnode.Count];
		for (var i=0;i<tmpnode.Count;i++) newarray[i]=tmpnode[i];
		return newarray;
	}
	Vector2[] json2vector2(JSONNode tmpnode){
		Vector2[] newarray=new Vector2[tmpnode.Count];
		for (var i=0;i<tmpnode.Count;i++) newarray[i]=tmpnode[i];
		return newarray;
	}
	string[] json2string(JSONNode tmpnode){
		string[] newarray=new string[tmpnode.Count];
		for (var i=0;i<tmpnode.Count;i++) newarray[i]=tmpnode[i];
		return newarray;
	}
}
