using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEngine.UI;

public class DialogueUIController : MonoBehaviour
{
    // Dialogue UI를 관리하는 script로, Dialogues Parent의 component가 됨
    
    [Header("References")]
    [SerializeField] private GameObject dialoguePrefab;
    
    [Header("Values")]
    [SerializeField] private int maxDialogueNum;
    [SerializeField] private float defaultLifetime, defaultDelay;
    [SerializeField] private float fadeDuration;
    
    private List<DialogueBlockController> _blockControllers = new List<DialogueBlockController>();
    private float _offset;
    private List<Dictionary<string, object>> _currData = new List<Dictionary<string, object>>();
    private int _currDataIdx;

    private void Awake()
    {
        InitDialogue();
    }

    public void InitDialogue()
    {
        // 혹시 남아 있는 Dialogue 있으면 삭제
        ClearDialogue();
        
        // height과 max dialogue num을 기준으로 각 dialogue 사이의 offset 계산
        float parentHeight = gameObject.GetComponent<RectTransform>().sizeDelta.y;
        float prefabHeight = dialoguePrefab.GetComponent<RectTransform>().sizeDelta.y;
        _offset = (parentHeight - (prefabHeight * maxDialogueNum)) / maxDialogueNum + prefabHeight;

        _currDataIdx = 0;
    }

    private void ClearDialogue()
    {
        // 현재 표시 중인 모든 Dialogue 삭제
        foreach (DialogueBlockController controller in _blockControllers)
            controller.Remove();
        _blockControllers.Clear();
    }

    public void StartDialogue(string fileName)
    {
        // file 이름을 통해 일련의 dialogue 진행
        _currDataIdx = 0;
        ClearDialogue();
        string path = System.IO.Path.Combine("Dialogues", fileName);
        _currData = CSVReader.Read(path);

        if (_currData == null) return;

        StartCoroutine(DialogueCoroutine());
    }

    private IEnumerator DialogueCoroutine()
    {
        while (_currDataIdx < _currData.Count)
        {
            // delay만큼 대기
            float delay;
            try
            {
                string tmp = _currData[_currDataIdx]["delay"].ToString();
                delay = Convert.ToSingle(tmp);
            }
            catch { delay = defaultDelay; }
            
            yield return new WaitForSeconds(delay);
            
            // 대기 종료 이후 dialogye block 생성 sequence 시작
            while (true)
            {
                // dialogueBlock이 이미 꽉 차 있으면 대기
                if (_blockControllers.Count < maxDialogueNum)
                {
                    yield return GenerateDialogueBlock();
                    break;
                }
                yield return null;
            }
        }
    }

    private IEnumerator GenerateDialogueBlock()
    {
        // 기존의 dialogue들은 아래로 한 칸씩 이동
        for (int i = 0; i < _blockControllers.Count; i++)
            _blockControllers[i].MoveTo(_offset * (_blockControllers.Count - i) * -1);
                    
        // 새로운 dialogue 생성
        GameObject newDialogue = Instantiate(dialoguePrefab, transform);
        DialogueBlockController newController = newDialogue.GetComponent<DialogueBlockController>();
        _blockControllers.Add(newController);
        newController.InitDialogueBlock(fadeDuration, _currData[_currDataIdx]["text"]);
        newController.Show();

        // lifetime이 끝나면 dialogue를 destroy하도록 설정
        float lifetime;
        try
        {
            string tmp = _currData[_currDataIdx]["lifetime"].ToString();
            lifetime = Convert.ToSingle(tmp);
        }
        catch { lifetime = defaultLifetime; }
        DOVirtual.DelayedCall(lifetime, () => RemoveDialogue(newController));
        
        _currDataIdx++;
        yield return null;
    }

    private void RemoveDialogue(DialogueBlockController controller)
    {
        controller.Remove();
        _blockControllers.Remove(controller);
    }

    public void StartOneLineDialogue(string fileName, int idx)
    {
        // 한 줄 짜리 dialogue를 출력
        
    }

    public void DebugDialogue(string fileName)
    {
        StartDialogue(fileName);
    }
}
