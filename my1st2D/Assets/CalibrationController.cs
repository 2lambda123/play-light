using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CalibrationController : MonoBehaviour
{
#region Vars for Calibration   
public GameObject[] lines;
public GameObject[] circles;
public double waitTime = 1f; // in seconds
private double nowTime;
private double lastInterval;
private int cont = 0;
private int lineCnt = 0;

////// WEBCAM vars
int currentCamIndex = 1; // 0 - built in camera, 1 - usb camera

WebCamTexture tex;
public float[] snapRmG = new float[10000000];
public int[] snapSizes = new int[3]; // width, height, tex.width * tex.height
public string fileNamePrefix = "image_";
public string path = "C:/Users/kpervushin/Downloads"; // = Application.dataPath;
private float[,,] grayimgs = new float[576,1024,10];
private Texture2D[] photos = new Texture2D[10];

//////// Calibration Table vars
public Camera calibrationUsingCamera;
private float[,] calibTable = new float[5,6];
private float[,] trM = new float[4,4]; //prospective transformation matrix

private float halfViewport = 0f;

#endregion

    void Start()
    {
        
        calibrationUsingCamera = UnityEngine.Camera.main;
        halfViewport = (calibrationUsingCamera.orthographicSize * calibrationUsingCamera.aspect);
        float aspectRatio = (float) Screen.width / (float) Screen.height;
        
        
        calibTable[1,5] = -halfViewport; //World sizes X left
        calibTable[2,5] = halfViewport; //World sizes X right
        calibTable[3,5] = -halfViewport/aspectRatio; //World sizes Y near (down)
        calibTable[4,5] = halfViewport/aspectRatio; //World sizes Y far (up)
        // Debug.Log(halfViewport);
        // Debug.Log("calibTable[4,5]: "+calibTable[4,5]);

        lines[0].SetActive(false);
        lines[1].SetActive(false);
        lines[2].SetActive(false);
        lines[3].SetActive(false);

        StartStopCam_Clicked();

        lastInterval = Time.realtimeSinceStartup;

    } //Start

    void Update(){
        nowTime = Time.realtimeSinceStartup;
        if (nowTime > lastInterval+waitTime && cont <=7)
        {
            Snap_Clicked();

            Debug.Log("lineCnt = " + lineCnt);

            if (lines[lineCnt].activeSelf == false)
            {
               lines[lineCnt].SetActive(true); 
            }
            else 
            {
               lines[lineCnt].SetActive(false); 
            }

            Debug.Log("waitTime passed, cont = " +cont);
            
            lineCnt=(1+cont)/2;
            cont++;
            lastInterval = nowTime;

        };

        if(nowTime > lastInterval+waitTime && cont == 8){
            
            // lines[0].SetActive(true);
            // lines[1].SetActive(true);
            // lines[2].SetActive(true);
            // lines[3].SetActive(true);
            
            Debug.Log("Process photos, cont = " +cont);
            cont++; //do processing once
            lastInterval = nowTime;
            Texture2D photoRes = new Texture2D(photos[1].width, photos[1].height);

            //Color[] pix = photos[1].GetPixels();
            for(int i=0;i<4;i++){
                lines[i].SetActive(true);
                PhotosSubstract(photos[i*2+1], photos[i*2+2], photoRes); 
                string image_path = path + $"/{ fileNamePrefix + i}.png";        
                byte[] bytes = photoRes.EncodeToPNG();
                File.WriteAllBytes(image_path, bytes);
                //(float aMax, int ai, int aj) = MaxPointInPhoto(photoRes);
                //print($"Maximum Intensity = {aMax}, on index {ai} and {aj}.");
                (float a, float b) = LinearRegression(photoRes,i);

                calibTable[1,i+1]= a;
                calibTable[2,i+1]= b;

                var cubeRenderer = lines[i].GetComponent<Renderer>();
                cubeRenderer.material.SetColor("_Color", Color.green);

            }//for(int i=0;i<4;i++) 
            // SaveCalibration();
            // calibTable[1,5]= 100f;
            // LoadCalibration();
            // Debug.Log("After LoadCalibration: " + calibTable[1,5]);

            (float cCx,float cCy) = IntersectionPoint(calibTable[1,1],  calibTable[2,1],  calibTable[1,2],  calibTable[2,2]);
            (float NLx,float NLy) = IntersectionPoint(calibTable[1,2],  calibTable[2,2],  calibTable[1,3],  calibTable[2,3]);
            (float NRx,float NRy) = IntersectionPoint(calibTable[1,1],  calibTable[2,1],  calibTable[1,3],  calibTable[2,3]);
            (float FLx,float FLy) = IntersectionPoint(calibTable[1,1],  calibTable[2,1],  calibTable[1,4],  calibTable[2,4]);
            (float FRx,float FRy) = IntersectionPoint(calibTable[1,2],  calibTable[2,2],  calibTable[1,4],  calibTable[2,4]);

//Debug.Log("Intersection point cC: " + cCx + ", "+cCy);

        // calibTable[1,5] = -halfViewport; //World sizes X left
        // calibTable[2,5] = halfViewport; //World sizes X right
        // calibTable[3,5] = -halfViewport/aspectRatio; //World sizes Y near (down)
        // calibTable[4,5] = halfViewport/aspectRatio; //World sizes Y far (up)
 
            float[, ] fixedPoints = { {0f,0f,0f}, {0f, calibTable[1,5],-calibTable[4,5]}, { 0f, calibTable[2,5], -calibTable[4,5]}, {  0f, calibTable[2,5], -calibTable[3,5]}, { 0f, calibTable[1,5], -calibTable[3,5]} };
         //     float[, ] fixedPoints = { {uWorldX(1),-uWorldY(2)}, { uWorldX(2), -uWorldY(2)}, {  uWorldX(2), -uWorldY(1)}, { uWorldX(1), -uWorldY(1)} };
            float[, ] movingPoints = {{0f,0f,0f}, {0f,FLx,FLy}, {0f,FRx,FRy}, {0f,NRx,NRy}, {0f,NLx,NLy}};

            float [] xy = {0f,FLx,FLy};
            float u=0f; float v=0f;
        //float[, ] movingPoints = {{0f, 246.3750f, 195.8750f},{0f, 246.3750f, 195.8750f},{0f, 800.6250f, 207.3750f},{0f, 738.3750f, 410.3750f},{0f, 13.8750f, 549.3750f}};
        //float[, ] fixedPoints = {{0f,0f,0f},{0f, 3.625f, 2.625f }, {0f, 1018.875f, 2.875f }, {0f, 739.125f, 414.125f }, {0f, 6.625f, 571.125f }};
        
//Debug.Log("movingPoints"); PrintMat4x2(movingPoints);
Debug.Log("fixedPoints"); PrintMat4x2(fixedPoints);

            Myfitgeotrans(trM, movingPoints,fixedPoints);
//Debug.Log("trM");PrintMat3x3(trM);

//         xy[1] = movingPoints[1,1]; 
//         xy[2] = movingPoints[1,2];
//         ( u,  v) = myCamera2World(trM, xy);
// Debug.Log("u,v of Intersection point in World: " + u + ", "+v);
        
            
            
            for(int k=1;k<=4;k++){
                xy[1] = movingPoints[k,1]; 
                xy[2] = movingPoints[k,2];
                ( u,  v) = myCamera2World(trM, xy);
            
                // Debug.Log("x,y of "+k+" th Intersection point in Camera: " + xy[1] + ", "+xy[2]);
                // Debug.Log("fixedPoints: " + fixedPoints[k,1] + ", "+fixedPoints[k,2]);
                // Debug.Log("u,v of Intersection point in World: " + u + ", "+v);

                circles[k-1].transform.position = new Vector3(u,v,0f);
            }
            
            SaveMatrix(trM, "trM"); 
            SaveMatrix(calibTable, "calibTable");
            LoadMatrix(trM, "trM");
            LoadMatrix(calibTable, "calibTable");

            tex.Stop(); // Stop camera
            tex = null;
  
       
       }//if(nowTime > lastInterval+waitTime && cont == 8)

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
        tex.requestedWidth=1024;
        tex.requestedHeight=576;

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
        
        Texture2D photoMy = new Texture2D(tex.width, tex.height); // for testing purposes, when PNG is written
        Texture2D photo = new Texture2D(tex.width, tex.height);
        photo.SetPixels(tex.GetPixels());
        photo.Apply();

        photos[cont] = photo; // store photos in array for processing
        

        int rowIndex = -1;
        int colIndex = -1;
        int maxIndex = -1;
        float maxInt = -2f; // can be less than -1f, eg, R - G => 0 - 1 = -1f
        var pixels = new Color[tex.width * tex.height];

        pixels = photos[cont].GetPixels();

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

            grayimgs[rowIndex,colIndex,cont] = (pixels[i].r + pixels[i].g + pixels[i].b)/3f;
            float snapRmG1 = pixels[i].r - pixels[i].g;  // substract Green from Red band
            if(snapRmG1<0f) snapRmG1 = 0f; // reset to zero if negative

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

        //Debug.Log("in TakePhoto, pixels[10].r: " + pixels[10].r);
        //Debug.Log("in TakePhoto, snapRmG[10]: " + snapRmG[10].ToString());

        //imagPosHor = colIndex/640f*10.0f;
        //imagPosVer = rowIndex/480f*5.0f;

        //print($"imagPosHor = {imagPosHor}, imagPosVer {imagPosVer}.");
        // https://docs.unity3d.com/ScriptReference/ImageConversion.EncodeToPNG.html

        string image_path = path + $"/{ fileNamePrefix + cont}.png";

        photoMy = photos[cont];
        byte[] bytes = photoMy.EncodeToPNG();
        File.WriteAllBytes(image_path, bytes);
       //File.WriteAllBytes("C:/Users/kpervushin/Downloads/photo.png", bytes);
    }


// https://stackoverflow.com/questions/41493876/how-to-save-a-png-using-unity-webcamtexture
#endregion

/////////////////// PROCESS PHOTOS /////////////

    private void PhotosSubstract(Texture2D photoBack, Texture2D photoFront, Texture2D photoResult){

        var pixBack = new Color[photoBack.height*photoBack.width];
        var pixFront = new Color[photoFront.height*photoFront.width];
        var pixRes = new Color[photoResult.height*photoResult.width];

        pixBack = photoBack.GetPixels();
        pixFront = photoFront.GetPixels();
    
    

        for (int i = 0; i < pixBack.Length; i++)
            {
                Color c = new Color(pixFront[i].r - pixBack[i].r, pixFront[i].g - pixBack[i].g, pixFront[i].b - pixBack[i].b, 1.0f);
                pixRes[i] = c;                             
            }

        photoResult.SetPixels(pixRes);
        photoResult.Apply();

    }//private void PhotosSubstract()



    private (float aOut, float bOut) LinearRegression(Texture2D photoDiff, int cont){

        int[,] points = new int[1024*576,2];
        int[,] pointsFiltered = new int[1024*576,2];
        float thresHold = 1.0f;
        int numPoints = 0;
        Color cBlack = new Color(0f,0f,0f, 1.0f);        
        Color cWhite = new Color(1f,1f,1f, 1.0f);
        float aIni=0f;
        float bIni=0f;

    // CUTOFF the image and extract list of points
        int[] origI = {photoDiff.height,photoDiff.width};
        var pixels = new Color[photoDiff.width * photoDiff.height];
        pixels = photoDiff.GetPixels();

        while(numPoints <1000 && thresHold>0.4f ){
            thresHold = thresHold - 0.01f;
            (float aMax, int ai, int aj)=MaxPointInPhoto( photoDiff);
            numPoints=0;
            for (int i=0;i<origI[0];i++){
                for (int j=0;j<origI[1];j++){

                    int idx = i*origI[1]+j;
                    float ampl = (pixels[idx].r + pixels[idx].g + pixels[idx].g); 
                    if (ampl > thresHold*aMax){
                        pixels[idx] =cWhite;
                        points[numPoints,0] = i;
                        points[numPoints,1] = j;
                        numPoints++;
                    }
                    else {
                        pixels[idx] =cBlack;
                    }

                }

            }//for (int i=0;i<origI[0];i++)

        }//while(numPoints <2000 && thresHold>0.4f )

        Debug.Log("Number of points: " + numPoints + ",  thresHold: " + thresHold);

        // (float aIni, float bIni, float eR)=CentroidSimpleLinearRegression(points,   numPoints);
        // Debug.Log("1st simple LR, aIni: " + aIni + ",  bIni: " + bIni + ",  eR: " + eR);


    // Hough transform (a version) in a and b space
    // https://www.section.io/engineering-education/computer-vision-straight-lines/
        // int NsampleTheta =170*2;
        // int NsampleRho =500;
        // //float rangePercent = 0.4f;
        // float[] rangeTheta = new float[] {3.0f*3.14159f/180f, 177.0f*3.14159f/180f};
        // float[] rangeRho = new float[] {-1200f, 1200f};
        // int maxNumPoints =0;

        // for(int a1 =0; a1<NsampleTheta; a1++){
        //     float th = rangeTheta[0] + (a1)*( rangeTheta[1]-rangeTheta[0] )/NsampleTheta;
        //     for(int b1 =0; b1<NsampleRho; b1++){
        //         float rh = rangeRho[0] + (b1)*( rangeRho[1]-rangeRho[0] )/NsampleRho;
        //         (float a, float b)=ThetaRhoToAB( th,  rh);
        //         int numP= PointsNearLine( a,  b, points,   numPoints, pointsFiltered);
        //         if(maxNumPoints<numP){
        //             maxNumPoints=numP;
        //             aIni = a;
        //             bIni = b;
        //         }
        //     }
        // }

(float rho, float theta, int maxAccu) = HoughTransform(photoDiff.height, photoDiff.width, points, numPoints);
Debug.Log("After Hough, rho: " + rho + ",  theta: " + theta+ ",  maxAccu: " + maxAccu );

(aIni, bIni) = ThetaRhoToAB( theta,  rho); 

Debug.Log("After grid search, aIni: " + aIni + ",  bIni: " + bIni );

 // output image for control       
        photoDiff.SetPixels(pixels);
        photoDiff.Apply();

        // int numP1= PointsNearLine( aIni,  bIni, points,   numPoints, pointsFiltered);
        // ( aIni,  bIni,  eR)=CentroidSimpleLinearRegression(pointsFiltered,   numPoints);
        // Debug.Log("2nd simple LR, aIni: " + aIni + ",  bIni: " + bIni + ",  eR: " + eR+ ",  numP: " + numP1);


        DrawLineInPhoto(aIni, bIni,  photoDiff,  photoDiff);
        string image_path = path + $"/{ fileNamePrefix + cont}_diff.png";
        byte[] bytes = photoDiff.EncodeToPNG();
        File.WriteAllBytes(image_path, bytes);

        return (aOut: aIni, bOut: bIni);


    }//private void LinearRegression



    private (float aMax, int ai, int aj) MaxPointInPhoto(Texture2D photoDiff){
        // float aMax = 0.9f;
        // int ai = 1;
        // int aj = 1;


        int rowIndex = -1;
        int colIndex = -1;
        int maxIndex = -1;
        float maxInt = -2f; // can be less than -1f, eg, R - G => 0 - 1 = -1f
        var pixels = new Color[photoDiff.width * photoDiff.height];

        pixels = photoDiff.GetPixels();

        //pixels = tex.GetPixels();
        snapSizes[0] = photoDiff.width;
        snapSizes[1] = photoDiff.height;
        snapSizes[2] = photoDiff.width * photoDiff.height;

        // if (snapSizes[2] > snapRmG.Length){
        //     Debug.Log("!!!in TakePhoto, tex.width * tex.height: " + snapSizes[2]);
        //     Debug.Log("!!!in TakePhoto, snapRmG.Length: " + snapRmG.Length);
        // }

        for (int i = 0; i < pixels.Length; i++) // get RmG array and find Max and Index
        {
            colIndex = i;
            colIndex %= (int) snapSizes[0]; // modulo of width gives position within row
            rowIndex = (int) i/snapSizes[0]; // how many full rows

            float snapRmG1 = (pixels[i].r + pixels[i].g + pixels[i].g)/3f;  // grey scale
            if(snapRmG1<0f) snapRmG1 = 0f; // reset to zero if negative

            if (snapRmG1 > maxInt)
            {
                maxInt = snapRmG1;
                maxIndex = i;
            }

        }
        
        
        // print($"width = {snapSizes[0]}, hight {snapSizes[1]}.");
        // print($"Maximum Intensity = {maxInt}, on index {maxIndex}.");

        colIndex = maxIndex;
        colIndex %= (int) snapSizes[0]; // modulo of width gives position within row
        rowIndex = (int) maxIndex/snapSizes[0]; // how many full rows


        return (aMax: maxInt,  ai: rowIndex,  aj: colIndex);

    } //private (float aMax, int ai, int aj) MaxPointInPhoto(Texture2D photoDiff)


    private void DrawLineInPhoto(float a, float b, Texture2D photoDiff, Texture2D photoRes){
       
        var pixels = new Color[photoDiff.width * photoDiff.height];
        Color cRed = new Color(1f,0f,0f, 1.0f);

        pixels = photoDiff.GetPixels();

        for (int j = 0; j < photoDiff.width; j++) 
        {    
            int i = Mathf.FloorToInt(a +b*j);
            int idx = i*photoDiff.width+j;
            if(idx < photoDiff.width * photoDiff.height) {
                pixels[idx] = cRed;
            }
        }
                
        photoRes.SetPixels(pixels);
        photoRes.Apply();

    } //private void DrawLineInPhoto


    private (float aIni, float bIni, float eR) CentroidSimpleLinearRegression(int[,] points, int  numPoints){
        int cX = 0;
        int cY = 0;


        for (int i = 0; i < numPoints; i++){
            cY = cY+points[i,0];
            cX = cX+points[i,1];
        }
        cY = Mathf.FloorToInt( ((float) cY)/( (float) numPoints)  ); 
        cX = Mathf.FloorToInt( ((float) cX)/( (float) numPoints)  ); 

        //Simple linear regression
        float s1=0f;
        float s2=0f;
        for (int i = 0; i < numPoints; i++){
            if(points[i,1]>0){
                    s1 = s1+(points[i,1]-cX)*(points[i,0]-cY);
                    s2 = s2+(points[i,1]-cX)*(points[i,1]-cX);
            }
        }

        float b = s1/s2;
        float a = cY-b*cX;
        // y = b*x+a

        //Errors
        
        s1 = 0f;
        int cnt=0;
        for (int i = 0; i < numPoints; i++){
            if(points[i,1]>0){
                s1 = s1 + DistanceToPoint(a,b,points[i,1],points[i,0]);
                cnt++;
            }
        }
        float eRout = s1/cnt;
        return (aIni: a,  bIni: b,  eR: eRout);
    }


    private int PointsNearLine(float a, float b, int[,] points, int  numPoints, int[,] pointsFiltered){
        
        float nearLine =2f;
        int cnt=0;
        for (int i = 0; i < numPoints; i++){
            pointsFiltered[i,0]=points[i,0];
            pointsFiltered[i,1]=points[i,1];
            if(points[i,1]>0){
                float s1 =  DistanceToPoint(a,b,points[i,1],points[i,0]);
                if(s1 < nearLine){
                    cnt++;
                }
                else{
                    pointsFiltered[i,0]=-1;
                    pointsFiltered[i,1]=-1;
                }              
            }//if(points[i,1]>0)
        } //for (int i = 0; i < numPoints; i++)
        

        return cnt;
    }


    private float DistanceToPoint(float aa, float bb, int x0, int y0){
        float dist=9999f;
        if(y0 > 0){  // if negative points are rejected
            dist = (y0-bb*x0-aa)/Mathf.Sqrt((bb*bb)+1f);
            dist = Mathf.Abs(dist);
        }
         return (dist);
    }


    private (float a, float b) ThetaRhoToAB(float Theta, float Rho){
        return (a: -Rho/Mathf.Sin(Theta), b: Mathf.Cos(Theta)/Mathf.Sin(Theta));
    }

    private (float rho, float theta, int maxAccu) HoughTransform(int h, int w, int[,] points, int  numPoints){
//https://towardsdatascience.com/lines-detection-with-hough-transform-84020b3b1549
        int num_rhos =  w*2;
        int num_thetas = 180*4;
        float[] range_thetas  = new float[2] {0.00f, 3.14159f}; // 0, 180 grad
        float[] range_rhos = new float[2] {-w*1.147f, w*1.147f}; //  diagonal length
        float rho = 0f;
        float theta = 0f;
        int idxRho = 0;
        

        int[,] accumulator = new int[num_rhos,num_thetas]; // rho and theta, init with zeros

        for (int k=0; k< numPoints; k++){
            for (int i=0; i< num_thetas; i++){
                theta = range_thetas[0] + i*( range_thetas[1]-range_thetas[0] )/num_thetas;
                rho = points[k,1]*Mathf.Cos(theta) - points[k,0]*Mathf.Sin(theta);
                idxRho = Mathf.FloorToInt((rho-range_rhos[0])/(range_rhos[1]-range_rhos[0])*num_rhos);
                // add to accumulator
                if(0 <= idxRho && idxRho <num_rhos){
                    accumulator[idxRho,i]++;
                }
            }
        }
            // find local maximum in accumulator
            int maxLocal = 0;
            int idxTheta = 0;
            idxRho = 0;
            for (int i=0; i< num_thetas; i++){
                for (int j=0; j< num_rhos; j++){
                    if(maxLocal<= accumulator[j,i]){
                        maxLocal = accumulator[j,i];
                        idxTheta = i;
                        idxRho = j;
                    }
                }
            }

        return(rho: range_rhos[0] + idxRho*( range_rhos[1]-range_rhos[0] )/num_rhos ,  theta: range_thetas[0] + idxTheta*( range_thetas[1]-range_thetas[0] )/num_thetas, maxAccu: maxLocal);
    }//private (float rho, float theta) HoughTransform


    // public void SaveCalibration(){ //https://www.youtube.com/watch?v=XOjd_qU2Ido
    //     SaveSystem.SaveCalibration(this);
    // }

    // public void LoadCalibration(){
    //     CalibrationData data =  SaveSystem.LoadCalibration();
    //     calibTable = data.calibrationTable;
    // }


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

    private void Myfitgeotrans(float[,] T, float[,] movingPoints1, float[,] fixedPoints1){
        
        //float[,] x = new float[movingPoints1.GetUpperBound(0)+1,movingPoints1.GetUpperBound(1)+1];
        float[,] mpMat = new float[3+1,3+1]; 
        float[,] resMat = new float[3+1,3+1];
        float[,] A = new float[3+1,3+1]; 
        float[,] B = new float[3+1,3+1]; 
        float[] vecX = new float[3+1];
        float[] vecL = new float[3+1];

        //movingPoints1.CopyTo(x,0);
        float[,] x = movingPoints1.Clone() as float[,];
// Debug.Log("movingPoints1 rank: " +movingPoints1.Rank + " dim 2: "+movingPoints1.GetUpperBound(1));
// Debug.Log("x rank: " +x.Rank + " dim 0: "+x.GetUpperBound(0)+ " dim 1: "+x.GetUpperBound(1));
// Debug.Log("movingPoints1[1,2] " + movingPoints1[1,2]);
// Debug.Log("x[1,2] " + x[1,2]);

        mpMat[1,1] = x[1,1]; mpMat[1,2] = x[2,1]; mpMat[1,3] = x[3,1];
        mpMat[2,1] = x[1,2]; mpMat[2,2] = x[2,2]; mpMat[2,3] = x[3,2];
        mpMat[3,1] = 1.0f;   mpMat[3,2] = 1.0f;   mpMat[3,3] = 1.0f;

//Debug.Log("mpMat"); PrintMat3x3(mpMat);

        vecX[1] = x[4,1]; vecX[2] = x[4,2]; vecX[3] = 1.0f;

        //vecL = MatDotVec(  Adjoint3x3(mpMat)/Det3x3(mpMat)  ,vecX);
        float d = 1f/Det3x3(mpMat);
//Debug.Log("1/det3 = "+d);
        Adjoint3x3(resMat,mpMat);
        MatDotScal(mpMat, resMat, d);
//Debug.Log("mpMat"); PrintMat3x3(mpMat);


        MatDotVec(vecL, mpMat, vecX);

        A[1,1] = vecL[1]*x[1,1]; A[1,2] = vecL[2]*x[2,1]; A[1,3] = vecL[3]*x[3,1];
        A[2,1] = vecL[1]*x[1,2]; A[2,2] = vecL[2]*x[2,2]; A[2,3] = vecL[3]*x[3,2];
        A[3,1] = vecL[1]*1.0f;    A[3,2] = vecL[2]*1.0f;    A[3,3] = vecL[3]*1.0f;

//Debug.Log("A"); PrintMat3x3(A);

        x = fixedPoints1.Clone() as float[,];

        mpMat[1,1] = x[1,1]; mpMat[1,2] = x[2,1]; mpMat[1,3] = x[3,1];
        mpMat[2,1] = x[1,2]; mpMat[2,2] = x[2,2]; mpMat[2,3] = x[3,2];
        mpMat[3,1] = 1.0f; mpMat[3,2] = 1.0f; mpMat[3,3] = 1.0f;

        vecX[1] = x[4,1]; vecX[2] = x[4,2]; vecX[3] = 1.0f;

        d = 1/Det3x3(mpMat);
        Adjoint3x3(resMat,mpMat);
        MatDotScal(mpMat, resMat, d);
        MatDotVec(vecL, mpMat, vecX);

        B[1,1] = vecL[1]*x[1,1]; B[1,2] = vecL[2]*x[2,1]; B[1,3] = vecL[3]*x[3,1];
        B[2,1] = vecL[1]*x[1,2]; B[2,2] = vecL[2]*x[2,2]; B[2,3] = vecL[3]*x[3,2];
        B[3,1] = vecL[1]*1.0f;    B[3,2] = vecL[2]*1.0f;    B[3,3] = vecL[3]*1.0f;

        //T = MatDotMat( B, Adjoint3x3(A)/Det3x3(A)  );
        d=1/Det3x3(A);
        Adjoint3x3(resMat,A);
        MatDotScal(A, resMat, d);
        MatDotMat(T,B,A);

    }//private float[,] T Myfitgeotrans
    private float Det3x3(float[,] A){
        float a=A[1,1]; float b=A[1,2]; float c=A[1,3];
        float d=A[2,1]; float e=A[2,2]; float f=A[2,3];
        float g=A[3,1]; float h=A[3,2]; float i=A[3,3];
        return a*(e*i-f*h) - b*(d*i-f*g) +c*(d*h-e*g);
    }//private float Det3x3
    private float Det2x2(float[,] A){
        return A[1-1,1-1]*A[2-1,2-1]-A[1-1,2-1]*A[2-1,1-1];
    }//private float Det3x3  
    private void Adjoint3x3(float[,] C, float[,] A){  
        

        float[,] a1 = { {A[2,2],A[2,3]}, {A[3,2],A[3,3]} }; 
        float[,] a2 = { {A[1,2],A[1,3]}, {A[3,2],A[3,3]} }; 
        float[,] a3 = { {A[1,2],A[1,3]}, {A[2,2],A[2,3]} };
        float[,] a4 = { {A[2,1],A[2,3]}, {A[3,1],A[3,3]} }; 
        float[,] a5 = { {A[1,1],A[1,3]}, {A[3,1],A[3,3]} }; 
        float[,] a6 = { {A[1,1],A[1,3]}, {A[2,1],A[2,3]} };
        float[,] a7 = { {A[2,1],A[2,2]}, {A[3,1],A[3,2]} }; 
        float[,] a8 = { {A[1,1],A[1,2]}, {A[3,1],A[3,2]} }; 
        float[,] a9 = { {A[1,1],A[1,2]}, {A[2,1],A[2,2]} };

        C[1,1]=Det2x2(a1); 
        C[1,2]=-1f*Det2x2(a2); 
        C[1,3]=Det2x2(a3);
        C[2,1]=-1f*Det2x2(a4); 
        C[2,2]=Det2x2(a5); 
        C[2,3]=-1f*Det2x2(a6);
        C[3,1]=Det2x2(a7); 
        C[3,2]=-1f*Det2x2(a8); 
        C[3,3]=Det2x2(a9);

    }//private void Adjoint3x3   
    private void MatDotVec(float[] u, float[,] A, float[] v){
        u[1]= A[1,1]*v[1]+A[1,2]*v[2]+A[1,3]*v[3];
        u[2]= A[2,1]*v[1]+A[2,2]*v[2]+A[2,3]*v[3];
        u[3]= A[3,1]*v[1]+A[3,2]*v[2]+A[3,3]*v[3];
    }//private void MatDotVec(f
    private void MatDotMat(float[,] C, float[,] A, float[,] B){
        for(int i=1;i<=3;i++){
            for(int j=1;j<=3;j++){
                C[i,j] = A[i,1]*B[1,j]+A[i,2]*B[2,j]+A[i,3]*B[3,j];
            }
        }

    }
    private void MatDotScal(float[,] C, float[,] A, float fc){
        for(int i=1;i<=3;i++){
            for(int j=1;j<=3;j++){
                C[i,j] = A[i,j]*fc;
            }
        }

    }
    private (float u, float v) DotProjMat(float[,] trMat, float[] xy){ // this one to use with fitgeotrans of matlab
        float A=trMat[1,1]; float D=trMat[1,2]; float G=trMat[1,3]; 
        float B=trMat[2,1]; float E=trMat[2,2]; float H=trMat[2,3];  
        float C=trMat[3,1]; float F=trMat[3,2]; float I=trMat[3,3]; 

        float x = xy[1];
        float y = xy[2];

        return(u: (A*x+B*y+C)/(G*x + H*y +I),   v: (D*x+ E*y +F)/(G*x + H*y +I));

    }
    private (float u, float v) myCamera2World(float[,] trMat, float[] xy){
        float[] xy3 = {0f,xy[1], xy[2], 1.0f};
        float[] xyp = {0f,0f,0f,0f};
        MatDotVec(xyp, trMat, xy3);
        return(u: xyp[1]/xyp[3], v: xyp[2]/xyp[3]);
    }

    private void PrintMat3x3(float[,] A){
        for(int i=1;i<=3;i++){
            Debug.Log("  " + A[i,1]+"  " + A[i,2]+"  " + A[i,3]);
        }
    }

    private void PrintMat4x2(float[,] A){
        for(int i=1;i<=4;i++){
            Debug.Log("  " + A[i,1]+"  " + A[i,2]);
        }
    }

    private void SaveMatrix(float[,] A, string basename){
        int iN = A.GetUpperBound(0)+1;
        int jN = A.GetUpperBound(1)+1;
        string element_name = $"{ basename + iN+jN*100}";
        for(int i=0;i<iN;i++){
            for(int j=0;j<jN;j++){
                element_name = $"{ basename + i+j*100}";
                //Debug.Log("element_name: "+element_name);
                PlayerPrefs.SetFloat(element_name,A[i,j]);
            }
        }
        
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



}//class
