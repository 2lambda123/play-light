using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;
using Debug=UnityEngine.Debug;

public class TrackerController : MonoBehaviour
{
    public GameObject[] circles;
    private float[,] calibTable = new float[5,6];
    private float[,] trM = new float[4,4]; //prospective transformation matrix

    private float speed = 0.3f;
    private float radius = 4f;
    private float pos = 0f;
    private float blobPosition_x = 0f;
    private float blobPosition_y = 0f;
    private float Amax =0f;
    private float area =0f;




////// WEBCAM vars
    int currentCamIndex = 1; // 0 - built in camera, 1 - usb camera
    WebCamTexture tex;
    int[] snapSizes = new int[3]; // width, height, tex.width * tex.height
    private Color[] pixels = new Color[1024*576];





    void Start()
    {
            LoadMatrix(trM, "trM");
            LoadMatrix(calibTable, "calibTable");
            StartStopCam_Clicked();

    }

    // Update is called once per frame
    void Update()
    {
        // Stopwatch watch = new Stopwatch();
        // watch.Start();
        
        Snap_Clicked();
//         watch.Stop();
//         var elapsedTime = watch.ElapsedMilliseconds;
// Debug.Log("elapsedTime= "+elapsedTime);

        pos += speed*Time.deltaTime;
        float x = Mathf.Sin(pos)*radius;
        float y = Mathf.Cos(pos)*radius;
Debug.Log("Time.deltaTime = " + Time.deltaTime + " angular vel = " + speed*Time.deltaTime + " pos = " +pos);
// Debug.Log("Time.deltaTime = " + Time.deltaTime + " x = " + x + " y = " + y);
// Debug.Log("blobPosition_x = " + blobPosition_x + " blobPosition_y = " + blobPosition_y);
        circles[0].transform.position = new Vector3(x,y,0f);
        circles[1].transform.position = new Vector3(blobPosition_x,-blobPosition_y,0f);
    }


    private void LoadMatrix(float[,] A, string basename){
        int iN = A.GetUpperBound(0)+1;
        int jN = A.GetUpperBound(1)+1;
        string element_name = $"{ basename + iN+jN*100}";
        for(int i=0;i<iN;i++){
            for(int j=0;j<jN;j++){
                element_name = $"{ basename + i+j*100}";
                //Debug.Log("element_name: "+element_name);
                A[i,j]=PlayerPrefs.GetFloat(element_name);
            }
        }
        
    }


#region WEBCAM

   public void StartStopCam_Clicked(){

        //Debug.Log("Start -- button pressed");
        if(tex != null)
        {
            StopCamera();

        }
        else
        {
        WebCamDevice device = WebCamTexture.devices[currentCamIndex];
        Debug.Log("Device name: " + device.name);
        tex = new WebCamTexture(device.name);

        tex.requestedFPS=60;
        // tex.requestedWidth=1024; // 80 ms
        // tex.requestedHeight=576;
        // tex.requestedWidth=320; // 7 ms
        // tex.requestedHeight=240;
        tex.requestedWidth=864; // 5 ms, SOLUTION!
        tex.requestedHeight=480;
        //display.texture = tex;

        tex.Play();
        }
    }

    private void StopCamera()
    {
        //display.texture = null;
        tex.Stop();
        tex = null;
    }

    public void Snap_Clicked(){

        //tic = Time.realtimeSinceStartup;
        // https://docs.unity3d.com/ScriptReference/Time-realtimeSinceStartup.html
        //Debug.Log("In Snap_Clicked, Time.realtimeSinceStartup: " + tic);

        if(tex != null) StartCoroutine(TakePhoto()); 

        //toc = Time.realtimeSinceStartup;
        //tictoc = toc - tic;
        //Debug.Log("In Snap_Clicked, tictoc: " + tictoc);

        //string s = string.Format("Snap time {0} s", tictoc);
        //tictocText.text = s.ToString();

    }
    
