using UnityEngine;
using Cinemachine;
using TMPro;

public class CinemachineCameraManager : MonoBehaviour
{
    [Header("CM")]
    [SerializeField] private CinemachineFreeLook hamsterCam;
    [SerializeField] private CinemachineFreeLook ballCam;
    [SerializeField] private CinemachineFreeLook ballWireCam;

    [Header("Value")]
    [Tooltip("카메라 줌 최소")]
    [SerializeField] private float zoomMin = 0.4f;
    [Tooltip("카메라 줌 최대")]
    [SerializeField] private float zoomMax = 2.5f;
    [Tooltip("게임 시작 시 설정되는 카메라 줌")]
    [SerializeField] private float zoomDefault = 1f;
    [Tooltip("마우스 휠 감도")]
    [SerializeField] private float zoomSpeed = 0.4f;
    [Tooltip("마우스 수직 움직임 민감도 (default : 1)")]
    [SerializeField, Range(0.1f, 4f)] private float mouseVerticalSensitivity = 1f;
    [Tooltip("마우스 수평 움직임 민감도 (default : 1)")]
    [SerializeField, Range(0.1f, 4f)] private float mouseHorizontalSensitivity = 1f;


    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI txt;

    private CinemachineBrain brain;
    private bool isBallWireCam;
    private float targetZoom; // 목표 줌 거리
    private float actualZoom; // 현재 줌 거리
    private float mouseDefaultVerticalSensitivity = 8f;
    private float mouseDefaultHorizontalSensitivity = 1000f;


    private struct CamRig {
        public Vector2[] rig;
        public CamRig(Vector2 top, Vector2 middle, Vector2 bottom) {
            rig = new Vector2[3];
            rig[0] = top;
            rig[1] = middle;
            rig[2] = bottom;
        }
    }

    private CamRig hamsterRig, ballRig, ballWireRig;


    void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        isBallWireCam = false;
        targetZoom = actualZoom = zoomDefault;

        hamsterRig = new CamRig(new Vector2(hamsterCam.m_Orbits[0].m_Height, hamsterCam.m_Orbits[0].m_Radius),
                                new Vector2(hamsterCam.m_Orbits[1].m_Height, hamsterCam.m_Orbits[1].m_Radius),
                                new Vector2(hamsterCam.m_Orbits[2].m_Height, hamsterCam.m_Orbits[2].m_Radius));

        ballRig = new CamRig(new Vector2(ballCam.m_Orbits[0].m_Height, ballCam.m_Orbits[0].m_Radius),
                            new Vector2(ballCam.m_Orbits[1].m_Height, ballCam.m_Orbits[1].m_Radius),
                            new Vector2(ballCam.m_Orbits[2].m_Height, ballCam.m_Orbits[2].m_Radius));

