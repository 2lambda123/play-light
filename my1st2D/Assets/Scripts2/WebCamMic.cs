using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using System.IO;

public class WebCamMic : MonoBehaviour
{
    int currentCamIndex = 0; // 0 - built in camera, 1 - usb camera

    WebCamTexture tex;

    public RawImage display;
    public int[] snapSizes = new int[3]; // width, height, tex.width * tex.height

    public Text stopStartText;

    public Text stopStartMicText;
    public Text cameraText;
    public Text microphoneText;
    public Text takeAudioText;
    public Text switchMicrophoneText;
    public Text filenameText;

    private string filename;
    private string reportFilename;

    private string fileNamePrefix = "meandr_";
    string path; // = Application.dataPath; //="C:/Users/kpervushin/Downloads"; //="/storage/emulated/0/Download/";
    
#region MICROPHONE
    private AudioSource audioSource;
    private string _deviceMic;
    private int currentMicIndex = 0; // 0 - built in camera, 1 - usb camera

    AudioClip _clipRecord;

    private float heightMultiplier = -1.0f;
    private static int numberOfSamples = 1024;
    public FFTWindow fftWindow;
    private float[] spectrum = new float[numberOfSamples];
    public GameObject[] circles;

    public GameObject[] sliders;
    private Slider myslider;

    private Vector3 origin = new Vector3(0f, -3.0f, 0f);
    private float radius =  0.3f; 
    private float speed = 0.25f;
    private float rotationDirection =0f;
    private float pos = 0f;
    private string foodInput;

    private int micStatus = 0; 
    // 0, do nothing, 1, mic activated for checking
    // 2, clip recording started
    // 3, clip recording ended
    // 4, clip playback started
    // 5, clip playback ended, reurn to 0

    // private float progressSquare =0.3f;

// Audio Mixer controls
    [SerializeField] string _volumeParameter = "MasterVolume"; 
    [SerializeField] AudioMixer _mixer; // set to -MAsterVolume at -80 at .play to remove echo, set to 0 at playback

#endregion

#region PSY
    public float psyTense = 0f; // 0, +4
    public float psyWorried = 0f; // 0, +4
    public float psyUpset = 0f; // 0, +4
    public float psyRelaxed = 0f; // 0, +4
    public float psyCalm = 0f; // 0, +4
    public float psyContent = 0f; // 0, +4
    public float psyDepressed = 0f; // 0, +4
    public float psyExcited = 0f; // 0, +4
#endregion

#region GPSLocation

public Text GPSStatusValue;
public Text latitudeValue;
public Text longitudeValue;
public Text altitudeValue;
public Text horizontalAccuracyValue;

public Text timeStampValue;


private string GPSStatusStr;
private string latitudeValueStr;
private string longitudeValueStr;
private string altitudeValueStr;
private string horizontalAccuracyValueStr;

private string timeStampValueStr;




public GameObject gpsIndicator;

#endregion
  
    private void StopMicrophone()
    {
        if(audioSource != null){

            if(Microphone.IsRecording(_deviceMic) == true) {Microphone.End(_deviceMic);}
            audioSource.Stop(); 
            audioSource = null;
            stopStartMicText.text = "Start Mic";
        }
        
    }

