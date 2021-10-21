using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOverSprite : MonoBehaviour
{
    SpriteRenderer rend; 
    SpriteRenderer puckG;
    public GameObject stickSmall;

    public GameObject puck;

    public GameObject[] respawns;
    private string STICKSMALL_TAG = "StickSmall";
    private string PUCK_TAG = "PuckGhost";

    void Start(){
        respawns = GameObject.FindGameObjectsWithTag(PUCK_TAG);
        //Debug.Log(" Found with tag PuckGhost: " + respawns.Length);

        foreach (GameObject respawn in respawns)
        {
            //Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation);
            //Debug.Log(respawn.name);
            puck = respawn;
        }
       
        respawns = GameObject.FindGameObjectsWithTag(STICKSMALL_TAG);
        //Debug.Log(" Found with tag StickOnHinge: " + respawns.Length);

        foreach (GameObject respawn in respawns)
        {
            //Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation);
            //.Log(respawn.name);
            stickSmall = respawn;
        }

        rend = stickSmall.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        rend.enabled = false;

        puckG = puck.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        puckG.color = new Color (0, 0, 1, 1);
    }//Start

    void Update () {
 
        //Gets the world position of the mouse on the screen        
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint( Input.mousePosition );
 
        //Checks whether the mouse is over the sprite
        bool overSprite = this.GetComponent<SpriteRenderer>().bounds.Contains( mousePosition );
     
        //If it's over the sprite
        if (overSprite){
            rend.enabled = true;
             //If we've pressed down on the mouse (or touched on the iphone)
            if (Input.GetButton("Fire1")){
                puckG.color = new Color (0, 1, 0, 1);
                     //Set the position to the mouse position
                this.transform.position = new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
                                                 Camera.main.ScreenToWorldPoint(Input.mousePosition).y,
                                                   0.0f);
            }else{
                puckG.color = new Color (0, 0, 1, 1);
            }
        }else{
            rend.enabled = false;
            puckG.color = new Color (0, 0, 1, 1);
        }
         
    }//Update

    
}
