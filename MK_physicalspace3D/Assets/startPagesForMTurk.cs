using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class startPagesForMTurk : MonoBehaviour {
	public int moveToNext=0;
	public GameObject startKeywordInputHolder;
	public string[] loginDict_ID, loginDict_PW;
	public Text textTop, text_ID, text_PW;
	public GameObject fullscreenTextHolder;
	public Toggle consentToggle1, consentToggle2;
	public Text fullscreenText;
	public Button nextButton;

	public GameObject demographHolder;
	public Text inputAge, agewarningText;
	public Dropdown dropdownSex;
	string subId;
	// Use this for initialization
	IEnumerator Start () {
		Debug.Log("start: startPagesForMTurk.cs");

		yield return initialLogIn();
		yield return instructionPhase();
		yield return demographicPhase();
		SceneManager.LoadScene("scn_July17"); //go back to main menu
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Return))
			Debug.Log("return pressed MK");
		if (Input.GetKey(KeyCode.Alpha1))
		{	if (Input.GetKeyDown(KeyCode.Alpha2)) 
			{		SceneManager.LoadScene("scn_July17"); //go back to main menu
			}
		}
	}
	IEnumerator initialLogIn(){
		Debug.Log("start: initialLogIn()");
		
		// load preset ID/PW information from text file
		string inputfn="online_loginDict"; // ID and PW info
		var maintestlist = Resources.Load<TextAsset>(inputfn);
        var tmp=maintestlist.text.Split("\n"[0]);
		loginDict_ID=new string[tmp.Length];
		loginDict_PW=new string[tmp.Length];
		for (int j=1;j<tmp.Length-1; j++)
		{	var mpt=tmp[j].Split(","[0]);
			loginDict_ID[j]=mpt[0]; // ID
			loginDict_PW[j]=mpt[1]; //PW
		}
		loginDict_ID[0]="psub99";loginDict_PW[0]="0000";
		// actual LogIn process
		startKeywordInputHolder.SetActive(true);
		int IDcheck=0;
		while(IDcheck==0){
			if (moveToNext==1)
			{	moveToNext=0;
				//check whether it's right ID/PW for the experiment
				
				
				for (int i=0; i<loginDict_ID.Length; i++){
					if(text_ID.text==loginDict_ID[i] & text_PW.text==loginDict_PW[i])
					{	Debug.Log("correct ID/PW"); IDcheck=1; subId=text_ID.text;
						break;
					}
				}
				if (IDcheck==1)
				{	textTop.text="Next part will begin soon..";
					startKeywordInputHolder.SetActive(false);
				}
				else
				{	textTop.text="Invalid ID/PW information! You can use ID:psub99, PW:0000 if you are not from MTurk website.";}
			}
			yield return null;
		}
		Debug.Log("end: initialLogin()");
		yield return null;
	}
	IEnumerator instructionPhase(){
		Debug.Log("start: instructionPhase()");
		fullscreenTextHolder.SetActive(true);
	//	var maintestlist=Resources.Load<TextAsset>("instructionText_20200323");
	//	fullscreenText.text=maintestlist.text;
		nextButton.gameObject.SetActive(false);
	//	yield return new WaitForSeconds(4);//let's disable the next button for few seconds so that subject don't accidentally press the button without reading the instruction
	//	nextButton.gameObject.SetActive(true);
		while (moveToNext==0)
		{   yield return null;
		}
		Debug.Log("end: instructionPhase()");
		moveToNext=0;//reset the movetoNext button before the new phase
		fullscreenTextHolder.SetActive(false);//remove the instruction text
	}
	IEnumerator demographicPhase(){
		demographHolder.SetActive(true);
		Debug.Log("start:demographicPhase()");
		int custombreakCondition=0;

		while (custombreakCondition==0)
		{    if (moveToNext==1)
			{    moveToNext=0; // reset the move To Next button
				if (inputAge.text!=""& dropdownSex.value!=0)
				{   var sex=dropdownSex.value;
					var age=int.Parse(inputAge.text);
					if (age>=18 & age<=35)
					{    custombreakCondition=1;

						GameObject obj=GameObject.Find("globalVariable");
						if (!obj)
						{	Debug.Log("globalVariable does not exist, go back to main menu!");
						}else 
						{	keepThisVariable scriptobj=obj.GetComponent("keepThisVariable") as keepThisVariable;
							scriptobj.subId=subId;
							scriptobj.sex=sex;
							scriptobj.age=age;
							scriptobj.run=0;
						}
					}
					else
					{
						agewarningText.text="<color=red>(You should be between 18 and 35 years old to participate in this study</color>)";
					}
				}
			}
			yield return null;
		}

	    demographHolder.SetActive(false); //remove the demographic component
	}
	public void ClickedNext(){
		moveToNext=1;
		Debug.Log("clickedNext()");
		EventSystem.current.SetSelectedGameObject(null);// to prevent the color of "pressed button" stay (this is related to keyboard navigation of UI. By deselect the object, I disable participants to choose different button using arrow keys)

	}
}
