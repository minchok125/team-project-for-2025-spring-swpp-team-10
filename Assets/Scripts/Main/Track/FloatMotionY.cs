using UnityEngine;

public class FloatMotionY : MonoBehaviour
{
    [SerializeField] private float amplitude = 5f;     // 최대 높이 변화
    [SerializeField] private float frequency = 0.2f;   // 왕복 속도
    [SerializeField] private float offset = 0f;
    [SerializeField] private float noise = 1f;

    private float _seed;
    private float _initY;

    void Start()
    {
        _seed = Random.Range(0f, 1000f); // 오브젝트별 고유한 seed 부여
        _initY = transform.localPosition.y;
    }

    void FixedUpdate()
    {
        float time = Time.time * frequency;
        float noiseOffset = (Mathf.PerlinNoise(_seed, time) - 0.5f) * 2f * noise;
        float yOffset = Mathf.Sin(offset + time * Mathf.PI * 2f) * amplitude + noiseOffset;

        transform.localPosition = new Vector3(transform.localPosition.x, _initY + yOffset, transform.localPosition.z);
    }
}