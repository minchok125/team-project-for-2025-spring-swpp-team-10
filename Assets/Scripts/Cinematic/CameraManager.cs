using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class CameraManager : MonoBehaviour
{
    [Header("Test")]
    [SerializeField] private Vector2 cam1Pos;
    [SerializeField] private Vector2 cam2Pos;
    [SerializeField] private Vector2 cam1Size;
    [SerializeField] private Vector2 cam2Size;
    
    [Header("Cam1")]
    [SerializeField] private Camera cam1;
    [SerializeField] private float cam1Width, cam1Height;
    [SerializeField] private float cam1PosX, cam1PosY;
    [SerializeField] private float cam1GapLeft, cam1GapRight, cam1GapTop, cam1GapBottom;
    
    [Header("Cam2")]
    [SerializeField] private Camera cam2;
    [SerializeField] private float cam2Width, cam2Height;
    [SerializeField] private float cam2PosX, cam2PosY;
    [SerializeField] private float cam2GapLeft, cam2GapRight, cam2GapTop, cam2GapBottom;

    private void Awake()
    {
        cam1.enabled = true;
        cam2.enabled = true;
    }

    public void DebugCamRect()
    {
        cam1.rect = new Rect(cam1Pos, cam1Size);
        cam2.rect = new Rect(cam2Pos, cam2Size);
    }

    private Vector2 CalculateCamPos()
    {
        return Vector2.zero;
    }
}
