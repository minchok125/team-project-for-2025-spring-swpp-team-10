using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://jeonhw.tistory.com/17, https://gps-homepage.tistory.com/16 참고
public class CameraController : MonoBehaviour
{
    public static float zoom { get {return currentDistance;} }
    

    [SerializeField] private Transform point;
    [SerializeField] private float rotSpeed = 15f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float zoomMinDist = 1.5f, zoomMaxDist = 30f;

    private static float currentDistance; // 현재 카메라 거리
    private float zoomDist = 12f; // 줌 거리
    private float smoothSpeed = 2f; // 부드럽게 이동할 속도

    private LayerMask objLayer; // Player 레이어를 제외한 모든 레이어
    private Vector2 m_Input;

    void Start()
    {
        int playerLayerIndex = LayerMask.NameToLayer("Player");
        objLayer = ~(1 << playerLayerIndex); // Player 레이어를 제외한 모든 레이어
        currentDistance = zoomDist;
    }

    void LateUpdate()
    {
        Rotate();
        Zoom();
    }

    void FixedUpdate()
    {
        CameraUpdate();
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

        zoomDist -= scroll;
        zoomDist = Mathf.Clamp(zoomDist, zoomMinDist, zoomMaxDist);
    }


    bool isBallWire = false;
    void CameraUpdate()
    {
        float targetDistance = zoomDist;
        float _smoothSpeed = smoothSpeed;

        if (PlayerManager.instance.onWire && PlayerManager.instance.isBall) {
            isBallWire = true;

            targetDistance = Mathf.Max(16, targetDistance);
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 70, 2 * Time.fixedDeltaTime);
        }
        else {
            if (isBallWire && zoomDist < 15)
                zoomDist = Mathf.Lerp(zoomDist, 16, 0.2f);
            isBallWire = false;

            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, 60, 0.2f * Time.fixedDeltaTime);
        }

        if (Physics.Raycast(point.position, -point.forward, out var hit, targetDistance, objLayer)) {
            float dis = Vector3.Distance(hit.point, point.position) - 1f;
            targetDistance = Mathf.Clamp(dis, zoomMinDist, targetDistance);
            if (targetDistance < currentDistance) // 장애물로 인해 카메라를 땡겨야 한다면 즉시 땡김
                currentDistance = targetDistance;
        }

        // 거리를 부드럽게 보간
        currentDistance = Mathf.Lerp(currentDistance, targetDistance, Time.fixedDeltaTime * _smoothSpeed);

        Camera.main.transform.position = point.position - point.forward * currentDistance;
        Camera.main.transform.LookAt(point.transform);
    }
}
