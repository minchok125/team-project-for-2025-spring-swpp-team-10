using UnityEngine;

// 오브젝트의 특성
// 해당 스크립트가 없다면 모든 특성이 false인 것으로 간주
// 특성에 따라 DrawOutline.cs는 자동으로 추가

public class ObjectProperties : MonoBehaviour
{
    [Tooltip("플레이어가 오브젝트 위에서 점프 가능")]
    public bool canPlayerJump = false;
    [Tooltip("공 모드에서 그랩 가능한 오브젝트")]
    public bool canGrabInBallMode = false;
    [Tooltip("햄스터 모드에서 그랩 가능한 오브젝트")]
    public bool canGrabInHamsterMode = false;

    private void Start()
    {
        if (canGrabInBallMode || canGrabInHamsterMode) {
            gameObject.layer = LayerMask.NameToLayer("Attachable");
            gameObject.AddComponent<DrawOutline>();

            if (canGrabInHamsterMode) {
                if (!TryGetComponent(out Rigidbody rb)) {
                    Debug.LogWarning(gameObject.name + ": Rigidbody가 없습니다. Rigidbody를 추가합니다.");
                    AddHamsterObjectRigidbody();
                }
            }
        }
        else {
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }


    private void AddHamsterObjectRigidbody()
    {
        Rigidbody rb = gameObject.AddComponent<Rigidbody>();

        rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
    }
}