    public void StartStopMic_Clicked(){



        if(micStatus==0){  
            // micStatus=1;
            if(audioSource != null)
            {
                StopMicrophone();

            }
            else
            {
                audioSource = GetComponent<AudioSource>();
                _deviceMic = Microphone.devices[currentMicIndex];
// Debug.Log("Mic Device name: " + _deviceMic.ToString());
                microphoneText.text = _deviceMic.ToString();
                audioSource.clip = Microphone.Start(_deviceMic, false, 30, 44100); // loop and 999 sec rec
        // Mute the sound with an Audio Mixer group bc we don't want the player to hear it
// https://www.youtube.com/watch?v=CdNBsWowRbE  7:32 (isRecording check )
                if(Microphone.IsRecording(_deviceMic)){
                    while(!(Microphone.GetPosition(_deviceMic)>0)){} //Wait untill recording is started
                } else {
            // microphone does nt work for some reason
                    Debug.Log(_deviceMic.ToString() + " does not work!");
                    microphoneText.text = _deviceMic.ToString()+ " doesn't not Start!";
                }


            float volumeOut = -80f;
            _mixer.SetFloat(_volumeParameter, volumeOut);

            audioSource.Play();

            stopStartMicText.text = "Stop Mic";
            } // else
        }//if(micStatus==0){


        if((micStatus==3)||(micStatus==5)){ // Recoding finished, or PLay finished

            micStatus=6;
            stopStartMicText.text = "Start/Stop Mic";
            switchMicrophoneText.text = "Switch Microphone";
            

            reportFilename = fileNamePrefix+ System.DateTime.Now.ToString("yy-MM-dd_HH-mm");
            filename =path +"/"+reportFilename; 
            filenameText.text = reportFilename;

            SavWav.Save(filename, audioSource.clip);
            filenameText.text = "Audio taken: "+reportFilename+".wav";
// Debug.Log("Audio taken: "+reportFilename+".wav");

            takeAudioText.text = "Take Audio";
            takeAudioText.color = Color.yellow;
            circles[8].transform.localScale = new Vector3(0.3f, 0.3f, 1f);


            if(File.Exists(path+"/log_audio_v1"+".txt") == false){ // print header
                string headr = "Datetime, latitude, longitude, altitude, horizontalAccuracy, GPSStatus, timeStamp, ";
                headr += "Audio File";
                SavWav.WriteString(path+"/log_audio_v1",headr);
            }
        // location to csv file
            string outS ="";
            outS = AddLocationToCSVstring(outS); // Add location data to log csv string
            outS +=  reportFilename + ".wav";
            SavWav.WriteString(path+"/log_audio_v1",outS);

            StopMicrophone();
            ResetAudioButtons();


        }




    }




    public void SwapMic_Clicked(){

        if(micStatus==3){
            ResetAudioButtons();
        } else {


            if( Microphone.devices.Length > 0 )
                {
                    currentMicIndex +=1;
                    currentMicIndex %= Microphone.devices.Length;  //  Inx not to go beyound Length

// Debug.Log("currentMicIndex = " + currentMicIndex);

            if(audioSource != null)
                {
                    StopMicrophone();
                    StartStopMic_Clicked();
                }

        }
        PlayerPrefs.SetInt("currentMicIndex",currentMicIndex); 
        }

    }

    public void SwapCam_Clicked(){

//        Debug.Log("SwapCam -- button pressed");

        if( WebCamTexture.devices.Length > 0 )
        {
            currentCamIndex +=1;
            currentCamIndex %= WebCamTexture.devices.Length;  //  Inx not to go beyound Length

// Debug.Log("currentCamIndex = " + currentCamIndex);

            if(tex != null)
            {
                StopCamera();
                StartStopCam_Clicked();
            }

        }

// Debug.Log("onDisable: currentCamIndex: "+currentCamIndex);
        PlayerPrefs.SetInt("currentCamIndex",currentCamIndex);
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
        //tictocText.text = device.name; // output as text above video
        cameraText.text = device.name;
        tex = new WebCamTexture(device.name);

        // tex.requestedFPS=60;
        // tex.requestedWidth=1024;
        // tex.requestedHeight=576;

        display.texture = tex;
        // tex.requestedFPS=60;
        // tex.requestedWidth=864; // 5 ms, SOLUTION!
        // tex.requestedHeight=480;
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
    // Debug.Log("In Snap_Clicked, Time.realtimeSinceStartup: " );

        if(tex != null) StartCoroutine(TakePhoto()); 

        //toc = Time.realtimeSinceStartup;
        //tictoc = toc - tic;
        //Debug.Log("In Snap_Clicked, tictoc: " + tictoc);

        //string s = string.Format("Snap time {0} s", tictoc);
        //tictocText.text = s.ToString();

    }

