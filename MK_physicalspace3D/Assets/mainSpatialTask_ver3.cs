// let's implement multiple task in this one script for now
// familiarisation period (=ball picking)
// object-location memory test (treasure chest)
// distance esimtation test and so on..
// 2020.06.03 MK

// to publish the experiment in Prolific, I made a several change
// e.g. I integrate the consent form within this script and remove sex/age demo info (because that will be provided by Prolific)
// e.g. I also allocate random subject ID and extract Prolific ID from URL
// 2020.08.24 MK

using SimpleJSON;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
// git commit test 2020/08/17 16:36
public class mainSpatialTask_ver3 : MonoBehaviour {
	[DllImport("__Internal")]
	private static extern double GetDPI();
	public int moveToNext=0;
	// GUI elements
	public Button nextButton;
	public Text text_top, text_fullscreen, text_topleft, text_topright, text_warning;
	public GameObject img_fullscreen;

	public GameObject debriefHolder, consentHolder,buttonProlific;
	public GameObject[] debriefQ, debriefQ2;//debrief dropdown questions(debrieefQ) and debrief text inputfield questions(debriefQ2)
	
	public Slider timerSlider;
	public GameObject distEstimateHolder, distEstHolder_2AFC;
	public Slider distEstSlider;
	public Image imgCenter, distEstCue1, distEstCue2,distEstEgoImg1,distEstEgoImg2,distEst2AFCImg1,distEst2AFCImg2,distEst2AFCImg3;
	public Sprite[] cueList, feedbackSmileSprites,cueList_sub, medalSprite;


	// character related
	public bool rotateAllow, translateAllow;
	public int moveConstraint=0;
	public float rotateSpeed=90;
	public float translateSpeed=4;
	public Transform markerFlat1, marker3D;
	public Transform guideArrow2D, guideArrow3D;
	public Vector3[] posList;
	public Transform character, char2D;
	public Transform characterCamera;
	public float characterRadius=0.25f; // size of character for collision detection
	public Vector2 targetLoc;
	public Transform prop2D, prop3D;
	public float distCollide;//distance for collision detection
	private string deli;
	public string subId;
	// Use this for initialization
	public Vector3 curr_norm2D;// this is the normalised coordinate and direction of the character
	public int trial, maxtrial;
	// Environment related parameters (ideally this should be also placed somewhere else..)
	public int envType=1; //critical, whether the environment is slope or cylinder
	public float sqDim=25;
	public	float Radius=25/Mathf.PI;
		
	// below, I declared task order/parameters as public global variable for easy debugging purpose
	// I think for the readability of script, such public global variable should be minimally used
	// but unfortunately, debugging Unity is not trivial and public variables visible in the inspector view is the easiest way (at least to my knowledge)
	public float testfloat;
	public float pointAccum;

	public Vector3[] objLoc,startLoc;
	public int[] learnOrder;
	public GameObject[] objList_sub, objList;
	public string[] objName_sub, objName;
	private JSONNode taskparam;
	public int[] debugInt;
	public Vector2[] debugVec2;
	public Vector3[] debugVec3;
	int sex,age, subSuffix;
	public GameObject corridor_floor, corridor_openWalls,corridor_closedWalls, curve_floor, curve_openWalls,curve_closedWalls;
	int isOpenEnv;	

	private string currentLocalTime,currentGMTime;
	public string overviewFn;// text file for summary of start/end of experiment
	IEnumerator Start () {

		//int subNum=1;
		int subNum=Random.Range(1,50);
		subId="msub"+subNum.ToString("D2");
		subSuffix=Random.Range(100,999);//at the beginning of each experiment, random 3digit number is assigned, it should help me to distinguish each participant (in addition to their offiical subId)
		overviewFn=subId+"_"+subSuffix+"_overview_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";

		pointAccum=0;//bonus point start at 0;
		text_topleft.text="";
		deli="\t";
		isOpenEnv=1;
		EnvironmentToggle(isOpenEnv,0);
	
	
		string PID="null";
		string fullURL="";
	#if UNITY_WEBGL && !UNITY_EDITOR
		// extract Prolific ID from URL
		var parameters=URLParameters.GetSearchParameters();
		fullURL=URLParameters.Href;
		Debug.Log("full URL="+fullURL);
		if (parameters.TryGetValue("PID", out PID)){
			Debug.Log("PID="+PID);
		}
		else
		{	Debug.Log("can't find PID=null");
			text_warning.gameObject.SetActive(true);
			text_warning.text="Fatal error! can't proceed to experiment. Contact the experimenter Dr.Misun Kim (mkim@cbs.mpg.de)";	
			PID="null";
			yield return new WaitForSeconds(300);
		}
	#endif
	
		string tmptext=fullURL+"\n"+PID;
		yield return getTime();
		tmptext=tmptext+"\n"+Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of exp";
		StartCoroutine(save2file(overviewFn,tmptext));
	
		yield return consentPhase();

		
	//	text_warning.gameObject.SetActive(false);
		rotateAllow=true; translateAllow=true;

		string the_JSON_string="";
		
	#if UNITY_WEBGL
		UnityWebRequest www=UnityWebRequest.Get(Application.dataPath+"/"+subId+"_inputParam.json");
		yield return www.SendWebRequest();
		if (www.isNetworkError||www.isHttpError)
			Debug.Log(www.error);
		else{
			Debug.Log(www.downloadHandler.text);
			the_JSON_string=www.downloadHandler.text;
			Debug.Log("load Web/"+subId+"_inputParam");
		}
	#endif
		
		if (the_JSON_string=="") //if I fail to load the json task param from the server, find it from Resources directory
		{	TextAsset jsonTextAsset= Resources.Load<TextAsset>(subId+"_inputParam");
			if (jsonTextAsset!=null){
				Debug.Log("load Resources/"+subId+"_inputParam");
				the_JSON_string=jsonTextAsset.text;
			}
			else{
				text_warning.gameObject.SetActive(true);
				text_warning.text="Fatal error! can't proceed to experiment. Contact the experimenter Dr.Misun Kim (mkim@cbs.mpg.de)";	
				Debug.Log("fail to load the input csv file");
				
			}
		}
		taskparam = JSON.Parse(the_JSON_string);
		

		distCollide=1f;

		moveConstraint=1;//by default, I yoke the move on 2D flattenend and 3D

		
		curr_norm2D=phy2DtoNorm2D(char2D);
		yield return initialiseObjList();

		yield return startOfExp();	
		yield return propFollowingTask();
		yield return objectLocationLearnPhase();

		learnOrder=json2int(taskparam["objlocTestRun1"]["learnOrder"]); // load Vector3 prop locations
		startLoc=json2vector3(taskparam["objlocTestRun1"]["startLoc"]); // load Vector3 prop locations
		yield return objectLocationTestPhase(learnOrder,startLoc,1);

		learnOrder=json2int(taskparam["objlocTestRun2"]["learnOrder"]); // load Vector3 prop locations
		startLoc=json2vector3(taskparam["objlocTestRun2"]["startLoc"]); // load Vector3 prop locations
		yield return objectLocationTestPhase(learnOrder,startLoc,2);

		yield return objectDistEstimate_egocentric();
		yield return objectDistEstimate_pairwise();
		yield return objectDistEstimate_2AFC();
	
		yield return debriefPhase();
		yield return endOfExp();
	
	//	yield return triangleCompletionTask();
	}
	
