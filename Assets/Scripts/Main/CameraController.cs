using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://jeonhw.tistory.com/17, https://gps-homepage.tistory.com/16 참고
public class CameraController : MonoBehaviour
{
    public static float zoom { get; private set; } = 10f; // 줌 거리

    [SerializeField] private Transform point;
    [SerializeField] private float rotSpeed = 15f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float zoomMinDist = 1.5f, zoomMaxDist = 30f;

    private float currentDistance; // 현재 카메라 거리
    private float smoothSpeed = 5f; // 부드럽게 이동할 속도

    private LayerMask objLayer; // Player 레이어를 제외한 모든 레이어
    private Vector2 m_Input;

    void Start()
    {
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        objLayer = ~(1 << playerLayerIndex); // Player 레이어를 제외한 모든 레이어
        currentDistance = zoom;
    }

    void Rotate()
    {
        if (!Input.GetKey(KeyCode.LeftAlt) && Time.timeScale > 0)
        {
            m_Input.x = Input.GetAxis("Mouse X");
            m_Input.y = -Input.GetAxis("Mouse Y");

            if (m_Input.magnitude != 0)
            {
                Quaternion q = point.rotation;
                float x = q.eulerAngles.x + m_Input.y * rotSpeed;
                x = x > 180 ? x - 360 : x;
                x = Mathf.Clamp(x, -80, 80);
                q.eulerAngles = new Vector3(x, q.eulerAngles.y + m_Input.x * rotSpeed, q.eulerAngles.z);
                point.rotation = q;
            }
        }
    }

    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        zoom -= scroll;
        zoom = Mathf.Clamp(zoom, zoomMinDist, zoomMaxDist);
    }

    void CameraUpdate()
    {
        float targetDistance = zoom;

        if (Physics.Raycast(point.position, -point.forward, out var hit, zoom, objLayer)) {
            float dis = Vector3.Distance(hit.point, point.position) - 1f;
            targetDistance = Mathf.Clamp(dis, zoomMinDist, zoom);
            if (targetDistance < currentDistance) // 장애물로 인해 카메라를 땡겨야 한다면 즉시 땡김
                currentDistance = targetDistance;
        }

        // 거리를 부드럽게 보간
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.fixedDeltaTime * smoothSpeed);

        Camera.main.transform.position = point.position - point.forward * currentDistance;
        Camera.main.transform.LookAt(point.transform);
    }

    public void LateUpdate()
    {
        Rotate();
        Zoom();
    }

    void FixedUpdate()
    {
        CameraUpdate();
    }
}
