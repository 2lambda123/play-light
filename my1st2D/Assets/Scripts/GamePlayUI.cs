using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GamePlayUI : MonoBehaviour
{
    
    public void RestartButton(){

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    public void HomeButton(){

        SceneManager.LoadScene("MainMenu");
    }


}//class
