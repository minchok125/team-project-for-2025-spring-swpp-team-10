using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PianoController : MonoBehaviour
{
    [Header("음표 설정")]
    [Tooltip("순서대로 나타날 음표 게임 오브젝트들을 담는 리스트입니다.")]
    public List<GameObject> musicalNotes;

    [Tooltip("각 음표가 나타나거나 사라지는 시간 간격입니다.")]
    public float delayBetweenNotes = 0.5f;

    [Tooltip("트리거를 나간 후, 음표가 사라지기 시작할 때까지의 대기 시간입니다.")]
    public float delayBeforeHide = 3f;

    // --- 추가된 부분: 음표 활성화 요청 카운트를 저장할 배열 ---
    private int[] noteActivationCounts;

    void Start()
    {
        noteActivationCounts = new int[musicalNotes.Count];

        for (int i = 0; i < musicalNotes.Count; i++)
        {
            if (musicalNotes[i] != null)
            {
                musicalNotes[i].SetActive(false);
            }
            noteActivationCounts[i] = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            StartCoroutine(ShowNotesSequentially());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<Rigidbody>() != null)
        {
            StartCoroutine(HideNotesSequentially());
        }
    }

    private IEnumerator ShowNotesSequentially()
    {
        //Debug.Log("새로운 '나타나기' 시퀀스를 시작합니다.");

        for (int i = 0; i < musicalNotes.Count; i++)
        {
            if (musicalNotes[i] != null)
            {
                noteActivationCounts[i]++;
                musicalNotes[i].SetActive(true);

                yield return new WaitForSeconds(delayBetweenNotes);
            }
        }
        //Debug.Log("'나타나기' 시퀀스 하나가 완료되었습니다.");
    }

    private IEnumerator HideNotesSequentially()
    {
        yield return new WaitForSeconds(delayBeforeHide);

        //Debug.Log("새로운 '숨기기' 시퀀스를 시작합니다.");

        for (int i = 0; i < musicalNotes.Count; i++)
        {
            if (musicalNotes[i] != null)
            {
                noteActivationCounts[i]--;

                if (noteActivationCounts[i] <= 0)
                {
                    noteActivationCounts[i] = 0;
                    musicalNotes[i].SetActive(false);
                }
                yield return new WaitForSeconds(delayBetweenNotes);
            }
        }
        //Debug.Log("'숨기기' 시퀀스 하나가 완료되었습니다.");
    }
}