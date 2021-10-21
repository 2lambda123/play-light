using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class WebCam : MonoBehaviour
{
    
    int currentCamIndex = 1; // 0 - built in camera, 1 - usb camera

    WebCamTexture tex;

    public RawImage display;

    public Text stopStartText;
    private double tic;
    private double toc;
    private double tictoc;
    public Text tictocText;

    public float[] snapRmG = new float[10000000];

    public int[] snapSizes = new int[3]; // width, height, tex.width * tex.height

    [SerializeField]
    private GameObject puckReference;

    private GameObject spawnedPuck;

    public float imagPosHor = 2f;
    public float imagPosVer = 1f;


    public void SwapCam_Clicked(){

        Debug.Log("SwapCam -- button pressed");

        if( WebCamTexture.devices.Length > 0 )
        {
        currentCamIndex +=1;
        currentCamIndex %= WebCamTexture.devices.Length;  //  Inx not to go beyound Length

        Debug.Log("currentCamIndex = " + currentCamIndex);

        if(tex != null)
        {
            StopCamera();
            StartStopCam_Clicked();
        }

        }
    }

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
        tictocText.text = device.name; // output as text above video
        tex = new WebCamTexture(device.name);

        tex.requestedFPS=60;
        tex.requestedWidth=1024;
        tex.requestedHeight=576;

        display.texture = tex;
        tex.requestedFPS=60;
        // tex.requestedWidth=1024; // 80 ms
        // tex.requestedHeight=576;
        // tex.requestedWidth=320; // 7 ms
        // tex.requestedHeight=240;
        tex.requestedWidth=864; // 5 ms, SOLUTION!
        tex.requestedHeight=480;
        tex.Play();
        stopStartText.text = "Stop Cam";
        }
    }

    private void StopCamera()
    {
        display.texture = null;
        tex.Stop();
        tex = null;
        stopStartText.text = "Start Cam";
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
    //Debug.Log("in TakePhoto: yield return");
     yield return null; //new WaitForEndOfFrame(); 

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
        var pixels = new Color[tex.width * tex.height];

        pixels = tex.GetPixels();
        snapSizes[0] = tex.width;
        snapSizes[1] = tex.height;
        snapSizes[2] = tex.width * tex.height;

        // if (snapSizes[2] > snapRmG.Length){
        //     Debug.Log("!!!in TakePhoto, tex.width * tex.height: " + snapSizes[2]);
        //     Debug.Log("!!!in TakePhoto, snapRmG.Length: " + snapRmG.Length);
        // }

        for (int i = 0; i < pixels.Length; i++) // get RmG array and find Max and Index
        {
            float snapRmG1 = pixels[i].r - pixels[i].g;  // substract Green from Red band
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
        // print($"rowIndex = {rowIndex}, colIndex {colIndex}.");

        //Debug.Log("in TakePhoto, pixels[10].r: " + pixels[10].r);
        //Debug.Log("in TakePhoto, snapRmG[10]: " + snapRmG[10].ToString());

        imagPosHor = colIndex/864f*10.0f;
        imagPosVer = rowIndex/480f*5.0f;

        //print($"imagPosHor = {imagPosHor}, imagPosVer {imagPosVer}.");

        //byte[] bytes = photo.EncodeToPNG();
        //File.WriteAllBytes("C:/Users/kpervushin/Downloads/photo.png", bytes);
    }

    public void ReturnHome(){
        SceneManager.LoadScene("MainMenu");
    }



void Start(){

   spawnedPuck = Instantiate(puckReference);
   spawnedPuck.transform.position = new Vector3(imagPosHor,imagPosVer,1f);

}

 public void FixedUpdate(){

     if(tex != null) StartCoroutine(TakePhoto()); 
     spawnedPuck.transform.position = new Vector3(5f-imagPosHor,imagPosVer,1f);

 }



}//class
