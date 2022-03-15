using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


[RequireComponent(typeof(ARRaycastManager))]
public class PlacementWithManyObjects : MonoBehaviour
{
public GameObject placePrefab;
private Vector2 touchPosition = default;
private ARRaycastManager aRRaycastManager;
private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

public void Awake(){
    aRRaycastManager = GetComponent<ARRaycastManager>();
}
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount>0){
            Touch touch = Input.GetTouch(0);
            if(touch.phase == TouchPhase.Began){
                touchPosition = touch.position;
                if(aRRaycastManager.Raycast(touchPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon)){
                    var hitPose = hits[0].pose;
                    Instantiate(placePrefab, hitPose.position, hitPose.rotation);
                }

            }
        }
    }
}
