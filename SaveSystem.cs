//-----------------------------------------------------------------------------------------------------
// FileName: SaveSystem.cs
// Description: Saves player data stored on the gameStateData obj into a bineary file on the players computer. This file can then be accessed later for loading.
// Author: Julian Beiboer
// Date: 20/05/2022
//-----------------------------------------------------------------------------------------------------
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{
    public static void SaveData(GameStateData gameStateData, string name="PlayerGame")
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/" + name + ".data";

        FileStream stream = new FileStream(path, FileMode.Create);
        SavedData data = new SavedData(gameStateData);
        formatter.Serialize(stream, data);
        stream.Close();
        
    }

    public static SavedData LoadData(string name= "PlayerGame")
    {
        string path = Application.persistentDataPath + "/" + name + ".data";
        if (File.Exists(path))
        {
            Debug.Log("Save File found at" + path);
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            SavedData data = formatter.Deserialize(stream) as SavedData;
            stream.Close();

            return data;
        }
        else
        {
            Debug.LogWarning("Save File not found at" + path);
            return null;
        }
    }
    
}
[System.Serializable]
public class SavedData
{
    public bool firstTimeSpawn;   
    public float musicVolume;
    public float SFXVolume;
    public float UIVolume;
    public bool vsync;
    public bool fullscreen;
    public int screenResolutionX;
    public int screenResolutionY;
    [Header("Game Progression")]
    public int[] timeTrialHighscores;
    public int mechanicVisited;
    public bool lightHousesFound;
    // dialogue Variables 
    public int partsFound;
    public int colorsFound;
    public int altitudeFound;
    public int sirFluffington;
    public int sergeantMerlin;
    public int captainZuko;

    public bool[] wingsUnlocked;
    public bool[] enginesUnlocked;
    public bool[] fuselagesUnlocked;
    public bool[] colorSetsUnlocked;
    public bool[] photosUnlocked;

    public PlaneVisualData planeVisualData;

    public SavedData(GameStateData data)
    {
        firstTimeSpawn = data.firstTimeSpawn;
        musicVolume = data.musicVolume;
        SFXVolume = data.SFXVolume;
        UIVolume = data.UIVolume;
        vsync = data.vsync;
        fullscreen = data.fullscreen;
        screenResolutionX = data.screenResolutionX;
        screenResolutionY = data.screenResolutionY;

        timeTrialHighscores = data.timeTrialHighscores;
        mechanicVisited = data.mechanicVisited;
        lightHousesFound = data.lightHousesFound;
        partsFound = data.partsFound;
        colorsFound = data.colorsFound;
        altitudeFound = data.altitudeFound;
        sirFluffington = data.sirFluffington;
        sergeantMerlin = data.sergeantMerlin;
        captainZuko = data.captainZuko;

    wingsUnlocked = data.wingsUnlocked;
        enginesUnlocked = data.enginesUnlocked;
        fuselagesUnlocked = data.fuselagesUnlocked;
        colorSetsUnlocked = data.colorSetsUnlocked;
        photosUnlocked = data.photosUnlocked;
        planeVisualData = data.planeVisualData;
    }
}
