using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // general player data
    [Header("General")]
    public int playerLevel = 1;


    public PlayerData()
    {

    }

    public void Initialize()
    {
        playerLevel = 1;
    }

    public void LevelUp()
    {
        playerLevel += 1;
    }
}
