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
    public float rotationSmoothTime = 0.12f; // Y�� ȸ�� �ε巯�� �ð�

    // X�� ȸ�� (Pitch) ���� ����
    private float _pitch = 0.0f;
    // private float _pitchVelocity; // Pitch�� ���� �����ϹǷ� SmoothDampAngle�� ������� ���� �� ����

    // Y�� ȸ�� (Yaw) ���� ���� - ������ �κ�
    private float _currentCameraYaw;
    private float _yawVelocity;

    // PlayerController�κ��� ���콺 ��Ÿ ���� �ޱ� ���� ����
    private Vector2 _cameraMouseDelta;

    void Start()
    {
        if (!target)
        {
            Debug.LogError("ThirdPersonCamera: Ÿ���� �������� �ʾҽ��ϴ�!");
            enabled = false;
            return;
        }
        _currentDistance = distance;

        // �ʱ� ī�޶� ���� ����
        // _pitch = transform.eulerAngles.x; // �ʿ��ϴٸ� �ʱ� ī�޶� X ������ ����
        _currentCameraYaw = transform.eulerAngles.y; // ���� ī�޶��� Y�� ������ �ʱ�ȭ
    }

    public void SetMouseDelta(Vector2 delta)
    {
        _cameraMouseDelta = delta;
    }

    void LateUpdate()
    {
        if (!target) return;

        // 1. �� ó�� (���콺 ��)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        _currentDistance = Mathf.SmoothDamp(_currentDistance, distance, ref _distanceVelocity, distanceSmoothTime);

        // 2. ī�޶� ���� ���� (Pitch) ���� (���콺 Y)
        // PlayerController�� canLook ���´� PlayerController���� mouseDelta�� ������ ���� �̹� ����Ǿ��� ����
        _pitch -= _cameraMouseDelta.y * mouseSensitivity * Time.deltaTime; // Time.deltaTime�� ���ؼ� ������ ����������
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

        // 3. ī�޶� �¿� ȸ�� (Yaw) ���� (�÷��̾��� Y�� ȸ���� ����) - ������ �κ�
        float targetYaw = target.eulerAngles.y; // �÷��̾��� ���� Y�� ȸ��
        _currentCameraYaw = Mathf.SmoothDampAngle(_currentCameraYaw, targetYaw, ref _yawVelocity, rotationSmoothTime);

        // 4. ���� ī�޶� ȸ�� �� ����
        transform.eulerAngles = new Vector3(_pitch, _currentCameraYaw, 0);

        // 5. ī�޶� ��ġ ���
        Vector3 targetHeadPosition = target.position + Vector3.up * yOffset;
        transform.position = targetHeadPosition - transform.forward * _currentDistance;

        // �Է� ó�� �� _cameraMouseDelta �ʱ�ȭ (���� �����ӿ� ���� ������)
        _cameraMouseDelta = Vector2.zero;
    }
}