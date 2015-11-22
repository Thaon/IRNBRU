using UnityEngine;
using System.Collections;

public class FPSController : MonoBehaviour {

    bool characterController;
    PauseMenu paused;
    FPSController player;
    float height = 0.0f;

    bool m_dialogueIsPrompted = false;

    public float speed = 10.0f;
    public float sprintSpeed=15.0f;

    //Input stuff
    bool left, right, up, down, sprint;

    //Mouselook stuff
    Vector2 _mouseAbsolute;
    Vector2 _smoothMouse;

    public Vector2 clampInDegrees = new Vector2(360, 180);
    public bool lockCursor;
    public Vector2 sensitivity = new Vector2(2, 2);
    public Vector2 smoothing = new Vector2(3, 3);
    public Vector2 targetDirection;
    public Vector2 targetCharacterDirection;
    public bool gamepadConnected = false;


	// Use this for initialization
	void Start () {

        // Set target direction to the camera's initial orientation.
        targetDirection = transform.localRotation.eulerAngles;
        paused = GetComponent<PauseMenu>();
        player = GetComponent<FPSController>();
       
	}


    void Update()
    {
       
        if (paused.paused == true || m_dialogueIsPrompted)
        {

        }
        else
        {
            GetComponent<CharacterController>().Move(Movement(speed) * Time.deltaTime);
            height = GetComponent<CharacterController>().height;
            MouseLook();
        }


        if (Input.GetJoystickNames().Length > 0)
        {
            if (Input.GetJoystickNames()[0] == "Controller (XBOX 360 For Windows)")
            {
                Debug.Log("Controller Connected");
                gamepadConnected = true;
            }
            else
                gamepadConnected = false;
        }

    }

    Vector3 Movement(float speed)
    {

        Vector3 pos = Vector3.zero;

        #region Input_Manager

        bool controllerSprint=Input.GetButton("Sprint");

        if (controllerSprint)
        {
            //Gamepad Controller
            if (Input.GetAxis("Horizontal") <= -0.1f)
            {
                pos -= transform.right * sprintSpeed;
            }
            if (Input.GetAxis("Horizontal") >= 0.1f)
            {
                pos += transform.right * sprintSpeed;
            }
            if (Input.GetAxis("Vertical") <= -0.1f)
            {
                pos -= transform.forward * sprintSpeed;
            }
            if (Input.GetAxis("Vertical") >= 0.1f)
            {
                pos += transform.forward * sprintSpeed;
            }
        }
        else if (!controllerSprint)
        {
            //Gamepad Controller
            if (Input.GetAxis("Horizontal") <= -0.1f)
            {
                pos -= transform.right * speed;
            }
            if (Input.GetAxis("Horizontal") >= 0.1f)
            {
                pos += transform.right * speed;
            }
            if (Input.GetAxis("Vertical") <= -0.1f)
            {
                pos -= transform.forward * speed;
            }
            if (Input.GetAxis("Vertical") >= 0.1f)
            {
                pos += transform.forward * speed;
            }
        }

       // bool crouch = Input.GetButton("Crouch");
        #endregion

        return pos;
    }

    void MouseLook()
    {
        if (paused.paused || m_dialogueIsPrompted)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        #region Mouselook

        // Allow the script to clamp based on a desired target value.
        var targetOrientation = Quaternion.Euler(targetDirection);

        // Get raw mouse input for a cleaner reading on more sensitive mice.
        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        var mouseDelta2 = new Vector2(Input.GetAxisRaw("Joystick X"), Input.GetAxisRaw("Joystick Y"));

      

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        if (gamepadConnected)
            mouseDelta = Vector2.Scale(mouseDelta2, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));
        else
            mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));
       

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

        // Find the absolute mouse movement value from point zero.
        _mouseAbsolute += _smoothMouse;

        // Clamp and apply the local x value first, so as not to be affected by world transforms.
        if (clampInDegrees.x < 360)
            _mouseAbsolute.x = Mathf.Clamp(_mouseAbsolute.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

        var xRotation = Quaternion.AngleAxis(-_mouseAbsolute.y, targetOrientation * Vector3.right);
        transform.localRotation = xRotation;

        // Then clamp and apply the global y value.
        if (clampInDegrees.y < 360)
            _mouseAbsolute.y = Mathf.Clamp(_mouseAbsolute.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

        transform.localRotation *= targetOrientation;

        // If there's a character body that acts as a parent to the camera

        var yRotation = Quaternion.AngleAxis(_mouseAbsolute.x, transform.InverseTransformDirection(Vector3.up));
        transform.localRotation *= yRotation;

        #endregion

    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.tag == "Grabable")
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
            body.AddForce(pushDir, ForceMode.Impulse);
        }
    }
    

    public void ToggleDialoguePrompt()
    {
        m_dialogueIsPrompted = !m_dialogueIsPrompted;
    }
}
