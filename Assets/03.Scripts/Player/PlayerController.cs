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
    private float _baseMoveSpeed; // 원래 속도를 저장할 변수

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
    private Coroutine _activeSpeedUpCoroutine;// 현재 실행 중인 SpeedUp 코루틴을 저장할 변수

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>(); // 씬에 하나만 있다면 이렇게 찾아도 됨
        _baseMoveSpeed = moveSpeed;                         // Awake 시점의 moveSpeed를 기본 속도로 저장

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
        // 1. 이미 실행 중인 SpeedUp 코루틴이 있다면 중단합니다.
        if (_activeSpeedUpCoroutine != null)
        {
            StopCoroutine(_activeSpeedUpCoroutine);
            // _activeSpeedUpCoroutine은 해당 코루틴의 finally 블록에서 null로 설정될 것입니다.
            // 또는 여기서 즉시 moveSpeed를 _baseMoveSpeed로 리셋할 수도 있지만,
            // 아래 새 코루틴이 어차피 _baseMoveSpeed 기준으로 속도를 설정하므로,
            // 중복되거나 순서 문제를 야기할 수 있습니다. 코루틴의 finally에서 처리하는 것이 더 안전합니다.
        }

        // 2. 새로운 SpeedUp 코루틴을 시작하고, 그 참조를 저장합니다.
        _activeSpeedUpCoroutine = StartCoroutine(SpeedUpCoroutineInternal(speedupValue, duration));
    }

    // 수정된 SpeedUpCoroutine (이름을 SpeedUpCoroutineInternal로 변경하여 명확히 구분)
    private IEnumerator SpeedUpCoroutineInternal(float extraSpeed, float duration)
    {
        // 이 코루틴 인스턴스가 _activeSpeedUpCoroutine에 할당된 바로 그 인스턴스인지 확인하기 위함입니다.
        // StartCoroutine()이 반환하는 Coroutine 객체를 직접 비교하는 것이 가장 확실합니다.
        Coroutine thisCoroutineInstance = _activeSpeedUpCoroutine;

        try
        {
            // 속도를 기본 속도(_baseMoveSpeed) 기준으로 증가시킵니다.
            moveSpeed = _baseMoveSpeed + extraSpeed;
            Debug.Log($"Speed UP: 속도가 {moveSpeed}로 변경되었습니다. (기본: {_baseMoveSpeed}, 추가: {extraSpeed}). 지속시간: {duration}초");

            yield return new WaitForSeconds(duration);
        }
        finally
        {
            // 이 finally 블록은 코루틴이 정상적으로 끝나거나, StopCoroutine으로 중단될 때 항상 실행됩니다.

            // 현재 _activeSpeedUpCoroutine이 이 코루틴 인스턴스 자신일 경우에만 속도를 원래대로 복구하고 참조를 null로 설정합니다.
            // 이렇게 하면, 이 코루틴이 중단되고 바로 다음 새 코루틴이 시작되었을 때,
            // 이전 코루틴의 finally 블록이 새 코루틴의 속도 설정을 덮어쓰는 것을 방지합니다.
            if (_activeSpeedUpCoroutine == thisCoroutineInstance)
            {
                moveSpeed = _baseMoveSpeed;
                _activeSpeedUpCoroutine = null; // 현재 활성화된 코루틴이 없음을 표시
                Debug.Log($"Speed UP: 효과 종료. 속도가 기본값({moveSpeed})으로 복구되었습니다.");
            }
            // else의 경우는 이 코루틴이 중단되었고, 그 사이에 _activeSpeedUpCoroutine이
            // 이미 새로운 코루틴의 참조로 업데이트된 경우입니다. 이 때는 아무 작업도 하지 않아
            // 새로운 코루틴의 상태를 방해하지 않습니다.
        }
    }
}
   