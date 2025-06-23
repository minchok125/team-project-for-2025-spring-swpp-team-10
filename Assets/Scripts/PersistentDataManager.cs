using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HamsterSkin { Golden, Gray }

public class PersistentDataManager : PersistentSingleton<PersistentDataManager>
{
    public string playerName;
    public int mainSceneIndex; // 0: 처음부터, 1: 거실, 2: 욕실, 3: 지하 구역
    public float clearTime; // 시작 ~ 클리어까지 시간

    public void SaveScore()
    {
        SaveScore(mainSceneIndex, playerName, clearTime);
    }

    public void SaveScore(float clearTime)
    {
        this.clearTime = clearTime;
        SaveScore(mainSceneIndex, playerName, clearTime);
    }

    public void SaveScore(int mainSceneIndex, string playerName, float clearTime)
    {
        string key = $"MainScene{mainSceneIndex}";

        // 기존 기록 불러오기
        string json = PlayerPrefs.GetString(key, "");
        StageScoreData stageData = string.IsNullOrEmpty(json) ? new StageScoreData() : JsonUtility.FromJson<StageScoreData>(json);

        // 기존 동일한 이름의 기록이 있는지 확인
        ScoreEntry existingEntry = stageData.scores.Find(entry => entry.playerName == playerName);

        if (existingEntry != null)
        {
            // 기존 기록보다 클리어 시간이 더 짧으면 갱신
            if (clearTime < existingEntry.clearTime)
            {
                existingEntry.clearTime = clearTime;
            }
        }
        else
        {
            // 새로운 기록 추가
            stageData.scores.Add(new ScoreEntry
            {
                playerName = playerName,
                clearTime = clearTime
            });
        }

        // 저장
        string updatedJson = JsonUtility.ToJson(stageData);
        PlayerPrefs.SetString(key, updatedJson);
        PlayerPrefs.Save();
    }
}