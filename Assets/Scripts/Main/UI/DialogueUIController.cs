using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEditor.UIElements;
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

    [Header("Character Sprites")]
    [SerializeField] private Sprite hamster;
    [SerializeField] private Sprite radio;
    
    private List<DialogueBlockController> _blockControllers = new List<DialogueBlockController>();
    private float _offset;
    private ObjectPool _objectPool;
    private TextProcessor _textProcessor;
    private List<Dictionary<string, object>> _oneLineDialogueData;

    

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
        // ClearDialogue();
        
        // height과 max dialogue num을 기준으로 각 dialogue 사이의 offset 계산
        float parentHeight = gameObject.GetComponent<RectTransform>().sizeDelta.y;
        float prefabHeight = dialoguePrefab.GetComponent<RectTransform>().sizeDelta.y;
        _offset = (parentHeight - prefabHeight * maxDialogueNum) /(maxDialogueNum - 1) + prefabHeight;
        
        // 한 줄 dialogue의 csv 파일 읽어 오기
        string path = System.IO.Path.Combine("Dialogues", "OneLine");
        _oneLineDialogueData = CSVReader.Read(path);
    }

    private void ClearDialogue()
    {
        // 현재 표시 중인 모든 Dialogue 삭제
        foreach (DialogueBlockController controller in _blockControllers)
            controller.Remove();
        _blockControllers.Clear();
    }

    private IEnumerator DialogueCoroutine(List<Dictionary<string, object>> dialogueData)
    {
        for(int i = 0; i < dialogueData.Count; i++)
        {
            // delay만큼 대기
            float delay;
            try
            {
                string tmp = dialogueData[i]["delay"].ToString();
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
                    float lifetime;
                    try
                    {
                        string tmp = dialogueData[i]["lifetime"].ToString();
                        lifetime = Convert.ToSingle(tmp);
                    }
                    catch { lifetime = defaultLifetime; }

                    GenerateDialogueBlock(dialogueData[i]["character"].ToString(), 
                        dialogueData[i]["text"].ToString(), lifetime);
                    break;
                }
                yield return null;
            }
        }
    }

    private void GenerateDialogueBlock(string character, string text, float lifetime)
    {
        // Object Pool을 통해 Dialogue Block 받아 온 뒤 Dialogue Block Controller 초기화
        GameObject newObj = _objectPool.GetObject();
        DialogueBlockController newController = newObj.GetComponent<DialogueBlockController>();
        _blockControllers.Add(newController);

        for (int i = 0; i < _blockControllers.Count; i++)
            _blockControllers[i].MoveTo(_offset * -i);

        string processedText = _textProcessor.Process(text);
        Sprite charSprite;
        switch (character.ToLower())
        {
            case "hamster": charSprite = hamster; break;
            case "radio": charSprite = radio; break;
            default: charSprite = null; Debug.LogError($"character 이름 이상: {character}"); break;
        }
        newController.InitDialogueBlock(fadeDuration, charSprite, processedText, _objectPool);
        newController.SetPosition(_offset * (_blockControllers.Count - 1) * -1);
        newController.Show();
        
        // lifetime이 끝나면 dialogue를 destroy하도록 설정
        // 0.05f : 삭제와 동시에 새로운 창이 생성이 될 때 _blockControllers.Count가 모호한 부분을 방지
        DOVirtual.DelayedCall(lifetime - 0.05f, () => RemoveDialogue(newController));
    }

    private void RemoveDialogue(DialogueBlockController controller)
    {
        controller.Remove();
        _blockControllers.Remove(controller);
        for (int i = 0; i < _blockControllers.Count; i++)
            _blockControllers[i].MoveTo(_offset * -i);
    }
    
    public void StartDialogue(string fileName)
    {
        // file 이름을 통해 일련의 dialogue 진행
        // ClearDialogue();
        string path = System.IO.Path.Combine("Dialogues", fileName);
        List<Dictionary<string, object>> dialogueData = CSVReader.Read(path);

        if (dialogueData == null) return;

        StartCoroutine(DialogueCoroutine(dialogueData));
    }

    public void StartDialogue(string character, string text, float lifetime)
    {
        // 직접 한 줄 짜리 dialogue 출력
        // ClearDialogue();
        GenerateDialogueBlock(character, text, lifetime);
    }

    public void StartDialogue(int idx)
    {
        // csv 파일을 통해 한 줄 짜리 dialogue를 출력
        // ClearDialogue();

        if (idx >= _oneLineDialogueData.Count+1)
        {
            Debug.LogError($"Index out of range [idx:{idx}, max index:{_oneLineDialogueData.Count+1}]");
            return;
        }
        else if (idx < 2)
        {
            Debug.LogError($"index는 2부터 시작입니다 [idx:{idx}]");
            return;
        }
        
        float lifetime;
        try
        {
            idx--;
            string tmp = _oneLineDialogueData[idx]["lifetime"].ToString();
            lifetime = Convert.ToSingle(tmp);
        }
        catch { lifetime = defaultLifetime; }
        
        GenerateDialogueBlock(_oneLineDialogueData[idx]["character"].ToString(),
            _oneLineDialogueData[idx]["text"].ToString(),
            lifetime);
    }

    public void DebugDialogue(string fileName)
    {
        StartDialogue(fileName);
    }

    public void DebugOneLineDialogue(string idx)
    {
        if (int.TryParse(idx, out int index))
        {
            StartDialogue(index);
        }
    }
}
