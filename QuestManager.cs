//-----------------------------------------------------------------------------------------------------
// FileName: QuestManager.cs
// Description: System still work in progress but is designed to manage what quests are available to the player and what the statis of each quest is. It is stored on a gameobject in the scene
// Author: Julian Beiboer
// Date: 20/01/2023
//-----------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public Quests[] LevelQuestGroups;
    public GameObject[] Locations;

    [SerializeField]
    private int _currentQuest = 0;

    private void Start()
    {
        LoadQuest(_currentQuest);
    }
    private void LoadQuest(int questNum)
    {
        LoadPassengersToStartingLocations(questNum);
        HideLocationsWithoutPassengers();
    }
    private void HideLocationsWithoutPassengers()
    {
        foreach (GameObject location in Locations)
        {
            if (!location.GetComponent<PickUpLocation>().PassengersAwaitingPickUp())
            {
                location.SetActive(false);
            }
            else
            {
                location.SetActive(true);
            }
        }
    }

    private void LoadPassengersToStartingLocations(int questNumber)
    {
        foreach (Passenger passenger in LevelQuestGroups[questNumber].passengers)
        {
            int locationNumber = passenger.PickUpLocation;//PassengerQuests[passenger.JourneyNumber].PickUpLocation;

            Locations[locationNumber].GetComponent<PickUpLocation>().AddPassengerToLocation(passenger);
        }
    }
    void SetCurrentQuest(int value)
    {
        _currentQuest = value;
    }
    void IncrementCurrentQuest(int value)
    {
        _currentQuest += value;
    }
    public int GetCurrentQuest()
    {
        return _currentQuest;
    }

    public void CheckIfQuestComplete()
    {
        if (LevelQuestGroups[GetCurrentQuest()].IsQuestComplete())
        {
            if(_currentQuest + 1 < LevelQuestGroups.Length)
            {
                IncrementCurrentQuest(1);
                //Load next set of journeys
                LoadQuest(_currentQuest);
            }
            else
            {
                Debug.Log("No More quests");
            }
        }
    }
}

[System.Serializable]
public class Quests
{
    public string QuestName;
    public Passenger[] passengers;
    public bool QuestComplete = false;


    public bool IsQuestComplete()
    {
        if (CheckIfAllPassengerQuestsComplete())
        {
            QuestComplete = true;
            return QuestComplete;
        }
        return false;
    }
    bool CheckIfAllPassengerQuestsComplete()
    {
        foreach (Passenger passenger in passengers)
        {
            if (!passenger.JourneyComplete) return false;
        }
        return true;
    }

    
}

[System.Serializable]
public class Passenger
{
    public string Name;
    public string DialogueCodeName = "Ch0";
    public int PickUpLocation;
    public int DropOffLocation;
    public bool JourneyComplete = false;

    public void SetJourneyStatus(bool value)
    {
        JourneyComplete = value;
    }
   
}

