using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyManager : MonoBehaviour
{
    [Header("bool")]
    public bool hasShip;
    public bool hasWeapon;
    public bool hasPod;

    [SerializeField] private int ownedShipIndex;

    public enum BuyType
    {
        Ship,
        Weapon,
        Pods
    }

    public BuyType type;


    /// <summary>
    /// This will be called on Onclick of the slots buttons when selected.
    /// </summary>
    /// <param name="shipIndex"></param>
    public void OnSelected()
    {
        if (hasShip)
            GameManager.Instance.confirmShipButton.GetComponent<Button>().interactable = true;
    }

    /// <summary>
    /// This will be called on Onclick of the slots buttons when deselected.
    /// </summary>
    public void OnDeselected()
    {
        //GameManager.Instance.confirmShipButton.GetComponent<Button>().interactable = false;
    }

    /// <summary>
    /// This is used to check which slot for buy/mod ship is selected.
    /// </summary>
    /// <param name="index"></param>
    public void Buy(int index)
    {
        GameManager.Instance.selectAndUpgradeYourShipPanel.GetComponent<Image>().sprite = GameManager.Instance.buyPanelSprite;
        GameManager.Instance.buy_modShipPopup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            "Are you sure you want to buy this ship?";
        for (int i = 0; i < 4; i++)
            GameManager.Instance.slotSelectedForBuy[i] = false;
        for (int i = 0; i < 4; i++)
            GameManager.Instance.slotSelectedForMod[i] = false;
        GameManager.Instance.slotSelectedForBuy[index] = true; 
    }

    /// <summary>
    /// This is used to Modify the current selected ship.
    /// </summary>
    /// <param name="index"></param>
    public void Mod(int index)
    {
        // Reset selected pods and guns when entering Mod.
        GameManager.Instance.selectedPods.Clear();
        GameManager.Instance.selectedWeapons.Clear();

        GameManager.Instance.confirmShipButton.GetComponent<Button>().interactable = true;

        GameManager.Instance.selectAndUpgradeYourShipPanel.GetComponent<Image>().sprite = GameManager.Instance.modPanelSprite;
        GameManager.Instance.HighLightShip(GameManager.Instance.ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipData);

        GameManager.Instance.shipTotalPrice = 0;
        GameManager.Instance.buy_modShipPopup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
            "Are you sure you want to mod this ship?";
        
        GameManager.Instance.navigationButtons.transform.GetChild(1).gameObject.SetActive(true);
        GameManager.Instance.navigationButtons.transform.GetChild(1).GetChild(0).GetComponent<Button>().interactable = false;
        // GameManager.Instance.navigationButtons.transform.GetChild(1).GetChild(0).GetComponent<Button>().transition =
        //     Selectable.Transition.ColorTint;

        for (int i = 0; i < 4; i++)
            GameManager.Instance.slotSelectedForMod[i] = false;
        for (int i = 0; i < 4; i++)
            GameManager.Instance.slotSelectedForBuy[i] = false;

        GameManager.Instance.slotSelectedForMod[index] = true;

        // for(int x = 0; x < 4; x++)
        // {
            // if(slotSelectedForBuy[x] == true)
            // for (int i = 0; i < 4; i++)
            //     GameManager.Instance.ownedShipSlots[index].GetComponent<BuyManager>().extraSpecsData[i].value = 0;
        // }

        for (int i = 0; i < slotShipData.podData.Count; i++)
        {
            GameManager.Instance.selectedPods.Add(slotShipData.podData[i]);
            GameManager.Instance.podsAdded++;
        }

            
        for (int i = 0; i < slotShipData.weaponData.Count; i++)
        {
            GameManager.Instance.selectedWeapons.Add(slotShipData.weaponData[i]);
            GameManager.Instance.weaponsAdded++;
        }
    }

    /// <summary>
    /// This is used to check which slot for repair ship is selected and calculate repair price.
    /// </summary>
    /// <param name="index"></param>
    public void Repair(int index)
    {
        int pilotDiscount = 0;
        int repairPrice = 10;

        GameManager.Instance.quickRepairDiscountPriceText.text = "";
        GameManager.Instance.mythLogo.enabled = true;

        // Calculate additional values from pilot's skills
        foreach(Skill skill in GameManager.Instance.selectedPilot.skills)
        {
            if(skill.skill_Type == Skill.Skill_Type.RepairCost)
            {
                pilotDiscount += skill.value;
                // Debug.Log("pilotDiscount: " + (float)(100 + pilotDiscount) / 100);
                break;
            }
        }

        if(GameManager.Instance.currency > 0)
        {
            for (int i = 0; i < 4; i++)
            GameManager.Instance.slotSelectedForRepair[i] = false;

            GameManager.Instance.slotSelectedForRepair[index] = true;

            int totalHealth = GameManager.Instance.ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipData.hull;
            int hullPodsValue = 0;
            for(int x = 0;x < GameManager.Instance.ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData.Count;x++)
            {
                // totalHealth += ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData[x].data.data_Type.ToString().Equals("Hull");
                if(GameManager.Instance.ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData[x].data.data_Type.ToString() == "Hull")
                {
                    hullPodsValue += GameManager.Instance.ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData[x].data.value;
                }
            }

            int repairTotalPrice = Convert.ToInt32(
                (1 - GameManager.Instance.ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipHealth)
                    * (totalHealth + hullPodsValue) * repairPrice
                );

            GameManager.Instance.repairShipTotalHealth = totalHealth + hullPodsValue;

            Debug.Log("repairTotalPrice: " + repairTotalPrice);

            GameManager.Instance.repairTotalPrice = repairTotalPrice;

            float repairAmount;
            // Set popup price text
            if (repairTotalPrice > GameManager.Instance.currency)
            {
                GameManager.Instance.quickRepairPriceText.text = GameManager.Instance.currency.ToString();
                repairAmount = GameManager.Instance.currency / repairPrice;
                repairTotalPrice = Convert.ToInt32(repairAmount * repairPrice);

                GameManager.Instance.totalPrice = repairTotalPrice;
            }
            else if(repairTotalPrice != 0)
            {
                repairAmount = (1 - GameManager.Instance.ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipHealth)
                    * (totalHealth + hullPodsValue);
                
                GameManager.Instance.totalPrice = repairTotalPrice;
                GameManager.Instance.quickRepairPriceText.text = repairTotalPrice.ToString();
            }
            else
                return;

            if(pilotDiscount < 0)
            {
                GameManager.Instance.mythLogo.enabled = false;
                GameManager.Instance.quickRepairPriceText.text = "<u color=#DB60D8>" + repairTotalPrice.ToString() + "</u>";

                // Calculate Repair Price
                int repairDiscountPrice = Convert.ToInt32(
                    repairTotalPrice * ((float)(100 + pilotDiscount) / 100)
                    );
                
                GameManager.Instance.totalPrice = repairDiscountPrice;
                
                GameManager.Instance.quickRepairDiscountPriceText.text = repairDiscountPrice.ToString();
            }

            GameManager.Instance.quickRepairPopup.SetActive(true);
            GameManager.Instance.quickRepairPopup.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Are you sure you want to repair " + repairAmount.ToString("F1") + " hull for:";

        }
    }

    /// <summary>
    /// This will refresh the slots and update the image and buttons depending on how many ships a player has bought.
    /// </summary>
    public void RefreshData()
    {
        switch (type)
        {
            case BuyType.Ship:
                if (hasShip)
                {
                    transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1);
                    transform.GetChild(0).GetComponent<Image>().sprite = slotShipData.shipData.shipAvatar;
                    transform.Find("HealthBar").GetChild(1).GetComponent<Image>().fillAmount = slotShipData.shipHealth;
                    if(slotShipData.shipHealth == 1 || GameManager.Instance.currency <= 0)
                        transform.GetChild(3).transform.GetChild(2).GetComponent<Button>().interactable = false;
                    else
                         transform.GetChild(3).transform.GetChild(2).GetComponent<Button>().interactable = true;

                    transform.GetChild(1).GetComponent<Image>().color = new(1, 1, 1, 0);
                    transform.GetChild(2).GetComponent<TextMeshProUGUI>().text =
                        slotShipData.shipData.shipType + " - Class " +
                        slotShipData.shipData.shipClass;
                    transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(true);
                    transform.GetChild(3).transform.GetChild(0).gameObject.SetActive(false);
                    // Turns Sell button interactable
                    transform.GetChild(3).transform.GetChild(3).GetComponent<Button>().interactable = true;
                    // Turns the healthbar visible
                    transform.GetChild(4).gameObject.SetActive(true);

                    GetComponent<Image>().sprite = GameManager.Instance.ownedShipPanel;

                    GetComponent<Button>().interactable = true;
                }
                else
                {
                    transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
                    transform.GetChild(1).GetComponent<Image>().color = new(1,1,1,1);
                    if (ownedShipIndex == 0)
                        transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Slot - I";
                    if (ownedShipIndex == 1)
                        transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Slot - II";
                    if (ownedShipIndex == 2)
                        transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Slot - III";
                    if (ownedShipIndex == 3)
                        transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = "Slot - IV";
                    transform.GetChild(3).transform.GetChild(1).gameObject.SetActive(false);
                    transform.GetChild(3).transform.GetChild(0).gameObject.SetActive(true);
                    transform.Find("HealthBar").gameObject.SetActive(false);
                    GetComponent<Button>().interactable = false;
                    GetComponent<Image>().sprite = GameManager.Instance.notOwnedShipPanel;
                    transform.GetChild(3).transform.GetChild(2).GetComponent<Button>().interactable = false;
                    transform.GetChild(3).transform.GetChild(3).GetComponent<Button>().interactable = false;
                }
                break;
            case BuyType.Weapon:
                // do something
                break;
            case BuyType.Pods:
                // do something
                break;
        }
    }

    /// <summary>
    /// This button will be called on the each sell button of the slot of ship.
    /// </summary>
    /// <param name="index"></param>
    public void Sell(int index) => GameManager.Instance.whichSlotShipToSell = index;

    public SlotShipData slotShipData;

    public ExtraShipSpecsData[] extraSpecsData = new ExtraShipSpecsData[5];
}

[Serializable]
public class SlotShipData
{
    public ShipData shipData;

    public int finalPrice;
    public float shipHealth;
    public List<PodData> podData;
    public List<WeaponData> weaponData;
}

[Serializable]
public class ExtraShipSpecsData
{
    public enum Specs_Type
    {
        Speed,
        Evasion,
        Shields,
        Hull,
        Price
    }

    public Specs_Type skill_Type;
    public int value;
}