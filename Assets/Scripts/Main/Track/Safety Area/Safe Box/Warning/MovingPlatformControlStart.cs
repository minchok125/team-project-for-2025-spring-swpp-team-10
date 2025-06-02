using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformControlStart : MonoBehaviour
{
    [SerializeField] private MovingPlatformController[] moves;

    private void Start()
    {
        for (int i = 0; i < moves.Length; i++)
            moves[i].enabled = false;
    }

    public void MovingStart()
    {
        for (int i = 0; i < moves.Length; i++)
            moves[i].enabled = true;
    }

    public void MovingStop()
    {
        for (int i = 0; i < moves.Length; i++)
            moves[i].enabled = false;
    }
}
