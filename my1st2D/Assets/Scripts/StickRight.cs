using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickRight : MonoBehaviour
{
    [SerializeField]
    private float pokeForce = 10f;
    [SerializeField]
    private float swingForce = 11f;

    private float movementX;

    private Rigidbody2D myBody;

    private SpriteRenderer sr;

    private Animator anim;
    private string TURN_ANIMATION = "TurnR";

    private bool isGrounded;
    private string GROUND_TAG = "Ground";
    private string PUCK_TAG = "Puck";

    private void Awake(){
        myBody = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

    } //Awake

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PlayerMoveKeyboard();
        AnimateStick();
        
    }

    private void FixedUpdate(){
        StickJump();
    }

    void PlayerMoveKeyboard() {

        movementX = Input.GetAxisRaw("Horizontal");
        transform.position += new Vector3(movementX, 0f, 0f)*Time.deltaTime*pokeForce;

    }

    void AnimateStick(){

        if (movementX > 0)
        {
            anim.SetBool(TURN_ANIMATION, true);
            //sr.flipX = false;
        }
        else if (movementX < 0)
        {
            anim.SetBool(TURN_ANIMATION, true);
            //sr.flipX = true;
        }
        else{
            anim.SetBool(TURN_ANIMATION, false);
        }

    }

    void StickJump(){
        if (Input.GetButtonDown("Jump") && isGrounded){
            //Debug.Log("Jump pressed");
            isGrounded = false;
            myBody.AddForce(new Vector2(0f,swingForce), ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(GROUND_TAG)){
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag(PUCK_TAG)){
            Destroy(gameObject);
        }
    }// OnCollision

    private void OnTriggerEnter2D(Collider2D collision){
        
        if(collision.CompareTag(PUCK_TAG))
        Destroy(gameObject);
    }
}// class