        ballWireRig = new CamRig(new Vector2(ballWireCam.m_Orbits[0].m_Height, ballWireCam.m_Orbits[0].m_Radius),
                                 new Vector2(ballWireCam.m_Orbits[1].m_Height, ballWireCam.m_Orbits[1].m_Radius),
                                 new Vector2(ballWireCam.m_Orbits[2].m_Height, ballWireCam.m_Orbits[2].m_Radius));
    }



    float x, y;
    bool changed;
    void Update()
    {
        Zoom();
        FreeLookCamSetting();
    }


    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        
        targetZoom -= scroll;
        targetZoom = Mathf.Clamp(targetZoom, zoomMin, zoomMax);

        actualZoom = Mathf.Lerp(actualZoom, targetZoom, 10f * Time.deltaTime);

        SetHeightRadius();
    }

    void SetHeightRadius()
    {
        SetHeightRadius(hamsterCam, hamsterRig);
        SetHeightRadius(ballCam, ballRig);
        SetHeightRadius(ballWireCam, ballWireRig);
    }

    void SetHeightRadius(CinemachineFreeLook cam, CamRig camRig)
    {
        for (int i = 0; i < 3; i++) {
            cam.m_Orbits[i].m_Height = camRig.rig[i].x * actualZoom;
            cam.m_Orbits[i].m_Radius = camRig.rig[i].y * actualZoom;
        }
    }

    void FreeLookCamSetting()
    {
        // 공중 와이어 액션 때 BallWireCam 활성화
        if (PlayerManager.instance.onWire && !GroundCheck.isGround)
            isBallWireCam = true;
        // 와이어를 아예 놓아야 BallNotWireCam 활성화
        else if (!PlayerManager.instance.onWire)
            isBallWireCam = false;


        // 활성화할 카메라
        if (PlayerManager.instance.isBall) {
            if (isBallWireCam) {
                FreeLookCamChange(ballWireCam);
            }
            else {
                FreeLookCamChange(ballCam);
            }
        }
        else {
            FreeLookCamChange(hamsterCam);
        }


        // 카메라 민감도 설정
        hamsterCam.m_YAxis.m_MaxSpeed = ballCam.m_YAxis.m_MaxSpeed = ballWireCam.m_YAxis.m_MaxSpeed
            = mouseDefaultVerticalSensitivity * mouseVerticalSensitivity;

        hamsterCam.m_XAxis.m_MaxSpeed = ballCam.m_XAxis.m_MaxSpeed = ballWireCam.m_XAxis.m_MaxSpeed
            = mouseDefaultHorizontalSensitivity * mouseHorizontalSensitivity;

        // 카메라가 뚝 끊기니까 그거 해결해 보려고 한 것
        ICinemachineCamera activeCam = brain.ActiveVirtualCamera;

        string s = "";
        if (activeCam is CinemachineFreeLook freeLookCam) {
            if (!changed && 
                (Mathf.Abs(Mathf.DeltaAngle(freeLookCam.m_XAxis.Value, x)) > 20f || Mathf.Abs(freeLookCam.m_YAxis.Value - y) > 0.15f)) {
                Debug.Log($"({freeLookCam.m_XAxis.Value:F2},{freeLookCam.m_YAxis.Value:F2}) => ({x:F2},{y:F2})");
                s = $"({freeLookCam.m_XAxis.Value:F2},{freeLookCam.m_YAxis.Value:F2}) => ({x:F2},{y:F2})";
                freeLookCam.m_XAxis.Value = x;
                freeLookCam.m_YAxis.Value = y;
            }

            if (activeCam.VirtualCameraGameObject == ballWireCam.VirtualCameraGameObject) {
                ballWireCam.m_YAxisRecentering.m_enabled = true;
            }
            else {
                ballWireCam.m_YAxisRecentering.m_enabled = false;
            }

            x = freeLookCam.m_XAxis.Value;
            y = freeLookCam.m_YAxis.Value;
        }

        txt.text = $"ham:({hamsterCam.m_XAxis.Value:F2},{hamsterCam.m_YAxis.Value:F2})\nball:({ballCam.m_XAxis.Value:F2},{ballCam.m_YAxis.Value:F2})\nballWire:({ballWireCam.m_XAxis.Value:F2},{ballWireCam.m_YAxis.Value:F2})\n{s}";
    
    }

    void FixedUpdate()
    {
        ICinemachineCamera activeCam = brain.ActiveVirtualCamera;

        if (activeCam is CinemachineFreeLook freeLookCam) {
            if (!changed && 
                (Mathf.Abs(Mathf.DeltaAngle(freeLookCam.m_XAxis.Value, x)) > 20f || Mathf.Abs(freeLookCam.m_YAxis.Value - y) > 0.15f)) {
                Debug.Log($"({freeLookCam.m_XAxis.Value},{freeLookCam.m_YAxis.Value}) => ({x},{y})");
                freeLookCam.m_XAxis.Value = x;
                freeLookCam.m_YAxis.Value = y;
            }

            x = freeLookCam.m_XAxis.Value;
            y = freeLookCam.m_YAxis.Value;
        }

        txt.text = $"ham:({hamsterCam.m_XAxis.Value:F2},{hamsterCam.m_XAxis.Value:F2})\nball:({ballCam.m_XAxis.Value:F2},{ballCam.m_XAxis.Value:F2})\nballWire:({ballWireCam.m_XAxis.Value:F2},{ballWireCam.m_XAxis.Value:F2})";
    }

    void FreeLookCamChange(CinemachineFreeLook curCam)
    {
        changed = true;
        ICinemachineCamera activeCam = brain.ActiveVirtualCamera;
        if (activeCam.VirtualCameraGameObject == curCam.VirtualCameraGameObject) {
            changed = false;
            return;
        }

        Debug.Log("Changed");
        curCam.MoveToTopOfPrioritySubqueue();
    }
}
