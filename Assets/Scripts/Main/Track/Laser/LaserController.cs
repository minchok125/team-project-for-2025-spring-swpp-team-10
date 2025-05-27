using System.Diagnostics;
using Hampossible.Utils;
using UnityEngine;
using VolumetricLines;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(VolumetricLineBehavior))]
public class LaserController : MonoBehaviour
{
    /// <summary>
    /// 레이저의 검출 기능 활성화 여부
    /// </summary>
    public bool isLaserActive = true;

    public enum LaserType { LightningShock, PlatformDisappear, None }
    [Tooltip("플레이어와 레이저가 닿았을 때 이벤트 타입\n"
           + "LightningShock : 3초간 플레이어 움직임 정지\n"
           + "PlatformDisappear : 정해진 플랫폼들이 일정 시간 동안 사라짐")]
    [SerializeField] private LaserType _laserType;
    [SerializeField] private LaserPlatformDisappearManager _platformDisappearMgr;


    [Header("Optimization")]
    [Tooltip("씬뷰에서 선을 표시할지 여부")]
    [SerializeField] private bool visulaizeLine = true;
    [Header("*씬뷰의 빨간선 조절*")]
    [Tooltip("레이저 중심점과 플레이어 사이의 거리가 일정 거리 이하일 때 감지 시작")]
    [SerializeField] public float detectPointDist = 300;
    [Tooltip("플레이어가 멀리 있을 때 레이저의 길이를 특정 길이로 설정할지 여부")]
    [SerializeField] private bool setDistWhenPlayerFar = false;
    [Tooltip("플레이어가 멀리 있을 때 설정될 레이저의 길이")]
    [SerializeField] private float distWhenPlayerFar = 0f;
    // LaserControllerEditor에서 접근하기 위해 public 설정
    [Header("*씬뷰의 초록선 조절*")]
    [Tooltip("레이저 선분과 플레이어 사이의 거리가 일정 거리 이하일 때 감지 시작")]
    [SerializeField] public float detectLineDist = 5;

    [Tooltip("레이저가 나가는 최대 거리")]
    [SerializeField] public float laserMaxDist = 1000f;

    [Tooltip("레이저 자체가 움직이거나, 레이저가 쏘는 지점의 물체가 "
           + "움직여서 레이저의 endPoint가 지속적으로 바뀌는 경우 true")]
    [SerializeField] private bool isEndPointDynamic;

    [Tooltip("레이저 자체가 움직이는 경우 true")]
    [SerializeField] private bool isLaserMoving;

    [Tooltip("연동할 레이저포가 있다면 연결. 없으면 null로 유지")]
    [SerializeField] private LaserShootController laserShoot;


    private const float OFFSET_FROM_HIT = 1.2f;
    private Vector3 _playerPosition;
    private Vector3 _laserOrigin; // 레이저 시작 지점 월드좌표
    private Vector3 _laserMaxPoint; // 레이저 길이가 laserMaxDist일 때의 끝 지점 월드좌표
    private Vector3 _laserCenter; // 레이저 길이가 laserMaxDist일 때의 중심 지점 월드좌표


    private ParticleSystem _laserHitParticle;
    private VolumetricLineBehavior _laserLineBehavior;
    private Renderer _myRenderer;
    private Color _laserColor;
    private RaycastHit _hit;
    private Transform _player;
    private LayerMask _layerMask;




    private void Start()
    {
        int laserDetectLayer = 1 << LayerMask.NameToLayer("LaserDetect");
        _layerMask = ~laserDetectLayer;

        _laserHitParticle = GetComponentInChildren<ParticleSystem>();
        ParticleStop();

        _laserLineBehavior = GetComponent<VolumetricLineBehavior>();
        _laserLineBehavior.StartPos = Vector3.zero;

        _myRenderer = GetComponent<Renderer>();
        _laserColor = _myRenderer.material.color;
        _player = PlayerManager.Instance.transform;

        if (!isLaserMoving)
        {
            _laserOrigin = transform.position;
            _laserMaxPoint = transform.position + transform.forward * laserMaxDist;
            _laserCenter = (_laserOrigin + _laserMaxPoint) / 2f;
        }

        // 플레이어의 renderQueue가 3000이므로 (5/13 기준) 플레이어보다 높게 설정
        GetComponent<Renderer>().material.renderQueue = 3100;

        ShootLaser();
    }