    public void TakeAudio_Clicked(){
        StartCoroutine(TakeAudio()); 
    }
    
    IEnumerator TakePhoto()  // Start this Coroutine on some button click
    {

    // NOTE - you almost certainly have to do this here:
    //Debug.Log("in TakePhoto: yield return");
     yield return new WaitForEndOfFrame(); 

    // it's a rare case where the Unity doco is pretty clear,
    // http://docs.unity3d.com/ScriptReference/WaitForEndOfFrame.html
    // be sure to scroll down to the SECOND long example on that doco page 

        Texture2D photo = new Texture2D(tex.width, tex.height);
        photo.SetPixels(tex.GetPixels());
        photo.Apply();

       
        // var pixels = new Color[tex.width * tex.height];

        // pixels = tex.GetPixels();

        reportFilename = fileNamePrefix+ System.DateTime.Now.ToString("yy-MM-dd_HH-mm");
        filename =path +"/"+reportFilename; 
        filenameText.text = reportFilename;
// Debug.Log("filename: " +filename);
        byte[] bytes = photo.EncodeToPNG();
        File.WriteAllBytes(filename+".png", bytes);
        filenameText.text = "Photo taken: "+reportFilename + ".png";
// Debug.Log("filenameText.text: " +"Photo taken: "+reportFilename + ".png");

            if(File.Exists(path+"/log_images_v1"+".txt") == false){ // print header
                string headr = "Datetime, latitude, longitude, altitude, horizontalAccuracy, GPSStatus, timeStamp, ";
                headr += "Food Image";
                SavWav.WriteString(path+"/log_images_v1",headr);
            }

        // location to csv file
        string outS ="";
        outS = AddLocationToCSVstring(outS); // Add location data to log csv string
        outS +=  reportFilename + ".png";
//Debug.Log(outS);
        SavWav.WriteString(path+"/log_images_v1",outS);
//Debug.Log(path+"/imges_0log.csv");


    }