    IEnumerator TakePhoto()  // Start this Coroutine on some button click
    {

    // NOTE - you almost certainly have to do this here:
    // Debug.Log("in TakePhoto: yield return");
    // yield return null; //new WaitForEndOfFrame(); 
        yield return new WaitForEndOfFrame(); 

    // it's a rare case where the Unity doco is pretty clear,
    // http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
    // be sure to scroll down to the SECOND long example on that doco page 
        
        // Texture2D photo = new Texture2D(tex.width, tex.height);
        // photo.SetPixels(tex.GetPixels());
        // photo.Apply();

        
        

        int rowIndex = -1;
        int colIndex = -1;
        int maxIndex = -1;
        float maxInt = -2f; // can be less than -1f, eg, R - G => 0 - 1 = -1f
        // var pixels = new Color[tex.width * tex.height];  // moved to class vars to prevent multiple allocations of vars
        //float[,,] RmG = new float[tex.height,tex.width,4];
        float snapRmG1 =0f;
        float[] bindingBox = {0f,0f,0f,0f,0f}; 
        float[] colorSignat ={0f,0f,0f,0f};

        // pixels = photo.GetPixels();
        for(int it =0; it<20;it++){
            pixels = tex.GetPixels(10,10, 600,10);
        }


        //pixels = tex.GetPixels();
        snapSizes[0] = tex.width;
        snapSizes[1] = tex.height;
        snapSizes[2] = tex.width * tex.height;

        // if (snapSizes[2] > snapRmG.Length){
        //     Debug.Log("!!!in TakePhoto, tex.width * tex.height: " + snapSizes[2]);
        //     Debug.Log("!!!in TakePhoto, snapRmG.Length: " + snapRmG.Length);
        // }

        for (int i = 0; i < pixels.Length; i++) // get RmG array and find Max and Index
        {
            colIndex = i;
            colIndex %= (int) snapSizes[0]; // modulo of width gives position within row
            rowIndex = (int) i/snapSizes[0]; // how many full rows
            

            snapRmG1 = Mathf.Abs(pixels[i].r - pixels[i].g);  // substract Green from Red band
            // RmG[rowIndex, colIndex,0] = snapRmG1;
            // RmG[rowIndex, colIndex,1] = pixels[i].r;
            // RmG[rowIndex, colIndex,2] = pixels[i].g;
            // RmG[rowIndex, colIndex,3] = pixels[i].b;

            if (snapRmG1 > maxInt)
            {
                maxInt = snapRmG1;
                maxIndex = i;
            }
            
            

        }
        
        
print($"width = {snapSizes[0]}, hight {snapSizes[1]}.");
print($"Maximum Intensity = {maxInt}, on index {maxIndex}.");

        colIndex = maxIndex;
        colIndex %= (int) snapSizes[0]; // modulo of width gives position within row
        rowIndex = (int) maxIndex/snapSizes[0]; // how many full rows
// print($"rowIndex = {rowIndex}, colIndex {colIndex}.");

        //( Amax,  blobPosition_x,  blobPosition_y,  area)=DetectBlobUnity(bindingBox, colorSignat, RmG);
        
    }//IEnumerator TakePhoto()

#endregion
    private (float Amax, float blobPosition_x, float blobPosition_y, float area) DetectBlobUnity(float[] bindingBox, float[] colorSignat, float[,,] imgLa){

        float [] origI = { imgLa.GetUpperBound(2), imgLa.GetUpperBound(0), imgLa.GetUpperBound(1)};
        float[,] cT = new float[5,6]; //local calibaration table
        // origI(0) = 4 (r-b, r,g,b)
        // sizeVert = origI(1);
        // sizeHor = origI(2);
// Debug.Log("origI[0]= "+origI[0]+"origI[1]= "+origI[1]+"origI[2]= "+origI[2]);


        (float cCx,float cCy) = IntersectionPoint(calibTable[1,1],  calibTable[2,1],  calibTable[1,2],  calibTable[2,2]);
        (float NLx,float NLy) = IntersectionPoint(calibTable[1,2],  calibTable[2,2],  calibTable[1,3],  calibTable[2,3]);
        (float NRx,float NRy) = IntersectionPoint(calibTable[1,1],  calibTable[2,1],  calibTable[1,3],  calibTable[2,3]);
        (float FLx,float FLy) = IntersectionPoint(calibTable[1,1],  calibTable[2,1],  calibTable[1,4],  calibTable[2,4]);
        (float FRx,float FRy) = IntersectionPoint(calibTable[1,2],  calibTable[2,2],  calibTable[1,4],  calibTable[2,4]);

// Debug.Log("NL: " + NLx+",  "+ NLy);
// Debug.Log("NR: " + NRx+",  "+ NRy);
// Debug.Log("FL: " + FLx+",  "+ FLy);
// Debug.Log("FR: " + FRx+",  "+ FRy);



        (cT[1,4], cT[2,4])=TwoPointsToLine(FLx,FLy, FRx,FRy); //up
        (cT[1,3], cT[2,3])=TwoPointsToLine(NLx,NLy, NRx,NRy); //lo
        (cT[1,2], cT[2,2])=TwoPointsToLine(NLx,NLy, FLx,FLy); //left
        (cT[1,1], cT[2,1])=TwoPointsToLine(NRx,NRy, FRx,FRy); //right
// Debug.Log("Up: " + cT[1,4]+",  "+cT[2,4]);
// Debug.Log("Lo: " + cT[1,3]+",  "+ cT[2,3]);
// Debug.Log("Left: " + cT[1,2]+",  "+ cT[2,2]);
// Debug.Log("Right: " + cT[1,1]+",  "+ cT[2,1]);

        // Centroid
        float keepPxV=40f; // in pixels
        float keepPxH=60f;
        float thresHold = 0.25f; // threshold for binarization in percent from max
        float cX=0f;
        float cY = 0f; // centroid
        float area = 0f; // area of blob
        float colorsR = 000.0f; // avearged colors of blob
        float colorsG = 000.0f; // avearged colors of blob
        float colorsB = 000.0f; // avearged colors of blob

        int[] ROIbox = {0, (int)Mathf.Floor(keepPxV*origI[1]/1080.0f), (int)Mathf.Floor(keepPxH*origI[2]/1920.0f)};
//Debug.Log("imgLa[i,j,0]: "+imgLa[240,500,0]);
        // finding max intensity within playable area 
        float Amax = -999f; int Ai=0;int Aj=0; //search for max point
        float AmaxAbs = -999f; int Aia=0;int Aja=0; //search for max point

        for (int i =1; i<=origI[1]; i++){
            for (int j =1; j<=origI[2]; j++){

                    if (AmaxAbs<=imgLa[i,j,0]){
                        AmaxAbs=imgLa[i,j,0];
                        Aja=j;
                        Aia=i;
                    }    

                int up=Mathf.FloorToInt(cT[1,4]+ cT[2,4]*j);
                int lo=Mathf.FloorToInt(cT[1,3]+ cT[2,3]*j);
                int left=Mathf.FloorToInt((i-cT[1,2])/cT[2,2]);
                int right=Mathf.FloorToInt((i-cT[1,1])/cT[2,1]);
// if(i==400 && j==500) {Debug.Log("Up: " + up+" lo: " + lo+" left: " + left+" right: " + right);}

                if((i <= up) && (i >= lo) && (j <= left) && (j >= right)){ // search in only playbale area
                    if (Amax<=imgLa[i,j,0]){
                        Amax=imgLa[i,j,0];
                        Aj=j;
                        Ai=i;
                    }       
                }
            }
        }
// Debug.Log("Amax: "+Amax + " Ai= " + Ai + " Aj= " + Aj + "  #  AmaxAbs: "+AmaxAbs + " Aia= " + Aia + " Aja= " + Aja);
            float [] xy = {0f,FLx,FLy};
            float u=0f; float v=0f;
                xy[1] = (float)Aj; 
                xy[2] = (float)Ai;
                ( u,  v) = myCamera2World(trM, xy);
// Debug.Log("u = " +u + " v = "+v);
        return(Amax: 0f, blobPosition_x: u, blobPosition_y: v, area: 0f);
    }//private (float Amax, float blobPosition_x, float blobPosition_y, float area) DetectBlobUnity








