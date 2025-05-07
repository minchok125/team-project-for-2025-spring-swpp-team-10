using UnityEngine;
using Cinemachine;

public class CMFreeLookSetting : MonoBehaviour
{
    void Awake()
    {
        CinemachineCore.GetInputAxis = ClickControl;
    }

    public float ClickControl(string axis)
    {
        if (!Input.GetKey(KeyCode.LeftAlt)) 
        {
            return UnityEngine.Input.GetAxis(axis);
        }

        return 0;
    }
}
