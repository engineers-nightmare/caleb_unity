using UnityEngine;
using System.Collections;

public class CrouchHandler : MonoBehaviour {

    public CharacterController Controller = null;
    public Transform CameraTransform = null;

    public float StandingHeight = 1.8f;
    public float CrouchHeight = 0.6f;
    public float StandingCameraOffset = 0.8f;
    public float CrouchCameraOffset = 0.2f;
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Controller.height = CrouchHeight;
            CameraTransform.localPosition = new Vector3(0, CrouchCameraOffset, 0);
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            Controller.height = StandingHeight;
            CameraTransform.localPosition = new Vector3(0, StandingCameraOffset, 0);
        }
	}
}
