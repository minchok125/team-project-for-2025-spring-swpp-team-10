using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWireShotPointController : MonoBehaviour
{
    [SerializeField] private Vector3 hamsterOffset;

    private Transform _player;

    private void Start()
    {
        _player = PlayerManager.Instance.transform;
    }

    private void FixedUpdate()
    {
        if (PlayerManager.Instance.isBall)
            transform.position = Vector3.Lerp(transform.position, _player.position, 20 * Time.fixedDeltaTime);
        else
            transform.position = Vector3.Lerp(transform.position, _player.position + hamsterOffset, 20 * Time.fixedDeltaTime);
    }
}
