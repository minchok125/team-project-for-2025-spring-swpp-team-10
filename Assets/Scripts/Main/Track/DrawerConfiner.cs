using System.Collections.Generic;
using Hampossible.Utils;
using UnityEngine;

public class DrawerConfiner : MonoBehaviour
{
    [Tooltip("서랍의 각 칸 오브젝트")]
    [SerializeField] GameObject[] drawers;
    [Tooltip("drawers의 순서에 맞는 오브젝트를 와이어로 끌어올 수 있는 최대거리")]
    [SerializeField] float[] drawersOpenMaxDist;
    [Tooltip("리지드바디 설정을 스크립트에서 바꾸지 않음")]
    [SerializeField] private bool customRigidSetting = false;

    private List<List<Collider>> _drawersCol;
    private Rigidbody[] _drawersRigid;
    private Vector3[] _drawersOrigin;

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
        _drawersCol = new List<List<Collider>>();
        _drawersOrigin = new Vector3[drawers.Length];
        _drawersRigid = new Rigidbody[drawers.Length];
        for (int i = 0; i < drawers.Length; i++)
        {
            _drawersCol.Add(new List<Collider>());
            foreach (Collider col in drawers[i].GetComponents<Collider>())
                _drawersCol[i].Add(col);

            _drawersOrigin[i] = drawers[i].transform.position;

            if (drawers[i].TryGetComponent(out Rigidbody rb))
                _drawersRigid[i] = rb;
            else
                _drawersRigid[i] = drawers[i].AddComponent<Rigidbody>();

            if (customRigidSetting)
                continue;

            _drawersRigid[i].mass = 0.8f;
            _drawersRigid[i].isKinematic = true;
            _drawersRigid[i].constraints = RigidbodyConstraints.FreezePositionY
                                        | RigidbodyConstraints.FreezeRotationX
                                        | RigidbodyConstraints.FreezeRotationY
                                        | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    private void FixedUpdate()
    {
        if (PlayerManager.Instance.isBall || !PlayerManager.Instance.onWire)
        {
            for (int i = 0; i < _drawersRigid.Length; i++)
                _drawersRigid[i].isKinematic = true;
            return;
        }

        for (int i = 0; i < _drawersRigid.Length; i++)
        {
            if (!ContainsCol(i))
                continue;

            // transform.forward 방향으로 서랍이 열린 거리
            float openDist = Vector3.Dot(transform.forward, _drawersRigid[i].transform.position - _drawersOrigin[i]);
            if (openDist >= drawersOpenMaxDist[i] - 0.05f)
            {
                _drawersRigid[i].MovePosition(_drawersOrigin[i] + transform.forward * drawersOpenMaxDist[i]);
                _drawersRigid[i].isKinematic = true;
            }
            else
            {
                _drawersRigid[i].isKinematic = false;
            }
        }
    }

    private bool ContainsCol(int idx)
    {
        for (int i = 0; i < _drawersCol[idx].Count; i++)
            if (_drawersCol[idx][i] == PlayerManager.Instance.onWireCollider)
                return true;
        return false;
    }
}
