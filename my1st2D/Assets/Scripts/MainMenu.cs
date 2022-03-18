using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;

public class MainMenu : MonoBehaviour
{
    public Text emailInput, passwordInput;

public void Login(){
    FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailInput.text,
        passwordInput.text).ContinueWith( ( task => {
            if(task.IsCanceled){
                return;
            }

            if(task.IsFaulted){
                Firebase.FirebaseException e = 
                task.Exception.Flatten().InnerExceptions[0] as Firebase.FirebaseException;
                GetErrorMessage((AuthError)e.ErrorCode);
                return;
            }

            if(task.IsCompleted){

            }

        }));
}//Login
public void Logout(){
    
}
public void Login_Anonymous(){
    
}
public void RegisterUser(){
    
}


public void ButtonFoodInput(){
        SceneManager.LoadScene("Audio");
    }
public void ButtonARplane(){
        SceneManager.LoadScene("ARPlaneDetection");
    }


void GetErrorMessage(AuthError errorCode){
    string msg = "";
    msg = errorCode.ToString();
    print(msg);
}


}//class
