using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class birdTreeAssociation_MTurk : MonoBehaviour {

	// Use this for initialization
	public string[] treeName;
	public Vector2[] treePos2d;
	public GameObject  freeRecallHolder, nextButton,imgBottom;
	public Camera camera1, camera2;
	public Transform markerObject2D,markerObject3D,markerTarget2D,markerTarget3D;
	public Text textTop, textLeft,textRight, textBottom, textCentre, textAboveCentre, textAboveCentre2, textFullScreen, textTopRight;
	public Text textPlaceHolder,textWarning; // text related to InputText
	public InputField freeRecallText;
	public GameObject imgHolderForDebrief;
	public Text[] titleText;
	public Image[] imgDebrief;
	string deli;
	public GameObject birdPrefab1, birdPrefab2;
	public Matrix4x4 m;
	public Transform cylinder;
	public Vector2 rangeX,rangeY,rangeZ;
	public Vector3 rotAngles;
	public string subId;
	public int subNumber;
	public int run;
	public int trial;
	//private birdScriptNewbeak birdScript1, birdScript2; 
	public float speed_morph;
	public Vector2[] startList, endList;
	public float[] midList;
	string outputfn_encodingButtonpress;
	public string generalLogFn;
	int isExtrapolateDemo=0;
	public int moveToNext=0;
	string textBottomPrefix="";
	float meanaccu=0;
	float[] meanaccuForBonus=new float[4];
	IEnumerator Start () {
		//what's needed?
		//1. load location-name for each subject
		//2. load initial bird-tree association (discrete) test?
		//3. do the task in the continuous trajectory settinng
		//4. do the task in discrete memory test (for behavioural priming test)
		//5. do the task in discontinuous trajectory setting
		
		int sex, age;
	
		GameObject obj=GameObject.Find("globalVariable");
		if (!obj)
		{	Debug.Log("globalVariable does not exist, so I will assume it is Misun self testing");
			subId="psub99";
			run=0; sex=1; age=30;
		}
		else{
			keepThisVariable scriptobj=obj.GetComponent("keepThisVariable") as keepThisVariable;
		//	subId=scriptobj.uniqueId;
			run=scriptobj.run;
			sex=scriptobj.sex; age=scriptobj.age;
		}
		char[] subNumChar=new char[2];
		subNumChar[0]=subId[subId.Length-2];
		subNumChar[1]=subId[subId.Length-1];
		string subNumPart=new string(subNumChar);
		subNumber=int.Parse(subNumPart);
		
		deli="\t";
		
	
		int group2Dto3D=0; // 2D to 3D mapping type. 
		switch (subNumber%3)
		{	case 1: group2Dto3D=1; break;
			case 2: group2Dto3D=2; break;
			case 0: group2Dto3D=3; break;
		} 
		switch (group2Dto3D)
		{	case 1:
				rotateCylinder(new Vector3(0,0,0)); break;// neck and leg coupled (gap at max neck), beak independent
			case 2:
				rotateCylinder(new Vector3(270,270,0)); break;// leg and beak coupled (gap at max leg), neck independent
			case 3:
				rotateCylinder(new Vector3(0,90,90)); break;// beak and neck coupled (gap at max beak), leg independent
			
		}
		generalLogFn=subId+"_generalLog"+"_group"+group2Dto3D+"_sex"+sex+"_age"+age+"_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
	
	//	birdScript1=birdPrefab1.GetComponent("birdScriptNewbeak") as birdScriptNewbeak;
	//	birdScript2=birdPrefab2.GetComponent("birdScriptNewbeak") as birdScriptNewbeak;
		camera1.enabled=false; camera2.enabled=false;
	
		string inputfn, outputfn;
		bool extraITI=false; //whether includes the fixation cross ITI (probably only for the scanning part)
		float RTlimit=5f;
		
		textFullScreen.enabled=false;imgBottom.SetActive(false);textAboveCentre.text="";textCentre.text="";
		// part 0: prep		
		textTop.text=""; textAboveCentre.text=""; 
		yield return loadBirdTreeAssociation(); //load bird name-order-location list
		yield return generalOverview(); 
		
		// part 1: encoding
		textBottom.text="Part 1: Encoding";
		inputfn="birdTreeTest_static_20200325_run"+9;
		outputfn=subId+"_birdtree_encoding_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		yield return simpleBirdTreeShow(inputfn,outputfn); // encodinng phase (passive viewing)
		
		// part 2: static test-pre
		yield return memoryTestStaticWrapper();
		meanaccuForBonus[1]=meanaccu;		

		// part 3: free recall-pre
		textBottom.text="Part 3: free recall";
		outputfn=subId+"_birdtree_freeRecall_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		yield return freeRecallTest(outputfn); // first free recall test
		
		// part 4: morphing test
		yield return memoryTestDynamicWrapper();// continuous morphing test one or two
		meanaccuForBonus[2]=meanaccu;

		// part 5: static test - post
		yield return memoryTestStaticWrapper_noPractice();
		meanaccuForBonus[3]=meanaccu;
		
		// part 6: free recall -post
		textBottom.text="Part 6: free recall";
		outputfn=subId+"_birdtree_freeRecall_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		yield return freeRecallTest(outputfn); // second free recall test
		
		// part 7: debrief 
		textBottom.text="Part 7: Debrief";
		outputfn=subId+"_birdtree_debrief_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		yield return debrief(outputfn);
		
		// Final: end of experiment. Give SurveyCode + thank participants
		yield return endOfExp();
	}
	
	IEnumerator endOfExp(){
		string runStartTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		
		nextButton.SetActive(false);
		textCentre.text="";
		textTop.text="";
		textFullScreen.enabled=true;
		string surveycode=subId+"_"+subNumber;
		string tmpstr="\nThis is the end of the experiment. Thanks so much for your participation!";
		tmpstr=tmpstr+"\n\nHere is the Surveycode for MTurk Workers: "+surveycode;

		float bonusMoney=0;
		float grandmeanaccuForBonus=(meanaccuForBonus[1]+meanaccuForBonus[2]+meanaccuForBonus[3])/3;
		if (grandmeanaccuForBonus>0.8)
			bonusMoney=3;	
		if (bonusMoney>0)
			tmpstr=tmpstr+"\n\nGood news! You will get a small bonus. It can take a few days for processing.";
		
		tmpstr=tmpstr+"\n\nYou can now close this browser. Have a nice day!";
		textFullScreen.text=tmpstr;
		
		string runEndTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		
		string generalLogText=runStartTime+deli+runEndTime+deli+"end: grand meanAccu="+grandmeanaccuForBonus+", bonus="+bonusMoney;
		StartCoroutine(save2file(generalLogFn,generalLogText));		

		yield return null;
	}
	IEnumerator memoryTestDynamicWrapper(){
		// instruction screen
		textBottom.text="Part 4 demo";
		textTop.text="";
		textCentre.text="";
		textFullScreen.enabled=true;
		string tmpstr="\nIn this task, you will see a bird that continuously changes its body shape.";
		tmpstr=tmpstr+"For instance, a bird that looks like the one associated with "+treeName[1]+" changes to the bird associated with "+treeName[2];
		tmpstr=tmpstr+", then it agains changes to the shape of the "+treeName[6]+" bird, and so on.";
		tmpstr=tmpstr+"\n\nSometimes, a question appear and you should select the corect tree name for the current bird. You will get a feedback after each question. You can get a small bonus if you achieve high score.";
		tmpstr=tmpstr+"To answer the question correctly and quickly (there is a time limit), you should pay attention to how the bird changes its body shape, and the associated tree name throughout the task.";
		tmpstr=tmpstr+"\n\nThis description of the task may sound a bit complex, but once you do a short practice sesion, it will become clear. \nLet's start the short practice. - Click next";
		textFullScreen.text=tmpstr;
		
		nextButton.SetActive(false);
		yield return new WaitForSeconds(5); //deactivate the next button for few seconds so that subject don't accidentally press the next bubon even before reading the instruction text
		nextButton.SetActive(true);
		
		while (moveToNext==0)
			yield return null;
		moveToNext=0; nextButton.SetActive(false); textFullScreen.enabled=false;

		// practice run
		string testSeqfn,outputfn;
		bool extraITI=false;
		float RTlimit=5f;
		testSeqfn="birdTreeTest_conti_20200325_run0";
		outputfn=subId+"_birdtree_conti_run"+0+"_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		
		textBottomPrefix="Part 4 demo:";
		textTop.text="Pay attention to how the bird changes and select the correct tree for the bird";
		yield return retrieveNameDuringTraj(testSeqfn, outputfn, extraITI, RTlimit);
	
		// short pause before the main test 1 start
		textBottomPrefix="Part 4 demo";
		textFullScreen.enabled=true;
		textFullScreen.text="\nHow was the practice session? If the task is still unclear, or if you faced any technical problem, contact me. Otherwise, let's start the main test. It will take ~12 min.";
		while(moveToNext==0){yield return null;}
		moveToNext=0;textFullScreen.enabled=false;nextButton.SetActive(false);

		// main run 1
		testSeqfn="birdTreeTest_conti_20200325_run1";
		outputfn=subId+"_birdtree_conti_run"+1+"_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		
		textBottomPrefix="Part 4:";
		textTop.text="Pay attention to how the bird changes and select the correct tree for the bird";
		yield return retrieveNameDuringTraj(testSeqfn, outputfn, extraITI, RTlimit);


		// short pause before the main test 1 start
		textBottomPrefix="Part 4 demo";
		textFullScreen.enabled=true;
		textFullScreen.text="\nYou will do the same task once more. Watch how the bird changes its shape, and select the right name for the bird when the question appears.";
		while(moveToNext==0){yield return null;}
		moveToNext=0;textFullScreen.enabled=false;nextButton.SetActive(false);

		// main run 2
		testSeqfn="birdTreeTest_conti_20200325_run2";
		outputfn=subId+"_birdtree_conti_run"+2+"_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		
		textBottomPrefix="Part 4:";
		textTop.text="Pay attention to how the bird changes and select the correct tree for the bird";
		yield return retrieveNameDuringTraj(testSeqfn, outputfn, extraITI, RTlimit);

	}
	IEnumerator memoryTestStaticWrapper(){
		// first instruction
		textBottom.text="Part 2 demo";
		textTop.text="";
		textFullScreen.enabled=true;
		string tmpstr="\nIn this task, one bird and two tree names will appear on screen. You should select the correct tree name associated with the bird.";
		tmpstr=tmpstr+"You can answer it by pressing 1 or 2 (the number keys above the letters on the keyboard)";		
		tmpstr=tmpstr+"\nAfter the choice, you will be told whether you are correct or not. Then the next question will appear. Timelimit for each question is 10 sec.";
		tmpstr=tmpstr+"\nAt the end of the task, your accuracy will be shown. You can get a small bonus if you get a high score.";
		tmpstr=tmpstr+"\n\nLet's do a short practice session - click next";
		textFullScreen.text=tmpstr;

		nextButton.SetActive(false);
		yield return new WaitForSeconds(5); //deactivate the next button for few seconds so that subject don't accidentally press the next bubon even before reading the instruction text
		nextButton.SetActive(true);

		while(moveToNext==0){yield return null;}
		moveToNext=0;textFullScreen.enabled=false;nextButton.SetActive(false);
		// then short pratice run
		
		string testSeqfn,outputfn;
		bool extraITI=true;
		float RTlimit=10f;
		testSeqfn="birdTreeTest_static_20200325_run0";
		outputfn=subId+"_birdtree_static_run"+0+"_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		
		textBottomPrefix="Part2 demo:";
		textTop.text="Select the tree for this bird (press 1 or 2)";
		yield return retrieveNameDuringTraj(testSeqfn, outputfn, extraITI, RTlimit);
		
		// short pause before the main test start
		textBottomPrefix="Part2 demo";
		
		textFullScreen.enabled=true;
		textFullScreen.text="\nHow was the practice session? If the task is still unclear, or if you faced any technical problem, contact me. Otherwise, let's start the main test. It will take ~6 min.";
		while(moveToNext==0){yield return null;}
		moveToNext=0;textFullScreen.enabled=false;nextButton.SetActive(false);
		
		// then the main full length static memory test
		textBottomPrefix="Part 2:";
		testSeqfn="birdTreeTest_static_20200325_run1";
		outputfn=subId+"_birdtree_static_run"+1+"_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		textTop.text="Select the tree for this bird (press 1 or 2)";
		yield return retrieveNameDuringTraj(testSeqfn, outputfn, extraITI, RTlimit);
	}
	IEnumerator memoryTestStaticWrapper_noPractice(){
		// first instruction

		textBottom.text="Part 5 demo";
		textTop.text="";
		textFullScreen.enabled=true;
		string tmpstr="\nIn this task, one bird and two tree names will appear on screen, and you should select the correct tree name associated with the bird.";
		tmpstr=tmpstr+"\nAfter the choice, you will be told whether you are correct or not. Then the next question will appear. Timelimit for each question is 10 sec.";
		tmpstr=tmpstr+"\nAt the end of the task, your accuracy will be shown.";
		tmpstr=tmpstr+"\n\nClick next when you are ready";
		textFullScreen.text=tmpstr;

		nextButton.SetActive(false);
		yield return new WaitForSeconds(5); //deactivate the next button for few seconds so that subject don't accidentally press the next bubon even before reading the instruction text
		nextButton.SetActive(true);
		while(moveToNext==0){yield return null;}
		moveToNext=0;textFullScreen.enabled=false;nextButton.SetActive(false);
	
		// then the main full length static memory test
		string testSeqfn,outputfn;
		bool extraITI=true;
		float RTlimit=10f;
		textBottomPrefix="Part 5:";
		testSeqfn="birdTreeTest_static_20200325_run2";
		outputfn=subId+"_birdtree_static_run"+2+"_"+System.DateTime.Now.ToString("yyyyMMdd_HHmm")+".txt";
		textTop.text="Select the tree for this bird (press 1 or 2)";
		yield return retrieveNameDuringTraj(testSeqfn, outputfn, extraITI, RTlimit);
	}
	void Update () { //called every frame
		if (Input.GetKey(KeyCode.Alpha9))
		{	if (Input.GetKeyDown(KeyCode.Alpha0))
		//		SceneManager.LoadScene("scn_mainMenu"); //go back to main menu
			trial=startList.Length-2; // to move to the end of each task, for the demo purpose. This should be disabled for actual experiment to prevent subject accidentally press this key combination
		}
	
	}
	IEnumerator loadBirdTreeAssociation(){
	    // load the location-tree association for each subject
		// then save the location-tree association info for each subject (in case  I later change the order/name of the symbol,  I still would know what was actually presented to each subject)
		Debug.Log("start loadBirdTreeASsociation");
		
		string inputfn1="birdTreeName_allSubject_20200227"; //order of tree 
		string outputfn=subId+"_nameBird_dict_"+System.DateTime.Now.ToString("yyyyMMdd")+".txt";
			
		Vector3[] treePos3d=new Vector3[9];
		treePos2d=new Vector2[9];
		treePos2d[1]=new Vector2(45, 0.146f);
		treePos2d[2]=new Vector2(135, 0.146f);
		treePos2d[3]=new Vector2(225, 0.146f);
		treePos2d[4]=new Vector2(315, 0.146f);
		treePos2d[5]=new Vector2(45, 0.854f);
		treePos2d[6]=new Vector2(135, 0.854f);
		treePos2d[7]=new Vector2(225, 0.854f);
		treePos2d[8]=new Vector2(315, 0.854f);
		
		treeName=new string[9];
		string [] treeDict={"","Maple","Fir","Oak","Pine","Birch","Willow","Poplar","Elm"};
		
		var maintestlist = Resources.Load<TextAsset>(inputfn1);
        var tmp=maintestlist.text.Split("\n"[0]);
		Debug.Log("treeDict.Length is "+treeDict.Length);
		var mpt=tmp[subNumber].Split("\t"[0]);
		string recordtext="";
			
		for (int j=1;j<=8; j++)
		{	treeName[j]=treeDict[int.Parse(mpt[j])]; //load location-tree order for each subject
			int tmptreenameIdx=int.Parse(mpt[j]);
			treePos3d[j]=MK2Dto3D(treePos2d[j]);
			
			if (j==1) //add header
			{	recordtext="loc2D_idx"+deli+"loc2D_x"+deli+"loc2D_y"+deli+"loc3D_x"+deli+"loc3D_y"+deli+"loc3D_z"+deli+"treenameIdx"+deli+"treeName"+"\n";
			}
			recordtext=recordtext+j+deli+MK2string(treePos2d[j])+deli+MK2string(treePos3d[j])+deli+tmptreenameIdx+deli+treeName[j]+"\n";//save location-tree order information for subject
		}
		StartCoroutine(save2file(outputfn,recordtext));
		yield return null;
		
	}
	IEnumerator generalOverview(){
		string runStartTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		
		textFullScreen.enabled=true;
		imgBottom.SetActive(true);
		textCentre.text="";
		string tmptextt=@"
	In this experiment, you will learn bird shape - tree name associations (examples below).
	Here is an overview of the exerpiment. (more detailed instruction will be given later)
	
	Part 1. You will watch the bird-name pair on screen [~6 min]
	Part 2. You will select the correct name for each bird [~7 min]
	Part 3. You will recall the tree names [~2 min]
	Part 4. A variation of the previous memory test [~25 min] 
	Part 5. You will select the correct name for each bird [~6 min]
	Part 6. You will recall the tree names [~2 min]
	Part 7. Debrief [~5 min]

	In total, it will take approx 1 hour and you will receive a Survey Completion code at the end.
	<color=red>** Attention please **</color>
	-Do this experiment in a quiet room without disturbance. Turn off your phone/music now.
	-Don't take a note for the memory test (no cheating!).
	-Read and follow the instruction carefully. If you leave the question fields blank, and press a random button which results in chance level accuracy (<60%), your work can be rejected.	
	-You can get a small bonus if you get a high score (>80%) in the memory tests.
	
	Click next to start.";
		textFullScreen.text=tmptextt;
		while (moveToNext==0)
		{   yield return null;
		}
		moveToNext=0; // reset
		imgBottom.SetActive(false); textFullScreen.enabled=false;

		string runEndTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");		
		string generalLogText=runStartTime+deli+runEndTime+deli+"generalOverview";
		StartCoroutine(save2file(generalLogFn,generalLogText));	
	}
	IEnumerator freeRecallTest(string outputfn){
		string runStartTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		
		nextButton.SetActive(true);
		freeRecallHolder.SetActive(true);
		textTop.text="What were the 8 tree names you saw in the previous task? Type them in the order that comes into your mind now. If you can't remember all, then type as many as you can.";
		textPlaceHolder.text="e.g. tree A, tree B, tree C, ...";
		textWarning.text="";
		var time1=Time.time;
		while(true){
			// if button is clicked and input field is not empty -> break the loop
			// if button is clicked and input field is empty -> warning, ask them to write something
			if (moveToNext==1&freeRecallText.text!="")
			{	moveToNext=0; break;
			}
			if (moveToNext==1 & freeRecallText.text=="")
			{	textWarning.text="Oops! you didn't type any tree names.";
				moveToNext=0;
			}
			yield return null;
		}
		Debug.Log("line 336: "+freeRecallText.text);
		textWarning.text="";
		var time2=Time.time;
		var recordtext=(time2-time1).ToString("F0")+ deli+ freeRecallText.text;
		StartCoroutine(save2file(outputfn,recordtext));
		freeRecallHolder.SetActive(false);
		freeRecallText.text="";
		
		string runEndTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");		

		string generalLogText=runStartTime+deli+runEndTime+deli+"freeRecallTest";
		StartCoroutine(save2file(generalLogFn,generalLogText));		
		
	}
	IEnumerator debrief(string outputfn){
		string runStartTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		imgHolderForDebrief.SetActive(true);
		for (int i=1;i<=8;i++){
			titleText[i].text=treeName[i];
			string tmpimgstring="treeBird/birdNew_"+MK2Dto3D(treePos2d[i]).x.ToString("F2")+","+MK2Dto3D(treePos2d[i]).y.ToString("F2")+","+MK2Dto3D(treePos2d[i]).z.ToString("F2");
			Debug.Log(tmpimgstring);
			Sprite tmpimg = Resources.Load<Sprite>(tmpimgstring);
			imgDebrief[i].sprite=tmpimg;
		}

		freeRecallHolder.SetActive(true);
		string tmpstr="Well done! You completed all memory tasks and just a few questions left before the end. Detailed answers will be greatly appreciated.";
		tmpstr=tmpstr+"\n\n<color=blue>1. Were some birds particuly confusing or difficult? If yes, what were they?</color> e.g. maple and elm were confusing.";
		tmpstr=tmpstr+"\n<color=blue>2. Was there some grouping of the birds?</color> What I meant 'grouping' is that when you saw one bird, you remember it by relating it to other birds. e.g. I thought maple is long neck version of elm.";
		tmpstr=tmpstr+"\n<color=blue>3. Is there anything you want to comment?</color> For instance, if you found the speed of changing bird too slow and fall asleep in some part of the experiment, please let me know.";
		textTop.text=tmpstr;
		textPlaceHolder.text="e.g.\n1. bird A and B were somehow confusing\n2.some birds were...\n3. I accidentally pressed the wrong button sometimes. The bird changed too fast, etc ";
		textWarning.text="";
		var time1=Time.time;
		while(true){
			// if button is clicked and input field is not empty -> break the loop
			// if button is clicked and input field is empty -> warning, ask them to write something
			if (moveToNext==1&freeRecallText.text!="")
			{	moveToNext=0; break;}
			if (moveToNext==1 & freeRecallText.text=="")
			{	textWarning.text="Please answer before you submit";
				moveToNext=0;
			}
			yield return null;
		}
		textWarning.text="";
		var recordtext=freeRecallText.text;
		StartCoroutine(save2file(outputfn,recordtext));
		freeRecallHolder.SetActive(false);
		freeRecallText.text="";

		string runEndTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");		

		string generalLogText=runStartTime+deli+runEndTime+deli+"debrief";
		StartCoroutine(save2file(generalLogFn,generalLogText));		
		imgHolderForDebrief.SetActive(false);	
	}

	public void ClickedNext(){
		moveToNext=1;
		Debug.Log("clickedNext()");
		EventSystem.current.SetSelectedGameObject(null);// to prevent the color of "pressed button" stay (this is related to keyboard navigation of UI. By deselect the object, I disable participants to choose different button using arrow keys)
	}
	IEnumerator simpleBirdTreeShow(string testSeqfn, string outputfn){
		Debug.Log("start simpleBirdTreeShow");
		string runStartTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		nextButton.SetActive(false);
		// first load the encoding order from text file
		var maintestlist = Resources.Load<TextAsset>(testSeqfn);
        var tmp=maintestlist.text.Split("\n"[0]);
		var correctloc=new int[tmp.Length-1];
		startList=new Vector2[tmp.Length-1];
		textCentre.text="+";
		for (int j=1;j<tmp.Length-1; j++)
		{	var mpt=tmp[j].Split("\t"[0]);
			correctloc[j]=Mathf.RoundToInt(float.Parse(mpt[5]));
			startList[j]=treePos2d[correctloc[j]];
		}
		// give instruction
		string tmptext1="In this encoding phase, you will see a series of bird-tree name pair on screen for about 6 minutes";
		tmptext1=tmptext1+"\nPlease remember which shape of the bird is associated with which tree name. Memory test will be given later.";
		textTop.text=tmptext1;
/*		
		string[] fillername={"","some name","other name","another name","so on.."};
		int[] filleridx={0,1,2,3,4};
		yield return new WaitForSeconds(4f); //show the example encoding phase after few seconds - to allow subject to read the instruction
		
		for (trial=1;trial<fillername.Length-1; trial++)
		{	camera1.enabled=true;
			Vector2 tmpcurrent2D=treePos2d[filleridx[trial]];
			putMarkerTarget(tmpcurrent2D, markerObject2D, markerObject3D);
			birdScript1.birdSizeSetNorm(MK2Dto3D(tmpcurrent2D));
			textAboveCentre.text=fillername[trial];
			yield return new WaitForSeconds(3f);
			textAboveCentre.text="";
			camera1.enabled=false;
			yield return new WaitForSeconds(1f);
				
		}
*/		nextButton.SetActive(true);
		while (moveToNext==0)
		{   yield return null;
		}
		moveToNext=0;
		
		// start main encoding phase
		nextButton.SetActive(false);
		textTop.text="Remember the bird-tree name associations.";
		string recordtext="trial"+deli+"loc2D_x"+deli+"loc2D_y"+deli+"loc2D_idx"+deli+"time";
		StartCoroutine(save2file(outputfn,recordtext));
		
		for (trial=1;trial<correctloc.Length-1; trial++)
		{	camera1.enabled=true;
			Vector2 tmpcurrent2D=treePos2d[correctloc[trial]];
			putMarkerTarget(tmpcurrent2D, markerObject2D, markerObject3D);
		//	birdScript1.birdSizeSetNorm(MK2Dto3D(tmpcurrent2D));
			textAboveCentre.text=treeName[correctloc[trial]];
			yield return new WaitForSeconds(5f);
			textAboveCentre.text="";
			camera1.enabled=false;
			yield return new WaitForSeconds(1f);
			
			recordtext=trial+deli+MK2string(tmpcurrent2D)+deli+correctloc[trial]+deli+Time.time;
			StartCoroutine(save2file(outputfn,recordtext));
				
	
		}	
		camera1.enabled=false;
		textTop.text="End of the encoding phase. Next part will start soon";			
		
		string runEndTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		string generalLogText=runStartTime+deli+runEndTime+deli+testSeqfn+deli+"(simpleBirdTreeShow)";
		StartCoroutine(save2file(generalLogFn,generalLogText));		

		yield return new WaitForSeconds(5f);
	}
	
	IEnumerator retrieveNameDuringTraj(string testSeqfn, string outputfn, bool extraITI, float RTlimit){
		Debug.Log("start retrieveNameDuringTraj");
		yield return null;
		
		// Load the test order from file
		camera1.enabled=true;
		var maintestlist = Resources.Load<TextAsset>(testSeqfn);
        var tmp=maintestlist.text.Split("\n"[0]);
		startList=new Vector2[tmp.Length-1];
		endList=new Vector2[tmp.Length-1];
		midList=new float[tmp.Length-1];
		var correctloc=new int[tmp.Length-1];
		var q1=new int[tmp.Length-1];
		var q2=new int[tmp.Length-1];
		var q3=new int[tmp.Length-1];
		var q4=new int[tmp.Length-1];
		var trialtype=new int[tmp.Length-1];
		for (int j=1;j<tmp.Length-1; j++)
		{	var mpt=tmp[j].Split("\t"[0]);
			startList[j]=new Vector2(float.Parse(mpt[0]),float.Parse(mpt[1]));
			endList[j]=new Vector2(float.Parse(mpt[2]),float.Parse(mpt[3]));
			midList[j]=float.Parse(mpt[4]);
			correctloc[j]=int.Parse(mpt[5]);
			q1[j]=int.Parse(mpt[6]);
			q2[j]=int.Parse(mpt[7]);
			trialtype[j]=int.Parse(mpt[8]);
		}
		var numcorrect=0;
		var numincorrect=0;
		var numtimeout=0;
		var numcorrectType1=0;
		var numcorrectType2=0;
		int numType1=0; int numType2=0;
		
		string runStartTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		textCentre.text="+";
			
		for (trial=1; trial<startList.Length; trial++)
		{	camera1.enabled=true;
			textBottom.text=textBottomPrefix+trial+"/"+(startList.Length-1);
			textAboveCentre.text="";
			Vector2 tmpStart2D=startList[trial];
			float tmpMid2D=midList[trial];
			Vector2 tmpEnd2D=endList[trial];
			float tmpdist=Vector2.Distance(new Vector2(tmpStart2D.x/360*Mathf.PI, tmpStart2D.y),new Vector2(tmpEnd2D.x/360*Mathf.PI,tmpEnd2D.y));
			float moviedur0=tmpdist/speed_morph;
			float moviedur1=moviedur0*tmpMid2D;
			float time_init=Time.time;
			setRendererColor(birdPrefab1,0f,0f,0f); // set the clor as black
			Vector2 tmpcurrent2D=tmpStart2D;
			putMarkerTarget(tmpcurrent2D, markerObject2D, markerObject3D);
			Vector3 tmp3D=MK2Dto3D(tmpcurrent2D);
		//	birdScript1.birdSizeSetNorm(MK2Dto3D(tmpcurrent2D));
			
			if (moviedur0>0)
			{	while  (Time.time-time_init<=moviedur1){
					tmpcurrent2D=tmpStart2D+(Time.time-time_init)/moviedur0*(tmpEnd2D-tmpStart2D);
					putMarkerTarget(tmpcurrent2D, markerObject2D, markerObject3D);
					
				//	birdScript1.birdSizeSetNorm(MK2Dto3D(tmpcurrent2D));
					yield return null;
				}
			}
			if (moviedur0>0  & moviedur1<moviedur0)
			{	setRendererColor(birdPrefab1,0.5f,0.5f,0.5f); // temporarily make it gray
				textAboveCentre.text="Imagine..";
				while  (Time.time-time_init<=moviedur0){
					tmpcurrent2D=tmpStart2D+(Time.time-time_init)/moviedur0*(tmpEnd2D-tmpStart2D);
					putMarkerTarget(tmpcurrent2D, markerObject2D, markerObject3D);
					yield return null;
				}
			}
			float time_Q=Time.time; float time_R=-1; int response=-1;
			if (correctloc[trial]!=0) // present question only when the trajectory ended in one of the landmark birds
			{
				var keycode1=KeyCode.Alpha1;
				var keycode2=KeyCode.Alpha2;
				var keycode3=KeyCode.Alpha3;
				var keycode4=KeyCode.Alpha4;

				textAboveCentre.text="1) "+treeName[q1[trial]]+"   2) "+treeName[q2[trial]];//+ "   3) "+treeName[q3[trial]]+"   4) "+treeName[q4[trial]]+" ?";

				while (!Input.GetKeyDown(keycode1)&!Input.GetKeyDown(keycode2)&!Input.GetKeyDown(keycode3)&!Input.GetKeyDown(keycode4)&Time.time-time_Q<RTlimit)
				{	yield return null;
				}
				time_R=Time.time;//when subject answers to the question
				if (Input.GetKeyDown(keycode1))
				{	response=q1[trial];}
				if (Input.GetKeyDown(keycode2))
				{	response=q2[trial];}
				if (Input.GetKeyDown(keycode3))
				{	response=q3[trial];}
				if (Input.GetKeyDown(keycode4))
				{	response=q4[trial];}

				
				string tmptext="";
				if (response==-1)
				{	tmptext="<color='red'>timeout</color>, the answer is "+ treeName[correctloc[trial]];
					numtimeout++;
					textAboveCentre.text=tmptext; yield return new WaitForSeconds(4f);
				}
				else
				{	if (response==correctloc[trial])
					{	tmptext="correct";numcorrect++;
						if (trialtype[trial]==1)
							numcorrectType1++;
						if (trialtype[trial]==2)
							numcorrectType2++;
						textAboveCentre.text=tmptext; yield return new WaitForSeconds(2f);
					}else
					{	tmptext="<color='red'>wrong</color>, the answer is "+ treeName[correctloc[trial]];numincorrect++;
						textAboveCentre.text=tmptext; yield return new WaitForSeconds(4f);
					}
				}
				
				if(trialtype[trial]==1)
					numType1++;
				if(trialtype[trial]==2)
					numType2++;
				
				//add extra ITI for disconinuous scanning task
				if (extraITI)
				{	textAboveCentre.text="";
					camera1.enabled=false;
					yield return new WaitForSeconds(1f);
				}
			}
			else{
				yield return new WaitForSeconds(2f);
			}
			string recordtext="";
			if (trial==1) //add header
			{	recordtext="trial"+deli+"start2D_x"+deli+"start2D_y"+deli+"end2D_x"+deli+"end2D_y"+deli+"middleLoc"+deli+"correctLoc"+deli+"responseLoc"+deli+"time_init"+deli+"time_Q"+deli+"time_R"+deli+"option1"+deli+"option2"+"\n";
			}
			recordtext=recordtext+trial+deli+MK2string(tmpStart2D)+deli+MK2string(tmpEnd2D)+deli+tmpMid2D.ToString("F2")+deli+correctloc[trial]+deli+response+deli+time_init+deli+time_Q+deli+time_R+deli+q1[trial]+deli+q2[trial];
			StartCoroutine(save2file(outputfn,recordtext));
		}
		camera1.enabled=false;
		textAboveCentre.text="";
		var percentcorrect=(100*numcorrect)/(numcorrect+numincorrect+numtimeout);
		string runEndTime=System.DateTime.Now.ToString("yyyyMMdd_HHmm");
		meanaccu=numcorrect*1.0f/(numcorrect+numincorrect+numtimeout);
		string generalLogText=runStartTime+deli+runEndTime+deli+testSeqfn+deli+"overall accu="+meanaccu.ToString("F2")+deli+"timeout="+numtimeout+deli+"T1 accu="+numcorrectType1+"/"+numType1+",T2 accu:"+numcorrectType2+"/"+numType2;
		StartCoroutine(save2file(generalLogFn,generalLogText));		
		
		string scoreDepFeedbackText="";
		if (percentcorrect>=90)
			scoreDepFeedbackText="Excellent! You remember it really well.";
		if (percentcorrect>=80 & percentcorrect<90)
			scoreDepFeedbackText="Very good. You remember it well.";
		if (percentcorrect>=70 & percentcorrect<80)
			scoreDepFeedbackText="Good.";
		if (percentcorrect<70)
			scoreDepFeedbackText="Let's do better in the next round.";

		string tmpstring="Score: "+numcorrect+" out of "+(numcorrect+numincorrect+numtimeout)+" correct ("+percentcorrect+ "%)\n" + scoreDepFeedbackText;	
		tmpstring=tmpstring+"\n\nClick next to proceed.";
		textTop.text=tmpstring;
		textCentre.text="+";
			
		nextButton.SetActive(true);
		while (moveToNext==0)
			yield return null;
		moveToNext=0;
		textTop.text="";
	}
	void rotateCylinder(Vector3 rotAngles){
		cylinder.localEulerAngles=rotAngles;
		
		Quaternion rotation = Quaternion.Euler(rotAngles.x, rotAngles.y, rotAngles.z);
        m= Matrix4x4.Rotate(rotation);
        float maxX=-100; float maxY=-100; float maxZ=-100;
		float minX=100; float minY=100; float minZ=100;
		for (int ang=0;ang<360;ang=ang+15)
		{	for (float k=-1; k<=1; k=k+0.2f)
			{	Vector2 tmpvec2=new Vector2(ang,k);
				Vector3 tmpvec3_org=new Vector3(Mathf.Cos(ang*Mathf.Deg2Rad), k, Mathf.Sin(ang*Mathf.Deg2Rad));
				Vector3 tmpvec3_rot= m.MultiplyPoint3x4(tmpvec3_org); // range can be larger than [-1 to 1] because of rotation
				maxX=Mathf.Max(maxX,tmpvec3_rot.x);
				maxY=Mathf.Max(maxY,tmpvec3_rot.y);
				maxZ=Mathf.Max(maxZ,tmpvec3_rot.z);
				minX=Mathf.Min(minX,tmpvec3_rot.x);
				minY=Mathf.Min(minY,tmpvec3_rot.y);
				minZ=Mathf.Min(minZ,tmpvec3_rot.z);
					
				
				//Transform tmpobj=Instantiate(markerObject3D,5*tmpvec3_rot, Quaternion.identity);
				//Renderer tmpRend=tmpobj.GetComponent(typeof(Renderer)) as Renderer;
				//tmpRend.material.SetColor("_Color",Color.red);
				
			}
		}	
		rangeX=new Vector2(minX, maxX);
		rangeY=new Vector2(minY, maxY);
		rangeZ=new Vector2(minZ, maxZ);
	}
	void setRendererColor(GameObject whichobj,float r,float g, float b)
	{	Debug.Log("testRenderColor pressed");
		Renderer[] rend;
		rend=whichobj.GetComponentsInChildren<Renderer>();
		if (rend==null)
			Debug.Log("can't find Renderer");
		else
		{	for (int i=0;i<rend.Length;i++)
				rend[i].material.SetColor("_EmissionColor",new Color(r,g,b));
		}
	}

	void putMarkerTarget(Vector2 input2D, Transform markerTarget2D, Transform markerTarget3D){
		// input2D should be [0,360deg],[0,1]
		Vector3 markerPosOnPlane = new Vector3 (input2D.x*2*Mathf.PI*5/360, 10*input2D.y, 0);// It depends on where I placed the unrolled plane in Unity World space
		markerTarget2D.localPosition = markerPosOnPlane;//don't duplicate marker
		
		Vector3 normalised3D=new Vector3(Mathf.Cos(input2D.x*Mathf.Deg2Rad)*0.5f+0.5f, input2D.y, Mathf.Sin(input2D.x*Mathf.Deg2Rad)*0.5f+0.5f);
		Vector3 markerPosOnCylinder = new Vector3 (normalised3D.x*10-5, 10*input2D.y-5, normalised3D.z*10-5);
		Vector3 tmprotVec=new Vector3(0f,-input2D.x,0f);
		markerTarget3D.localEulerAngles=tmprotVec;
		markerTarget3D.localPosition=markerPosOnCylinder;
	}
	Vector3 MK2Dto3D(Vector2 input2D)
	{	//input2D should be ([0,360 deg], [0,1])
		Vector3 tmpvec3_org=new Vector3(Mathf.Cos(input2D.x*Mathf.Deg2Rad), input2D.y*2-1, Mathf.Sin(input2D.x*Mathf.Deg2Rad));
		Vector3 tmpvec3_rot= m.MultiplyPoint3x4(tmpvec3_org); // range can be larger than [-1 to 1] because of rotation
		Vector3 normalized3D=new Vector3((tmpvec3_rot.x-rangeX.x)/(rangeX.y-rangeX.x), (tmpvec3_rot.z-rangeZ.x)/(rangeZ.y-rangeZ.x), (tmpvec3_rot.y-rangeY.x)/(rangeY.y-rangeY.x));
		return normalized3D;
	}	

	//////////// utility function //////////////////////////
	string MK2string(Vector3 tmpvec){
		string output;
		output=tmpvec.x.ToString("F3")+deli+tmpvec.y.ToString("F3")+deli+tmpvec.z.ToString("F3");
		return output;
	}
	string MK2string(Vector2 tmpvec){
		string output;
		output=tmpvec.x+deli+tmpvec.y;
		return output;
	}
	IEnumerator save2file(string fn,string textToWrite){
		int whereToSave=1;
		if (whereToSave>=0) //local storage
		{   string saveLocalDir="./";
			StreamWriter sw=new StreamWriter(saveLocalDir+fn,true);
			sw.WriteLine(textToWrite);
			sw.Close();
		}
		if (whereToSave>=1){ //Server storage
			WWWForm form= new WWWForm();
			form.AddField("fn", fn);
			form.AddField("content", textToWrite+"\n");

			var www = UnityWebRequest.Post("http://vm-mkim-1.cbs.mpg.de/testPHP/php-example.php", form);
			{
				yield return www.SendWebRequest();

				if (www.isNetworkError) //it only return system error (DNS, socket) not the error from server side (404/Not Found)
				{
					Debug.Log(www.error);
					textTopRight.text="Oops! Server problem, stop the experiment and contact the researcher";
				}
				else
				{
					Debug.Log("Form upload complete!");
					textTopRight.text="";
				}
			}
		}
	}
	
}