    private void FixedUpdate()
    {
        bool playerNear = IsPlayerNearLaserOrigin();
        // 레이이저 비활성화 || 가벼운 Point-Point 검사
        if (!isLaserActive || !playerNear)
        {
            ParticleStop();
            if (!playerNear && setDistWhenPlayerFar)
                _laserLineBehavior.EndPos = Vector3.forward * distWhenPlayerFar;
            return;
        }

        // 1. 끝점이 지속적으로 바뀌는 환경이라면, 최적화 불가능
        // 2. 정적인 레이저라면, Line-Point 검사로 최적화
        //    플레이어와 충분히 가까울 때만 RayCast 실행
        if (isEndPointDynamic || IsPlayerNearLaserLine())
        {
            ShootLaser();
        }
    }


    // 변수가 잘못 설정되지 않았는지 검사
    [Conditional("UNITY_EDITOR")]
    private void ValidateCheck()
    {
        if (!isLaserMoving &&
                (TryGetComponent(out MovingPlatformController mov)
                || TryGetComponent(out PingPongMovingPlatformController ppMov))
            )
        {
            HLogger.General.Warning("레이저에 Moving 스크립트가 부착되어 있으나, isLaserMoving이 false입니다.", this);
        }

        if (_laserType == LaserType.PlatformDisappear && _platformDisappearMgr == null)
        {
            _platformDisappearMgr = GetComponentInParent<LaserPlatformDisappearManager>();
            if (_platformDisappearMgr == null)
                HLogger.General.Error("레이저에 platformDisappearMgr를 설정해 주거나\n"
                    + "부모 오브젝트 위치에 LaserPlatformDisappearManager를 추가해 주세요", this);
        }
    }

    // 레이저를 발사해 레이저 끝점 검출, 플레이어 검출 이벤트
    private void ShootLaser()
    {
        if (Physics.Raycast(transform.position, transform.forward, out _hit, laserMaxDist, _layerMask, QueryTriggerInteraction.Ignore))
        {
            _laserLineBehavior.EndPos
                = Vector3.forward * ((_hit.point - transform.position).magnitude - OFFSET_FROM_HIT);

            if (_laserHitParticle != null)
            {
                _laserHitParticle.transform.position = _hit.point - transform.forward * 0.7f;
                ParticlePlay();
            }

            // 플레이어와 레이저가 닿음
            if (_hit.collider.CompareTag("Player"))
            {
                DetectedPlayer();
            }
        }
        else
        {
            ParticleStop();
            _laserLineBehavior.EndPos = Vector3.forward * laserMaxDist;
        }
    }


    // 레이저 중심지점과 플레이어 사이의 거리가 일정 거리 이하인지 확인
    private bool IsPlayerNearLaserOrigin()
    {
        // 레이저가 움직인다면, 레이저의 중심점 계산
        if (isLaserMoving)
            _laserCenter = transform.position + transform.forward * laserMaxDist * 0.5f;

        return (_laserCenter - _player.position).sqrMagnitude < detectPointDist * detectPointDist;
    }


    // 레이저 선분과 플레이어 사이의 거리가 일정 거리 이하인지 확인
    private bool IsPlayerNearLaserLine()
    {
        float _detectLineDist;
        // 햄스터 와이어로 물체를 끌어오는 경우를 대비해서 감지 거리를 늘림
        if (PlayerManager.Instance.onWire && !PlayerManager.Instance.isBall && detectLineDist < 60)
            _detectLineDist = 60;
        else
            _detectLineDist = detectLineDist;

        return GetSqrDistFromPlayerToLaserLine() < _detectLineDist * _detectLineDist;
    }


    // 레이저 선분과 플레이어 사이의 거리
    private float GetSqrDistFromPlayerToLaserLine()
    {
        _playerPosition = _player.position;

        // 레이저가 움직인다면, 레이저의 시작점과 끝점 계산
        if (isLaserMoving)
        {
            _laserOrigin = transform.position;
            _laserMaxPoint = transform.position + transform.forward * laserMaxDist;
        }

        Vector3 line = _laserMaxPoint - _laserOrigin;
        Vector3 toPoint = _playerPosition - _laserOrigin;

        float t = Vector3.Dot(toPoint, line) / line.sqrMagnitude;
        t = Mathf.Clamp01(t);

        Vector3 projection = _laserOrigin + t * line;
        return (_playerPosition - projection).sqrMagnitude;
    }


