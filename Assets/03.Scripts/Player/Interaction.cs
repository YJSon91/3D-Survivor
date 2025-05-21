using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f;
    private float lastCheckTime;
    public float maxCheckDistance; // 플레이어로부터 실제 상호작용 가능한 최대 거리
    public LayerMask layerMask;    // 상호작용 가능한 오브젝트들의 레이어

    public GameObject curInteractGameObject;
    private IInteractable curInteractable;

    public TextMeshProUGUI promptText;
    private Camera _mainCamera; // 메인 카메라 참조

    void Start()
    {
        // 메인 카메라를 찾아서 할당합니다. 카메라 태그가 "MainCamera"인지 확인해주세요.
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("Interaction Script: Main Camera를 찾을 수 없습니다! 카메라의 태그가 'MainCamera'로 설정되어 있는지 확인해주세요.");
            enabled = false; // 스크립트 비활성화
        }
    }

    void Update()
    {
        if (_mainCamera == null) return; // 카메라가 없으면 실행 중지

        if (Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;

           

            // 1. 카메라 화면 중앙에서 Ray를 쏴서 조준점(targetPoint) 찾기
            Ray cameraRay = _mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            Vector3 targetPoint;
            RaycastHit hitInfoForAim;

            // 카메라에서 Ray를 쏴서 무엇을 바라보고 있는지 확인합니다.
            // 이 첫 번째 Ray의 길이는 매우 길게 설정하거나, 적당한 최대 거리를 설정할 수 있습니다.
            // layerMask는 여기서 모든 충돌 가능한 오브젝트를 포함하는 것이 이상적이지만,
            // 우선 제공된 layerMask (상호작용 가능한 오브젝트 레이어)를 사용합니다.
            // (만약 상호작용 불가능한 벽 뒤의 오브젝트와 상호작용 되는 것을 막으려면, 이 첫번째 Raycast의 layerMask를 조절해야 합니다)
            if (Physics.Raycast(cameraRay, out hitInfoForAim, Mathf.Infinity, layerMask)) // 또는 적당히 긴 거리 (예: 200f)
            {
                targetPoint = hitInfoForAim.point; // Ray가 충돌한 지점
            }
            else
            {
                // Ray가 아무것에도 충돌하지 않으면 카메라가 바라보는 방향으로 아주 멀리 떨어진 지점을 목표로 설정
                targetPoint = cameraRay.GetPoint(maxCheckDistance * 10f); // 실제 상호작용은 두 번째 Raycast의 maxCheckDistance로 제한됨
            }

            // 2. 플레이어 위치에서 위에서 찾은 targetPoint를 향해 두 번째 Ray 발사
            Vector3 playerInteractionOrigin = transform.position + Vector3.up * 1.5f; // 플레이어 눈높이(조절 필요)
            Vector3 directionToTarget = (targetPoint - playerInteractionOrigin).normalized; // 방향 벡터

            RaycastHit hitInfoForInteraction; // 실제 상호작용을 위한 RaycastHit 정보

            // Scene 뷰에서 Ray를 시각적으로 확인하려면 아래 줄의 주석을 해제하세요 (Gizmos 설정 필요).
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