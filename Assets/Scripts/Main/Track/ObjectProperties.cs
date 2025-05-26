using Hampossible.Utils;
using System.Collections.Generic;
using UnityEngine;


// 오브젝트의 특성
// 해당 스크립트가 없다면 모든 특성이 false인 것으로 간주
// 특성에 따라 DrawOutline.cs는 자동으로 추가

public class ObjectProperties : MonoBehaviour
{
    [Tooltip("플레이어가 해당 지면 위에 있을 때 점프 가능")]
    /// <summary>
    /// 플레이어가 해당 지면 위에 있을 때 점프 가능 여부
    /// </summary>
    public bool canPlayerJump = false;

    [Tooltip("공 모드에서 그랩 가능한 오브젝트")]
    /// <summary>
    /// 공 모드에서 그랩 가능한 오브젝트
    /// </summary>
    public bool canGrabInBallMode = false;

    [Tooltip("햄스터 모드에서 그랩 가능한 오브젝트")]
    /// <summary>
    /// 햄스터 모드에서 그랩 가능한 오브젝트
    /// </summary>
    public bool canGrabInHamsterMode = false;
    public bool generateRigidbody = true;

    [Tooltip("그랩 오브젝트 검사하는 레이저에서, 해당 오브젝트를 통과하고 뒤의 오브젝트를 검사")]
    /// <summary>
    /// 그랩 오브젝트 검사하는 레이저에서, 해당 오브젝트를 통과하고 뒤의 오브젝트를 검사
    /// </summary>
    public bool nonDetectable = false;

    private void Awake()
    {
        if (nonDetectable)
        {
            gameObject.layer = LayerMask.NameToLayer("NonDetectable");
            canGrabInBallMode = canGrabInHamsterMode = false;
            RemoveOutlineMaterial();
        }
        else if (canGrabInBallMode || canGrabInHamsterMode) 
        {
            gameObject.layer = LayerMask.NameToLayer("Attachable");

            if (!HasOutlineInParents() && !TryGetComponent(out Outline outline))
                gameObject.AddComponent<Outline>();
            if (!TryGetComponent(out DrawOutline drawOutline))
                gameObject.AddComponent<DrawOutline>();

            if (canGrabInHamsterMode)
            {
                if (generateRigidbody && !TryGetComponent(out Rigidbody rb))
                {
                    HLogger.General.Info(gameObject.name + ": Rigidbody가 없습니다. Rigidbody를 추가합니다.", this);
                    AddHamsterObjectRigidbody();
                }
            }
        }
        else 
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    // 부모가 Outline을 생성할 예정이라면, 해당 오브젝트에서 Outline을 생성할 경우 중복해서 외곽선이 생성됩니다.
    private bool HasOutlineInParents()
    {
        Transform current = transform.parent;

        while (current != null)
        {
            if (current.TryGetComponent(out ObjectProperties objProp))
            {
                if (objProp.canGrabInBallMode || objProp.canGrabInHamsterMode)
                    return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void RemoveOutlineMaterial()
    {
        Renderer rd = GetComponent<Renderer>();
        Material[] mats = rd.materials;

        if (mats.Length > 1 && mats[1] != null)
        {
            List<Material> matList = new List<Material>(mats);
            matList.RemoveAt(1); // 두 번째 매테리얼 제거
            rd.materials = matList.ToArray(); // 다시 할당
        }
    }


    private void AddHamsterObjectRigidbody()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();

        rb.drag = 1f;

        rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
    }
}
