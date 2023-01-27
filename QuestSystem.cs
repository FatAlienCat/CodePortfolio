//-----------------------------------------------------------------------------------------------------
// FileName: QuestSystem.cs
// Description: Works in conjunction with the quest manager, this is placed on the player object and is triggered when the player starts or finishes a quest.
// Author: Julian Beiboer
// Date: 20/01/2023
//-----------------------------------------------------------------------------------------------------
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class QuestSystem : MonoBehaviour
{
    [SerializeField]
    private QuestManager _questManager;
    [SerializeField]
    private DialogueInput _dialogueInput;
    [SerializeField]
    private List<Passenger> _passengersOnboard;

    [SerializeField]
    private Passenger[] _passengerData;
    [ReadOnly]
    [SerializeField]
    private GameObject[] _locations;

    [ReadOnly]
    [SerializeField]
    private List<int> _currentlyActivePickups;

    private void Start()
    {
        _locations = _questManager.Locations;
    }
    public void PickUpPassenger(Passenger newPassenger)
    {
        int questNumber = _questManager.GetCurrentQuest();
        Debug.Log("PickUpOff");
        AddPassenger(newPassenger);
        SetAllLocationVisibility(false);

        //Load drop off location
        LoadDropOffLocation(newPassenger.DropOffLocation, true);

        //Start Dialogue
        string dialogueStart = newPassenger.DialogueCodeName + "_" + _questManager.LevelQuestGroups[questNumber].QuestName + "_start";
        _dialogueInput.SetCurrentNode(dialogueStart);
    }


    public void DropOffPassenger()
    {
        int questNumber = _questManager.GetCurrentQuest();
        Debug.Log("DropOff");
        Passenger dropOffPassenger = _passengersOnboard[0];// dodgey assumes that there is only one passenger
        //Start Dialogue
        string dialogueEnd = dropOffPassenger.DialogueCodeName + "_" + _questManager.LevelQuestGroups[0].QuestName + "_dropOff";
        _dialogueInput.SetCurrentNode(dialogueEnd);

        dropOffPassenger.SetJourneyStatus(true); // Set passenger journey to complete
        _questManager.CheckIfQuestComplete(); // Check if quest complete

        LoadDropOffLocation(dropOffPassenger.DropOffLocation, false);
        RemovePassenger(dropOffPassenger);
        //dropOffPassenger.JourneyNumber++; // increment quest
        

        ShowPickUpsWithPassengers();// Show locations with passengers
        //return dropOffPassenger;
    }
    private void SetAllLocationVisibility(bool visible)
    {
        foreach (GameObject location in _locations)
        {
            location.SetActive(visible);
        }
    }
    private void ShowPickUpsWithPassengers()
    {
        foreach (GameObject pickup in _locations)
        {
            if (pickup.GetComponent<PickUpLocation>().PassengersAwaitingPickUp())
            {
                LoadPickUpLocation(pickup, true);
            }
            else
            {
                LoadPickUpLocation(pickup, false);
                //pickup.SetActive(false);
            }
        }
    }


    private Passenger GetPassengerData(string name)
    {
        return Array.Find(_passengerData, passenger => passenger.Name == name);
    }
    private void LoadDropOffLocation(int num, bool visibility)
    {
        _locations[num].GetComponent<PickUpLocation>().SwitchLocationType(PickUpLocation.LocationType.DropOff);
        _locations[num].SetActive(visibility);
        //_dropOffLocations[num].SetActive(visibility);
    }

    private void LoadPickUpLocation(GameObject pickUp, bool visibility)
    {
        pickUp.GetComponent<PickUpLocation>().SwitchLocationType(PickUpLocation.LocationType.PickUp);
        pickUp.SetActive(visibility);
    }




    void AddPassenger(Passenger newPassenger)
    {
        if (!_passengersOnboard.Contains(newPassenger))
        {
            _passengersOnboard.Add(newPassenger);
        }
        else
        {
            Debug.LogError("Passenger:" + newPassenger.Name + " already onboard");
        }
    }

    void RemovePassenger(Passenger passenger)
    {
        if (_passengersOnboard.Contains(passenger))
        {
            _passengersOnboard.Remove(passenger);
        }
        else
        {
            Debug.LogError("Passenger:" + passenger.Name + " not onboard");
        }
    }



}





