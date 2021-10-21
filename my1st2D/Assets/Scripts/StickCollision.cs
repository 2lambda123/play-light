using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickCollision : MonoBehaviour
{
    //private string PUCK_TAG = "PuckGhost";
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Debug.Log("OnCollisionEnter2D");
        // if (collision.gameObject.CompareTag(PUCK_TAG)){

        //     transform.eulerAngles = new Vector3(
        //     transform.eulerAngles.x,
        //     transform.eulerAngles.y,
        //     transform.eulerAngles.z
        //     );

        // }
    }// OnCollisionEnter2D

    private void OnTriggerEnter2D(Collider2D collision){
        //Debug.Log("OnTriggerEnter2D");
        // if(collision.CompareTag(PUCK_TAG)){

        //     transform.eulerAngles = new Vector3(
        //     transform.eulerAngles.x,
        //     transform.eulerAngles.y,
        //     transform.eulerAngles.z
        //     );
        
        // }

        
    }//OnTriggerEnter2D
}
