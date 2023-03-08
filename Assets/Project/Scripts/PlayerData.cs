using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // general player data
    [Header("General")]
    public int playerLevel = 1;
    public Dictionary<int, int> levelHighScore;

    public PlayerData()
    {

    }

    public void Initialize()
    {
        playerLevel = 1;
        levelHighScore = new Dictionary<int, int>();
    }

    public void LevelUp()
    {
        playerLevel += 1;
    }

    public void UpdateHighScore(int level, int score)
    {
        if (levelHighScore.ContainsKey(level))
        {
            if (score > levelHighScore[level]) levelHighScore[level] = score;
        } else
        {
            levelHighScore.Add(level, score);
        }
    }
}
