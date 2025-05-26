using UnityEngine;
using DG.Tweening;

public class AutomaticDoorController : MonoBehaviour
{
    private Transform _leftDoor;
    private Transform _rightDoor;

    private void Start()
    {
        _leftDoor = transform.GetChild(0);
        _rightDoor = transform.GetChild(1);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _leftDoor.DOLocalMoveZ(-6, 2f);
        _rightDoor.DOLocalMoveZ(6, 2f);

        GameManager.PlaySfx(SfxType.AutomaticDoorOpen);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _leftDoor.DOLocalMoveZ(-3, 2f);
        _rightDoor.DOLocalMoveZ(3, 2f);

        GameManager.PlaySfx(SfxType.AutomaticDoorClose);
    }
}
