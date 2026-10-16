using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PilotManager : MonoBehaviour
{
    private ShipManager shipManager;

    [Header("List")]
    [SerializeField] private List<Button> pilots;
    public List<PilotData> pilotsData;

    [Header("bool")]
    [SerializeField] private bool[] pilotSelected = new bool[5];

    [Header("Button")]
    [SerializeField] private Button hangar;
    public GameObject confirmPilotButton;
    public GameObject yesPilotButton;

    [Header("GameObject")]
    public GameObject changePilotPopup;
    public GameObject pilotObjects;

    [Header("Sprites")]
    public Sprite pilotDeselectedSprite;
    public Sprite pilotSelectedSprite;

    private void Awake()
    {
        shipManager = FindAnyObjectByType<ShipManager>();
    }


    /// <summary>
    /// This is used to select a pilot when we click on the pilot cards.
    /// </summary>
    public void OnSelected(int pilotIndex)
    {
        AudioManager.Instance.PlayMusic("Button Click");
        pilotSelected[pilotIndex] = true;
        GameManager.Instance.HighlightPilot(pilotsData[pilotIndex]);
        for(int x = 0;x < 5;x++)
            pilotObjects.transform.GetChild(x).GetChild(0).Find("Border Frame").GetComponent<Image>().sprite = pilotDeselectedSprite;
        
        pilotObjects.transform.GetChild(pilotIndex).GetChild(0).Find("Border Frame").GetComponent<Image>().sprite = pilotSelectedSprite;
    }


    /// <summary>
    /// This is used to select pilot from popup.
    /// </summary>
    public void OnPilotChange()
    {
        GameManager.Instance.YesChangePilot();
        GameManager.Instance.SetPilot(GameManager.Instance.highlightedPilot);
        GameManager.Instance.currencyText.text = GameManager.Instance.currency.ToString("#,#", CultureInfo.InvariantCulture);
    }


    public void OnDeselected(int pilotIndex)
    {
        pilotSelected[pilotIndex] = false;
    }


    /// <summary>
    /// This is used when we open the Pilot Panel it keeps the pilot selected.
    /// </summary>
    public void AutoSelect()
    {
        if(GameManager.Instance.selectedPilot == null)
            return;

        for (int i = 0; i < pilots.Count; i++)
        {
            if (pilots[i].name == GameManager.Instance.selectedPilot.name)
            {
                pilots[i].Select();
                OnSelected(i);
                break;
            }
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !IsConfirmPilotButtonClicked() && !IsYesPilotButtonClicked())
        {
            if (!AnyPilotSelected())
            {
                hangar.interactable = false;
            }
        }

        if (GameManager.Instance.highlightedPilot != null && GameManager.Instance.selectedPilot != null && GameManager.Instance.selectedPilot != GameManager.Instance.highlightedPilot)
            hangar.interactable = true;
        else
            hangar.interactable = false;
    }


    /// <summary>
    /// This is used to check if the ConfirmPilot button is selected.
    /// </summary>
    private bool IsConfirmPilotButtonClicked()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
            return clickedObject == confirmPilotButton;
        }
        return false;
    }


    /// <summary>
    /// This is used to check if the Yes button is selected.
    /// </summary>
    private bool IsYesPilotButtonClicked()
    {
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
        {
            GameObject clickedObject = EventSystem.current.currentSelectedGameObject;
            return clickedObject == yesPilotButton;
        }
        return false;
    }


    /// <summary>
    /// This is used to check if any pilot is selected.
    /// </summary>
    private bool AnyPilotSelected()
    {
        foreach (bool selected in pilotSelected)
        {
            if (selected)
                return true;
        }
        return false;
    }


    /// <summary>
    /// This is responsible for increasing ship's stats as per the pilots stats.
    /// </summary>
    public void UpdateShipsSpecsDependingOnPilot()
    {
        for (int i = 0; i < shipManager.shipSpecsUI.Count; i++)
        {
            for (int j = 0; j < GameManager.Instance.selectedPilot.skills.Count; j++)
            {
                if (shipManager.shipSpecsUI[i].name.Equals(GameManager.Instance.selectedPilot.skills[j].skill_Type.ToString()))
                {
                    print(GameManager.Instance.selectedPilot.skills[j].skill_Type.ToString());
                }
            }
        }
    }
}