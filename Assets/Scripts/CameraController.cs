using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private const float reOrientThreshold = 0.01f;
    private const float minReOrientSpeedMult = 0.01f;
    [SerializeField] private float reorientSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float boostMultiplier;
    [SerializeField] private float dampingMultiplierDuringTransition;
    [SerializeField] private float initialDampingDuration;

    InputAction movementAction;
    [Space] [SerializeField] private InputActionAsset cameraControls;

    // Input variables
    private float Horizontal
    {
        get
        {
            var keyboard = Keyboard.current;
            var horizontal = 0;
            if (keyboard.aKey.isPressed)
            {
                horizontal += -1;
            }
            if (keyboard.dKey.isPressed)
            {
                horizontal += 1;
            }
            return horizontal;
        }
    }
    private float Vertical
    {
        get 
        {
            var keyboard = Keyboard.current;
            var vertical = 0;
            if (keyboard.wKey.isPressed)
            {
                vertical += 1;
            }
            if (keyboard.sKey.isPressed)
            {
                vertical += -1;
            }
            return vertical;  
        }
    }
    private float MouseDeltaX
    {
        get
        {
            var mouse = Mouse.current;
            var mouseDeltaX = mouse.delta.x.ReadValue();
            // Combination of 2 dampings 0.1 and 0.5 due to old mouse sensitivity and windows damping
            // https://forum.unity.com/threads/mouse-delta-input.646606/
            return mouseDeltaX * 0.05f;
        }
    }
    private float MouseDeltaY
    {
        get
        {
            var mouse = Mouse.current;
            var mouseDeltaY = mouse.delta.y.ReadValue();
            return mouseDeltaY * 0.05f;
        }
    }
    private float UpDownInput { get
        {
            var keyboard = Keyboard.current;
            var upDown = 0;
            if (keyboard.qKey.isPressed)
            {
                upDown += 1;
            }
            if (keyboard.eKey.isPressed)
            {
                upDown += -1;
            }
            return upDown;
        }
    }
    private bool IsBoosting
    {
        get
        {
            var keyboard = Keyboard.current;
            return keyboard.leftShiftKey.isPressed;
        }
    }
    private bool PressedSpaceKey
    {
        get
        {
            var keyboard = Keyboard.current;
            return keyboard.spaceKey.isPressed;
        }
    }

    float currentMovementDamp = 0.001f;
    bool isReorienting = false;
    bool isBoosting = false;
    float currentBoostMult = 1f;

    private void Awake()
    {
        var cameraControllerActionMap = cameraControls.FindActionMap("CameraMap");

        movementAction = cameraControllerActionMap.FindAction("Movement");

        Cursor.lockState = CursorLockMode.Locked;
        Invoke(nameof(RemoveDamp), initialDampingDuration);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var inputX = Horizontal;
        var inputY = Vertical;
        var inputRotX = MouseDeltaX;
        var inputRotY = MouseDeltaY;
        var upDownInput = UpDownInput;
        isBoosting = IsBoosting;

        currentBoostMult = 1f + Convert.ToInt32(isBoosting) * (boostMultiplier - 1);

        transform.Rotate(Vector3.up, rotationSpeed * currentMovementDamp * inputRotX * Time.fixedDeltaTime, Space.World);
        transform.Rotate(Vector3.left, rotationSpeed * currentMovementDamp * inputRotY * Time.fixedDeltaTime);

        transform.Translate(Vector3.right * moveSpeed * currentMovementDamp * currentBoostMult * inputX * Time.fixedDeltaTime);
        transform.Translate(Vector3.forward * moveSpeed * currentMovementDamp * currentBoostMult * inputY * Time.fixedDeltaTime);

        transform.Translate(Vector3.up * moveSpeed * currentMovementDamp * currentBoostMult * upDownInput * Time.fixedDeltaTime);

    }

    private void Update()
    {
        if (PressedSpaceKey && !isReorienting)
        {
            StartCoroutine(nameof(ReOrient));
            AddDampTemporarily(10f);
        }
    }

    void RemoveDamp()
    {
        currentMovementDamp = 1f;
    }
    /// <summary>
    /// Reorientation of object rotation to make it level with the ground (i.e, the object will be facing forward in any horizontal direction).<br/>
    /// Can be called as a Coroutine from a trigger script, uses the flag isReorienting for operation.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReOrient()
    {
        isReorienting = true;

        var uprightness = Vector3.Angle(transform.up, Vector3.up) / 180f;
        var upwardTilt = Vector3.Dot(transform.forward, Vector3.up);

        // Temp value that stores the new upwardTilt value before assignment
        float newUpwardTilt;

        var leftwardTilt = Vector3.Dot(transform.right, Vector3.up);

        // Temp value that stores the new leftwardTilt value before assignment
        float newLeftwardTilt;

        var upwardTiltSignChangeCount = 0;
        var leftwardTiltSignChangeCount = 0;
        while (uprightness >= reOrientThreshold)
        {
            // If looking above, face downwards.
            transform.Rotate(-transform.right, rotationSpeed * Mathf.Sign(uprightness) * Mathf.Sign(-upwardTilt) * (Mathf.Abs(uprightness) + minReOrientSpeedMult) * (100f / (100f + upwardTiltSignChangeCount*upwardTiltSignChangeCount)) * Time.fixedDeltaTime, Space.World);
            // If tilted sideways, rotate face
            transform.Rotate(-transform.forward, rotationSpeed * Mathf.Sign(uprightness) * Mathf.Sign(leftwardTilt) * (Mathf.Abs(uprightness) + minReOrientSpeedMult) * (100f / (100f + leftwardTiltSignChangeCount * leftwardTiltSignChangeCount)) * Time.fixedDeltaTime, Space.World);

            uprightness = Vector3.Angle(transform.up, Vector3.up) / 180f;
            
            newUpwardTilt = Vector3.Dot(transform.forward, Vector3.up);
            newLeftwardTilt = Vector3.Dot(transform.right, Vector3.up);
            // If there is a sign change between the last frame and current frame, dampen the upright reposition velocity
            // Dampen factor is 100 / (100 + x^2)
            if (newUpwardTilt * upwardTilt < 0)
            {
                upwardTiltSignChangeCount += 10;
            }
            else
            {
                upwardTiltSignChangeCount = Mathf.Max(upwardTiltSignChangeCount - 1, 0);
            }
            if (newLeftwardTilt * leftwardTilt < 0)
            {
                leftwardTiltSignChangeCount += 10;
            }
            else
            {
                leftwardTiltSignChangeCount = Mathf.Max(upwardTiltSignChangeCount - 1, 0);
            }

            upwardTilt = newUpwardTilt;
            leftwardTilt = newLeftwardTilt;


            yield return new WaitForFixedUpdate();

        }

        RemoveDamp();
        isReorienting = false;
    }

    void AddDampTemporarily(float dampTime)
    {
        currentMovementDamp = dampingMultiplierDuringTransition;
        Invoke(nameof(RemoveDamp), dampTime);
    }
}