    /// 레이저가 플레이어를 검출했을 때 이벤트
    private void DetectedPlayer()
    {
        if (_laserType == LaserType.LightningShock)
        {
            NotifyLaserShoot();
            PlayerManager.Instance.LightningShock();
        }
        else if (_laserType == LaserType.PlatformDisappear)
        {
            _platformDisappearMgr.PlatformDisappear();
        }
    }

    float _laserShootRotationSpeed;
    float _laserShootLaserShootCooltime;
    float _laserShootLaserSpeed;

    private void NotifyLaserShoot()
    {
        if (laserShoot != null && PlayerManager.Instance.canLightningShock)
        {
            _laserShootRotationSpeed = laserShoot.rotationSpeed;
            _laserShootLaserShootCooltime = laserShoot.laserShootCooltime;
            _laserShootLaserSpeed = laserShoot.laserSpeed;

            laserShoot.rotationSpeed = 30f;
            laserShoot.laserShootCooltime = 0.05f;
            laserShoot.laserSpeed = 350f;

            Invoke(nameof(LaserShootBurstEnd), 3.5f);
        }
    }

    private void LaserShootBurstEnd()
    {
        laserShoot.rotationSpeed = _laserShootRotationSpeed;
        laserShoot.laserShootCooltime = _laserShootLaserShootCooltime;
        laserShoot.laserSpeed = _laserShootLaserSpeed;
    }

    private void ParticlePlay()
    {
        if (_laserHitParticle?.isPlaying == false)
        {
            _laserHitParticle.gameObject.SetActive(true);
            _laserHitParticle.Play();
        }
    }

