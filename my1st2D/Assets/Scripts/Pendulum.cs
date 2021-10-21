using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    
    public GameObject puck;
    public GameObject stick;

    public GameObject stickBlade;

    public GameObject[] respawns;
    Rigidbody2D rb2d;
    
    
    private Transform target;

    private Transform source;

    public float pokeDistance = 4f;
    public float speed = 2f; 

    private Vector3 originPos;
    private Vector3 puckPos;
    private Vector3 dirDown = new Vector3(0f, -1f, 0f);

    private bool pokeInProgress = false;
    private bool pokeForward = true;

    private float stickInitZ = 0f;

    private float gravityStrength = 10f;

    private string PUCK_TAG = "PuckGhost";

    
    void Awake(){
        Physics2D.gravity = new Vector2(gravityStrength, 0f); // (1f, -1f)
     }
        void Start()
    {


        rb2d = GetComponent<Rigidbody2D>();


        //if (respawns == null)
        respawns = GameObject.FindGameObjectsWithTag(PUCK_TAG);
        //Debug.Log(" Found with tag PuckGhost: " + respawns.Length);

        foreach (GameObject respawn in respawns)
        {
            //Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation);
            //Debug.Log(respawn.name);
            puck = respawn;
        }
       
        respawns = GameObject.FindGameObjectsWithTag("StickOnHinge");
        //Debug.Log(" Found with tag StickOnHinge: " + respawns.Length);

        foreach (GameObject respawn in respawns)
        {
            //Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation);
            //.Log(respawn.name);
            stick = respawn;
        }

        respawns = GameObject.FindGameObjectsWithTag("StickBlade");
        //Debug.Log(" Found with tag StickBlade: " + respawns.Length);

        foreach (GameObject respawn in respawns)
        {
            //Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation);
            //Debug.Log(respawn.name);
            stickBlade = respawn;
        }

        target = puck.transform;
        source = stick.transform;
        originPos = source.position;
        puckPos = target.position;
        stickInitZ = stickBlade.transform.rotation.z;

        // Set the hinge limits for a stick rotation.
        HingeJoint2D hinge = stickBlade.GetComponent(typeof(HingeJoint2D)) as HingeJoint2D;

        //HingeJoint hinge = GetComponent<HingeJoint>();

        JointAngleLimits2D limits = hinge.limits;
        limits.min = -90;
        limits.max = 90;
        hinge.limits = limits;
        hinge.useLimits = true;

    } //Start 

    // Update is called once per frame
    void Update()
    {
        
        Vector3 targetDir = puck.transform.position - originPos;
        Vector3 sourceDir = stickBlade.transform.position - originPos;
        float angle = Vector3.Angle(targetDir, sourceDir);
        float magn = Vector3.Magnitude(targetDir);
        // Debug.Log("magn to puck: "+ magn);
        // Debug.Log("angle: " + angle);


        Physics2D.gravity = new Vector2(targetDir.x/magn*gravityStrength, targetDir.y/magn*gravityStrength); // (1f, -1f)

         if(pokeInProgress == true){
            PokePuck();
         }else{
            if (angle < 5.0f){               
                PokePuck();
                pokeInProgress = true;
            }
         }
         
  
            

        
    }

    public void PokePuck(){
        float step =  speed * Time.deltaTime; // calculate distance to move

        if(pokeInProgress == false){
            originPos = source.position;
            puckPos = target.position;
            pokeInProgress = true;

        }
        else{
            source.position = Vector3.MoveTowards(source.position, puckPos, step);

        // Check if the pokeDistance is traveled.
            if (Vector3.Distance(source.position, originPos) > pokeDistance && pokeForward == true)
            {
            // return back to the original position of stick.
                puckPos = originPos;
                pokeForward = false;
            }
            // Check if stick reached original position
            if(Vector3.Distance(source.position, originPos) < 0.001f && pokeForward == false){
                source.position = originPos;
                pokeForward = true;
                pokeInProgress = false;
            }

        }

        

    
    }// PokePuck

    // private void OnCollisionEnter2D(Collision2D collision)
    // {

    //     if (collision.gameObject.CompareTag(PUCK_TAG)){
    //         Destroy(gameObject);
    //     }
    // }// OnCollision

    // private void OnTriggerEnter2D(Collider2D collision){
        
    //     if(collision.CompareTag(PUCK_TAG))
    //     Destroy(gameObject);
    // }

}//class
