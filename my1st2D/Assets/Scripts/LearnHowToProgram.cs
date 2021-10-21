using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LearnHowToProgram : MonoBehaviour
{
   
   private Rigidbody2D myBody;
   private BoxCollider2D myCollider;
   private Animator animator;
   private AudioSource audioSource;
   private Transform myTransform;
   
   // awake is the 1 function that is called
   private void Awake(){

   }
   // 2nd function called
   private void OnEnable(){

   }
// 3rd function called
    private void Start() {

    myBody = GetComponent<Rigidbody2D>();
    audioSource = GetComponent<AudioSource>();
    audioSource.Play();
    //myTransform = GetComponent<Transform>();
    myTransform = transform;
    myTransform.position = new Vector3(1,2,3);





    // Player player = new Player("Puck","blue",1.0f, 101);

    // player.Info();
    // player.SetHealth(99);
    // Debug.Log("The health var in class: " + player.GetHealth());
    // player.Health = 98;
    // Debug.Log("The health var in class: " + player.Health);

    // Puck puck = new Puck("puck1","red",2f, 95);
    // puck.Health = 20;
    // //puck.Color = "reddish";
    // puck.Info();
    // player.Attack();
    // puck.Attack();

    } //Start

} // class 