	IEnumerator initialiseObjList(){
		// set the object identity for each participant differently
		// using the base object identity, name and then sort it accordingly
		int[] objIdentity=json2int(taskparam["objIdentity"]);
		
		objList_sub=new GameObject[objList.Length];// subject specific object list
		objName_sub=new string[objName.Length];
		cueList_sub=new Sprite[cueList.Length];
		objLoc=json2vector3(taskparam["objLoc"]); // load Vector3 prop locations

		string strObjName="";
		for (int i=1;i<objIdentity.Length;i++){
			objList_sub[i]=objList[objIdentity[i]];
			objName_sub[i]=objName[objIdentity[i]];
			cueList_sub[i]=cueList[objIdentity[i]];
			strObjName=strObjName+"\n"+i+deli+MK2string(objLoc[i])+deli+objName_sub[i];
		}

		string savefn=subId+"_"+subSuffix+"_objIdentity_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";
		string demostr="sex"+sex+",age"+age+",isOpenEnv"+isOpenEnv;
		
		StartCoroutine(save2file(savefn,demostr+"\n"+strObjName));

		yield return null;
		/*
		// place some cone object
		// then set start and target location for goal?
		Debug.Log("initialise() in mainSpatialTask_ver2.cs");
		Vector3[] pos2Dlist=new Vector3[10];
		pos2Dlist[0]=new Vector3(0.1f,0.9f, 0f);
		pos2Dlist[1]=new Vector3(0.9f,0.9f, 0f);
		pos2Dlist[2]=new Vector3(0.9f,0.1f, 0f);
		
		pos2Dlist[3]=new Vector3(0.63f,0.63f,0f);
		pos2Dlist[4]=new Vector3(0.36f,0.36f,0f);


		pos2Dlist[5]=new Vector3(-0.1f,0.1f, 0f);
		pos2Dlist[6]=new Vector3(-0.9f,0.1f, 0f);
		pos2Dlist[7]=new Vector3(-0.9f,0.9f, 0f);
		
		pos2Dlist[8]=new Vector3(-0.63f,0.37f,0f);
		pos2Dlist[9]=new Vector3(-0.36f,0.64f,0f);
		
		for (int i=0; i<pos2Dlist.Length;i++)
		{	GameObject tmpObj=Instantiate(prop2D.gameObject, Vector3.zero,Quaternion.identity);
			norm2DtoPhy2D(pos2Dlist[i],tmpObj.transform);
			tmpObj=Instantiate(prop3D.gameObject, Vector3.zero,Quaternion.identity);
			norm2DtoPhy3D(pos2Dlist[i],tmpObj.transform);
			//tmpObj=Instantiate(prop3D.gameObject, Vector3.zero,Quaternion.identity);
			
			Vector3 tmpVec3=new Vector3(1,0,0);
		//	norm2DtoPhy3D(pos2Dlist[i]-tmpVec3,tmpObj.transform);
			
		}
		*/
	}
	void EnvironmentToggle(int IsOpen, int IsCorridor){
		// default: closed environment
		// closed environment for main task:
		// closed environmetn for ego dist task:
		// transparent environment for main task: I should 1)deactivate closedCurve, corridor and 2)activate openCurve
		// transparent environment for ego dist task: I should deactivate 1)all curve 2)activate crridor with openCorridor
		if (IsOpen==1 & IsCorridor==1)
		{	corridor_floor.SetActive(true); corridor_openWalls.SetActive(true); corridor_closedWalls.SetActive(false);
			curve_floor.SetActive(false);
		}
		if (IsOpen==1 & IsCorridor==0)
		{	corridor_floor.SetActive(false);
			curve_floor.SetActive(true); curve_openWalls.SetActive(true);curve_closedWalls.SetActive(false);
		}
		if (IsOpen==0)
		{	corridor_floor.SetActive(true); corridor_openWalls.SetActive(false);corridor_closedWalls.SetActive(true);
			curve_floor.SetActive(true); curve_openWalls.SetActive(false); curve_closedWalls.SetActive(true);
		}


	}
	void Update () {
		if (moveConstraint==1) // update the locationon the flattened surface(2D) and then translate into 3D
		{
		/*	if (Input.GetKeyDown(KeyCode.Alpha3))
				StartCoroutine(simulTrans(new Vector3(-0.8f,0.2f,45), new Vector3(-0.4f,0.6f,45)));			
			if (Input.GetKeyDown(KeyCode.Alpha4))
				StartCoroutine(simulTrans(new Vector3(0.2f,0.2f,45), new Vector3(0.6f,0.6f,45)));
			if (Input.GetKeyDown(KeyCode.Alpha5))
				StartCoroutine(simulRot(new Vector3(-0.8f,0.2f,45), new Vector3(-0.8f,0.2f,135)));		
			if (Input.GetKeyDown(KeyCode.Alpha6))
				StartCoroutine(simulRot(new Vector3(0.2f,0.2f,45), new Vector3(0.2f,0.2f,135)));
		*/	
			if (rotateAllow){	
				char2D.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed*Time.deltaTime, 0);
			}
			
			Vector3 old_2Dpos=char2D.position;
			if (translateAllow){
				if (Input.GetKey(KeyCode.UpArrow))
				{	char2D.Translate(Vector3.forward*translateSpeed*Time.deltaTime);
				}
				if (Input.GetKey(KeyCode.DownArrow))
				{	char2D.Translate(-Vector3.forward*translateSpeed*Time.deltaTime);
				}
			}
			curr_norm2D=phy2DtoNorm2D(char2D);//extract the normalised 2D coordinates and direction from arrow on 2D
			if(curr_norm2D.x<-0.98f | curr_norm2D.x>0.98f | curr_norm2D.y<0.02f | curr_norm2D.y>0.98f) // if the movement makes subject outside the range of arena, move back to where they were
			{	char2D.position=old_2Dpos;
				curr_norm2D=phy2DtoNorm2D(char2D);
			}
			
			norm2DtoPhy3D(curr_norm2D,character);// place character on 3D location using the normalised 2D coordinate and facing direction 
		}
		if (moveConstraint==2)//when they move along the corridor for the egocentric distance estimation task
		{	if (Input.GetKey(KeyCode.UpArrow))
			{
				character.Translate(Vector3.forward*translateSpeed*Time.deltaTime);
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{	character.Translate(-Vector3.forward*translateSpeed*Time.deltaTime);
			}
			float xPos=Mathf.Clamp(character.position.x,-24f,38.7f);
			character.position=new Vector3(xPos,character.position.y,character.position.z);
			
		}
		if (moveConstraint==0){// free movement
			RaycastHit hit;
			Vector3 p1=character.position+character.up*0.2f; //capsule cast bottom position, depending on the hover distance between the character center and ground
			Vector3 p2=character.position+character.up*1; //capsule cast top position 
			
			if (Input.GetKey(KeyCode.UpArrow)&translateAllow)
			{	if (Physics.CapsuleCast(p1,p2,characterRadius,character.forward,out hit, distCollide))
					Debug.Log("touch:"+hit.collider);
				else
					character.Translate(Vector3.forward*translateSpeed*Time.deltaTime);
			}
		/*	if (Input.GetKey(KeyCode.DownArrow)&translateAllow)
			{	if (Physics.CapsuleCast(p1,p2,characterRadius,-character.forward,out hit, distCollide))
					Debug.Log("touch:"+hit.collider);
				
				else
					character.Translate(-Vector3.forward*translateSpeed*Time.deltaTime);
			}
		*/	if (rotateAllow)
				character.Rotate(0,Input.GetAxis("Horizontal")*rotateSpeed*Time.deltaTime,0);
			
			
		}
		
		if (Input.GetKey(KeyCode.F1))
		{	if(Input.GetKeyDown(KeyCode.F2))
			{	trial=maxtrial;
				text_top.text="jump to the last trial";
			}//set the trial number to the max trial so that I can jump on to the next task
		}
		if (Input.GetKey(KeyCode.Alpha9))
		{	if (Input.GetKeyDown(KeyCode.Alpha0))
				Debug.Log(GetScreenInfo());
		}
		

/*		if (Input.GetKeyDown(KeyCode.Alpha1)) //this should be removed
			EnvironmentToggle(1,1);
		if (Input.GetKeyDown(KeyCode.Alpha2))
			EnvironmentToggle(1,0);
		if (Input.GetKeyDown(KeyCode.Alpha3))
			EnvironmentToggle(0,1);
		if (Input.GetKeyDown(KeyCode.Alpha4))
			EnvironmentToggle(0,0);

		if (Input.GetKeyDown(KeyCode.Alpha2))
		{	RenderSettings.fog=true;
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{	RenderSettings.fog=false;
		}
*/
/*		if (Input.GetKeyDown(KeyCode.Return)){
			string tmpstr="rand "+Random.Range(0,100);
			Debug.Log(tmpstr);
			StartCoroutine(save2file("save0717.txt", tmpstr));
		}
*/	}
	
