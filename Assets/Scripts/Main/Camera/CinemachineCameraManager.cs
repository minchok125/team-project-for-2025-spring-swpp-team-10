using UnityEngine;
using Cinemachine;
using TMPro;

[RequireComponent(typeof(CMFreeLookSetting))]
public class CinemachineCameraManager : RuntimeSingleton<CinemachineCameraManager>
{
    [Header("CM")]
    [SerializeField] private CinemachineFreeLook hamsterCam;
    [SerializeField] private CinemachineFreeLook hamsterWireCam;
    [SerializeField] private CinemachineFreeLook ballCam;
    [SerializeField] private CinemachineFreeLook ballWireCam1, ballWireCam2;

    [Header("Value")]
    [Tooltip("카메라 줌 최소")]
    [SerializeField] private float zoomMin = 0.4f;
    [Tooltip("카메라 줌 최대")]
    [SerializeField] private float zoomMax = 2.5f;
    [Tooltip("게임 시작 시 설정되는 카메라 줌")]
    [SerializeField] private float zoomDefault = 1f;
    [Tooltip("마우스 휠 감도")]
    [SerializeField] private float zoomSensitivity = 0.4f;
    [Tooltip("마우스 수직 움직임 민감도 (default : 1)")]
    [SerializeField, Range(0.1f, 4f)] private float mouseVerticalSensitivity = 1f;
    [Tooltip("마우스 수평 움직임 민감도 (default : 1)")]
    [SerializeField, Range(0.1f, 4f)] private float mouseHorizontalSensitivity = 1f;


    [Header("Debug")]
    [SerializeField] private TextMeshProUGUI txt;

    private const float _defaultHorizontalSensitivity = 1f;
    private const float _defaultVerticalSensitivity = 1f;
    private const float _defaultZoomSensitivity = 0.4f;

    private CinemachineBrain brain;
    private bool isBallWireCam; // ballWireCam1/2 라면 true
    private float targetZoom; // 목표 줌 거리
    private float actualZoom; // 현재 줌 거리 (targetZoom으로 수렴)
    private float mouseDefaultVerticalSensitivity = 8f;
    private float mouseDefaultHorizontalSensitivity = 1000f;

    private PlayerWireController player;

    private float camXAxis, camYAxis;
    private bool isCamChanged;


    private struct CamRig {
        public Vector2[] rig;
        public CamRig(Vector2 top, Vector2 middle, Vector2 bottom) {
            rig = new Vector2[3];
            rig[0] = top;
            rig[1] = middle;
            rig[2] = bottom;
        }
    }

    private CamRig hamsterRig, hamsterWireRig, ballRig, ballWireRig;

    public float MouseHorizontalSensitivity => mouseHorizontalSensitivity;
    public float MouseVerticalSensitivity => mouseVerticalSensitivity;
    public float ZoomSensitivity => zoomSensitivity;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        brain = Camera.main.GetComponent<CinemachineBrain>();
        isBallWireCam = false;
        targetZoom = actualZoom = zoomDefault;

        player = GameObject.Find("Player").GetComponent<PlayerWireController>();

