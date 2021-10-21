using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[DefaultExecutionOrder(-1)]
public class InputManager : Singleton<InputManager>
{
    public delegate void StartTouchEvent(Vector2 position, float time);
    public event StartTouchEvent OnStartTouch;
    public delegate void EndTouchEvent(Vector2 position, float time);
    public event EndTouchEvent OnEndTouch;



   private TouchControlls touchControlls;
   
   private void Awake(){

       touchControlls = new TouchControlls();

   }

    private void OnEnable(){
    touchControlls.Enable();
    TouchSimulation.Enable();
    UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown +=FingerDown;
    }
    private void OnDisable(){
    touchControlls.Disable();
    TouchSimulation.Disable();
    UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -=FingerDown;
    }

    private void Start(){
        touchControlls.Touch.TouchPress.started += ctx => StartTouch(ctx);
        touchControlls.Touch.TouchPress.canceled += ctx => EndTouch(ctx);
        
    }

    private void StartTouch(InputAction.CallbackContext context){
        //Debug.Log("Touch Started: " + touchControlls.Touch.TouchPosition.ReadValue<Vector2>());
        if(OnStartTouch != null) OnStartTouch(touchControlls.Touch.TouchPosition.ReadValue<Vector2>(), (float) context.startTime);
    }
    private void EndTouch(InputAction.CallbackContext context){
        //Debug.Log("Touch Ended: " + touchControlls.Touch.TouchPosition.ReadValue<Vector2>());
        if(OnEndTouch != null) OnEndTouch(touchControlls.Touch.TouchPosition.ReadValue<Vector2>(), (float) context.time);
    }

    private void FingerDown(Finger finger){
        if(OnStartTouch != null) OnStartTouch(finger.screenPosition, Time.time);

    }
    private void Update(){
        //Debug.Log(UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches);
        foreach(UnityEngine.InputSystem.EnhancedTouch.Touch touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches){
            //Debug.Log(touch.phase == UnityEngine.InputSystem.TouchPhase.Began);

        }
    }

}
