using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using AudioSystem;

public class CubeStageCountButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countTxt;
    [SerializeField] private Transform door;

    private int _remainButtonCount = 4;
    private const float MOVE_TIME = 6.5f;
    private float startLocalY = 3f;
    private float endLocalY = 180f;


    void Start()
    {
        countTxt.text = "Remain : " + _remainButtonCount.ToString();
    }

    public void ButtonClick()
    {
        _remainButtonCount--;
        countTxt.text = "Remain : " + _remainButtonCount.ToString();

        if (_remainButtonCount <= 0)
        {
            AudioManager.Instance.PlaySfx2D(SfxType.SecureAreaRoom1DoorOpen);
            StartCoroutine(OpenDoor());
        }
    }

    IEnumerator OpenDoor()
    {
        float time = 0f;
        while (time < MOVE_TIME)
        {
            float y = Mathf.Lerp(startLocalY, endLocalY, time / MOVE_TIME);
            door.localPosition = new Vector3(door.localPosition.x, y, door.localPosition.z);
            yield return null;
            time += Time.deltaTime;
        }
        Destroy(door.gameObject);
    }
}
