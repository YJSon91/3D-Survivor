using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private Vector2 curMovementInput;
    public float jumpPower;
    public LayerMask groundLayerMask;

    [Header("Look")]
    public float lookSensitivity = 1.0f; // 기본값을 1.0f 정도로 설정하거나 인스펙터에서 조절

    // ... (다른 변수들은 그대로) ...
    // public Transform cameraContainer; // 이 변수는 이제 3인칭에서 직접 사용 안 함
    // public float minXLook;           // 새 카메라 스크립트에서 관리
    // public float maxXLook;           // 새 카메라 스크립트에서 관리
    // private float camCurXRot;        // 새 카메라 스크립트에서 관리
    // ...

    public Action inventory;
    private Vector2 mouseDelta;

    [HideInInspector]
    public bool canLook = true;

   
    private Rigidbody rigidbody;
    private Animator animator;
    private SkinnedMeshRenderer[] meshRenderers;
    private EquipTool equipTool;
    public ThirdPersonCamera thirdPersonCamera;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>(); // 씬에 하나만 있다면 이렇게 찾아도 됨
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LateUpdate()
    {
        if (canLook)
        {
            // 플레이어의 좌우 회전 (Y축)은 계속 담당
            transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity * Time.deltaTime * 50f, 0); // Time.deltaTime 보정 및 스케일링
        }
        // 매 프레임 mouseDelta를 초기화 (카메라 스크립트에서도 개별적으로 할 수 있음)
        // mouseDelta = Vector2.zero; // PlayerController가 입력을 받고 즉시 전달 후 초기화
        // 또는 ThirdPersonCamera에서 자체적으로 _cameraMouseDelta를 매 프レ임 초기화
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.SetMouseDelta(mouseDelta); // ThirdPersonCamera 스크립트로 mouseDelta 전달
        }
    }
    // LateUpdate에서 mouseDelta.x를 사용한 플레이어 회전은 유지

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            curMovementInput = context.ReadValue<Vector2>();
            animator.SetBool("Moving", true);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
            animator.SetBool("Moving", false);
        }
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        
        if (context.phase == InputActionPhase.Started && IsGrounded())
        {
            animator.SetTrigger("Jump");
            rigidbody.AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
            
        }
    }

    private void Move()
    {
        Vector3 moveDir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = rigidbody.velocity.y; // y속도 유지

        rigidbody.velocity = velocity; // 최소한 덮어쓰기 줄이기
    }

    void CameraLook()
    {
        // 플레이어의 좌우 회전 (Y축)
        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);

        // 기존 카메라 상하 회전 (X축) 및 cameraContainer 조작은 주석 처리
        /*
        camCurXRot += mouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);
        */
    }

    bool IsGrounded()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) +(transform.up * 0.01f), Vector3.down)
        };

        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], 0.1f, groundLayerMask))
            {
                return true;
            }
        }

        return false;
    }
    public void OnInventoryInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }
    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }
    public void GetRigidBody(out Rigidbody rb)
    {
        rb = rigidbody;
    }
    public void SpeedUp(float speedupValue, float duration)
    {
        StartCoroutine(SpeedUpCoroutine(speedupValue, duration));
    }

    private IEnumerator SpeedUpCoroutine(float speedupValue, float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed += speedupValue;

        yield return new WaitForSeconds(duration);

        moveSpeed = originalSpeed;
    }
}
   