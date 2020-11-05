// in progress to visualise behavioural data..ss

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class checkBehavData_ver1 : MonoBehaviour {
    private JSONNode taskparam;
    public Transform character;
    public Vector3[] posList, eulerList;
    public int trial;
    public int[] publicIntList;
    public Text text_top;
    public Transform pictureCube;
    private Vector3[] objLoc;
    public float time_step = 0.05f;
    public int moveConstraint=3;
    // Use this for initialization
    IEnumerator Start () {
        Debug.Log("start of checkBehavData_ver1.cs");
        TextAsset jsonTextAsset = Resources.Load<TextAsset>("msub14_inputParam_1001");
        string the_JSON_string = jsonTextAsset.text;

        taskparam = JSON.Parse(the_JSON_string);
        objLoc=json2vector3(taskparam["objLoc"]);
        yield return null;
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyDown(KeyCode.Return))
        {   Debug.Log("Return pressed in checkBehavData_ver1.cs"); 
            StopCoroutine(replayTraj());
            StartCoroutine(replayTraj());
            Debug.Log("hi misun"); 
        }

        float rotateSpeed=80;
            float translateSpeed=8;
            float distCollide=0.5f;
            float characterRadius=0.25f;
            
        if (moveConstraint==4) // fly movement with easy rotation
        {   float rotationX = character.localEulerAngles.y + Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime;
            float rotationY = character.localEulerAngles.x - Input.GetAxis("Vertical") * rotateSpeed * Time.deltaTime;
            character.localEulerAngles = new Vector3(rotationY, rotationX, 0);
        }  
        
        if (moveConstraint == 3) // fly movement with difficult rotation 
        {   character.Rotate(0, 1*Input.GetAxis("Horizontal") * rotateSpeed * Time.deltaTime, 0, Space.Self);
            character.Rotate(-Input.GetAxis("Vertical") * rotateSpeed * Time.deltaTime, 0, 0, Space.Self);
        }

            Vector3 p1 = character.position + character.up * -0.2f; //capsule cast bottom position, depending on the hover distance between the character center and ground
            Vector3 p2 = character.position + character.up * 0.2f; //capsule cast top position 
            Debug.DrawLine(p1, p1 + character.forward * distCollide, Color.red);
            Debug.DrawLine(p2, p2 + character.forward * distCollide, Color.red);

            RaycastHit hit;
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

    Vector3[] json2vector3(JSONNode tmpnode)
    {
        Vector3[] newarray = new Vector3[tmpnode.Count];
        for (var i = 0; i < tmpnode.Count; i++) newarray[i] = tmpnode[i];
        return newarray;
    }
    IEnumerator replayTraj()
    {   Debug.Log("replayTraj(), trial="+trial);
        //var maintestlist = Resources.Load<TextAsset>("msub10_ull_familiarisation_20201027_171052");
        //var maintestlist=Resources.Load<TextAsset>("msub28_ull_familiarisation_20201029_114502");
        //var maintestlist=Resources.Load<TextAsset>("msub14_ull_ObjLoc_TestRun1Traj_20201030_175945");
        var maintestlist=Resources.Load<TextAsset>("behResult/msub14_ull_ObjLoc_TestRun1Traj_20201103_113630");
        var tmp = maintestlist.text.Split("\n"[0]);
        int[] trialIdx=new int[tmp.Length];
        //start, end, target
        // while interp?
        // traj_x, traj_y, traj_z[t]
        int n=0;//how many rows are there for each trial?
        for (int j = 1; j < tmp.Length - 1; j++)
        {
            var mpt = tmp[j].Split("\t"[0]);
            try{
                if (int.Parse(mpt[7])==trial)
                {   trialIdx[n]=j;
                    n++;
                }
            }catch{
                Debug.Log("problem with parsing text, tmp[j].Split= "+tmp[j]);
            }
        }
        publicIntList=trialIdx;
        posList = new Vector3[n];
        eulerList = new Vector3[n];
        int[] phaseList=new int[n];
        int[] objIdList=new int[n];
        for (int j = 0; j < posList.Length; j++)
        {
            var mpt = tmp[trialIdx[j]].Split("\t"[0]);
            posList[j] = new Vector3(float.Parse(mpt[1]), float.Parse(mpt[2]), float.Parse(mpt[3]));
            eulerList[j] = new Vector3(float.Parse(mpt[4]), float.Parse(mpt[5]), float.Parse(mpt[6]));
            phaseList[j] = int.Parse(mpt[9]);
            objIdList[j]=int.Parse(mpt[8]);
        }
        

        Debug.Log("posList[0]" + posList[0].ToString("F1"));
        Debug.Log("posList[1]" + posList[1].ToString("F1"));
        Debug.Log("posList[end-1]" + posList[posList.Length - 2].ToString("F1"));
        Debug.Log("posList[end]" + posList[posList.Length-1].ToString("F1"));
        Vector3 curPos;
        Quaternion curQuat;

        pictureCube.position=norm2DtoPhy3D(objLoc[objIdList[0]],pictureCube);
        pictureCube.gameObject.SetActive(false);
        for (int i = 0; i < posList.Length-1; i++) {
            float time_init = Time.time;
            
            Quaternion preQuat = Quaternion.Euler(eulerList[i].x, eulerList[i].y, eulerList[i].z);
            Quaternion postQuat = Quaternion.Euler(eulerList[i+1].x, eulerList[i+1].y, eulerList[i+1].z);
            //text_top.text="phase "+phaseList[i];
            if (phaseList[i]>0){
                pictureCube.gameObject.SetActive(true);
            }

            while (Time.time-time_init <= time_step)
            {
                hitCheck("Respawn");

                curPos = Vector3.Lerp(posList[i], posList[i + 1], (Time.time - time_init) / time_step);
                curQuat = Quaternion.Slerp(preQuat, postQuat, (Time.time - time_init) / time_step);
                character.position = curPos;
                character.rotation = curQuat;

                yield return null;
            }

        //    yield return null;
        }
        yield return  null;
    }
   bool hitCheck(string tagName)
    {
        float characterRadius=0.5f;
        float distCollide=0.5f;

        RaycastHit hit;
        Vector3 p1 = character.position + character.up * -1f; //capsule cast bottom position, depending on the hover distance between the character center and ground
        Vector3 p2 = character.position + character.up * 1f; //capsule cast top position 
        Vector3 p3 = character.position + character.up * 0f+character.right*characterRadius; //capsule cast top position 
        Vector3 p4 = character.position + character.up * 0f-character.right*characterRadius; //capsule cast top position 
        Debug.DrawLine(character.position, character.position + character.up*2, Color.blue);
        Debug.DrawLine(p1, p1 + character.forward*distCollide, Color.blue);
        Debug.DrawLine(p2, p2 + character.forward*distCollide, Color.blue);
        Debug.DrawLine(p3, p3 + character.forward*distCollide, Color.blue);
        Debug.DrawLine(p4, p4 + character.forward*distCollide, Color.blue);
        
        bool didHit = false;
        //testfloat=Vector3.Distance(character.position,prop3D.position);
        if (Physics.CapsuleCast(p1, p2, characterRadius, character.forward, out hit, distCollide))
        {
            if (hit.collider.tag == tagName)
            {    didHit = true;
                text_top.text="hit the target,tag= "+tagName;
            }
            text_top.text="hit something else";
        }else{
            text_top.text="";
        }
        return didHit;
    }

    Vector3 norm2DtoPhy3D(Vector3 normPos2D, Transform obj3D)
    {   
        // convert normalised 2D coordinate onto 3D coordinate in Unity
        // input should be x=(-1,1), y=(0,1)
        // when I adjust the size/offset of environment on Unity, I should adjust this part correctly
        // I also have to update "conv3Dto2D" function correctly
        // because it's not simple linear transformation matrix, I can't just use the inverse of rigic body transformatio matrix here
        float Radius = 0;
        float sqDim=25;
        float tmpx = 0; float tmpy = 0; float tmpz = 0;
        float rotY = normPos2D.z;
        Vector3 baseNormal = new Vector3(0, 0, 0);// base Euler angles on the surface
        int envType=4;
        switch (envType)
        {
            case 1: // slope
                if (normPos2D.x < 0)
                {
                    tmpx = normPos2D.x * sqDim + 0f;
                    tmpy = 0f;
                    tmpz = normPos2D.y * sqDim - 70f;

                    baseNormal = new Vector3(0, 0, 0);
                }
                if (normPos2D.x >= 0)
                {
                    tmpx = normPos2D.x * Mathf.Cos(Mathf.Deg2Rad * 45) * sqDim + 0f;
                    tmpy = normPos2D.x * Mathf.Sin(Mathf.Deg2Rad * 45) * sqDim + 0f;
                    tmpz = normPos2D.y * sqDim - 70f;

                    baseNormal = new Vector3(0, 0, 45);
                }
                break;
            case 2: // half cylinder
                Radius = 25 / Mathf.PI;
                if (normPos2D.x < 0)
                {
                    tmpx = normPos2D.x * sqDim + 0f;
                    tmpy = 0f;
                    tmpz = normPos2D.y * sqDim - 25f;

                    baseNormal = new Vector3(0, 0, 0);
                }
                if (normPos2D.x >= 0)
                {
                    tmpx = Mathf.Sin(normPos2D.x * Mathf.PI) * Radius + 0f; //slight off set of 0.5 because step between the flat surface and half moon
                    tmpy = -Mathf.Cos(normPos2D.x * Mathf.PI) * Radius + Radius;
                    tmpz = normPos2D.y * sqDim - 25f;

                    baseNormal = new Vector3(0, 0, (normPos2D.x * Mathf.PI) * Mathf.Rad2Deg);
                }
                break;
            case 3: //S shape curve
                Radius = 25 / Mathf.PI;
                if (normPos2D.x < 0)
                {
                    tmpx = normPos2D.x * sqDim + 0f;
                    tmpy = 0f;
                    tmpz = normPos2D.y * sqDim - 40f;

                    baseNormal = new Vector3(0, 0, 0);
                }
                if (normPos2D.x >= 0 & normPos2D.x < 0.5f)
                {
                    tmpx = Mathf.Sin(normPos2D.x * Mathf.PI) * Radius;
                    tmpy = Radius - Mathf.Cos(normPos2D.x * Mathf.PI) * Radius;
                    tmpz = normPos2D.y * sqDim - 40f;

                    baseNormal = new Vector3(0, 0, (normPos2D.x * Mathf.PI) * Mathf.Rad2Deg);
                }
                if (normPos2D.x >= 0.5f & normPos2D.x < 1f)
                {
                    tmpx = -Mathf.Cos((normPos2D.x - 0.5f) * Mathf.PI) * Radius + 2 * Radius;
                    tmpy = Radius + Mathf.Sin((normPos2D.x - 0.5f) * Mathf.PI) * Radius;
                    tmpz = normPos2D.y * sqDim - 40f;

                    baseNormal = new Vector3(0, 0, (Mathf.PI - normPos2D.x * Mathf.PI) * Mathf.Rad2Deg);
                }
                if (normPos2D.x >= 1f)
                {
                    tmpx = 2 * Radius + (normPos2D.x - 1) * sqDim;
                    tmpy = 2 * Radius;
                    tmpz = normPos2D.y * sqDim - 40f;
                    baseNormal = new Vector3(0, 0, 0);
                }
                break;


            case 4: // 270deg cylinder
                //Radius = 25 / Mathf.PI*2/3;
                Radius = 5.3f;
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
                break;
        }

        Vector3 outPos3D = new Vector3(tmpx, tmpy, tmpz);

        obj3D.position = outPos3D;
        obj3D.eulerAngles = baseNormal;
        obj3D.Rotate(new Vector3(0, rotY, 0), Space.Self);

        Debug.DrawLine(obj3D.position, obj3D.position + obj3D.up * 1, Color.red);
        Debug.DrawLine(obj3D.position + obj3D.up * 1, obj3D.position + obj3D.up * 1 + obj3D.forward * 2, Color.red);

        return outPos3D;
    }
}
