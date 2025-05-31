using UnityEngine;

public class ShotBehavior : MonoBehaviour {
	public float speed = 10f;

	private Rigidbody _rb;

    void Start()
    {
		_rb = GetComponent<Rigidbody>();
    }

	// Update is called once per frame
	void FixedUpdate()
	{
		//transform.position += transform.forward * Time.deltaTime * speed;
		_rb.MovePosition(_rb.transform.position + transform.forward * Time.fixedDeltaTime * speed);
	}
}