    IEnumerator TakeAudio(){

            if(micStatus ==4 ){ // record ready to play
            takeAudioText.text = "Take Audio";
            takeAudioText.color = Color.blue;
            micStatus=5; //  clip is ready to save or discard
            audioSource.Stop(); 
            yield return new WaitForSeconds(1); // 3 more seconds of recording
            stopStartMicText.text = "Save Rec";


        } 

         if(micStatus ==3 ){ // record ready to play
            _mixer.SetFloat(_volumeParameter, 0f);
            takeAudioText.text = "Stop Play";
            takeAudioText.color = Color.yellow;
            micStatus=4; //  playing is going, ready to stop

            yield return new WaitForSeconds(1); // 3 more seconds of recording
// Debug.Log("Playing clip");
            audioSource.Play(); 
        } 

        if(micStatus ==2 ){ // recording is in progress
            takeAudioText.text = "3 more sec..";
            takeAudioText.color = Color.yellow;
            micStatus=3; //  recording stopped, ready to play

            yield return new WaitForSeconds(3); // 3 more seconds of recording
            takeAudioText.text = "Play Rec";
            if(Microphone.IsRecording(_deviceMic) == true) {Microphone.End(_deviceMic);}
            audioSource.Stop(); 
            stopStartMicText.text = "Save Rec";
            switchMicrophoneText.text = "Reset";


        } 
        
        if(micStatus ==0 ){ // 0, idling, ready to start rec
        // if(audioSource != null)
        // {
            StopMicrophone();
            audioSource = GetComponent<AudioSource>();
            _deviceMic = Microphone.devices[currentMicIndex];
// Debug.Log("Take Audio: Mic Device name: " + _deviceMic.ToString());
            microphoneText.text = _deviceMic.ToString();
            audioSource.clip = Microphone.Start(_deviceMic, false, 30, 44100); // no-loop and 30 sec rec
            if(Microphone.IsRecording(_deviceMic)){
                while(!(Microphone.GetPosition(_deviceMic)>0)){} //Wait untill recording is started
            } else {
            // microphone does nt work for some reason
                Debug.Log(_deviceMic.ToString() + " does not work!");
                microphoneText.text = _deviceMic.ToString()+ " doesn't not Start!";
            }


            float volumeOut = -80f;
            _mixer.SetFloat(_volumeParameter, volumeOut);

            audioSource.Play();
            if(Microphone.IsRecording(_deviceMic) == true){
                takeAudioText.text = "Stop Rec";
                takeAudioText.color = Color.green;
                micStatus=2; // Start recording
                yield return new WaitForSeconds(1);
            }

        }//if(micStatus ==2 ){

        // reportFilename = fileNamePrefix+ System.DateTime.Now.ToString("yy-MM-dd-hh-mm-tt");
        // filename =path +"/tmp/"+reportFilename; 
        // filenameText.text = reportFilename;

        // SavWav.Save(filename, audioSource.clip);
        // filenameText.text = "Audio taken: "+reportFilename+".wav";
        // takeAudioText.text = "Take Audio";
        // takeAudioText.color = Color.yellow;

        // string outS = "Stressed-Relaxed, " + stressedRelaxed.ToString("##.#")+",   ";
        // outS += "Exhaused-Energized, " + exhausedEnergized.ToString("##.#")+",   ";
        // outS += "Depressed-Excited, " + excitedDepressed.ToString("##.#")+",   ";
        // SavWav.WriteString(filename,outS);



//         // audioSource.Play();



//         stopStartMicText.text = "Stop Mic";

       // }//if(audio)

    }//TakeAudio


private void ResetAudioButtons(){
    
    StopMicrophone();
    takeAudioText.text = "Take Audio";
    takeAudioText.color = Color.green;
    stopStartMicText.text = "Start/Stop Mic";
    switchMicrophoneText.text = "Switch Microphone";
    circles[8].transform.localScale = new Vector3(0.3f, 0.3f, 1f);
    micStatus=0;
    // filenameText.text = path;

}