    private (float px,float py) IntersectionPoint(float a1,float b1, float a2,float b2){
        float a = b1;
        float c = a1;
        float b = b2; 
        float d = a2;
        float dt= (a-b);
        if( (0<=dt) && (dt < 0.0001) ){ dt =0.0001f; }
        if( (0>=dt) && (dt > -0.0001) ){ dt =-0.0001f; }
        return(px: (d-c)/dt, py: a*(d-c)/dt+c);
    }//private (float px,float py) IntersectionPoint
    private (float a, float b) TwoPointsToLine(float x0,float y0, float x1,float y1){

    // y = bx +a

// x0 = 420;
// y0 = 353;
// x1 = 431;
// y1 = 439;

        float dt = (x0-x1);
        if ((0f <= dt) && (dt<0.001f)){ dt =  0.001f;} 
        if ((0f >= dt) && (dt>-0.001f)){dt = -0.001f;}
    

        float bt = (y0-y1)/dt;
        float a0 = y0-bt*x0;
        float a1 = y1-bt*x1;

        return(a: (a0+a1)/2.0f, b: bt);
    }
    private (float u, float v) myCamera2World(float[,] trMat, float[] xy){
        float[] xy3 = {0f,xy[1], xy[2], 1.0f};
        float[] xyp = {0f,0f,0f,0f};
        MatDotVec(xyp, trMat, xy3);
        return(u: xyp[1]/xyp[3], v: xyp[2]/xyp[3]);
    }
    private void MatDotVec(float[] u, float[,] A, float[] v){
        u[1]= A[1,1]*v[1]+A[1,2]*v[2]+A[1,3]*v[3];
        u[2]= A[2,1]*v[1]+A[2,2]*v[2]+A[2,3]*v[3];
        u[3]= A[3,1]*v[1]+A[3,2]*v[2]+A[3,3]*v[3];
    }//private void MatDotVec(f





}//Class
