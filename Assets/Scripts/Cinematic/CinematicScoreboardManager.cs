using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CinematicScoreboardManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI completeText;
    [SerializeField] private TextMeshProUGUI rankingText;
    [SerializeField] private TextMeshProUGUI clearTimeText;
    
    public void UpdateScoreboard()
    {
        completeText.text = GameManager.Instance.cinematicMode == GameManager.CinematicModes.GoodEnding ? "Mission\nCompleted!" 
            : "Mission\nCompleted...?";
        
        PrintTopScores(PersistentDataManager.Instance.mainSceneIndex);
        PrintClearTime();
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

        int count = Mathf.Min(scores.Count, 8);

        rankingText.text = "";
        if (count == 0) rankingText.text = "--";
        for (int i = 0; i < count; i++)
        {
            string rank = $"{i + 1}위".PadRight(4);
            string name = CenterAlign(scores[i].playerName, 15);

            int clearMin = (int)(scores[i].clearTime / 60f);
            float clearSec = scores[i].clearTime % 60f;
            int clearMicroSec = (int)((clearSec % 1f) * 10);

            rankingText.text += $"{rank}    {name}    {clearMin}분 {(int)clearSec}.{clearMicroSec}초 \n";
        }
    }

    void PrintClearTime()
    {
        // string rank = $"{i + 1}위".PadRight(4);
        // string name = CenterAlign(PersistentDataManager.Instance.playerName, 22);

        int clearMin = (int)(PersistentDataManager.Instance.clearTime / 60f);
        float clearSec = PersistentDataManager.Instance.clearTime % 60f;
        int clearMicroSec = (int)((clearSec % 1f) * 10);

        clearTimeText.text += $"{name}: {clearMin}분 {(int)clearSec}.{clearMicroSec}초 \n";
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
