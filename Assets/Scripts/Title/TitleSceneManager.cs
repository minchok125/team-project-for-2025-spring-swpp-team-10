using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInputField;
    [SerializeField] GameObject notAcceptTxt;

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
        SceneManager.LoadScene(idx);
    }
}
