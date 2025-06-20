using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PianoController : MonoBehaviour
{
    // 인스펙터 창에서 설정할 변수들
    [Header("음표 설정")]
    [Tooltip("순서대로 나타날 음표 게임 오브젝트들을 담는 리스트입니다.")]
    public List<GameObject> musicalNotes; // 음표 게임 오브젝트 리스트

    [Tooltip("각 음표가 나타나는 시간 간격입니다.")]
    public float delayBetweenNotes = 0.5f; // 음표 사이의 딜레이 시간

    // 내부적으로 사용할 변수
    private bool isSequencePlaying = false;

    void Start()
    {
        foreach (GameObject note in musicalNotes)
        {
            if (note != null)
            {
                note.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isSequencePlaying)
        {
            StartCoroutine(ShowNotesSequentially());
        }
    }

    private IEnumerator ShowNotesSequentially()
    {
        isSequencePlaying = true;

        Debug.Log("피아노 시퀀스를 시작합니다.");

        foreach (GameObject note in musicalNotes)
        {
            if (note != null)
            {
                note.SetActive(true);

                yield return new WaitForSeconds(delayBetweenNotes);
            }
        }

        Debug.Log("피아노 시퀀스가 종료되었습니다.");

        isSequencePlaying = false;
    }
}