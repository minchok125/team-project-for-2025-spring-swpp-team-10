using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    public enum State { Hamster, BallNotWire, BallWire }
    [SerializeField] private State state;

    //[SerializeField, Range(0,1)] private float lerp;
    [SerializeField] private float wireCamPointDist;

    private Transform player;
    private Transform hitPoint;
    private Vector3 offset = new Vector3(0, 1.1f, 0);


    void Start()
    {
        player = GameObject.Find("Player").transform;
        hitPoint = player.GetComponent<PlayerWireController>().hitPoint;
    }


    void Update()
    {
        Vector3 plr = player.position + offset;

        if (state == State.Hamster) {
            transform.position = plr;
        }
        else if (state == State.BallNotWire) {
            transform.position = plr;
        }
        else if (state == State.BallWire) {
            Vector3 diff = hitPoint.position + offset - plr;
            if (diff.sqrMagnitude < wireCamPointDist * wireCamPointDist)
                transform.position = hitPoint.position + offset;
            else
                transform.position = plr + diff.normalized * wireCamPointDist;
        }
    }
}
