using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera2WorldCalibrator : MonoBehaviour
{
    SpriteRenderer rend; 
    public GameObject stripeNLFR;
    public GameObject[] respawns;
    private string STRIPE_TAG = "NL-FR";

    void Start()
    {
        respawns = GameObject.FindGameObjectsWithTag(STRIPE_TAG);
        Debug.Log(" Found with tag STRIPE_TAG: " + respawns.Length);

        foreach (GameObject respawn in respawns)
        {
            //Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation);
            //Debug.Log(respawn.name);
            stripeNLFR = respawn;
        }

        rend = stripeNLFR.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        rend.enabled = true;
        StartCoroutine(  PauseSecToggle(3f, rend)  );
        Debug.Log(" after subroutine " );
        StartCoroutine(  PauseSecToggle(3f, rend)  );
        StartCoroutine(  PauseSecToggle(3f, rend)  );
        
        

    } //Start




    IEnumerator PauseSecToggle(float delay, SpriteRenderer rendS){
            yield return new WaitForSeconds(delay);
            Debug.Log(" Wait is over " );
            if (rendS.enabled == false) {
                rendS.enabled = true;
            }
            if (rendS.enabled == true) {
                rendS.enabled = false;
            }
            
    }
}
