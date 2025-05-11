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

    [Tooltip("그랩 오브젝트 검사하는 레이저에서, 해당 오브젝트를 통과하고 뒤의 오브젝트를 검사")]
    /// <summary>
    /// 그랩 오브젝트 검사하는 레이저에서, 해당 오브젝트를 통과하고 뒤의 오브젝트를 검사
    /// </summary>
    public bool nonDetectable = false;

    private void Start()
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

            if (!TryGetComponent(out DrawOutline draw))
                gameObject.AddComponent<DrawOutline>();

            if (canGrabInHamsterMode) 
            {
                if (!TryGetComponent(out Rigidbody rb)) 
                {
                    HLogger.General.Warning(gameObject.name + ": Rigidbody가 없습니다. Rigidbody를 추가합니다.", this);
                    AddHamsterObjectRigidbody();
                }
            }
        }
        else 
        {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
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