    public void ReturnHome(){
        SceneManager.LoadScene("MainMenu");
    }

public void Tense(float newpsyTense){
    psyTense = newpsyTense;
// Debug.Log("in Tense(),  psyTense: " +psyTense);
}
public void Worried(float npsyWorried){
    psyWorried = npsyWorried;
}
public void Upset(float npsyUpset){
    psyUpset = npsyUpset;
}
public void Relaxed(float npsyRelaxed){
    psyRelaxed = npsyRelaxed;
}
public void Calm(float npsyCalm){
    psyCalm = npsyCalm;
}
public void Content(float npsyContent){
    psyContent = npsyContent;
}
public void Excited(float npsyExcited){
    psyExcited = npsyExcited;
}
public void Depressed(float npsyDepressed){
    psyDepressed = npsyDepressed;
}

IEnumerator GPSLoc(){
    if(!Input.location.isEnabledByUser){
        GPSStatusStr = "isEnabledByUser: False";
        GPSStatusValue.text = GPSStatusStr; 
        gpsIndicator.GetComponent<Renderer>().material.color = Color.red;
        yield break;
    }

    Input.location.Start(1, 0.1f);

    int maxWait = 20;
    while(Input.location.status == LocationServiceStatus.Initializing && maxWait > 0){
        yield return new WaitForSeconds(1);
        maxWait--;
    }

    if(maxWait<1){
        GPSStatusStr = "Location: Time out";
        GPSStatusValue.text = GPSStatusStr; 
        gpsIndicator.GetComponent<Renderer>().material.color = Color.magenta;
        yield break;
    }

    // connection failed 
    if(Input.location.status == LocationServiceStatus.Failed){
        GPSStatusStr = "Unable to determine device location";
        GPSStatusValue.text = GPSStatusStr; 
        gpsIndicator.GetComponent<Renderer>().material.color = Color.yellow; 
        yield break;
    }else{
        //Access granted
        GPSStatusStr = "Location running";
        GPSStatusValue.text = GPSStatusStr; 
        gpsIndicator.GetComponent<Renderer>().material.color = Color.green; 
        InvokeRepeating("UpdateGPSData", 0.5f, 1f);
        yield break;
    }

}//IEnumerator GPSLoc(){


private void UpdateGPSData(){
    if(Input.location.status == LocationServiceStatus.Running){
// Access granted to GPS values and it has been init
        GPSStatusStr= "Running";
        GPSStatusValue.text = GPSStatusStr; 
        gpsIndicator.GetComponent<Renderer>().material.color = Color.green;


// filenameText.text =  "Input.location.lastData.latitude.ToString()";
        latitudeValueStr = Input.location.lastData.latitude.ToString();
        longitudeValueStr = Input.location.lastData.longitude.ToString();
        altitudeValueStr = Input.location.lastData.altitude.ToString();
        horizontalAccuracyValueStr = Input.location.lastData.horizontalAccuracy.ToString();
        timeStampValueStr = Input.location.lastData.timestamp.ToString();


        latitudeValue.text = latitudeValueStr;
        longitudeValue.text = longitudeValueStr;
        altitudeValue.text = altitudeValueStr;
        horizontalAccuracyValue.text = horizontalAccuracyValueStr;
        timeStampValue.text = timeStampValueStr;


        // latitudeValue.text = Input.location.lastData.latitude.ToString();
        // longitudeValue.text = Input.location.lastData.longitude.ToString();
        // altitudeValue.text  = Input.location.lastData.altitude.ToString();
        // horizontalAccuracyValue.text = Input.location.lastData.horizontalAccuracy.ToString();
        // horizontalAccuracyValue.text = Input.location.lastData.timestamp.ToString();

    }else{
        // service stopped
        GPSStatusStr="Stoped";
        GPSStatusValue.text = GPSStatusStr;
        gpsIndicator.GetComponent<Renderer>().material.color = Color.black; 

    }
}

private string AddLocationToCSVstring(string outS){
        // location to csv file
        // outS= ""; // initialize with empty
        outS += System.DateTime.Now.ToString("yy-MM-dd HH:mm") + ", "; //date time     
        // outS +=  "Latitude, Longitude:, " + latitudeValueStr + ", " + longitudeValueStr+ ", ";
        // outS +=  "Altitude: , " +altitudeValueStr+ ", ";
        // outS +=  "Accuracy: , " +horizontalAccuracyValueStr+ ", ";
        // outS +=  "Location status: , " +GPSStatusStr+ ", "; 
        // outS +=  "Location TimeStamp: , " +timeStampValueStr+ ", ";

            if(System.String.IsNullOrEmpty(latitudeValueStr) ){outS += "0 , ";}
            else {outS +=  latitudeValueStr+", ";}
            if(System.String.IsNullOrEmpty(longitudeValueStr) ){outS += "0 , ";}
            else {outS +=  longitudeValueStr+", ";}
            if(System.String.IsNullOrEmpty(altitudeValueStr) ){outS += "0 , ";}
            else {outS +=  altitudeValueStr+", ";}
            if(System.String.IsNullOrEmpty(horizontalAccuracyValueStr) ){outS += "0 , ";}
            else {outS +=  horizontalAccuracyValueStr+", ";}
            if(System.String.IsNullOrEmpty(GPSStatusStr) ){outS += "0 , ";}
            else {outS +=  GPSStatusStr+", ";}
            if(System.String.IsNullOrEmpty(timeStampValueStr) ){outS += "0 , ";}
            else {outS +=  timeStampValueStr+", ";}

        return outS;
}


public void onFoodInputClicked(){
    SceneManager.LoadScene("SpeechTextSpeech");
}


void Start(){

    // filename for saving png and wav
    // path = Application.dataPath+"/";
    path = Application.persistentDataPath+"/";  
// Debug.Log("Application.persistentDataPath: "+path);


    filenameText.text = path;
    //="/storage/emulated/0/Download/";

    currentCamIndex = PlayerPrefs.GetInt("currentCamIndex",currentCamIndex);
    currentMicIndex = PlayerPrefs.GetInt("currentMicIndex",currentMicIndex);

    currentCamIndex %= WebCamTexture.devices.Length;  //  Inx not to go beyound Length
    currentMicIndex %= Microphone.devices.Length;  //  Inx not to go beyound Length
// Debug.Log("Start: currentCamIndex: "+currentCamIndex);




    StartCoroutine(GPSLoc());
    StartStopCam_Clicked();

    foodInput = PlayerPrefs.GetString("foodText","3401180");
    if( foodInput.Contains("3401180") == false ){ 
        filenameText.text = "  Logfile: log_food_v1.txt will be updated on close";
    }

}