        hamsterRig = SetCamRig(hamsterCam);
        hamsterWireRig = SetCamRig(hamsterWireCam);
        ballRig = SetCamRig(ballCam);
        ballWireRig = SetCamRig(ballWireCam1);
    }

    /// <summary>
    /// freelook 카메라의 Toprig, MiddleRig, BottomRig의 height, radius 정보 저장
    /// </summary>
    CamRig SetCamRig(CinemachineFreeLook curCam)
    {
        return new CamRig(new Vector2(curCam.m_Orbits[0].m_Height, curCam.m_Orbits[0].m_Radius),
                          new Vector2(curCam.m_Orbits[1].m_Height, curCam.m_Orbits[1].m_Radius),
                          new Vector2(curCam.m_Orbits[2].m_Height, curCam.m_Orbits[2].m_Radius));
    }


    void Update()
    {
        Zoom();
        FreeLookCamSetting();
    }

    public void SetHorizontalSensitivity(float value)
    {
        mouseHorizontalSensitivity = value;
    }

    public void SetVerticalSensitivity(float value)
    {
        mouseVerticalSensitivity = value;
    }

    public void SetZoomSensitivity(float value)
    {
        zoomSensitivity = value;
    }

    public void ResetCameraSettings()
    {
        mouseHorizontalSensitivity = _defaultHorizontalSensitivity;
        mouseVerticalSensitivity = _defaultVerticalSensitivity;
        zoomSensitivity = _defaultZoomSensitivity;
    }


    /// <summary>
    /// 마우스 휠로 줌 거리 조절
    /// </summary>
    void Zoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
        
        targetZoom -= scroll;
        targetZoom = Mathf.Clamp(targetZoom, zoomMin, zoomMax);

        actualZoom = Mathf.Lerp(actualZoom, targetZoom, 10f * Time.deltaTime);

        SetHeightRadius();
    }


    /// <summary>
    /// zoom 거리에 따라서 모든 freelook 카메라 Rig의 height, radius 갱신
    /// </summary>
    void SetHeightRadius()
    {
        SetHeightRadius(hamsterCam, hamsterRig);
        SetHeightRadius(hamsterWireCam, hamsterWireRig);
        SetHeightRadius(ballCam, ballRig);
        SetHeightRadius(ballWireCam1, ballWireRig);
        SetHeightRadius(ballWireCam2, ballWireRig);
    }

    /// <summary>
    /// 주어진 FreeLook 카메라의 Rig 설정(height, radius)을 현재 줌 비율(actualZoom)에 따라 갱신합니다.
    /// </summary>
    /// <param name="cam">설정을 적용할 CinemachineFreeLook 카메라</param>
    /// <param name="camRig">초기 height, radius 값을 저장한 CamRig 구조체</param>
    void SetHeightRadius(CinemachineFreeLook cam, CamRig camRig)
    {
        for (int i = 0; i < 3; i++) 
        {
            cam.m_Orbits[i].m_Height = camRig.rig[i].x * actualZoom;
            cam.m_Orbits[i].m_Radius = camRig.rig[i].y * actualZoom;
        }
    }



    /// <summary>
    /// 플레이어 상태에 따라 적절한 FreeLook 카메라를 활성화하고,
    /// 각 카메라의 마우스 감도 설정 및 카메라 전환 시 자연스럽게 이어지도록 보정합니다.
    /// </summary>
    void FreeLookCamSetting()
    {
        // 공중 와이어 액션 때 BallWireCam 활성화
        if (PlayerManager.Instance.onWire && !PlayerManager.Instance.isGround)
            isBallWireCam = true;
        // 와이어를 아예 놓아야 BallNotWireCam 활성화
        else if (!PlayerManager.Instance.onWire)
            isBallWireCam = false;


        // 활성화할 카메라
        if (PlayerManager.Instance.isBall) 
        {
            if (isBallWireCam && player.isHitPoint1) 
            {
                FreeLookCamChange(ballWireCam1);
            }
            else if (isBallWireCam && !player.isHitPoint1) 
            {
                FreeLookCamChange(ballWireCam2);
            }
            else 
            {
                FreeLookCamChange(ballCam);
            }
        }
        else 
        {
            if (PlayerManager.Instance.onWire) 
            {
                FreeLookCamChange(hamsterWireCam);
            }
            else 
            {
                FreeLookCamChange(hamsterCam);
            }
        }


        // 카메라 민감도 설정
        hamsterCam.m_YAxis.m_MaxSpeed = hamsterWireCam.m_YAxis.m_MaxSpeed = ballCam.m_YAxis.m_MaxSpeed = 
        ballWireCam1.m_YAxis.m_MaxSpeed = ballWireCam2.m_YAxis.m_MaxSpeed
            = mouseDefaultVerticalSensitivity * mouseVerticalSensitivity;

        hamsterCam.m_XAxis.m_MaxSpeed = hamsterWireCam.m_XAxis.m_MaxSpeed = ballCam.m_XAxis.m_MaxSpeed = 
        ballWireCam1.m_XAxis.m_MaxSpeed = ballWireCam2.m_XAxis.m_MaxSpeed
            = mouseDefaultHorizontalSensitivity * mouseHorizontalSensitivity;

        // 카메라가 뚝 끊기니까 그거 해결해 보려고 한 것
        ICinemachineCamera activeCam = brain.ActiveVirtualCamera;

        // string s = "";
        // if (activeCam is CinemachineFreeLook freeLookCam) 
        // {
        //     if (!isCamChanged 
        //         && (Mathf.Abs(Mathf.DeltaAngle(freeLookCam.m_XAxis.Value, camXAxis)) > 20f
        //             || Mathf.Abs(freeLookCam.m_YAxis.Value - camYAxis) > 0.15f)) 
        //     {
        //         Debug.Log($"({freeLookCam.m_XAxis.Value:F2},{freeLookCam.m_YAxis.Value:F2}) => ({camXAxis:F2},{camYAxis:F2})");
        //         s = $"({freeLookCam.m_XAxis.Value:F2},{freeLookCam.m_YAxis.Value:F2}) => ({camXAxis:F2},{camYAxis:F2})";
        //         freeLookCam.m_XAxis.Value = camXAxis;
        //         freeLookCam.m_YAxis.Value = camYAxis;
        //     }

        //     camXAxis = freeLookCam.m_XAxis.Value;
        //     camYAxis = freeLookCam.m_YAxis.Value;
        // }

        // if (txt != null) 
        // {
        //     txt.text = $"ham:({hamsterCam.m_XAxis.Value:F2},{hamsterCam.m_YAxis.Value:F2})\n"
        //                + $"ball:({ballCam.m_XAxis.Value:F2},{ballCam.m_YAxis.Value:F2})\n"
        //                + $"ballWire:({ballWireCam1.m_XAxis.Value:F2},{ballWireCam1.m_YAxis.Value:F2})";
        //     txt.text += "\n" + s;
        // }
    }


    /// <summary>
    /// 전달된 카메라(curCam)를 현재 활성 카메라로 전환하며,
    /// 이미 활성화된 카메라일 경우 전환하지 않습니다.
    /// </summary>
    /// <param name="curCam">우선순위를 최상위로 설정할 CinemachineFreeLook 카메라</param>
    void FreeLookCamChange(CinemachineFreeLook curCam)
    {
        isCamChanged = true;
        ICinemachineCamera activeCam = brain?.ActiveVirtualCamera;
        if (activeCam?.VirtualCameraGameObject == curCam.VirtualCameraGameObject) 
        {
            isCamChanged = false;
            return;
        }

        curCam.MoveToTopOfPrioritySubqueue();
    }
}
