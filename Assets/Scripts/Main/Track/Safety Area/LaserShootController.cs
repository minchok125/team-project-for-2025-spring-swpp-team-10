using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LaserObjectPool : MonoBehaviour
{
    private GameObject _prefab;
    private int _initialSize;
    private Transform _parent;

    private Queue<GameObject> _pool = new Queue<GameObject>();

    public LaserObjectPool(GameObject prefab, Transform parent, int initialSize)
    {
        _prefab = prefab;
        _initialSize = initialSize;
        _parent = parent;
        Init();
    }

    private void Init()
    {
        // 초기 풀 생성
        for (int i = 0; i < _initialSize; i++)
        {
            GameObject obj = Instantiate(_prefab, _parent);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }

    public GameObject GetObject()
    {
        // 재사용 가능한 오브젝트 반환
        if (_pool.Count > 0)
        {
            GameObject obj = _pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            // 풀에 남은 게 없으면 새로 생성
            GameObject obj = Instantiate(_prefab, _parent);
            obj.SetActive(true);
            return obj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        _pool.Enqueue(obj);
    }
}





public class LaserShootController : MonoBehaviour
{

    [Tooltip("레이저 중심점과 플레이어 사이의 거리가 일정 거리 이하일 때 감지 시작")]
    [SerializeField] public float detectPointDist = 300;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private GameObject shootLaserPrefab;
    [SerializeField] private float laserShootCooltime = 0.2f;
    [SerializeField] private float laserSpeed = 1f;

    private Transform _player;
    private Transform _rayCastPoint;
    private LineRenderer _lineRenderer;
    private LaserObjectPool _objectPool;
    private float _lastLaserShootTime = -1f;

    private void Start()
    {
        _player = PlayerManager.Instance.transform;
        _rayCastPoint = transform.GetChild(1);
        _objectPool = new LaserObjectPool(shootLaserPrefab, null, 6);

        if (!TryGetComponent(out _lineRenderer))
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
    }

    private void FixedUpdate()
    {
        if (!IsPlayerNearLaserOrigin())
        {
            _lineRenderer.enabled = false;
            return;
        }

        Vector3 playerDir = (_player.position - transform.position).normalized;
        Quaternion target = Quaternion.LookRotation(playerDir);
        transform.rotation = Quaternion.Slerp(
                                transform.rotation,
                                target,
                                rotationSpeed * Time.fixedDeltaTime);

        Shoot(playerDir);
    }

    // 플레이어 사이의 거리가 일정 거리 이하인지 확인
    private bool IsPlayerNearLaserOrigin()
    {
        return (transform.position - _player.position).sqrMagnitude < detectPointDist * detectPointDist;
    }

    private void Shoot(Vector3 playerDir)
    {
        if (Physics.Raycast(_rayCastPoint.position, transform.forward, out RaycastHit hit, detectPointDist))
        {
            _lineRenderer.enabled = true;

            // 시작점 설정
            _lineRenderer.SetPosition(0, transform.position);

            // 끝점 설정
            _lineRenderer.SetPosition(1, hit.point);

            if (Time.time - _lastLaserShootTime > laserShootCooltime)
            {
                ShootLaser(playerDir); 
            }
        }
        else
        {
            _lineRenderer.enabled = false;
        }


    }

    private void ShootLaser(Vector3 playerDir)
    {
        GameObject obj = _objectPool.GetObject();
        //GameObject obj = GameObject.Instantiate(shootLaserPrefab, _rayCastPoint.position, Quaternion.LookRotation(playerDir)) as GameObject;
        obj.transform.position = _rayCastPoint.position;
        obj.transform.rotation = Quaternion.LookRotation(playerDir);
        LaserShootObjectController laser = obj.GetComponent<LaserShootObjectController>();
        obj.GetComponent<ShotBehavior>().speed = laserSpeed;
        laser.Init(_objectPool);
        //laser.GetComponent<Rigidbody>().velocity = Vector3.zero;//playerDir * laserSpeed;
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
    }
}
#endif
#endregion