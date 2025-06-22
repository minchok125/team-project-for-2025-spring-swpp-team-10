using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] GameObject notAcceptTxt;
    [SerializeField] private Image fadePanel;

    private void Awake()
    {
        fadePanel.color = Color.clear;
        fadePanel.gameObject.SetActive(false);
    }

    private IEnumerator LoadSceneCoroutine()
    {
        fadePanel.gameObject.SetActive(true);
        fadePanel.DOColor(Color.black, 3f);
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene(2);
    }

    public void GoToOpening(int idx)
    {
        string name = nameInputField.text.Trim();
        if (name.Length < 2 || name.Length > 20)
        {
            notAcceptTxt.SetActive(true);
            return;
        }

        PersistentDataManager.Instance.mainSceneIndex = idx;
        PersistentDataManager.Instance.playerName = name;
        StartCoroutine(LoadSceneCoroutine());
    }
}
