using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject[] puckReference;

    private GameObject spawnedPuck;

    [SerializeField]
    private Transform leftPos, rightPos;

    private int randomInx;
    private int randomSide;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SpawnPucks());
    }

    IEnumerator SpawnPucks(){

        while(true){
            yield return new WaitForSeconds(Random.Range(1,5));

            randomInx = Random.Range(0, puckReference.Length);
            randomSide = Random.Range(0,2);

            spawnedPuck = Instantiate(puckReference[randomInx]);

            //left side
            if(randomSide == 0)
            {
                spawnedPuck.transform.position = leftPos.position;
                spawnedPuck.GetComponent<PuckOrange>().speed = Random.Range(4,10);
            } else 
            {
            // right side
                spawnedPuck.transform.position = rightPos.position;
                spawnedPuck.GetComponent<PuckOrange>().speed = -Random.Range(4,10);
                //spawnedPuck.transform.localScale = new Vector3(-1f,1f,1f);

            }

        }//while
 
    } // IEnum
} //class
