using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField]
    private GameObject[] characters;

    private int _charInx;
    public int CharInx{
        get{return _charInx;}
        set{_charInx = value;}
    }

    private void Awake(){
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }
    
    
    private void OnEnable(){
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }
    private void OnDisable(){
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode){

        if(scene.name == "SampleScene")
        Instantiate(characters[CharInx]);

    }


}
