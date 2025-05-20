using Hampossible.Utils;
using UnityEngine;

public class DrawerConfiner : MonoBehaviour
{
    [Tooltip("서랍의 각 칸 오브젝트")]
    [SerializeField] GameObject[] drawers;
    [Tooltip("drawers의 순서에 맞는 오브젝트를 와이어로 끌어올 수 있는 최대거리")]
    [SerializeField] float[] drawersOpenMaxDist;

    private Collider[] drawersCol;
    private Rigidbody[] drawersRigid;
    private Vector3[] drawersOrigin;

    private void Start()
    {
        if (drawers.Length != drawersOpenMaxDist.Length)
        {
            HLogger.General.Error("서랍의 칸수와 drawersOpenMaxDist 리스트의 길이가 일치하지 않습니다.", this);
            return;
        }

        Init();
    }

    private void Init()
    {
        drawersCol = new Collider[drawers.Length];
        drawersOrigin = new Vector3[drawers.Length];
        drawersRigid = new Rigidbody[drawers.Length];
        for (int i = 0; i < drawers.Length; i++)
        {
            drawersCol[i] = drawers[i].GetComponent<Collider>();
            drawersOrigin[i] = drawers[i].transform.position;

            if (drawers[i].TryGetComponent(out Rigidbody rb))
                drawersRigid[i] = rb;
            else
                drawersRigid[i] = drawers[i].AddComponent<Rigidbody>();

            drawersRigid[i].mass = 0.8f;
            drawersRigid[i].isKinematic = true;
            drawersRigid[i].constraints = RigidbodyConstraints.FreezePositionY
                                        | RigidbodyConstraints.FreezeRotationX
                                        | RigidbodyConstraints.FreezeRotationY
                                        | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void FixedUpdate()
    {
        if (PlayerManager.Instance.isBall || !PlayerManager.Instance.onWire)
        {
            return;
        }

        for (int i = 0; i < drawersRigid.Length; i++)
        {
            if (PlayerManager.Instance.onWireCollider != drawersCol[i])
                continue;

            // transform.forward 방향으로 서랍이 열린 거리
            float openDist = Vector3.Dot(transform.forward, drawersRigid[i].transform.position - drawersOrigin[i]);
            if (openDist >= drawersOpenMaxDist[i] - 0.05f)
            {
                drawersRigid[i].MovePosition(drawersOrigin[i] + transform.forward * drawersOpenMaxDist[i]);
                drawersRigid[i].isKinematic = true;
            }
            else
            {
                drawersRigid[i].isKinematic = false;
            }
        }
    }
}
