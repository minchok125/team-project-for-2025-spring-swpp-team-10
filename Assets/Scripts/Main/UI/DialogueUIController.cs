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
    
    [Header("Colors")]
    [SerializeField] private Color gray;
    [SerializeField] private Color red;
    [SerializeField] private Color orange;
    [SerializeField] private Color yellow;
    [SerializeField] private Color green;
    [SerializeField] private Color blue;
    [SerializeField] private Color purple;
    
    private List<DialogueBlockController> _blockControllers = new List<DialogueBlockController>();
    private float _offset;
    private List<Dictionary<string, object>> _currData = new List<Dictionary<string, object>>();
    private int _currDataIdx;
    private ObjectPool _objectPool;
    private TextProcessor _textProcessor;

    private void Awake()
    {
        InitDialogue();
        _objectPool = gameObject.AddComponent<ObjectPool>();
        _objectPool.InitObjectPool(dialoguePrefab, transform, maxDialogueNum * 2);
        _textProcessor = new TextProcessor(gray, red, orange, yellow, green, blue, purple);
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
            yield return WaitForDelay();
            
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

    private IEnumerator WaitForDelay()
    {
        float delay;
        try
        {
            string tmp = _currData[_currDataIdx]["delay"].ToString();
            delay = Convert.ToSingle(tmp);
        }
        catch { delay = defaultDelay; }
        yield return new WaitForSeconds(delay);
    }

    private IEnumerator GenerateDialogueBlock()
    {
        // 기존의 dialogue들은 아래로 한 칸씩 이동
        for (int i = 0; i < _blockControllers.Count; i++)
            _blockControllers[i].MoveTo(_offset * (_blockControllers.Count - i) * -1);
                    
        // Object Pool을 통해 Dialogue Block 받아 온 뒤 Dialogue Block Controller 초기화
        GameObject newObj = _objectPool.GetObject();
        DialogueBlockController newController = newObj.GetComponent<DialogueBlockController>();
        _blockControllers.Add(newController);
        
        
        
        string processedText = _textProcessor.Process(_currData[_currDataIdx]["text"].ToString());
        newController.InitDialogueBlock(fadeDuration, processedText, _objectPool);
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