    private void ParticleStop()
    {
        if (_laserHitParticle?.isPlaying == true)
        {
            _laserHitParticle.Stop();
            _laserHitParticle.gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// 레이저의 투명도를 a로 설정합니다.
    /// </summary>
    /// <param name="a">레이저의 투명도 (0~1)</param>
    public void SetLaserAlpha(float a)
    {
        _myRenderer.material.color = _laserColor * a;
    }




    #region OnValidate
    private bool _prevIsLaserMoving;
    private bool _prevIsEndPointDynamic;
    private void OnValidate()
    {
        // islaserMoving이 변경된 경우
        if (isLaserMoving != _prevIsLaserMoving)
        {
            if (isLaserMoving)
                isEndPointDynamic = true;
        }

        // isEndPointDynamic이 변경된 경우
        if (isEndPointDynamic != _prevIsEndPointDynamic)
        {
            if (!isEndPointDynamic)
                isLaserMoving = false;
        }

        // 현재 값을 다음 비교를 위해 저장
        _prevIsLaserMoving = isLaserMoving;
        _prevIsEndPointDynamic = isEndPointDynamic;

        GetComponent<VolumetricLineBehavior>().EndPos = Vector3.forward * laserMaxDist;
    }
    #endregion
}




#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(LaserController))]
[CanEditMultipleObjects]
class LaserControllerEditor : Editor
{
    LaserController _target;


    SerializedProperty laserTypeProp;
    SerializedProperty platformDisappearMgrProp;
    SerializedProperty visulaizeLineProp;
    SerializedProperty detectPointDistProp;
    SerializedProperty distWhenPlayerFarProp;
    SerializedProperty setDistWhenPlayerFarProp;
    SerializedProperty detectLineDistProp;
    SerializedProperty laserMaxDistProp;
    SerializedProperty isEndPointDynamicProp;
    SerializedProperty isLaserMovingProp;
    SerializedProperty laserShootProp;

    private void OnEnable()
    {
        laserTypeProp = serializedObject.FindProperty("_laserType");
        platformDisappearMgrProp = serializedObject.FindProperty("_platformDisappearMgr");
        visulaizeLineProp = serializedObject.FindProperty("visulaizeLine");
        detectPointDistProp = serializedObject.FindProperty("detectPointDist");
        setDistWhenPlayerFarProp = serializedObject.FindProperty("setDistWhenPlayerFar");
        distWhenPlayerFarProp = serializedObject.FindProperty("distWhenPlayerFar");
        detectLineDistProp = serializedObject.FindProperty("detectLineDist");
        laserMaxDistProp = serializedObject.FindProperty("laserMaxDist");
        isEndPointDynamicProp = serializedObject.FindProperty("isEndPointDynamic");
        isLaserMovingProp = serializedObject.FindProperty("isLaserMoving");
        laserShootProp = serializedObject.FindProperty("laserShoot");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (isEndPointDynamicProp.boolValue)
            EditorGUILayout.HelpBox("플레이어가 빨간색 영역 안에 있을 때만 레이저가 갱신됩니다.", MessageType.Info);
        else
            EditorGUILayout.HelpBox("플레이어가 빨간색/초록색 영역 안에 있을 때만 레이저가 갱신됩니다.", MessageType.Info);

        EditorGUILayout.PropertyField(laserTypeProp);
        if (laserTypeProp.enumValueIndex == (int)LaserController.LaserType.PlatformDisappear)
            EditorGUILayout.PropertyField(platformDisappearMgrProp);

        EditorGUILayout.PropertyField(visulaizeLineProp);
        EditorGUILayout.PropertyField(detectPointDistProp);
        EditorGUILayout.PropertyField(setDistWhenPlayerFarProp);
        if (setDistWhenPlayerFarProp.boolValue)
            EditorGUILayout.PropertyField(distWhenPlayerFarProp);

        if (!isEndPointDynamicProp.boolValue)
            EditorGUILayout.PropertyField(detectLineDistProp);
        
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(laserMaxDistProp);
        EditorGUILayout.PropertyField(isEndPointDynamicProp);
        EditorGUILayout.PropertyField(isLaserMovingProp);

        EditorGUILayout.PropertyField(laserShootProp);

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI()
    {
        if (!visulaizeLineProp.boolValue)
            return;

        _target = target as LaserController; 

        // Scene 뷰에서 조절 가능한 구형 영역 표시
        HandleDetectPointDist();

        // Scene 뷰에서 조절 가능한 캡슐형 영역 표시
        if (!isEndPointDynamicProp.boolValue)
            HandleDetectLineDist();

        // 값이 바뀌었다면 객체를 Undo에 기록하고 dirty 상태로 만들어 저장되도록 함
        if (GUI.changed)
        {
            Undo.RecordObject(_target, "Modify Laser Detection Range");
            EditorUtility.SetDirty(_target);
        }
    }

    private void HandleDetectPointDist()
    {
        Handles.color = Color.red;

        Vector3 start = _target.transform.position;
        Vector3 end = start + _target.transform.forward * _target.laserMaxDist;
        Vector3 laserCenter = (start + end) / 2f;

        float newRadius = Handles.RadiusHandle(
            Quaternion.LookRotation(end - start),
            laserCenter,
            _target.detectPointDist
        );

        if (newRadius != _target.detectPointDist)
        {
            _target.detectPointDist = newRadius;
            //serializedObject.ApplyModifiedProperties();
        }
    }


    private void HandleDetectLineDist()
    {
        Vector3 start = _target.transform.position;
        Vector3 end = start + _target.transform.forward * _target.laserMaxDist;

        // 현재 detectLineDist 값을 핸들로 조절
        float radius = 0;
        Handles.color = Color.green;

        foreach (Vector3 pos in new Vector3[] { start, end })
        {
            radius = Handles.RadiusHandle(
                Quaternion.LookRotation(end - start),
                pos,
                _target.detectLineDist
            );

            if (radius != _target.detectLineDist)
            {
                _target.detectLineDist = radius;
                //serializedObject.ApplyModifiedProperties();
            }
        }

        float height = Vector3.Distance(start, end);

        // 캡슐 형태 시각화: 원기둥 본체
        Handles.DrawWireDisc(start, (end - start).normalized, radius); // 시작점 원
        Handles.DrawWireDisc(end, (end - start).normalized, radius);   // 끝점 원

        // 본체 원기둥 라인
        Vector3 dir = (end - start).normalized;

        foreach (Vector3 axe in new Vector3[] { Vector3.right, Vector3.forward, Vector3.up })
        {
            Vector3 ortho = Vector3.Cross(dir, axe).normalized * radius;
            Handles.DrawLine(start + ortho, end + ortho);
            Handles.DrawLine(start - ortho, end - ortho);
        }
        

        // 양 끝의 구 영역 (캡슐의 반구)
        //Handles.DrawWireDisc(start, ortho, radius);
        //Handles.DrawWireDisc(start, Vector3.right, radius);
        //Handles.DrawWireDisc(end, ortho, radius);
        //Handles.DrawWireDisc(end, Vector3.right, radius);
    }
}
#endif
#endregion