	IEnumerator simulTrans(Vector3 start, Vector3 target){
		//
		float distSq=Mathf.Pow(start.x-target.x,2)+Mathf.Pow(start.y-target.y,2);
		float dur=Mathf.Sqrt(distSq)/(translateSpeed/25);
		float timeinit=Time.time;
		while (Time.time-timeinit<=dur)
		{	float t=(Time.time-timeinit)/dur;
			Vector3 tmpPos2D;
			tmpPos2D.x=Mathf.Lerp(start.x,target.x,t);
			tmpPos2D.y=Mathf.Lerp(start.y,target.y,t);
			tmpPos2D.z=start.z;
			norm2DtoPhy2D(tmpPos2D,char2D);
			yield return null;
		}
	}
IEnumerator simulRot(Vector3 start, Vector3 target){
		//
		float dur=(target.z-start.z)/(rotateSpeed);
		float timeinit=Time.time;
		while (Time.time-timeinit<=dur)
		{	float t=(Time.time-timeinit)/dur;
			Vector3 tmpPos2D;
			tmpPos2D.x=start.x;
			tmpPos2D.y=start.y;
			tmpPos2D.z=Mathf.Lerp(start.z,target.z,t);
			norm2DtoPhy2D(tmpPos2D,char2D);
			yield return null;
		}
	}
	void placeGuideArrow(Vector3 start,Vector3 target){
		float currentDistSq=Mathf.Pow(start.x-target.x,2)+Mathf.Pow(start.y-target.y,2);
		float minDist=0.2f;
		//text_top.text="currentDist="+Mathf.Sqrt(currentDistSq).ToString("F3");
		if (currentDistSq>minDist*minDist)
		{	
			guideArrow3D.gameObject.SetActive(true);//if the character is alreay close to the target object, then don't show the guide arrow
			guideArrow2D.gameObject.SetActive(true);//if the character is alreay close to the target object, then don't show the guide arrow

			float tmpang=Mathf.Atan2(target.x-start.x,target.y-start.y);
			float guideDist=0.05f;
			float tmpx=start.x+guideDist*Mathf.Sin(tmpang);
			float tmpy=start.y+guideDist*Mathf.Cos(tmpang);
			norm2DtoPhy3D(new Vector3(tmpx,tmpy,tmpang*Mathf.Rad2Deg),guideArrow3D);
			norm2DtoPhy2D(new Vector3(tmpx,tmpy,tmpang*Mathf.Rad2Deg),guideArrow2D);
		}
		else
		{
			guideArrow3D.gameObject.SetActive(false);//if the character is alreay close to the target object, then don't show the guide arrow
			guideArrow2D.gameObject.SetActive(false);//if the character is alreay close to the target object, then don't show the guide arrow
			
		}
	}
	