 public void Update(){

// UpdateGPSData();
     GetComponent<AudioSource>().GetSpectrumData(spectrum,0,fftWindow);
// Debug.Log("Spectrum 200 : " + spectrum[200]);

// Position and rotate circles

    for(int i=0;i<8;i++){
        float freqsum =0f;
        for(int j=(i*numberOfSamples/8); j< ((i+1)*numberOfSamples/8);j++) {
            freqsum +=spectrum[j];
        }
        if(freqsum < 1e-11f){freqsum = 1e-11f;}
// Debug.Log("log freqsum: " + i + " sum: " + Mathf.Log(freqsum, 3));


    if((micStatus ==2) && (Microphone.IsRecording(_deviceMic) == false) ){
	    audioSource.Stop(); 
	    micStatus=3;
        takeAudioText.text = "Play Rec";
        takeAudioText.color = Color.green;
    }

    if(Microphone.IsRecording(_deviceMic) == true){ // white spot shows GetPosition of recording

// Debug.Log("Microphone.GetPosition(_deviceMic): " + Microphone.GetPosition(_deviceMic)); //        Microphone.GetPosition(_deviceMic)

        rotationDirection = 1f;
        float gp = (float)Microphone.GetPosition(_deviceMic)/120000f;
        circles[8].transform.localScale = new Vector3(gp, 0.3f, 1f); //pos/10f



    } else { // to animate indicator
        rotationDirection = -0.2f;
    }

        pos += rotationDirection*speed*Time.deltaTime;
        float x = Mathf.Sin(pos+2f*3.14159f/8f*i)*(radius + heightMultiplier/Mathf.Log(freqsum, 3));
        float y = Mathf.Cos(pos+2f*3.14159f/8f*i)*(radius + heightMultiplier/Mathf.Log(freqsum, 3));
        circles[i].transform.position = new Vector3(x,y,0f) + origin;
    }
// cameraText.text = "micStatus: " + micStatus.ToString();

 }//Update

void OnApplicationFocus(bool hasFocus) //https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnApplicationFocus.html
    {
// Debug.Log("OnApplicationFocus (save psy): " + hasFocus);
// Debug.Log("in OnApplicationFocus(),  psyTense: " +psyTense.ToString("##.#"));

// Save psy when app goes to background or closed
        if(hasFocus == false){ // when app is closed. 
            // string outS = "Tense, " + psyTense.ToString("##.#")+", ";
            // outS +=  "Relaxed, " + psyRelaxed.ToString("##.#")+", ";
            
            // string outS = "Exhausted, " + (offs+psyTense).ToString("##.#")+", ";
            // outS +=  "Energized, " + (offs+psyRelaxed).ToString("##.#")+", ";
            // outS += "Worried, " + (offs+psyWorried).ToString("##.#")+", ";
            // outS += "Upset, " + (offs+psyUpset).ToString("##.#")+", ";
            // outS += "Depressed, " + (offs+psyDepressed).ToString("##.#")+", ";
            //  outS += "Calm, " + (offs+psyCalm).ToString("##.#")+", ";
            // outS += "Content, " + (offs+psyContent).ToString("##.#")+", ";
            // outS += "Excited, " + (offs+psyExcited).ToString("##.#")+", ";


// Debug.Log( File.Exists(path+"/log_psy_v1"+".txt") ? "File exists." : "File does not exist."  ); 
// PSY
            if(File.Exists(path+"/log_psy_v1"+".txt") == false){ // print header
                string headr = "Datetime, latitude, longitude, altitude, horizontalAccuracy, GPSStatus, timeStamp, ";
                headr += "Exhausted, Worried, psyUpset, Depressed, Relaxed, Calm, Content, Excited";
                SavWav.WriteString(path+"/log_psy_v1",headr);
            }


            string outS ="";
            if(System.String.IsNullOrEmpty((psyTense).ToString("##.#"))){outS += "0 , ";}
            else {outS +=  psyTense.ToString("##.#")+", ";}
            if(System.String.IsNullOrEmpty((psyWorried).ToString("##.#"))){outS += "0 , ";}
            else {outS +=  psyWorried.ToString("##.#")+", ";}            
            if(System.String.IsNullOrEmpty((psyUpset).ToString("##.#"))){outS += "0 , ";}
            else {outS +=  psyUpset.ToString("##.#")+", ";}            
            if(System.String.IsNullOrEmpty((psyDepressed).ToString("##.#"))){outS += "0 , ";}
            else {outS +=  psyDepressed.ToString("##.#")+", ";}   

            if(System.String.IsNullOrEmpty((psyRelaxed).ToString("##.#"))){outS += "0 , ";}
            else {outS +=  psyRelaxed.ToString("##.#")+", ";}   
            if(System.String.IsNullOrEmpty((psyCalm).ToString("##.#"))){outS += "0 , ";}
            else {outS +=  psyCalm.ToString("##.#")+", ";}            
            if(System.String.IsNullOrEmpty((psyContent).ToString("##.#"))){outS += "0 , ";}
            else {outS +=  psyContent.ToString("##.#")+", ";}
            if(System.String.IsNullOrEmpty((psyExcited).ToString("##.#"))){outS += "0  ";}
            else {outS +=  psyExcited.ToString("##.#");}

    

            // location to csv file
            string outSo ="";
            outSo = AddLocationToCSVstring(outSo); // Add location data to log csv string
            outSo +=  outS;
            SavWav.WriteString(path+"/log_psy_v1",outSo);
            filenameText.text = "  Logfile: log_psy_v1.txt is updated";

// FOOD

            if(File.Exists(path+"/log_food_v1"+".txt") == false){ // print header
                string headr = "Datetime, latitude, longitude, altitude, horizontalAccuracy, GPSStatus, timeStamp, ";
                headr += "Food Text";
                SavWav.WriteString(path+"/log_food_v1",headr);
            }
            
            foodInput = PlayerPrefs.GetString("foodText","3401180");
            if( foodInput.Contains("3401180") == false ){ // save food Input if input exists
                PlayerPrefs.SetString("foodText","3401180"); // to prevent mulitple addings of food string
                PlayerPrefs.Save();

                outSo ="";
                outSo = AddLocationToCSVstring(outSo); // Add location data to log csv string
                outSo +=  foodInput; // Add food string
                SavWav.WriteString(path+"/log_food_v1",outSo);
                filenameText.text = "  Logfile: log_food_v1.txt is updated";
            }
            // reset sliders 
            for(int i=0;i<8;i++){
                myslider = sliders[i].GetComponent<Slider>();
                myslider.value = 0f;
            }

            // reset all
            ResetAudioButtons();
        }

    }

}//class
