using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    public enum State { Hamster, BallNotWire, BallWire1, BallWire2 }
    [SerializeField] private State state;

    //[SerializeField, Range(0,1)] private float lerp;
    [SerializeField] private float wireCamPointDist;

    private Transform player;
    private Transform hitPoint1, hitPoint2;
    private Vector3 offset = new Vector3(0, 1.1f, 0);


    void Start()
    {
        player = GameObject.Find("Player").transform;
        hitPoint1 = player.GetComponent<PlayerWireController>().hitPoint1;
        hitPoint2 = player.GetComponent<PlayerWireController>().hitPoint2;
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
        else if (state == State.BallWire1) {
            Vector3 diff = hitPoint1.position + offset - plr;
            if (diff.sqrMagnitude < wireCamPointDist * wireCamPointDist)
                transform.position = hitPoint1.position + offset;
            else
                transform.position = plr + diff.normalized * wireCamPointDist;
        }
        else if (state == State.BallWire2) {
            Vector3 diff = hitPoint2.position + offset - plr;
            if (diff.sqrMagnitude < wireCamPointDist * wireCamPointDist)
                transform.position = hitPoint2.position + offset;
            else
                transform.position = plr + diff.normalized * wireCamPointDist;
        }
    }
}
