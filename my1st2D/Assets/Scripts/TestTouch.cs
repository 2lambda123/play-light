using UnityEngine;

public class TestTouch : MonoBehaviour
{
    private InputManager inputManager;
    private Camera cameraMain;

    private void Awake(){
        inputManager = InputManager.Instance;
        cameraMain = Camera.main;
    }

    private void OnEnable(){
        inputManager.OnStartTouch += Move;
    }
    private void OnDisable(){
        inputManager.OnEndTouch -= Move;
    }

    public void Move(Vector2 sceenPosition, float time){
        Vector3 screenCoordinates = new Vector3(sceenPosition.x, sceenPosition.y, cameraMain.nearClipPlane);
        Vector3 worldCoordinates = cameraMain.ScreenToWorldPoint(screenCoordinates);
        worldCoordinates.z = 0;
        transform.position = worldCoordinates;
    }
}
