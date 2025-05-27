using UnityEngine;

public class FloatMotion : MonoBehaviour
{
    [SerializeField] private float amplitude = 5f;     // 각 축의 최대 이동 범위
    [SerializeField] private float rotAmplitude = 10f;       // 각 축의 최대 회전 범위
    [SerializeField] private float frequency = 0.2f;       // 움직이는 속도
    [SerializeField] private Vector3 noiseOffset = new Vector3(0f, 100f, 200f); // 축별 노이즈 시드 오프셋
    [Tooltip("Y축으로 움직임을 줄지 여부")]
    [SerializeField] public bool moveYAxis = true;

    public bool canMove = true;

    public Vector3 initialPosition;
    private float _seed;
    private float _time;

    void Start()
    {
        _time = 0f;
        _seed = Random.Range(0f, 1000f); // 오브젝트별 고유한 seed 부여
        initialPosition = transform.position;
        Move();
    }

    void FixedUpdate()
    {
        if (!canMove)
            return;

        _time += Time.fixedDeltaTime * frequency;

        Move();
    }

    private void Move()
    {
        float xOffset = (Mathf.PerlinNoise(_seed + noiseOffset.x, _time) - 0.5f) * 2f * amplitude;
        float yOffset = (Mathf.PerlinNoise(_seed + noiseOffset.y, _time) - 0.5f) * 2f * amplitude;
        if (!moveYAxis) yOffset = 0;
        float zOffset = (Mathf.PerlinNoise(_seed + noiseOffset.z, _time) - 0.5f) * 2f * amplitude;

        transform.position = initialPosition + new Vector3(xOffset, yOffset, zOffset);

        float xRot = (Mathf.PerlinNoise(_seed + noiseOffset.x, _time) - 0.5f) * 2f * rotAmplitude;
        float yRot = (Mathf.PerlinNoise(_seed + noiseOffset.y, _time) - 0.5f) * 2f * rotAmplitude;
        float zRot = (Mathf.PerlinNoise(_seed + noiseOffset.z, _time) - 0.5f) * 2f * rotAmplitude;

        transform.rotation = Quaternion.Euler(xRot, yRot, zRot);
    }

    public void SetPosition(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
