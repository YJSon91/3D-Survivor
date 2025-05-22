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
    private float _baseMoveSpeed; // ���� �ӵ��� ������ ����

    [Header("Look")]
    public float lookSensitivity = 1.0f; // �⺻���� 1.0f ������ �����ϰų� �ν����Ϳ��� ����

    // ... (�ٸ� �������� �״��) ...
    // public Transform cameraContainer; // �� ������ ���� 3��Ī���� ���� ��� �� ��
    // public float minXLook;           // �� ī�޶� ��ũ��Ʈ���� ����
    // public float maxXLook;           // �� ī�޶� ��ũ��Ʈ���� ����
    // private float camCurXRot;        // �� ī�޶� ��ũ��Ʈ���� ����
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
    private Coroutine _activeSpeedUpCoroutine;// ���� ���� ���� SpeedUp �ڷ�ƾ�� ������ ����

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        thirdPersonCamera = FindObjectOfType<ThirdPersonCamera>(); // ���� �ϳ��� �ִٸ� �̷��� ã�Ƶ� ��
        _baseMoveSpeed = moveSpeed;                         // Awake ������ moveSpeed�� �⺻ �ӵ��� ����

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
            // �÷��̾��� �¿� ȸ�� (Y��)�� ��� ���
            transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity * Time.deltaTime * 50f, 0); // Time.deltaTime ���� �� �����ϸ�
        }
        // �� ������ mouseDelta�� �ʱ�ȭ (ī�޶� ��ũ��Ʈ������ ���������� �� �� ����)
        // mouseDelta = Vector2.zero; // PlayerController�� �Է��� �ް� ��� ���� �� �ʱ�ȭ
        // �Ǵ� ThirdPersonCamera���� ��ü������ _cameraMouseDelta�� �� ������ �ʱ�ȭ
    }

    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
        if (thirdPersonCamera != null)
        {
            thirdPersonCamera.SetMouseDelta(mouseDelta); // ThirdPersonCamera ��ũ��Ʈ�� mouseDelta ����
        }
    }
    // LateUpdate���� mouseDelta.x�� ����� �÷��̾� ȸ���� ����

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
        velocity.y = rigidbody.velocity.y; // y�ӵ� ����

        rigidbody.velocity = velocity; // �ּ��� ����� ���̱�
    }

    void CameraLook()
    {
        // �÷��̾��� �¿� ȸ�� (Y��)
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
        // 1. �̹� ���� ���� SpeedUp �ڷ�ƾ�� �ִٸ� �ߴ��մϴ�.
        if (_activeSpeedUpCoroutine != null)
        {
            StopCoroutine(_activeSpeedUpCoroutine);
            // _activeSpeedUpCoroutine�� �ش� �ڷ�ƾ�� finally ��Ͽ��� null�� ������ ���Դϴ�.
            // �Ǵ� ���⼭ ��� moveSpeed�� _baseMoveSpeed�� ������ ���� ������,
            // �Ʒ� �� �ڷ�ƾ�� ������ _baseMoveSpeed �������� �ӵ��� �����ϹǷ�,
            // �ߺ��ǰų� ���� ������ �߱��� �� �ֽ��ϴ�. �ڷ�ƾ�� finally���� ó���ϴ� ���� �� �����մϴ�.
        }

        // 2. ���ο� SpeedUp �ڷ�ƾ�� �����ϰ�, �� ������ �����մϴ�.
        _activeSpeedUpCoroutine = StartCoroutine(SpeedUpCoroutineInternal(speedupValue, duration));
    }

    // ������ SpeedUpCoroutine (�̸��� SpeedUpCoroutineInternal�� �����Ͽ� ��Ȯ�� ����)
    private IEnumerator SpeedUpCoroutineInternal(float extraSpeed, float duration)
    {
        // �� �ڷ�ƾ �ν��Ͻ��� _activeSpeedUpCoroutine�� �Ҵ�� �ٷ� �� �ν��Ͻ����� Ȯ���ϱ� �����Դϴ�.
        // StartCoroutine()�� ��ȯ�ϴ� Coroutine ��ü�� ���� ���ϴ� ���� ���� Ȯ���մϴ�.
        Coroutine thisCoroutineInstance = _activeSpeedUpCoroutine;

        try
        {
            // �ӵ��� �⺻ �ӵ�(_baseMoveSpeed) �������� ������ŵ�ϴ�.
            moveSpeed = _baseMoveSpeed + extraSpeed;
            Debug.Log($"Speed UP: �ӵ��� {moveSpeed}�� ����Ǿ����ϴ�. (�⺻: {_baseMoveSpeed}, �߰�: {extraSpeed}). ���ӽð�: {duration}��");

            yield return new WaitForSeconds(duration);
        }
        finally
        {
            // �� finally ����� �ڷ�ƾ�� ���������� �����ų�, StopCoroutine���� �ߴܵ� �� �׻� ����˴ϴ�.

            // ���� _activeSpeedUpCoroutine�� �� �ڷ�ƾ �ν��Ͻ� �ڽ��� ��쿡�� �ӵ��� ������� �����ϰ� ������ null�� �����մϴ�.
            // �̷��� �ϸ�, �� �ڷ�ƾ�� �ߴܵǰ� �ٷ� ���� �� �ڷ�ƾ�� ���۵Ǿ��� ��,
            // ���� �ڷ�ƾ�� finally ����� �� �ڷ�ƾ�� �ӵ� ������ ����� ���� �����մϴ�.
            if (_activeSpeedUpCoroutine == thisCoroutineInstance)
            {
                moveSpeed = _baseMoveSpeed;
                _activeSpeedUpCoroutine = null; // ���� Ȱ��ȭ�� �ڷ�ƾ�� ������ ǥ��
                Debug.Log($"Speed UP: ȿ�� ����. �ӵ��� �⺻��({moveSpeed})���� �����Ǿ����ϴ�.");
            }
            // else�� ���� �� �ڷ�ƾ�� �ߴܵǾ���, �� ���̿� _activeSpeedUpCoroutine��
            // �̹� ���ο� �ڷ�ƾ�� ������ ������Ʈ�� ����Դϴ�. �� ���� �ƹ� �۾��� ���� �ʾ�
            // ���ο� �ڷ�ƾ�� ���¸� �������� �ʽ��ϴ�.
        }
    }
}
   