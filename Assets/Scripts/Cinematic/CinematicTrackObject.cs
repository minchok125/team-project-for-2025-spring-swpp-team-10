using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicTrackObject
{
    public Transform Transform;
    public CinematicHamsterController Hamster;

    public CinematicTrackObject(Transform transform, CinematicHamsterController hamsterController)
    {
        Transform = transform;
        Hamster = hamsterController;
        
        Hamster.gameObject.SetActive(false);
    }
}
