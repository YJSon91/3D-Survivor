// ThirdPersonCamera.cs

using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Distancing")]
    public float distance = 5.0f;
    public float minDistance = 1.0f;
    public float maxDistance = 15.0f;
    public float zoomSpeed = 5.0f;
    public float distanceSmoothTime = 0.2f;
    private float _currentDistance;
    private float _distanceVelocity;

    [Header("Rotation & Vertical Offset")]
    public float yOffset = 1.0f;
    public float mouseSensitivity = 2.0f;
    public float pitchMin = -40.0f;
    public float pitchMax = 80.0f;
    public float rotationSmoothTime = 0.12f; // Y축 회전 부드러움 시간

    // X축 회전 (Pitch) 관련 변수
    private float _pitch = 0.0f;
    // private float _pitchVelocity; // Pitch는 직접 제어하므로 SmoothDampAngle을 사용하지 않을 수 있음

    // Y축 회전 (Yaw) 관련 변수 - 수정된 부분
    private float _currentCameraYaw;
    private float _yawVelocity;

    // PlayerController로부터 마우스 델타 값을 받기 위한 변수
    private Vector2 _cameraMouseDelta;

    void Start()
    {
        if (!target)
        {
            Debug.LogError("ThirdPersonCamera: 타겟이 설정되지 않았습니다!");
            enabled = false;
            return;
        }
        _currentDistance = distance;

        // 초기 카메라 각도 설정
        // _pitch = transform.eulerAngles.x; // 필요하다면 초기 카메라 X 각도로 설정
        _currentCameraYaw = transform.eulerAngles.y; // 현재 카메라의 Y축 각도로 초기화
    }

    public void SetMouseDelta(Vector2 delta)
    {
        _cameraMouseDelta = delta;
    }

    void LateUpdate()
    {
        if (!target) return;

        // 1. 줌 처리 (마우스 휠)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        _currentDistance = Mathf.SmoothDamp(_currentDistance, distance, ref _distanceVelocity, distanceSmoothTime);

        // 2. 카메라 상하 각도 (Pitch) 조절 (마우스 Y)
        // PlayerController의 canLook 상태는 PlayerController에서 mouseDelta를 보내기 전에 이미 고려되었을 것임
        _pitch -= _cameraMouseDelta.y * mouseSensitivity * Time.deltaTime; // Time.deltaTime을 곱해서 프레임 독립적으로
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

        // 3. 카메라 좌우 회전 (Yaw) 조절 (플레이어의 Y축 회전을 따라감) - 수정된 부분
        float targetYaw = target.eulerAngles.y; // 플레이어의 현재 Y축 회전
        _currentCameraYaw = Mathf.SmoothDampAngle(_currentCameraYaw, targetYaw, ref _yawVelocity, rotationSmoothTime);

        // 4. 최종 카메라 회전 값 설정
        transform.eulerAngles = new Vector3(_pitch, _currentCameraYaw, 0);

        // 5. 카메라 위치 계산
        Vector3 targetHeadPosition = target.position + Vector3.up * yOffset;
        transform.position = targetHeadPosition - transform.forward * _currentDistance;

        // 입력 처리 후 _cameraMouseDelta 초기화 (다음 프레임에 영향 없도록)
        _cameraMouseDelta = Vector2.zero;
    }
}