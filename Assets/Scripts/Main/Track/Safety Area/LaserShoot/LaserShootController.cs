using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum LaserShootType { Push, LightningShock }
public class LaserShootController : MonoBehaviour
{
    public LaserShootType laserShootType;

    [Tooltip("레이저 중심점과 플레이어 사이의 거리가 일정 거리 이하일 때 감지 시작")]
    public float detectPointDist = 300;
    [Tooltip("레이저 포가 최대로 갈 수 있는 거리. 이 거리를 넘어서면 포는 사라짐")]
    public float maxLaserShootMoveDist = 300;
    public float rotationSpeed = 5f;
    [SerializeField] private bool isShootLaser = true;
    [SerializeField] private bool rotateOnlyWhenPlayerIsVisible = true;
    public float laserShootCooltime = 0.2f;
    public float laserSpeed = 1f;

    private Transform _player;
    private Transform _rayCastPoint;
    private LineRenderer _lineRenderer;
    private float _lastLaserShootTime = -1f;
    private LayerMask _layerMask;

    private void Start()
    {
        _player = PlayerManager.Instance.transform;
        _rayCastPoint = transform.GetChild(1);

        if (!TryGetComponent(out _lineRenderer))
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;

        int laserDetectLayer = 1 << LayerMask.NameToLayer("LaserDetect");
        _layerMask = ~laserDetectLayer;
    }

    private void FixedUpdate()
    {
        if (!IsPlayerNearLaserOrigin())
        {
            _lineRenderer.enabled = false;
            return;
        }

        Vector3 playerDir = (_player.position - transform.position).normalized;

        if (rotateOnlyWhenPlayerIsVisible
            && Physics.Raycast(transform.position + playerDir * 19f,
                    playerDir,
                    out RaycastHit hit,
                    detectPointDist,
                    ~0,
                    QueryTriggerInteraction.Ignore)
            )
        {
            if (!hit.collider.CompareTag("Player"))
                return;
        }

        Quaternion target = Quaternion.LookRotation(playerDir);
        float angleDifference = Quaternion.Angle(transform.rotation, target);
        float _rotationSpeed = rotationSpeed * (2 - 0.25f * Mathf.Clamp(angleDifference, 0f, 4f));
        transform.rotation = Quaternion.Slerp(
                                transform.rotation,
                                target,
                                _rotationSpeed * Time.fixedDeltaTime);

        if (isShootLaser)
            Shoot();
    }

    // 플레이어 사이의 거리가 일정 거리 이하인지 확인
    private bool IsPlayerNearLaserOrigin()
    {
        return (transform.position - _player.position).sqrMagnitude < detectPointDist * detectPointDist;
    }

    private void Shoot()
    {
        if (Time.time - _lastLaserShootTime > laserShootCooltime)
        {
            ShootLaser();
        }
    }

    private void ShootLaser()
    {
        GameObject obj;
        if (laserShootType == LaserShootType.Push)
            obj = LaserShootYellowPool.Instance.GetObject();
        else
            obj = LaserShootBluePool.Instance.GetObject();

        obj.transform.position = _rayCastPoint.position;
        obj.transform.rotation = Quaternion.LookRotation(transform.forward);
        LaserShootObjectController laser = obj.GetComponent<LaserShootObjectController>();
        obj.GetComponent<ShotBehavior>().speed = laserSpeed;
        laser.Init(maxLaserShootMoveDist);
        _lastLaserShootTime = Time.time;
    }
}



#region Editor
#if UNITY_EDITOR
[CustomEditor(typeof(LaserShootController))]
[CanEditMultipleObjects]
class LaserShootControllerEditor : Editor
{
    LaserShootController _target;

    private void OnEnable()
    {

    }

    public void OnSceneGUI()
    {
        _target = target as LaserShootController; 

        // Scene 뷰에서 조절 가능한 구형 영역 표시
        HandleDetectPointDist();

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

        float newRadius = Handles.RadiusHandle(
            Quaternion.identity,
            _target.transform.position,
            _target.detectPointDist
        );

        if (newRadius != _target.detectPointDist)
        {
            _target.detectPointDist = newRadius;
        }


        Handles.color = Color.green;

        newRadius = Handles.RadiusHandle(
            Quaternion.identity,
            _target.transform.position,
            _target.maxLaserShootMoveDist
        );

        if (newRadius != _target.maxLaserShootMoveDist)
        {
            _target.maxLaserShootMoveDist = newRadius;
        }
    }
}
#endif
#endregion