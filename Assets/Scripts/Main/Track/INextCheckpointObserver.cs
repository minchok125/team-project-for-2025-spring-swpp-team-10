using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INextCheckpointObserver
{
    void OnNextCheckpointChanged(Vector3? nextPosition);
}
