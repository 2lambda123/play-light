using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TextSpeech;

public class SampleSpeechToText : MonoBehaviour
{
    public GameObject loading;
    public InputField inputLocale;
    public InputField inputText;
    public float pitch;
    public float rate;

    public Text txtLocale;
    public Text txtPitch;
    public Text txtRate;
    private bool exampleShown = false;
    void Start()
    {
        inputLocale.text = "en-US";
        txtLocale.text = inputLocale.text;
        Setting(inputLocale.text);

        inputText.text = "Example (where, what, weight estimate or portion - say in any order): I had lunch in canteen 3, I ate a bowl of fish soup, about 200 gramms of steamed vegetables and 2 pieces of watermelon";
        exampleShown = true;

        loading.SetActive(false);
        SpeechToText.instance.onResultCallback = OnResultSpeech;

    }
    

    public void StartRecording()
    {
        if(exampleShown == true){
            inputText.text = txtLocale.text+": ";
            exampleShown = false;
        }

#if UNITY_EDITOR
#else
        SpeechToText.instance.StartRecording("Speak any");
#endif
    }

    public void StopRecording()
    {
#if UNITY_EDITOR
        OnResultSpeech("Not support in editor.");
#else
        SpeechToText.instance.StopRecording();
#endif
#if UNITY_IOS
        loading.SetActive(true);
#endif
    }
    void OnResultSpeech(string _data)
    {
        inputText.text +=" "+ _data;
#if UNITY_IOS
        loading.SetActive(false);
#endif
    }
    public void OnClickSpeak()
    {
        TextToSpeech.instance.StartSpeak(inputText.text);
    }
    public void  OnClickStopSpeak()
    {
        TextToSpeech.instance.StopSpeak();
    }
    public void Setting(string code)
    {
        TextToSpeech.instance.Setting(code, pitch, rate);
        SpeechToText.instance.Setting(code);
        txtLocale.text = "Locale: " + code;
        txtPitch.text = "Pitch: " + pitch;
        txtRate.text = "Rate: " + rate;
    }
    public void OnClickApply()
    {
        Setting(inputLocale.text);
    }

    public void onBackAndDiscard(){

// Debug.Log("onBackAndDiscard");
        inputText.text = "3401180"; // discard input signal
        PlayerPrefs.SetString("foodText",inputText.text);
        PlayerPrefs.Save();
        // string player = PlayerPrefs.GetString("username");
        SceneManager.LoadScene("Audio");

    }
        public void onBackAndSave(){    
// Debug.Log("onBackAndSave");
        PlayerPrefs.SetString("foodText",inputText.text);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Audio");
    }
}
