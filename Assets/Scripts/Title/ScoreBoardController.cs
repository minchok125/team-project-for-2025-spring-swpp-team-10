using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ScoreEntry
{
    public string playerName;
    public float clearTime;
}

[System.Serializable]
public class StageScoreData
{
    public List<ScoreEntry> scores = new List<ScoreEntry>();
}

public class ScoreBoardController : MonoBehaviour
{
    [SerializeField] private Image[] buttonImgs;
    [SerializeField] private TextMeshProUGUI rankingTxt;

    private void OnEnable()
    {
        PlayerPrefs.DeleteAll();
        OnButtonClick(0);
        PersistentDataManager.Instance.SaveScore(0, "가나다", 666.55f);
        PersistentDataManager.Instance.SaveScore(0, "마바사", 163.711f);
        PersistentDataManager.Instance.SaveScore(0, "아아라", 1773.1767f);
        PersistentDataManager.Instance.SaveScore(1, "qwerqwer", 563.1f);
        PersistentDataManager.Instance.SaveScore(1, "마바자", 1636.0f);
        for (int i = 0; i < 30; i++)
            PersistentDataManager.Instance.SaveScore(0, "가나다", 666.55f);
        PrintTopScores(0);
    }

    public void OnButtonClick(int idx)
    {
        for (int i = 0; i < buttonImgs.Length; i++)
        {
            if (i == idx)
                buttonImgs[i].color = new Color(0.85f, 0.85f, 0.85f, 1f);
            else
                buttonImgs[i].color = Color.white;
        }
        PrintTopScores(idx);
    }

    List<ScoreEntry> LoadScores(int mainSceneIndex)
    {
        string key = $"MainScene{mainSceneIndex}";
        string json = PlayerPrefs.GetString(key, "");

        if (string.IsNullOrEmpty(json))
            return new List<ScoreEntry>();

        StageScoreData data = JsonUtility.FromJson<StageScoreData>(json);
        return data.scores;
    }

    void PrintTopScores(int mainSceneIndex)
    {
        var scores = LoadScores(mainSceneIndex);
        scores.Sort((a, b) => a.clearTime.CompareTo(b.clearTime)); // 빠른 시간 순

        int count = Mathf.Min(scores.Count, 25);

        rankingTxt.text = "";
        if (count == 0) rankingTxt.text = "--";
        for (int i = 0; i < count; i++)
        {
            string rank = $"{i + 1}위".PadRight(4);
            string name = CenterAlign(scores[i].playerName, 22);

            int clearMin = (int)(scores[i].clearTime / 60f);
            float clearSec = scores[i].clearTime % 60f;
            int clearMicroSec = (int)((clearSec % 1f) * 10);

            rankingTxt.text += $"{rank}    {name}    {clearMin}분 {(int)clearSec}.{clearMicroSec}초 \n";
        }
    }

    string CenterAlign(string text, int totalWidth)
    {
        if (text.Length >= totalWidth)
            return text;

        int leftPadding = (totalWidth - text.Length) / 2;
        int rightPadding = totalWidth - text.Length - leftPadding;

        return new string(' ', leftPadding) + text + new string(' ', rightPadding);
    }
}
