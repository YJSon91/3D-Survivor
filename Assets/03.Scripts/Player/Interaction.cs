using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f;
    private float lastCheckTime;
    public float maxCheckDistance; // �÷��̾�κ��� ���� ��ȣ�ۿ� ������ �ִ� �Ÿ�
    public LayerMask layerMask;    // ��ȣ�ۿ� ������ ������Ʈ���� ���̾�

    public GameObject curInteractGameObject;
    private IInteractable curInteractable;

    public TextMeshProUGUI promptText;
    private Camera _mainCamera; // ���� ī�޶� ����

    void Start()
    {
        // ���� ī�޶� ã�Ƽ� �Ҵ��մϴ�. ī�޶� �±װ� "MainCamera"���� Ȯ�����ּ���.
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("Interaction Script: Main Camera�� ã�� �� �����ϴ�! ī�޶��� �±װ� 'MainCamera'�� �����Ǿ� �ִ��� Ȯ�����ּ���.");
            enabled = false; // ��ũ��Ʈ ��Ȱ��ȭ
        }
    }

    void Update()
    {
        if (_mainCamera == null) return; // ī�޶� ������ ���� ����

        if (Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

           

            // 1. ī�޶� ȭ�� �߾ӿ��� Ray�� ���� ������(targetPoint) ã��
            Ray cameraRay = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            Vector3 targetPoint;
            RaycastHit hitInfoForAim;

            // ī�޶󿡼� Ray�� ���� ������ �ٶ󺸰� �ִ��� Ȯ���մϴ�.
            // �� ù ��° Ray�� ���̴� �ſ� ��� �����ϰų�, ������ �ִ� �Ÿ��� ������ �� �ֽ��ϴ�.
            // layerMask�� ���⼭ ��� �浹 ������ ������Ʈ�� �����ϴ� ���� �̻���������,
            // �켱 ������ layerMask (��ȣ�ۿ� ������ ������Ʈ ���̾�)�� ����մϴ�.
            // (���� ��ȣ�ۿ� �Ұ����� �� ���� ������Ʈ�� ��ȣ�ۿ� �Ǵ� ���� ��������, �� ù��° Raycast�� layerMask�� �����ؾ� �մϴ�)
            if (Physics.Raycast(cameraRay, out hitInfoForAim, Mathf.Infinity, layerMask)) // �Ǵ� ������ �� �Ÿ� (��: 200f)
            {
                targetPoint = hitInfoForAim.point; // Ray�� �浹�� ����
            }
            else
            {
                // Ray�� �ƹ��Ϳ��� �浹���� ������ ī�޶� �ٶ󺸴� �������� ���� �ָ� ������ ������ ��ǥ�� ����
                targetPoint = cameraRay.GetPoint(maxCheckDistance * 10f); // ���� ��ȣ�ۿ��� �� ��° Raycast�� maxCheckDistance�� ���ѵ�
            }

            // 2. �÷��̾� ��ġ���� ������ ã�� targetPoint�� ���� �� ��° Ray �߻�
            Vector3 playerInteractionOrigin = transform.position + Vector3.up * 1.5f; // �÷��̾� ������(���� �ʿ�)
            Vector3 directionToTarget = (targetPoint - playerInteractionOrigin).normalized; // ���� ����

            RaycastHit hitInfoForInteraction; // ���� ��ȣ�ۿ��� ���� RaycastHit ����

            // Scene �信�� Ray�� �ð������� Ȯ���Ϸ��� �Ʒ� ���� �ּ��� �����ϼ��� (Gizmos ���� �ʿ�).
            // Debug.DrawRay(playerInteractionOrigin, directionToTarget * maxCheckDistance, Color.green, checkRate);

            if (Physics.Raycast(playerInteractionOrigin, directionToTarget, out hitInfoForInteraction, maxCheckDistance, layerMask))
            {
                if (hitInfoForInteraction.collider.gameObject != curInteractGameObject)
                {
                    curInteractGameObject = hitInfoForInteraction.collider.gameObject;
                    curInteractable = hitInfoForInteraction.collider.GetComponent<IInteractable>();
                    SetPromptText();
                }
            }
            else
            {
                curInteractGameObject = null;
                curInteractable = null;
                if (promptText != null)
                {
                    promptText.gameObject.SetActive(false);
                }
            }
            
        }
    }

    private void SetPromptText()
    {
        if (promptText == null) return;

        if (curInteractable != null)
        {
            promptText.gameObject.SetActive(true);
            promptText.text = curInteractable.GetInteractPrompt();
        }
        else
        {
            promptText.gameObject.SetActive(false);
        }
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && curInteractable != null)
        {
            curInteractable.OnInteract();
            curInteractGameObject = null;
            curInteractable = null;
            if (promptText != null)
            {
                promptText.gameObject.SetActive(false);
            }
        }
    }
}