using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicTownObject
{
    public Transform Transform;
    public CinematicHamsterController Hamster;
    public GameObject Police, Lights;

    public CinematicTownObject(Transform transform, CinematicHamsterController hamsterController, GameObject police,
        GameObject lights)
    {
        Transform = transform;
        Hamster = hamsterController;
        Police = police;
        Lights = lights;
        
        Hamster.gameObject.SetActive(false);
        Police.gameObject.SetActive(false);
        Lights.SetActive(false);
    }
}
