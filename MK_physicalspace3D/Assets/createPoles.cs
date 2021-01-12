using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class createPoles : MonoBehaviour {
	public Transform parentSphere, parentCircle;
	public Transform polePrefab;
	public Transform characterMK;
	public Transform trafficCone;
	// Use this for initialization
	void Start () {
		PolesOnSphere();
		PolesOnCircle();
		Transform cone1=Instantiate(trafficCone);
		sphCoordinate(cone1,90,80);
		Transform cone2=Instantiate(trafficCone);
		sphCoordinate(cone2,90,10);
		Transform cone3=Instantiate(trafficCone);
		sphCoordinate(cone3,0,10);
		

	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Alpha1))
			characterMK.position=new Vector3(0,14.5f,0);
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{	characterMK.position=new Vector3(50f,1.4f,0);
			characterMK.eulerAngles=new Vector3(0,0,0);
		}
	}
	void sphCoordinate(Transform objTrans, float azi, float pit){
		float Radius=12.5f;
		float tmpy=Radius*Mathf.Sin(pit*Mathf.Deg2Rad);
		float tmpx=Radius*Mathf.Cos(pit*Mathf.Deg2Rad)*Mathf.Cos(azi*Mathf.Deg2Rad);
		float tmpz=Radius*Mathf.Cos(pit*Mathf.Deg2Rad)*Mathf.Sin(azi*Mathf.Deg2Rad);
		Vector3 tmpPos=new Vector3(tmpx,tmpy,tmpz);
		Quaternion tmpRot=Quaternion.Euler(0,-azi,pit);
		objTrans.position=tmpPos;
		objTrans.rotation=tmpRot;
	}
	void PolesOnSphere(){
		float Radius=12.5f;
		int nPole=160;
		float[] aziList={0f,30f,90f,0f,45f};
		float[] pitList={0f,0f,0f,45f,45f};
		for (int i=0;i<nPole;i++){
			//float azi=aziList[i];
			//float pit=pitList[i];
			float azi=Random.Range(0,359);
			float pit=Random.Range(-89,89);
			float tmpy=Radius*Mathf.Sin(pit*Mathf.Deg2Rad);
			float tmpx=Radius*Mathf.Cos(pit*Mathf.Deg2Rad)*Mathf.Cos(azi*Mathf.Deg2Rad);
			float tmpz=Radius*Mathf.Cos(pit*Mathf.Deg2Rad)*Mathf.Sin(azi*Mathf.Deg2Rad);
			Vector3 tmpPos=new Vector3(tmpx,tmpy,tmpz);
			Quaternion tmpRot=Quaternion.Euler(0,-azi,pit);
			Transform tmpObj=Instantiate(polePrefab, tmpPos, tmpRot,parentSphere);
			tmpObj.name="pole"+azi+","+pit;
		}
	}
	void PolesOnCircle(){
		float Radius=12.5f;
		int nPole=40;
		float[] aziList={0f,30f,90f,0f,45f};
		float[] pitList={0f,0f,0f,45f,45f};
		for (int i=0;i<nPole;i++){
			//float azi=aziList[i];
			//float pit=pitList[i];
			float azi=Random.Range(0,359);
			float r=Random.Range(0.1f,Radius);
			float tmpy=0;
			float tmpx=r*Mathf.Cos(azi*Mathf.Deg2Rad);
			float tmpz=r*Mathf.Sin(azi*Mathf.Deg2Rad);
			Vector3 posOffset=new Vector3(50,0,0);
			//Vector3 tmpPos=new Vector3(tmpx,tmpy,tmpz)+posOffset;
			
			Vector2 insideCirclePos=Random.insideUnitCircle;
			Vector3 tmpPos=new Vector3(insideCirclePos.x,0,insideCirclePos.y)*Radius+posOffset;
			Quaternion tmpRot=Quaternion.Euler(0,0,90);
			Transform tmpObj=Instantiate(polePrefab, tmpPos, tmpRot,parentCircle);
			tmpObj.name="pole"+azi+","+r.ToString("#");
		}
	}
}
