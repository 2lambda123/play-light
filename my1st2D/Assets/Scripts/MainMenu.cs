using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame(){
        //Debug.Log("Button Select Player pressed");
        
        string objCalled = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
        int selectedCharacter = int.Parse(objCalled);

        GameManager.instance.CharInx = selectedCharacter;

        // int[] a = new int[10];
        // a[selectedCharacter] = 1;

        //Debug.Log("Index: " + objCalled);

        SceneManager.LoadScene("SampleScene");
    }

    public void WebCamTest(){
        SceneManager.LoadScene("Webcam");
    }
}
