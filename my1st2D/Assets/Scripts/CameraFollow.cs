using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    private Transform player;

    private string PLAYER_TAG = "Player";

    private Vector3 tempPos;

    [SerializeField]
    private float minX, maxX;


    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag(PLAYER_TAG).transform;
        //Debug.Log("Index passed from GameManager: " + GameManager.instance.CharInx);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if(!player) //if(player == null)
            return;

        tempPos = transform.position;
        tempPos.x = player.position.x;

        if (tempPos.x < minX)
            tempPos.x = minX;

        if (tempPos.x > maxX)
            tempPos.x = maxX;

        transform.position = tempPos;
        
    }
}// class