	Vector3 phy2DtoNorm2D(Transform obj3D){
		// this scale and offset should be adjusted if I change the size of objects in Unity
		
		float tmpx=obj3D.position.x/25f; 
		float tmpy=(obj3D.position.z-87.5f)/25f;
		float ang=obj3D.eulerAngles.y;
		return new Vector3(tmpx,tmpy,ang);
	}
	void norm2DtoPhy2D(Vector3 normPos2D, Transform obj3D){
		// this scale and offset should be adjusted if I change the size of objects in Unity
		float tmpx=normPos2D.x*25f;
		float tmpy=0.5f;
		float tmpz=normPos2D.y*25f+87.5f;
		obj3D.position=new Vector3(tmpx,tmpy,tmpz);
		float rotY=normPos2D.z;
		obj3D.eulerAngles=new Vector3(0,rotY,0);
	}
	Vector3 norm2DtoPhy3D(Vector3 normPos2D,Transform obj3D){
		// convert normalised 2D coordinate onto 3D coordinate in Unity
		// input should be x=(-1,1), y=(0,1)
		// when I adjust the size/offset of environment on Unity, I should adjust this part correctly
		// I also have to update "conv3Dto2D" function correctly
		// because it's not simple linear transformation matrix, I can't just use the inverse of rigic body transformatio matrix here
		float tmpx=0;float tmpy=0; float tmpz=0;
		float rotY=normPos2D.z;
		Vector3 baseNormal=new Vector3(0,0,0);// base Euler angles on the surface
		switch (envType)
		{	case 1: // slope
				if (normPos2D.x<0){
					tmpx=normPos2D.x*sqDim+0f;
					tmpy=0f;
					tmpz=normPos2D.y*sqDim-70f;
					
					baseNormal=new Vector3(0,0,0);
				}
				if (normPos2D.x>=0){
					tmpx=normPos2D.x*Mathf.Cos(Mathf.Deg2Rad*45)*sqDim+0f;
					tmpy=normPos2D.x*Mathf.Sin(Mathf.Deg2Rad*45)*sqDim+0f;
					tmpz=normPos2D.y*sqDim-70f;
				
					baseNormal=new Vector3(0,0,45);
				}
				break;
			case 2: // half cylinder
				if (normPos2D.x<0){
					tmpx=normPos2D.x*sqDim+0f;
					tmpy=0f;
					tmpz=normPos2D.y*sqDim-25f;

					baseNormal=new Vector3(0,0,0);
				}
				if (normPos2D.x>=0){
					tmpx=Mathf.Sin(normPos2D.x*Mathf.PI)*Radius+0f; //slight off set of 0.5 because step between the flat surface and half moon
					tmpy=-Mathf.Cos(normPos2D.x*Mathf.PI)*Radius+Radius;
					tmpz=normPos2D.y*sqDim-25f;

					baseNormal=new Vector3(0,0,(normPos2D.x*Mathf.PI)*Mathf.Rad2Deg);
				}				
				break;
			case 3: //S shape curve
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

		Debug.DrawLine(obj3D.position,obj3D.position+obj3D.up*1,Color.red);
		Debug.DrawLine(obj3D.position+obj3D.up*1,obj3D.position+obj3D.up*1+obj3D.forward*2,Color.red);

		return outPos3D;
	}
	IEnumerator consentPhase(){
		text_top.text="";
		text_topright.text="";
		text_fullscreen.text="";
		timerSlider.gameObject.SetActive(false);
		Debug.Log("consentPhase");
		nextButton.gameObject.SetActive(false);
		img_fullscreen.gameObject.SetActive(true);
		consentHolder.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		consentHolder.SetActive(false);
		
		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"end of consentPhase";
		StartCoroutine(save2file(overviewFn,tmptext));

	}
	IEnumerator startOfExp(){
		text_top.text="";
		text_topright.text="";
		timerSlider.gameObject.SetActive(false);
		Debug.Log("startOfExp()");
		string tmptext="<b>Introduction:</b>";
		tmptext=tmptext+"\nImagine that you are driving a car in the virtual world where the car can move up/down the steepest hill without falling. It can even move upside down.";
		tmptext=tmptext+" It's somewhat similar to a rollercoaster that always moves along the track.";
		tmptext=tmptext+"\n\nI am investigating how people perceive such environment and how well they find their way within it. You will do a series of tasks in this virtual world.";
		tmptext=tmptext+" <color=red>Instruction will be given on the top of the screen throughout the experiment.</color>";
		tmptext=tmptext+"\n\nClick next to begin.";
		text_fullscreen.text=tmptext;
		img_fullscreen.SetActive(true); //put background image
		
		
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
	//	img_fullscreen.SetActive(false);
	}
	IEnumerator endOfExp(){
		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of EndOfExp";
		StartCoroutine(save2file(overviewFn,tmptext));

		nextButton.gameObject.SetActive(false);
		img_fullscreen.SetActive(true);
		text_top.text="The end";
	
		char[] subNumChar=new char[1];
		subNumChar[0]=subId[subId.Length-1];
		string subNumPart=new string(subNumChar);
		int subNumber=int.Parse(subNumPart);
		
		string surveycode=subId+"_"+subSuffix;
		string tmpstr="\nThis is the end of the experiment. Thanks so much for your participation!";
		tmpstr=tmpstr+" Click the button to move back to Prolific and confirm your participation";
		tmpstr=tmpstr+"\n\nIf you have a problem in accessing Prolific website, send me the message with this unique Confirmation Code: <color=red>"+surveycode+"</color>";
		tmpstr=tmpstr+"\n\nNow, you can close the browser. Have a nice day!";
		text_fullscreen.text=tmpstr;	
		
		buttonProlific.SetActive(true);
		yield return new WaitForSeconds(60);
		yield return null;
	}
	IEnumerator propFollowingTask(){
		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of propFollowingTask";
		StartCoroutine(save2file(overviewFn,tmptext));
		
		Vector3[] pos2DList=json2vector3(taskparam["familiarise"]["path"]); // load Vector3 prop locations
		maxtrial=pos2DList.Length-1;
	//	Vector3[] pos2DList=new Vector3[4];
	//	pos2DList[1]=new Vector3(-0.5f,0.9f, 0);
	//	pos2DList[2]=new Vector3(0.5f,0.1f, 0);
	//	pos2DList[3]=new Vector3(0.7f,0.5f,0);	
		
		norm2DtoPhy2D(new Vector3(0,0.5f,45),char2D); //starting position of character

		timerSlider.value=0;
		text_topright.text="";
		text_top.text="";
		img_fullscreen.SetActive(true); //put background image
		tmptext="<b>Familiarisation:</b>";
		tmptext=tmptext+"\nLet's first practice how to move around in this virtual world. You can simply move forward/backward by pressing the up/down arrow keys, and you can steer your vehicle right/left by pressing the right/left arrow keys.";
		tmptext=tmptext+"\n\nIn this task, you should find and move to a traffic cone. Once you reach the cone, the cone will disappear and reappear somewhere else. Then move to the cone again";
		tmptext=tmptext+" This task will last for a few minutes.";
		tmptext=tmptext+"\n<b>Tip:</b> An arrow on the ground will guide you to the traffic cone. This guide arrow is visible only when you are far away from the cone. (When you are close enough to the cone, you won't need a guide)";
		tmptext=tmptext+"\n\nClick next to begin.";

		text_fullscreen.text=tmptext;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		nextButton.gameObject.SetActive(false);
		img_fullscreen.SetActive(false);
		text_fullscreen.text="";
		timerSlider.gameObject.SetActive(true);

		Debug.Log("start of propFollowingTask()");
		text_top.text="Find the cone and move to it (arrow keys)";
		float inittime=Time.time;
		float timelimit=5*60;// max 6min
		
		string savefn=subId+"_"+subSuffix+"_familiarisation_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";
		string savetext="";
		
		maxtrial=pos2DList.Length-1;
		trial=1;
		while (Time.time-inittime<timelimit & trial<pos2DList.Length){
			text_topright.text=trial+"/"+(pos2DList.Length-1);
			timerSlider.value=(Time.time-inittime)/timelimit;
			norm2DtoPhy2D(pos2DList[trial],prop2D); // place the prop (e.g. traffic cone) to predefined location
			norm2DtoPhy3D(pos2DList[trial],prop3D); // place the prop (e.g. traffic cone) to predefined location
			placeGuideArrow(curr_norm2D, pos2DList[trial]);
			RaycastHit hit;
			Vector3 p1=character.position; //capsule cast bottom position, depending on the hover distance between the character center and ground
			Vector3 p2=character.position+character.up*1; //capsule cast top position 
			
			Debug.DrawLine(p1,p1+character.forward*2,Color.red);
			Debug.DrawLine(p2,p2+character.forward*2,Color.blue);
		
			if (Physics.CapsuleCast(p1, p2,characterRadius, character.forward, out hit, distCollide))
			{	Debug.Log("hit forward"); // if character face any wall
				if (hit.collider.name == prop3D.name) {//If character get sufficiently close to the prop
					text_top.text="good";
					yield return new WaitForSeconds(0.5f);
					Debug.Log("touch:"+hit.collider);
					trial++;
					text_top.text="Find the cone and move to it (arrow keys)";
				}
			}// then move the object
			
			float cameraPitch=characterCamera.localEulerAngles.x;
			//savetext=savetext+Time.time.ToString("F3")+deli+MK2string(character.position)+deli+MK2string(character.rotation)+deli+cameraPitch.ToString("F1")+"\n";
			savetext=savetext+trial+deli+Time.time.ToString("F3")+deli+MK2string(curr_norm2D)+deli+cameraPitch.ToString("F1")+"\n";// should I record only the normalised position
			
		//	if (Input.GetKeyDown(KeyCode.Escape)) break; //break condition just in case (for debugging) this should be removed before posting online in case subject accidenatlly press the wrong button
			yield return null; // yield return null is absolute necessary for while loop!
		}
		StartCoroutine(save2file(savefn,savetext));
		// tell subjects that he completed the task and guide to the next task.. (how? full screen text? top text?)
		prop2D.gameObject.SetActive(false);
		prop3D.gameObject.SetActive(false);
		text_top.text="End of the movement practice phase. Click Next to continue the experiment";
		Debug.Log("end of propFollowingTask()");
		img_fullscreen.SetActive(true);
		
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		
	}
	IEnumerator triangleCompletionTask(){
		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of triangleCompletion";
		StartCoroutine(save2file(overviewFn,tmptext));

	//	Vector3[] pos2DList=json2vector3(taskparam["familiarisePropLoc"]); // load Vector3 prop locations
		Vector3[] start2DList=new Vector3[3];
		Vector3[] mid2DList=new Vector3[3];
		Vector3[] end2DList=new Vector3[3];
		start2DList[1]=new Vector3(0.2f-1,0.2f, 0);
		mid2DList[1]=new Vector3(0.5f-1,0.5f, 0);
		end2DList[1]=new Vector3(0.8f-1,0.1f, 0);
		
		start2DList[2]=new Vector3(0.2f,0.2f, 0);
		mid2DList[2]=new Vector3(0.5f,0.5f, 0);
		end2DList[2]=new Vector3(0.8f,0.1f, 0);
		
		text_top.text="";
		img_fullscreen.SetActive(true); //put background image
		tmptext="Triangle completion task:\n";
		tmptext=tmptext+"\nFollowing the guide arrow,you should move to the cone.";
		tmptext=tmptext+"\nKeep track of where you are going, and then later move back to the start position when prompted";
		tmptext=tmptext+"\nClick next to begin the task";

		text_fullscreen.text=tmptext;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		nextButton.gameObject.SetActive(false);
		img_fullscreen.SetActive(false);
		text_fullscreen.text="";

		Debug.Log("start of triangleCompletionTask()");
		float inittime=Time.time;
		float timelimit=15*60;// max 15min
		
		string savefn=subId+"_"+subSuffix+"_triangle_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";
		string savetext="";
		trial=1;
		Transform prop2D_mid=Instantiate(prop2D); prop2D_mid.name="cone2D_mid";
		Transform prop2D_end=Instantiate(prop2D); prop2D_end.name="cone2D_end";
		for (trial=1;trial<start2DList.Length;trial++){
			
			text_top.text="Follow the guide arrow and move to the cone";
			norm2DtoPhy2D(start2DList[trial],char2D);// place the character on 2D flat plane (then in the update function, character on 3D will be also updated)

			norm2DtoPhy2D(start2DList[trial],prop2D); // place the prop at the start
			norm2DtoPhy2D(mid2DList[trial],prop2D_mid); // place the prop (e.g. traffic cone) to predefined location
			norm2DtoPhy2D(end2DList[trial],prop2D_end); // place the prop (e.g. traffic cone) to predefined location
			
			norm2DtoPhy3D(mid2DList[trial],prop3D); // place the prop (e.g. traffic cone) to predefined location
			
			RaycastHit hit;
			bool didHit=false;
			while (!didHit)
			{	placeGuideArrow(curr_norm2D, mid2DList[trial]);
				Vector3 p1=character.position-character.up*1; //capsule cast bottom position, depending on the hover distance between the character center and ground
				Vector3 p2=character.position+character.up*1; //capsule cast top position 
				
				Debug.DrawLine(p1,p1+character.forward*2,Color.red);
				Debug.DrawLine(p2,p2+character.forward*2,Color.blue);
			
				if (Physics.CapsuleCast(p1, p2,characterRadius, character.forward, out hit, distCollide))
				{	Debug.Log("hit forward"); // if character face any wall
					if (hit.collider.tag == "Respawn") {//If character get sufficiently close to the prop
						Debug.Log("touch:"+hit.collider);
						didHit=true;
					}
				}// then move the object
				yield return null;
			}
			norm2DtoPhy3D(end2DList[trial],prop3D); // place the prop (e.g. traffic cone) to predefined location
			didHit=false;
			while (!didHit)
			{	placeGuideArrow(curr_norm2D, end2DList[trial]);
				Vector3 p1=character.position-character.up*1; //capsule cast bottom position, depending on the hover distance between the character center and ground
				Vector3 p2=character.position+character.up*1; //capsule cast top position 
				
				Debug.DrawLine(p1,p1+character.forward*2,Color.red);
				Debug.DrawLine(p2,p2+character.forward*2,Color.blue);
			
				if (Physics.CapsuleCast(p1, p2,characterRadius, character.forward, out hit, distCollide))
				{	Debug.Log("hit forward"); // if character face any wall
					if (hit.collider.tag == "Respawn") {//If character get sufficiently close to the prop
						Debug.Log("touch:"+hit.collider);
						didHit=true;
					}
				}// then move the object
				yield return null;
			}

			text_top.text="Now, face yourself to the start position, then press the space bar";
			translateAllow=false; rotateAllow=true;
			while (!Input.GetKeyDown(KeyCode.Space))
				yield return null;
			yield return new WaitForSeconds(0.1f);
			text_top.text="Now, move straight to the start position, then press the space bar";
			translateAllow=true; rotateAllow=false;
			while (!Input.GetKeyDown(KeyCode.Space))
				yield return null;
			translateAllow=true; rotateAllow=true;   
			text_top.text="good. start position is visible again. your score is ..., press spacebar to move to the next trial";
			norm2DtoPhy3D(start2DList[trial],prop3D); // place the prop (e.g. traffic cone) to predefined location
			yield return new WaitForSeconds(0.1f);
			while (!Input.GetKeyDown(KeyCode.Space))
				yield return null;

			// fix position
			// then fix rotation, movee forward
			//float cameraPitch=characterCamera.localEulerAngles.x;
			//savetext=savetext+Time.time.ToString("F3")+deli+MK2string(character.position)+deli+MK2string(character.rotation)+deli+cameraPitch.ToString("F1")+"\n";

			if (Input.GetKeyDown(KeyCode.Escape)) break; //break condition just in case (for debugging) this should be removed before posting online in case subject accidenatlly press the wrong button
			yield return null; // repeat every 0.1s
		}
		StartCoroutine(save2file(savefn,savetext));
		// tell subjects that he completed the task and guide to the next task.. (how? full screen text? top text?)
		prop2D.gameObject.SetActive(false);
		prop3D.gameObject.SetActive(false);
		text_top.text="End of the triangle completion task. Click Next to continue the experiment";
		Debug.Log("end of triangleCompletionTask()");
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		
	}
	IEnumerator objectDistEstimate_2AFC(){

		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of objDistEst_2AFC";
		StartCoroutine(save2file(overviewFn,tmptext));

		text_top.text="";
		text_topright.text="";
		img_fullscreen.SetActive(true); //put background image
		tmptext="<b>Distance estimation task:</b>\nIn this task, you should estimate how long it would take to move from one picture cube to other picture cube and compare the distances.";
		tmptext=tmptext+"\nYou should press 1 or 2 to indicate the picture cube that is closer to the reference picture cube. ";
		tmptext=tmptext+"\n\nClick next to begin.";
		text_fullscreen.text=tmptext;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		nextButton.gameObject.SetActive(false);
		text_fullscreen.text="";

		distEstHolder_2AFC.SetActive(true);
		string savefnSum=subId+"_"+subSuffix+"_DistEst2AFC_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";
		//Vector3[] distEstOrder=new Vector3[3];
		//distEstOrder[1]=new Vector3(1,2,3);
		//distEstOrder[2]=new Vector3(6,7,5);
		Vector3[] distEstOrder=json2vector3(taskparam["distEst2AFC"]["triplet"]);
		maxtrial=distEstOrder.Length-1;

		for (trial=1; trial<distEstOrder.Length; trial++){
			text_top.text="Which one is closer? Press 1 or 2";
			text_topright.text=trial+"/"+(distEstOrder.Length-1);

			int item1=Mathf.RoundToInt(distEstOrder[trial].x);
			int item2=Mathf.RoundToInt(distEstOrder[trial].y);
			int item3=Mathf.RoundToInt(distEstOrder[trial].z);

			distEst2AFCImg1.sprite=cueList_sub[item1];
			distEst2AFCImg2.sprite=cueList_sub[item2];
			distEst2AFCImg3.sprite=cueList_sub[item3];
			float inittime=Time.time;
			//float timelimit1=60f; // X sec to find the goal
			float RT=-1;
			float response=-1;
			while(!Input.GetKeyDown(KeyCode.Alpha1)&!Input.GetKeyDown(KeyCode.Alpha2))
			{//	timerSlider.value=(Time.time-inittime)/timelimit1;
				yield return null;
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{	response=1;}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{	response=2;}
			RT=Time.time-inittime;
			string savetextSum="";
			savetextSum=trial+deli+inittime+deli+item1+deli+item2+deli+item3+deli+response+deli+RT;
			StartCoroutine(save2file(savefnSum,savetextSum));

			distEst2AFCImg1.sprite=null;
			distEst2AFCImg2.sprite=null;
			distEst2AFCImg3.sprite=null;
			yield return new WaitForSeconds(1);
			
		}
		text_top.text="End of distance estimation task. Click next.";
		distEstHolder_2AFC.SetActive(false);

		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
	
	}
	IEnumerator objectDistEstimate_egocentric(){
		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of objDistEst_egocentric";
		StartCoroutine(save2file(overviewFn,tmptext));
		// 1. show start and end object
		// 2. replace subject at the start line, disable rotation cue/backward cue
		// 3. let them move straight and wait until they press the button
		// 4. ITI, 
		EnvironmentToggle(isOpenEnv, 1);

		moveConstraint=0;//now I will cut the link between the 2D flattened surface and 3D, and directly move the 3D character
		
		text_top.text="";
		text_topright.text="";
		timerSlider.gameObject.SetActive(false);
		img_fullscreen.SetActive(true); //put background image
		tmptext="<b>Distance estimation task:</b>\nIn this task, you should imagine how long it would take when you move from one picture cube to the other picture cube.";
		tmptext=tmptext+"\n\nAt the beginning of each trial, you will be placed at the end of a long corridor. And the two picture cues will be shown at the corner of the screen.";
		tmptext=tmptext+" You should estimate the distance between the picture cubes and move forward by the remembered distance between the picture cubes.";
		tmptext=tmptext+"<i>Try to estimate the distance as precise as you can, and press the spacebar once you reach the distance.</i>";
		tmptext=tmptext+"\n\n<b>You can only move forward (no backward movement or turn!).</b>";
		tmptext=tmptext+"\n\nClick next to begin.";
		text_fullscreen.text=tmptext;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		nextButton.gameObject.SetActive(false);
		text_fullscreen.text="";
		img_fullscreen.SetActive(false);

		Vector2[] distEstOrder_ego=json2vector2(taskparam["distEstEgo"]["pairList"]);
		maxtrial=distEstOrder_ego.Length-1;
		//Vector2[] distEstOrder_ego=new Vector2[3];
		//distEstOrder_ego[1]=new Vector2(1,2);
		//distEstOrder_ego[2]=new Vector2(3,4);
		guideArrow3D.gameObject.SetActive(false);
		
		Vector3 startPosForDistEst=new Vector3(-23f,1f,13.5f);
		Vector3 startRotForDistEst=new Vector3(0,90,0);
		distEstEgoImg1.gameObject.SetActive(true);
		distEstEgoImg2.gameObject.SetActive(true);
		string savetext="";
		string savefnSum=subId+"_"+subSuffix+"_DistEstEgo_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";
		for (trial=1;trial<distEstOrder_ego.Length;trial++){
			// temporarily hide the view because subject will be relocated
			character.position=startPosForDistEst;
			character.eulerAngles=startRotForDistEst;
		
			text_topright.text=trial+"/"+(distEstOrder_ego.Length-1);
			float timeInit=Time.time;
			text_top.text="Move forward by the distance between the pictures, then press the spacebar";
			int item1=Mathf.RoundToInt(distEstOrder_ego[trial].x);
			int item2=Mathf.RoundToInt(distEstOrder_ego[trial].y);
			distEstEgoImg1.enabled=true;
			distEstEgoImg2.enabled=true;
			distEstEgoImg1.sprite=cueList_sub[item1];
			distEstEgoImg2.sprite=cueList_sub[item2];
			
			
			
			translateAllow=true; rotateAllow=false;
			
			int custombreakCondition=0;
			while (custombreakCondition==0)
			{   if (Input.GetKeyDown(KeyCode.Space))
				{   if (Vector3.Distance(character.position,startPosForDistEst)>0) 
						custombreakCondition=1; //
					else
						text_top.text="<color=red>Move forward by the distance between the picture, before pressing the spacebar</color>"; 
				}
				yield return null;
			}
			
			Vector3 endPos=character.position;
			float timeEnd=Time.time;
			savetext=trial+deli+timeInit+deli+timeEnd+deli+item1+deli+item2+deli+MK2string(startPosForDistEst)+deli+MK2string(endPos);
			StartCoroutine(save2file(savefnSum,savetext));
			yield return new WaitForSeconds(0.1f);

			img_fullscreen.SetActive(true);
			distEstEgoImg1.enabled=false;
			distEstEgoImg2.enabled=false;
			text_top.text="the next trial will start soon...";
			yield return new WaitForSeconds(2f);
			img_fullscreen.SetActive(false);
	
		}
		distEstEgoImg1.gameObject.SetActive(false);
		distEstEgoImg2.gameObject.SetActive(false);
		Debug.Log("end of distance estimation egocentric task");
		text_top.text="End of the task. Press next button to continue the experiment";
		img_fullscreen.SetActive(true);
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//

	}
	IEnumerator objectDistEstimate_pairwise(){
		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of objDistEst_pairwise";
		StartCoroutine(save2file(overviewFn,tmptext));

		translateAllow=false; rotateAllow=false;
		text_top.text="";text_topright.text="";
		timerSlider.gameObject.SetActive(false);
		timerSlider.value=0;
		img_fullscreen.SetActive(true); //put background image
		tmptext="<b>Distance estimation:</b>\nIn this task, you should estimate how long it would take to move from one location to other location";
		tmptext=tmptext+"\nYou will adjust the slider to indicate the distance from 'very close' to 'very far' (e.g. pair of pictures that can be farthest away in the environment).";
		tmptext=tmptext+"\nPlease try to use the whole range of the slider.";
		tmptext=tmptext+"\n\nClick next to begin.";
		text_fullscreen.text=tmptext;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		nextButton.gameObject.SetActive(false);
		text_fullscreen.text="";

		distEstimateHolder.SetActive(true); 
		Vector2[] distEstOrder=json2vector2(taskparam["distEstPair"]["pairList"]);
		maxtrial=distEstOrder.Length-1;
		string savefnSum=subId+"_"+subSuffix+"_DistEst1_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";
		for (trial=1; trial<distEstOrder.Length; trial++){
			text_top.text="How far are these two items? Adjust the slider and press the spacebar";
			text_topright.text=trial+"/"+(distEstOrder.Length-1);
			int item1=Mathf.RoundToInt(distEstOrder[trial].x);
			int item2=Mathf.RoundToInt(distEstOrder[trial].y);

			distEstCue1.sprite=cueList_sub[item1];
			distEstCue2.sprite=cueList_sub[item2];
			float inittime=Time.time;
			float RT=-1;
			while(!Input.GetKeyDown(KeyCode.Space))
			{	yield return null;
			}
			RT=Time.time-inittime;
			string savetextSum="";
			savetextSum=trial+deli+item1+deli+item2+deli+distEstSlider.value+deli+RT;
			StartCoroutine(save2file(savefnSum,savetextSum));

			distEstCue1.sprite=null;
			distEstCue2.sprite=null;
			yield return new WaitForSeconds(0.5f);
			
		}
		text_top.text="End of distance estimation task. Click next";
		distEstimateHolder.SetActive(false);

		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
	}

	IEnumerator objectLocationTestPhase(int[] learnOrder, Vector3[] startLoc, int run){
		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of objLocTest";
		StartCoroutine(save2file(overviewFn,tmptext));

		pointAccum=0;//reset the point
		text_topleft.text="";
		text_topright.text="";
		timerSlider.value=0;
		
		maxtrial=learnOrder.Length-1;

		text_top.text="";
		img_fullscreen.SetActive(true); //put background image
		
		tmptext="";
		if (run==1){
			tmptext="<b>Object-location test run1:</b>";
			tmptext=tmptext+"\nThis is a test phase. You should move to the remembered location of item that you learned previously.";
			tmptext=tmptext+" Try to locate the item as precise and fast as you can. You will get a feedback by smily or frowning face.";
			tmptext=tmptext+"\n\nClick next to begin.";
		}
		if (run==2){
			tmptext="<b>Object-location test run2:</b>";
			tmptext=tmptext+"\nThis is the second round of the test phase. As before, you should move to the remembered location of item that you learned previously.";
			tmptext=tmptext+" Try to locate the item as precise and fast as you can. You will get a feedback by smily or frowning face.";
			tmptext=tmptext+" Let's try to score the highest point in this round :)";
			tmptext=tmptext+"\n\nClick next to begin.";
		}
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
		{	norm2DtoPhy3D(objLoc[j],objList_sub[j].transform);
			objList_sub[j].SetActive(false);
		}
		string savefnSum=subId+"_"+subSuffix+"_ObjLoc_TestRun"+run+"Sum_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";
		string savefnTraj=subId+"_"+subSuffix+"_ObjLoc_TestRun"+run+"Traj_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";

		float sumError=0f; //to calculate the mean distance error at the end of task
		int nCompleteTrial=0;
		for (trial=1; trial<learnOrder.Length;trial++){
			guideArrow3D.gameObject.SetActive(false);
			text_topright.text=trial+"/"+(learnOrder.Length-1);
			Vector3 currentObjLoc=objLoc[learnOrder[trial]];//normalised current object location (x,y,orientation)
			Vector2 currentObjLoc2D=new Vector2(currentObjLoc.x,currentObjLoc.y);
			norm2DtoPhy2D(currentObjLoc, prop2D);
			GameObject currentObj=objList_sub[learnOrder[trial]];
			// temporarily hide the view because subject will be relocated
			norm2DtoPhy2D(startLoc[trial],char2D);
			norm2DtoPhy3D(startLoc[trial],character);

			
			// retriev phase
			currentObj.SetActive(false);
			
			imgCenter.sprite=cueList_sub[learnOrder[trial]];
			imgCenter.gameObject.SetActive(true);
			text_top.text="Go to the "+objName_sub[learnOrder[trial]]+", then press the spacebar";
			translateAllow=false;rotateAllow=false;
			yield return new WaitForSeconds(2);
			translateAllow=true;rotateAllow=true;
			imgCenter.gameObject.SetActive(false);
			
			float inittime=Time.time;
			float timelimit1=60f; // X sec to find the goal
			string savetextSum="";
			string savetextTraj="";
			while(Time.time-inittime<timelimit1 & !Input.GetKeyDown(KeyCode.Space))
			{	timerSlider.value=(Time.time-inittime)/timelimit1;
				float cameraPitch=characterCamera.localEulerAngles.x;
				savetextTraj=savetextTraj+trial+deli+Time.time.ToString("F3")+deli+MK2string(curr_norm2D)+deli+cameraPitch.ToString("F1")+deli+"0"+"\n";
				yield return null;
			}
			float endtime=Time.time;
			// should I give feedback during the test phase? if I want to stop further learning then I shouldn't.
			currentObj.SetActive(true);
			
			float distPathNormal=-1;
			if (Input.GetKeyDown(KeyCode.Space))
			{
				distPathNormal=Mathf.Sqrt(Mathf.Pow(curr_norm2D.x-currentObjLoc.x,2)+Mathf.Pow(curr_norm2D.y-currentObjLoc.y,2));
				//	distEucl=curr_norm2D.x-Vector2.Distance(character.position, currentObj.transform.position);
				string feedbacktext="Error is "+(25*distPathNormal).ToString("F1");
				feedbacktext="";
				translateAllow=false;rotateAllow=false;
				yield return FeedbackSmile(distPathNormal,feedbacktext);
				translateAllow=true;rotateAllow=true;
				sumError=sumError+distPathNormal;
				nCompleteTrial++;
			}
			else{
				string feedbacktext="Timeout";
				translateAllow=false;rotateAllow=false;
				yield return FeedbackSmile(100, feedbacktext);
				translateAllow=true;rotateAllow=true;
				
			}
			savetextSum=trial+deli+inittime+deli+endtime+deli+MK2string(currentObjLoc)+deli+MK2string(curr_norm2D)+deli+distPathNormal.ToString("F3");
		
			StartCoroutine(save2file(savefnSum,savetextSum));
			
			text_top.text="Move to the "+objName_sub[learnOrder[trial]]+", and try to remember it";
			inittime=Time.time;
			
			int didHitCheck=1;
			timelimit1=60f;// so one can relearn the locatin of objects
			while (Time.time-inittime<timelimit1)
			{	placeGuideArrow(curr_norm2D, objLoc[learnOrder[trial]]);
				timerSlider.value=(Time.time-inittime)/timelimit1;
				float cameraPitch=characterCamera.localEulerAngles.x;
				
				savetextTraj=savetextTraj+trial+deli+Time.time.ToString("F3")+deli+MK2string(curr_norm2D)+deli+cameraPitch.ToString("F1")+deli+didHitCheck+"\n";
				
				if (hitCheck())
				{	text_top.text="Press spacebar to continue";
					didHitCheck=2;
				}
				if (didHitCheck==2&Input.GetKeyDown(KeyCode.Space))
					break;
				yield return null;
			}
			if (didHitCheck==1)
			{	text_top.text="<color=red>Next trial begins soon</color>";
				yield return new WaitForSeconds(2);
			}
			StartCoroutine(save2file(savefnTraj,savetextTraj));
			
			img_fullscreen.SetActive(true);
			yield return new WaitForSeconds(1f);
			img_fullscreen.SetActive(false);
			currentObj.SetActive(false);
		}
		string tmpstring="End of the test phase, click next to continue.";
		img_fullscreen.SetActive(true);
		imgCenter.gameObject.SetActive(true);
		text_fullscreen.text="Score: "+pointAccum+"/48";
		if (pointAccum>=40){	
			imgCenter.sprite=medalSprite[0];
		}
		if (pointAccum>=31 & pointAccum<40){
			imgCenter.sprite=medalSprite[1];
		}if (pointAccum>=24 & pointAccum<31){
			imgCenter.sprite=medalSprite[2];
		}
		text_top.text=tmpstring;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;//
		imgCenter.gameObject.SetActive(false);

		yield return getTime();
		tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"end of objLocTest"+deli+"point:+"+pointAccum;
		StartCoroutine(save2file(overviewFn,tmptext));

	}
	IEnumerator FeedbackSmile(float distError, string feedbackText)
	{	imgCenter.gameObject.SetActive(true);
		if (distError<0.1f)
		{	imgCenter.sprite=feedbackSmileSprites[1];
			pointAccum=pointAccum+2;
		} // big smile
		if (distError>=0.1f& distError<0.2f)
		{	imgCenter.sprite=feedbackSmileSprites[2];
			pointAccum=pointAccum+1;
		} // little smile
		if (distError>=0.2f& distError<0.4f)
		{	imgCenter.sprite=feedbackSmileSprites[3];} // neutral
		if (distError>=0.4f & distError<0.6f)
		{	imgCenter.sprite=feedbackSmileSprites[4];} // little frown
		if (distError>=0.6f)
		{	imgCenter.sprite=feedbackSmileSprites[5];
		} // big frown
		
		text_top.text=feedbackText;
		text_topleft.text="point sum:"+pointAccum.ToString("#");
				
		yield return new WaitForSeconds(2);	
		imgCenter.gameObject.SetActive(false);	
		
	}
	bool hitCheck()
	{	RaycastHit hit;
		Vector3 p1=character.position; //capsule cast bottom position, depending on the hover distance between the character center and ground
		Vector3 p2=character.position+character.up*1; //capsule cast top position 
		Debug.DrawLine (p1,p1+character.forward, Color.blue);
		Debug.DrawLine (p2,p2+character.forward, Color.blue);
		
		bool didHit=false;
		if (Physics.CapsuleCast(p1, p2,characterRadius, character.forward, out hit, distCollide))
		{	if (hit.collider.tag=="Respawn")
				didHit=true;
		}
		return didHit;
	}

	IEnumerator objectLocationLearnPhase(){
		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of objLocLearn";
		StartCoroutine(save2file(overviewFn,tmptext));
		
		text_topright.text="";
		timerSlider.value=0;
			
		learnOrder=json2int(taskparam["objlocLearn"]["learnOrder"]); // load Vector3 prop locations
		startLoc=json2vector3(taskparam["objlocLearn"]["startLoc"]); // load Vector3 prop locations
		maxtrial=learnOrder.Length-1;

		text_top.text="";
		img_fullscreen.SetActive(true); //put background image
		tmptext="<b>Object-location learing:</b>\nIn this task, you will find several picture cubes in the enviroment, one by one.";
		tmptext=tmptext+"Try to remember the location of each picture as best as you can. Your spatial memory will be tested later.";
		tmptext=tmptext+"\n\nAs before, the guide arrow on the ground will help you to find the picture cube when the cube is far away from you.";
		tmptext=tmptext+"\n\nClick next to begin.";

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
		{	norm2DtoPhy3D(objLoc[j],objList_sub[j].transform);
			objList_sub[j].SetActive(false);
		}
		
		
		// encoding phase
		// place one object
		// wait subject to grab the object
		// give some seconds to encode the location
		
		string savefnTraj=subId+"_"+subSuffix+"_ObjLoc_LearnPhaseTraj_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";

		for (trial=1; trial<learnOrder.Length;trial++){
			text_topright.text=trial+"/"+(learnOrder.Length-1);
			
			Vector3 currentObjLoc=objLoc[learnOrder[trial]];//normalised current object location (x,y,orientation)
			Vector2 currentObjLoc2D=new Vector2(currentObjLoc.x,currentObjLoc.y);
			GameObject currentObj=objList_sub[learnOrder[trial]];
			currentObj.SetActive(true);
			text_top.text="Find the picture box and touch it";
			float inittime=Time.time;
			float timelimit1=60*1.5f;
			string savetextTraj="";
		
			int didHitCheck=1;
			while (Time.time-inittime<timelimit1)
			{	placeGuideArrow(curr_norm2D, objLoc[learnOrder[trial]]);
				timerSlider.value=(Time.time-inittime)/timelimit1;
				float cameraPitch=characterCamera.localEulerAngles.x;
				
				savetextTraj=savetextTraj+trial+deli+Time.time.ToString("F3")+deli+MK2string(curr_norm2D)+deli+cameraPitch.ToString("F1")+deli+didHitCheck+"\n";
				
				if (hitCheck())
				{	text_top.text="Remember the location of the "+objName_sub[learnOrder[trial]]+ ", then press the space bar";
					didHitCheck=2;
				}
				if (didHitCheck==2&Input.GetKeyDown(KeyCode.Space))
					break;
				yield return null;
			}
			if (didHitCheck==1)
			{	text_top.text="<color=red>Please move faster next time</color>";
				yield return new WaitForSeconds(2);
			}
			
			StartCoroutine(save2file(savefnTraj, savetextTraj));
			// temporarily hide the view because subject will be relocated
			norm2DtoPhy2D(startLoc[trial],char2D);
			norm2DtoPhy3D(startLoc[trial],character);
			img_fullscreen.SetActive(true);
			yield return new WaitForSeconds(2f);
			img_fullscreen.SetActive(false);
			currentObj.SetActive(false);

		}
		string tmpstring="End of encoding phase, click Next to continue the experiment";
		img_fullscreen.SetActive(true);
		text_top.text=tmpstring;
		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;

		yield return getTime();
		tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"end of objLocLearn";
		StartCoroutine(save2file(overviewFn,tmptext));
		// retrieval phase
	}
	IEnumerator debriefPhase(){
		yield return getTime();
		string tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"start of debrief";
		StartCoroutine(save2file(overviewFn,tmptext));
		
		text_top.text="Debrief questionnaire";
		text_topright.text="";
		timerSlider.gameObject.SetActive(false);
		string savefn=subId+"_"+subSuffix+"_debrief_"+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt";
		
		img_fullscreen.SetActive(true);
		text_fullscreen.text="<b>Debrief:</b>\nAlmost done! Please answer to a few questions regarding your experience about this experiment.\n\nClick next to begin.";

		nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{    yield return null;
		}moveToNext=0;
		text_fullscreen.text="";

		nextButton.gameObject.GetComponentInChildren<Text>().text="Complete";
	
		debriefHolder.SetActive(true);
		nextButton.gameObject.SetActive(true);
		Dropdown[] debrief_dropdown=new Dropdown[debriefQ.Length];
		string[] debrief_q=new string[debriefQ.Length];
		for (int i=0; i<debriefQ.Length; i++)
		{	debrief_dropdown[i]=debriefQ[i].GetComponentInChildren<Dropdown>();
			debrief_q[i]=debriefQ[i].GetComponent<Text>().text;
		//	Debug.Log(debrief_dropdown[i].options[1].text);
		}

		InputField[] debrief_inputfield=new InputField[debriefQ2.Length];
		string[] debrief_q2=new string[debriefQ2.Length];
		for (int i=0; i<debriefQ2.Length; i++)
		{	debrief_q2[i]=debriefQ2[i].GetComponent<Text>().text;
			debrief_inputfield[i]=debriefQ2[i].GetComponentInChildren<InputField>();
		}
		nextButton.gameObject.SetActive(true);
		
		int custombreakCondition=0;
		while (custombreakCondition==0)
		{   if (moveToNext==1)
			{    moveToNext=0; // reset the move To Next button
				
				bool isAllAnswered=true;
				string sumtext="";
				for (int i=0;i<debrief_dropdown.Length;i++)
				{	isAllAnswered=isAllAnswered&(debrief_dropdown[i].value!=0);
					sumtext=sumtext+debrief_q[i]+"\n"+debrief_dropdown[i].options[debrief_dropdown[i].value].text+"\n";
				}
				for (int i=0;i<debrief_inputfield.Length;i++)
				{	Debug.Log("debrief_inputfield:"+debrief_inputfield[i].text);
					isAllAnswered=isAllAnswered&(debrief_inputfield[i].text!="");
					sumtext=sumtext+debrief_q2[i]+"\n"+debrief_inputfield[i].text+"\n";
				}
				if (isAllAnswered)
				{	sumtext=sumtext+GetScreenInfo();
					StartCoroutine(save2file(savefn, sumtext));
					custombreakCondition=1;
				}
				else
				{
					text_top.text="<color=red>Please answer to all questions before click the next</color>";
				}
			}
			yield return null;
		}
		nextButton.gameObject.SetActive(false);
		debriefHolder.SetActive(false);
		yield return null;

		yield return getTime();
		tmptext=Time.time.ToString("0")+deli+currentGMTime+deli+System.DateTime.Now.ToString("yyyyMMdd_HHmmss")+deli+"end of debrief";
		StartCoroutine(save2file(overviewFn,tmptext));
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
		#if UNITY_WEBGL && !UNITY_EDITOR
			whereToSave=1; // for web build, save it into server
			Debug.Log("whereToSave=1 for WEBGL");
		#endif
		//I can also manually change this behaviour if I want to test the server save behaviour in the Editor
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

			var www = UnityWebRequest.Post("http://vm-mkim-1.cbs.mpg.de/testPHP/php-example.php", form);
		//	var www = UnityWebRequest.Post("http://wwwuser.gwdg.de/~misun.kim01/testPHP/php-example.php", form);
			{
				yield return www.SendWebRequest();

				if (www.isNetworkError) //it only return system error (DNS, socket) not the error from server side (404/Not Found)
				{
					Debug.Log(www.error);
					text_warning.gameObject.SetActive(true);
					text_warning.text="Oops! Server problem, stop the experiment and contact the researcher";
				}
				else
				{
					Debug.Log("Form upload complete!");
					text_warning.gameObject.SetActive(false);
					text_warning.text="";
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

	string GetScreenInfo() {
		float dpi=0;
#if UNITY_WEBGL && !UNITY_EDITOR
		double dpiFromJavascript=GetDPI();
		dpi=(float)dpiFromJavascript;// device pixel ratio (1 for normal screen, 2 for Retina,4K high resolution dpi device)
		Debug.Log("try to call DPI from javascript:"+dpi);
#else
		dpi=Screen.dpi;//absolute dpi (I think it only works within Unity Editor)
#endif
		string screenInfo="width"+Screen.width+"_height"+Screen.height+"_dpi"+dpi.ToString("F1")+"_fullscr"+Screen.fullScreen;
		return screenInfo;
	}
	IEnumerator getTime()    {
        Debug.Log ("connecting to php");
        string url="http://vm-mkim-1.cbs.mpg.de/getdatetime.php";
		WWW www = new WWW (url);
        yield return www;
        if (www.error != null) {
            Debug.Log ("Error in retrieving date and time");
        } else {
            Debug.Log ("Received date and time from php");
        }
        currentGMTime= www.text;
    }

}
