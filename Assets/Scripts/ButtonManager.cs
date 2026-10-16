using System;
using System.Globalization;
using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Skill;

public class ButtonManager : MonoBehaviour
{
    public Sprite defaultStateImage;
    public Sprite selectedStateImage;
    public Sprite hoverStateImage;

    private PilotManager pilotManager;
    private ShipManager shipManager;
    [SerializeField] private BuyManager buyManager;

    public bool defaultState = true;
    [SerializeField] private bool isSelected;

    public enum ButtonType
    {
        Pilot,
        ShipClasses,
        Weapon,
        Pods,
        PodSlot,
        WeaponSlot,
        MenuButtons,
        SellShip,
        ShipTypes,
        launch
    }

    public ButtonType type;

    public void OnEnable()
    {
        switch (type)
        {
            case ButtonType.Pilot:
                PilotInfo();
                break;
            case ButtonType.ShipTypes:
                ShipClassesInfo();
                break;
            case ButtonType.Weapon:
                WeaponInfo();
                break;
            case ButtonType.Pods:
                PodInfo();
                break;
        }
    }

    private void Awake()
    {
        pilotManager = FindAnyObjectByType<PilotManager>();
        shipManager = FindAnyObjectByType<ShipManager>();
        buyManager = transform.parent.parent.GetComponent<BuyManager>();
    }


    /// <summary>
    /// Call this method on the buttons to change its state to selected.
    /// </summary>
    public void ChangeState(int buttonNumber)
    {
        if(type == ButtonType.MenuButtons)
        {
            GameManager.Instance.ResetMainMenuButtons(buttonNumber);
            // Added these resets for the buy and mod sections to work properly.
            GameManager.Instance.ResetSlotSelectedForBuyAndMod();
            GameManager.Instance.ResetBuyShipAndArmoryPanel();
        }
        else if(type == ButtonType.ShipClasses)
            GameManager.Instance.ResetShipClassesButtons(buttonNumber);
        else if (type == ButtonType.ShipTypes)
            GameManager.Instance.ResetShipTypesButtons(buttonNumber);

        GetComponent<Image>().sprite = selectedStateImage;
        defaultState = false;

        if(hoverStateImage != null)
            ChangeHighlightedSprite();
    }


    /// <summary>
    /// Call this method to change the highlighted sprite image
    /// </summary>
    public void ChangeHighlightedSprite()
    {
        SpriteState spriteState = GetComponent<Button>().spriteState;

        if (defaultState)
            spriteState.highlightedSprite = hoverStateImage;
        else
            spriteState.highlightedSprite = null;

        GetComponent<Button>().spriteState = spriteState;
    }


    /// <summary>
    /// Call this method on the buttons to reset its state.
    /// </summary>
    public void ResetState()
    {
        AudioManager.Instance.PlayMusic("Button Close Cancel");
        GetComponent<Image>().sprite = defaultStateImage;

        defaultState = true;

        if (hoverStateImage != null)
            ChangeHighlightedSprite();
    }


    /// <summary>
    /// This function populates all the pilot information from the scriptable objects.
    /// </summary>
    private void PilotInfo()
    {
        for (int i = 0; i < pilotManager.pilotsData.Count; i++)
        {
            if (gameObject.name == pilotManager.pilotsData[i].pilotName && gameObject.name != "Female Illuminated")
            {
                if(pilotManager.pilotsData[i].pilotName.Equals("Human Male"))
                {
                    transform.GetChild(0).GetComponent<Image>().sprite = pilotManager.pilotsData[i].pilotAvatar;
                    transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Origin: " + pilotManager.pilotsData[i].origin + "\n\n" +
                                 pilotManager.pilotsData[i].description + "\n\n" +
                                 pilotManager.pilotsData[i].skills[0].value + "% " + FormatEnumName(pilotManager.pilotsData[i].skills[0].skill_Type) + "\n" +
                                 pilotManager.pilotsData[i].skills[1].value + "% " + FormatEnumName(pilotManager.pilotsData[i].skills[1].skill_Type) + "\n";
                    transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = pilotManager.pilotsData[i].nickName;
                }

                if (pilotManager.pilotsData[i].pilotName.Equals("Female Demon"))
                {
                    transform.GetChild(0).GetComponent<Image>().sprite = pilotManager.pilotsData[i].pilotAvatar;
                    transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Origin: " + pilotManager.pilotsData[i].origin + "\n\n" +
                                 pilotManager.pilotsData[i].description + "\n\n" +
                                 "+" + pilotManager.pilotsData[i].skills[0].value + "% " + FormatEnumName(pilotManager.pilotsData[i].skills[0].skill_Type) + "\n" +
                                 "+" + pilotManager.pilotsData[i].skills[1].value + "% " + FormatEnumName(pilotManager.pilotsData[i].skills[1].skill_Type) + "\n";
                    transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = pilotManager.pilotsData[i].nickName;
                }

                if (pilotManager.pilotsData[i].pilotName.Equals("Male Orc"))
                {
                    transform.GetChild(0).GetComponent<Image>().sprite = pilotManager.pilotsData[i].pilotAvatar;
                    transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Origin: " + pilotManager.pilotsData[i].origin + "\n\n" +
                                 pilotManager.pilotsData[i].description + "\n\n" +
                                 "+" + pilotManager.pilotsData[i].skills[0].value + "% " + FormatEnumName(pilotManager.pilotsData[i].skills[0].skill_Type) + "\n" +
                                 "+" + pilotManager.pilotsData[i].skills[1].value + "% " + FormatEnumName(pilotManager.pilotsData[i].skills[1].skill_Type) + "\n";
                    transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = pilotManager.pilotsData[i].nickName;
                }

                if (pilotManager.pilotsData[i].pilotName.Equals("Female Alien"))
                {
                    transform.GetChild(0).GetComponent<Image>().sprite = pilotManager.pilotsData[i].pilotAvatar;
                    transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Origin: " + pilotManager.pilotsData[i].origin + "\n\n" +
                                 pilotManager.pilotsData[i].description + "\n\n" +
                                 pilotManager.pilotsData[i].skills[0].value + "% " + FormatEnumName(pilotManager.pilotsData[i].skills[0].skill_Type) + "\n" +
                                 "+" + pilotManager.pilotsData[i].skills[1].value + "% " + FormatEnumName(pilotManager.pilotsData[i].skills[1].skill_Type) + "\n";
                    transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = pilotManager.pilotsData[i].nickName;
                }
            }

            else if (gameObject.name == "Female Illuminated")
            {
                transform.GetChild(0).GetComponent<Image>().sprite = pilotManager.pilotsData[4].pilotAvatar;
                transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Origin: " + pilotManager.pilotsData[4].origin + "\n\n" +
                             pilotManager.pilotsData[4].description + "\n\n" +
                             "+" + pilotManager.pilotsData[4].skills[0].value + "% " + pilotManager.pilotsData[4].skills[0].skill_Type + "\n" +
                             "+" + pilotManager.pilotsData[4].skills[1].value + "% " + pilotManager.pilotsData[4].skills[1].skill_Type + "\n" +
                             "+" + pilotManager.pilotsData[4].skills[2].value + "% " + pilotManager.pilotsData[4].skills[2].skill_Type + "\n" +
                             "+" + pilotManager.pilotsData[4].skills[3].value + "% " + pilotManager.pilotsData[4].skills[3].skill_Type + "\n";
                transform.parent.GetChild(1).GetComponent<TextMeshProUGUI>().text = pilotManager.pilotsData[4].nickName;
            }
        }
    }


    /// <summary>
    /// Helper method to format the enum value with a space
    /// </summary>
    private string FormatEnumName(Skill_Type enumValue)
    {
        string enumName = enumValue.ToString();
        // Insert a space between "XP" and "Required"
        if (enumName == "XPRequired")
        {
            return "XP Required";
        }

        if (enumName == "RepairCost")
        {
            return "Repair Cost";
        }
        // For other enum values, just return the original enum name
        return enumName;
    }


    /// <summary>
    /// Enabling of the pilot hover image.
    /// </summary>
    public void PilotHoverImage()
    {
        AudioManager.Instance.PlayMusic("Button Hover");

        if (isSelected)
            return;

        transform.GetChild(2).gameObject.SetActive(true);
    }


    /// <summary>
    /// Disabling of the pilot hover image.
    /// </summary>
    public void PilotUnHoverImage()
    {
        transform.GetChild(2).gameObject.SetActive(false);
    }


    /// <summary>
    /// This Function keeps track if the button is selected or not.
    /// </summary>
    public void ChangeButtonState(bool state) => isSelected = state; 


    /// <summary>
    /// This function populates all the weapon information from the scriptable objects.
    /// </summary>
    private void WeaponInfo()
    {
        for (var i = 0; i < shipManager.weaponData.Count; i++)
        {
            if (gameObject.name.Equals(shipManager.weaponData[i].name))
            {
                transform.GetChild(0).GetComponent<Image>().sprite = shipManager.weaponData[i].weaponAvatar;
                transform.GetChild(0).GetComponent<Image>().preserveAspect = true;
                transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = shipManager.weaponData[i].weapon_Name;
                transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = 
                    shipManager.weaponData[i].price.ToString("#,#", CultureInfo.InvariantCulture);
                transform.GetChild(3).GetComponent<TextMeshProUGUI>().text = "Accuracy: <color=green>" + shipManager.weaponData[i].data[0].value.ToString() + "</color>";
                transform.GetChild(4).GetComponent<TextMeshProUGUI>().text = "Damage: <color=green>" + shipManager.weaponData[i].data[2].value.ToString() + "</color>";
                transform.GetChild(5).GetComponent<TextMeshProUGUI>().text = "Speed: <color=green>" + shipManager.weaponData[i].data[1].value.ToString() + "</color>";
                transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "Weapon type: " + shipManager.weaponData[i].weapon_type;
                transform.GetChild(7).GetComponent<TextMeshProUGUI>().text = shipManager.weaponData[i].description;
            }
        }
    }


    /// <summary>
    /// This function populates all the pods information from the scriptable objects.
    /// </summary>
    private void PodInfo()
    {
        for (var i = 0; i < shipManager.podData.Count; i++)
        {
            if (gameObject.name.Equals(shipManager.podData[i].name))
            {
                transform.GetChild(0).GetComponent<Image>().sprite = shipManager.podData[i].podAvatar;
                transform.GetChild(0).GetComponent<Image>().preserveAspect = true;
                transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = shipManager.podData[i].pod_Name;
                transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = 
                    shipManager.podData[i].price.ToString("#,#", CultureInfo.InvariantCulture);
                transform.GetChild(3).GetComponent<TextMeshProUGUI>().text =
                    shipManager.podData[i].data.data_Type + " <color=green>+" + shipManager.podData[i].data.value.ToString() + "</color>";
            }
        }
    }


    /// <summary>
    /// This function will display the specs for each class of the ship in the ship selection screen.
    /// </summary>
    private void ShipClassesInfo()
    {
        int pilotDiscount = 0;

        // Calculate additional values from pilot's skills
        if(GameManager.Instance.selectedPilot)
        {
            foreach(Skill skill in GameManager.Instance.selectedPilot.skills)
            {
                if(skill.skill_Type == Skill_Type.Price)
                {
                    pilotDiscount += skill.value;
                    break;
                }
            }
        }

        for (int i = 0; i < shipManager.shipData.Count; i++)
        {
            int finalPrice = Convert.ToInt32(shipManager.shipData[i].price * ((float)(100 + pilotDiscount) / 100));
            if(gameObject.name.Equals(shipManager.shipData[i].name))
            {
                if(pilotDiscount < 0)
                {
                    transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = 
                        finalPrice.ToString();

                    transform.GetChild(4).GetComponent<Image>().sprite = GameManager.Instance.pinkMythLogo;
                    
                    transform.GetChild(4).GetComponent<RectTransform>().sizeDelta = new Vector2(45f, 45f);

                    // Create your color
                    Color32 myColor32 = new Color32(219, 96, 216, 255);

                    // Get the TextMeshProUGUI component
                    TextMeshProUGUI textMesh = transform.GetChild(2).GetComponent<TextMeshProUGUI>();

                    // Force TextMeshPro to update its mesh
                    textMesh.ForceMeshUpdate();

                    // Get the mesh
                    Mesh mesh = textMesh.mesh;

                    // Get the size of the vertices array in the mesh
                    int vertexCount = mesh.vertices.Length;

                    // Create an array of the required size
                    Color32[] colors = new Color32[vertexCount];

                    // Fill the array with your color
                    for (int x = 0; x < vertexCount; x++)
                    {
                        colors[x] = myColor32;
                    }

                    // Assign the array to the mesh
                    mesh.colors32 = colors;

                    // Update the mesh with the new colors
                    textMesh.canvasRenderer.SetMesh(mesh);
                }
                else
                {
                    transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = 
                        finalPrice.ToString();

                    transform.GetChild(4).GetComponent<Image>().sprite = GameManager.Instance.whiteMythLogo;
                
                    transform.GetChild(4).GetComponent<RectTransform>().sizeDelta = new Vector2(30f, 30f);
                }
            }
        }
    }


    /// <summary>
    /// It Shows the info of ship which we want to sell.
    /// </summary>
    /// <param name="index"></param>
    public void ShowSellShipInfo(int index)
    {
        shipManager.shipSellSpecs[0].GetComponent<Image>().preserveAspect = true;
        shipManager.shipSellSpecs[0].GetComponent<Image>().sprite = buyManager.slotShipData.shipData.shipAvatar;
        shipManager.shipSellSpecs[1].GetComponent<TextMeshProUGUI>().text =
            buyManager.slotShipData.shipData.shipType + " - " + buyManager.slotShipData.shipData.shipClass;
        shipManager.shipSellSpecs[2].GetComponent<TextMeshProUGUI>().text =
            buyManager.slotShipData.finalPrice.ToString("#,#", CultureInfo.InvariantCulture);

        for(var i = 0; i < buyManager.slotShipData.podData.Count; i++)
        {
            shipManager.shipSellPods[i].GetComponent<Image>().sprite = GameManager.Instance.canSelectPod_WeaponSprite;
            shipManager.shipSellPods[i].transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1);
            shipManager.shipSellPods[i].transform.GetChild(0).GetComponent<Image>().sprite = 
                buyManager.slotShipData.podData[i].podAvatar;
        }

        for(var i = buyManager.slotShipData.podData.Count; i < GameManager.Instance.allPodSlots.Count; i++)
        {
            shipManager.shipSellPods[i].GetComponent<Image>().sprite =
                GameManager.Instance.cannotSelectPod_WeaponSprite;
        }

        for (var i = 0; i < buyManager.slotShipData.weaponData.Count; i++)
        {
            shipManager.shipSellWeapons[i].GetComponent<Image>().sprite = GameManager.Instance.canSelectPod_WeaponSprite;
            shipManager.shipSellWeapons[i].transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1);
            shipManager.shipSellWeapons[i].transform.GetChild(0).GetComponent<Image>().sprite =
                buyManager.slotShipData.weaponData[i].weaponAvatar;
        }

        for(var i = buyManager.slotShipData.weaponData.Count; i < GameManager.Instance.allWeaponSlots.Count; i++)
        {
            shipManager.shipSellWeapons[i].GetComponent<Image>().sprite =
                GameManager.Instance.cannotSelectPod_WeaponSprite;
        }

        if (index == 0)
            shipManager.shipSellSpecs[3].GetComponent<TextMeshProUGUI>().text = "Slot I - Sell Ship";
        if (index == 1)
            shipManager.shipSellSpecs[3].GetComponent<TextMeshProUGUI>().text = "Slot II - Sell Ship";
        if (index == 2)
            shipManager.shipSellSpecs[3].GetComponent<TextMeshProUGUI>().text = "Slot III - Sell Ship";
        if (index == 3)
            shipManager.shipSellSpecs[3].GetComponent<TextMeshProUGUI>().text = "Slot IV - Sell Ship";
    }


    /// <summary>
    /// This function is store which are pods selected by player. Call this function on the button which will select the pods.
    /// </summary>
    public void SelectPod(int index)
    {
        AudioManager.Instance.PlayMusic("Equip Mod Pods");
        string baseAndPilotSpecs = "";

        for (int i = 0; i < shipManager.podData.Count; i++)
        {
            if (gameObject.name.Equals(shipManager.podData[i].name))
            {
                if(GameManager.Instance.highlightedShip != null)
                    if (GameManager.Instance.podsAdded == GameManager.Instance.highlightedShip.maxMiscPods)
                        return;

                for (int j = 0; j < shipManager.shipSpecsUI.Count; j++)
                {
                    if (shipManager.shipSpecsUI[j].name.Equals(shipManager.podData[index].data.data_Type.ToString()))
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            if (GameManager.Instance.slotSelectedForBuy[k] || GameManager.Instance.slotSelectedForMod[k])
                            {
                                for (int l = 0; l < 5; l++)
                                {
                                    if (GameManager.Instance.ownedShipSlots[k].GetComponent<BuyManager>().extraSpecsData[l].
                                        skill_Type.ToString().Equals(shipManager.podData[index].data.data_Type.ToString()))
                                    {
                                        GameManager.Instance.ownedShipSlots[k].GetComponent<BuyManager>().extraSpecsData[l].value +=
                                        shipManager.podData[index].data.value;

                                        switch(index)
                                        {
                                            case 0:
                                                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.hull, Skill_Type.Hull);
                                                break;
                                            case 1:
                                                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.shields, Skill_Type.Shields);
                                                break;
                                            case 2:
                                                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.jumpSpeed, Skill_Type.Speed);
                                                break;
                                            case 3:
                                                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.evasion, Skill_Type.Evasion);
                                                break;
                                        }

                                        shipManager.shipSpecsUI[j].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = baseAndPilotSpecs + " <color=green>+" +
                                            GameManager.Instance.ownedShipSlots[k].GetComponent<BuyManager>().extraSpecsData[l].value.ToString() + "</color>";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }

                GameManager.Instance.selectedPods.Add(shipManager.podData[i]);
                GameManager.Instance.podsAdded++;
            }
        }

        for (int i = 0; i < GameManager.Instance.podsAdded; i++)
        {
            if (GameManager.Instance.allPodSlots[i].GetComponent<Button>().interactable == false)
            {
                GameManager.Instance.shipTotalPrice += shipManager.podData[index].price;
                shipManager.shipSpecsUI[8].GetComponent<TextMeshProUGUI>().text = 
                    Math.Abs(GameManager.Instance.shipTotalPrice).ToString("#,#", CultureInfo.InvariantCulture);

                GameManager.Instance.allPodSlots[i].GetComponent<Button>().interactable = true;
                GameManager.Instance.allPodSlots[i].transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1);
                // When you click on a pod, show the last element of the selectedPods List which is the one
                // just clicked.
                GameManager.Instance.allPodSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = 
                    GameManager.Instance.selectedPods[GameManager.Instance.selectedPods.Count - 1].podAvatar;
                break;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (GameManager.Instance.slotSelectedForMod[i] && GameManager.Instance.shipTotalPrice >= 0)
            {
                GameManager.Instance.modCredit.SetActive(false);
                break;
            }
        }
    }


    /// <summary>
    /// This is being called on OnClick on each Pod Slots.
    /// </summary>
    public void DeSelectPod(int index)
    {
        AudioManager.Instance.PlayMusic("Unequip Mod Pods");
        string baseAndPilotSpecs = "";
        string podName = transform.GetChild(0).GetComponent<Image>().sprite.name;

        for (int i = 0; i < shipManager.podData.Count; i++)
        {
            if (podName.Equals(shipManager.podData[i].name))
            {
                for (int j = 0; j < shipManager.shipSpecsUI.Count; j++)
                {
                    if (shipManager.shipSpecsUI[j].name.Equals(shipManager.podData[i].data.data_Type.ToString()))
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            if (GameManager.Instance.slotSelectedForBuy[k] || GameManager.Instance.slotSelectedForMod[k])
                            {
                                for (int l = 0; l < 5; l++)
                                {
                                    if (GameManager.Instance.ownedShipSlots[k].GetComponent<BuyManager>().extraSpecsData[l].
                                        skill_Type.ToString().Equals(shipManager.podData[i].data.data_Type.ToString()))
                                    {
                                        GameManager.Instance.ownedShipSlots[k].GetComponent<BuyManager>().extraSpecsData[l].value -=
                                        shipManager.podData[i].data.value;

                                        switch(l)
                                        {
                                            case 0:
                                                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.jumpSpeed, Skill_Type.Speed);
                                                break;
                                            case 1:
                                                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.evasion, Skill_Type.Evasion);
                                                break;
                                            case 2:
                                                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.shields, Skill_Type.Shields);
                                                break;
                                            case 3:
                                                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.hull, Skill_Type.Hull);
                                                break;
                                        }
                                        // Check if number equals to zero before changing color to white
                                        if (Convert.ToInt32(GameManager.Instance.ownedShipSlots[k].GetComponent<BuyManager>().extraSpecsData[l].value) == 0)
                                        {
                                            shipManager.shipSpecsUI[j].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = baseAndPilotSpecs;
                                        }else{
                                            shipManager.shipSpecsUI[j].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = baseAndPilotSpecs + " <color=green>+" +
                                                GameManager.Instance.ownedShipSlots[k].GetComponent<BuyManager>().extraSpecsData[l].value.ToString() + "</color>";
                                        }
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < GameManager.Instance.selectedPods.Count; i++)
        {
            if (podName.Equals(GameManager.Instance.selectedPods[i].name))
            {
                GameManager.Instance.shipTotalPrice -= GameManager.Instance.selectedPods[i].price;
                shipManager.shipSpecsUI[8].GetComponent<TextMeshProUGUI>().text = 
                    Math.Abs(GameManager.Instance.shipTotalPrice).ToString("#,#", CultureInfo.InvariantCulture);

                GameManager.Instance.selectedPods.RemoveAt(i);
                GameManager.Instance.podsAdded--;
                // Added this to set the interactivity of the the pod slot to false so the cross doesn't appear on hover
                GameManager.Instance.allPodSlots[index].GetComponent<Button>().interactable = false;
                ResetPodSlotImage();
                break;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (GameManager.Instance.slotSelectedForMod[i] && GameManager.Instance.shipTotalPrice < 0)
            {
                GameManager.Instance.modCredit.SetActive(true);
                break;
            }
        }
    }


    /// <summary>
    /// This is being called on Select Event Trigger on each Pod Slots.
    /// </summary>
    private void ResetPodSlotImage()
    {
        transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
        transform.GetChild(0).GetComponent<Image>().sprite = null;
        GetComponent<Image>().sprite = GameManager.Instance.canSelectPod_WeaponSprite;
    }


    /// <summary>
    /// This function is store which are weapons selected by player. Call this function on the button which will select the weapons.
    /// </summary>
    public void SelectWeapon(int index)
    {
        AudioManager.Instance.PlayMusic("Equip Mod Pods");
        for (int i = 0; i < shipManager.weaponData.Count; i++)
        {
            if (gameObject.name.Equals(shipManager.weaponData[i].name))
            {
                if (GameManager.Instance.highlightedShip != null)
                    if (GameManager.Instance.weaponsAdded == GameManager.Instance.highlightedShip.maxGuns)
                        return;

                GameManager.Instance.selectedWeapons.Add(shipManager.weaponData[i]);
                GameManager.Instance.weaponsAdded++;
            }
        }

        for (int i = 0; i < GameManager.Instance.weaponsAdded; i++)
        {
            if (GameManager.Instance.allWeaponSlots[i].GetComponent<Button>().interactable == false)
            {
                GameManager.Instance.shipTotalPrice += shipManager.weaponData[index].price;
                shipManager.shipSpecsUI[8].GetComponent<TextMeshProUGUI>().text = 
                    Math.Abs(GameManager.Instance.shipTotalPrice).ToString("#,#", CultureInfo.InvariantCulture);

                GameManager.Instance.allWeaponSlots[i].GetComponent<Button>().interactable = true;
                GameManager.Instance.allWeaponSlots[i].transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1);
                // When you click on a gun, show the last element of the selectedWeapons array which is the one
                // just clicked.
                GameManager.Instance.allWeaponSlots[i].transform.GetChild(0).GetComponent<Image>().sprite =
                    GameManager.Instance.selectedWeapons[GameManager.Instance.selectedWeapons.Count - 1].weaponAvatar;
                break;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (GameManager.Instance.slotSelectedForMod[i] && GameManager.Instance.shipTotalPrice >= 0)
            {
                GameManager.Instance.modCredit.SetActive(false);
                break;
            }
        }
    }


    /// <summary>
    /// This is being called on OnClick on each Weapon Slots.
    /// </summary>
    public void DeSelectWeapon(int index)
    {
        AudioManager.Instance.PlayMusic("Unequip Mod Pods");

        string weaponName = transform.GetChild(0).GetComponent<Image>().sprite.name;

        for (int i = 0; i < GameManager.Instance.selectedWeapons.Count; i++)
        {
            if (weaponName.Equals(GameManager.Instance.selectedWeapons[i].name))
            {
                GameManager.Instance.shipTotalPrice -= GameManager.Instance.selectedWeapons[i].price;
                shipManager.shipSpecsUI[8].GetComponent<TextMeshProUGUI>().text =
                    Math.Abs(GameManager.Instance.shipTotalPrice).ToString("#,#", CultureInfo.InvariantCulture);

                GameManager.Instance.selectedWeapons.RemoveAt(i);
                GameManager.Instance.weaponsAdded--;
                // Added this to set the interactivity of the the gun slot to false so the cross doesn't appear on hover
                GameManager.Instance.allWeaponSlots[index].GetComponent<Button>().interactable = false;
                ResetWeaponSlotImage();
                break;
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (GameManager.Instance.slotSelectedForMod[i] && GameManager.Instance.shipTotalPrice < 0)
            {
                GameManager.Instance.modCredit.SetActive(true);
                break;
            }
        }
    }


    /// <summary>
    /// This is being called on Select Event Trigger on each Weapon Slots.
    /// </summary>
    private void ResetWeaponSlotImage()
    {
        transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
        transform.GetChild(0).GetComponent<Image>().sprite = null;
        GetComponent<Image>().sprite = GameManager.Instance.canSelectPod_WeaponSprite;
    }


    /// <summary>
    /// Call the method on the buttons in the buy ships section where ships and there classes show up.
    /// </summary>
    public void SelectShips(int shipIndex)
    {
        GameManager.Instance.HighLightShip(shipManager.shipData[shipIndex]);
        GameManager.Instance.ResetPodAndWeaponCount();
    }


    /// <summary>
    /// This function will display the info of ship.
    /// </summary>
    public void SpecsInfo(int _class)
    {
        GameManager.Instance.ship.GetComponent<Animator>().Play("Play Grid");

        for(var i = 1; i < 7; i++)
        {
            shipManager.shipSpecsUI[i].transform.GetChild(0).gameObject.SetActive(true);
            // shipManager.shipSpecsUI[i].transform.GetChild(2).gameObject.SetActive(true);
        }

        shipManager.shipSpecsUI[0].GetComponent<TextMeshProUGUI>().text =
            shipManager.shipData[_class].shipType + " - Class " + shipManager.shipData[_class].shipClass.ToString();

        shipManager.shipSpecsUI[1].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            GetSpecText(shipManager.shipData[_class].hull, Skill_Type.Hull);
        shipManager.shipSpecsUI[2].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            GetSpecText(shipManager.shipData[_class].jumpSpeed, Skill_Type.Speed);
        shipManager.shipSpecsUI[3].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            shipManager.shipData[_class].maxGuns.ToString();
        shipManager.shipSpecsUI[4].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            GetSpecText(shipManager.shipData[_class].shields, Skill_Type.Shields);
        shipManager.shipSpecsUI[5].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            GetSpecText(shipManager.shipData[_class].evasion, Skill_Type.Evasion);
        shipManager.shipSpecsUI[6].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
                shipManager.shipData[_class].maxMiscPods.ToString();

        shipManager.shipSpecsUI[7].GetComponent<Image>().sprite = shipManager.shipData[_class].shipAvatar;
        shipManager.shipSpecsUI[7].GetComponent<Image>().color = new(1, 1, 1, 1);


        int shipTotalPrice = shipPriceCalculation(shipManager.shipData[_class].price);
        GameManager.Instance.shipTotalPrice = shipTotalPrice;

        shipManager.shipSpecsUI[8].GetComponent<TextMeshProUGUI>().text = 
            GameManager.Instance.shipTotalPrice.ToString("#,#", CultureInfo.InvariantCulture);
    }

    private int shipPriceCalculation(int shipPrice)
    {
        int pilotDiscount = 0;
        int shipTotalPrice = shipPrice;

        // Calculate additional values from pilot's skills
        if(GameManager.Instance.selectedPilot)
        {
            foreach(Skill skill in GameManager.Instance.selectedPilot.skills)
            {
                if(skill.skill_Type == Skill_Type.Price)
                {
                    pilotDiscount += skill.value;
                    break;
                }
            }
        }
        
        if(pilotDiscount < 0)
        {
            shipTotalPrice = Convert.ToInt32(shipPrice * ((float)(100 + pilotDiscount) / 100));
        }

        return shipTotalPrice;
    }

    private string GetSpecText(int baseValue, Skill_Type skillType)
    {
        int additionalValue = 0;

        // Calculate additional values from pilot's skills
        if(GameManager.Instance.selectedPilot)
        {
            foreach(Skill skill in GameManager.Instance.selectedPilot.skills)
            {
                if(skill.skill_Type == skillType)
                {
                    additionalValue += skill.value;
                    break;
                }
            }
        }

        return additionalValue > 0 ? $"{baseValue} <color=#FFC0CB>+{Math.Round(baseValue * additionalValue / 100.0, MidpointRounding.AwayFromZero)}</color>" : baseValue.ToString();
    }

    /// <summary>
    /// This function will display the specs of ship which we are modifying.
    /// </summary>
    public void ShowModShipSpecs()
    {
        GameManager.Instance.sliderContent.GetComponent<RectTransform>().anchoredPosition = new(-1740, 384);

        GameManager.Instance.ship.GetComponent<Animator>().Play("Play Grid");

        GameManager.Instance.navigationButtons.transform.GetChild(0).GetChild(2).
            GetComponent<Button>().interactable = true;
        
        for (var i = 1; i < 7; i++)
        {
            shipManager.shipSpecsUI[i].transform.GetChild(0).gameObject.SetActive(true);
            // shipManager.shipSpecsUI[i].transform.GetChild(2).gameObject.SetActive(true);
        }

        shipManager.shipSpecsUI[0].GetComponent<TextMeshProUGUI>().text =
            transform.parent.parent.GetComponent<BuyManager>().slotShipData.shipData.shipType + " - Class " +
            transform.parent.parent.GetComponent<BuyManager>().slotShipData.shipData.shipClass;

        shipManager.shipSpecsUI[1].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            transform.parent.parent.GetComponent<BuyManager>().slotShipData.shipData.hull.ToString();
        ModShipSpecs(1, 3);

        shipManager.shipSpecsUI[2].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            transform.parent.parent.GetComponent<BuyManager>().slotShipData.shipData.jumpSpeed.ToString();
        ModShipSpecs(2, 0);

        shipManager.shipSpecsUI[3].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            transform.parent.parent.GetComponent<BuyManager>().slotShipData.shipData.maxGuns.ToString();
        shipManager.shipSpecsUI[3].transform.GetChild(2).GetComponent<TextMeshProUGUI>().color = Color.white;
        shipManager.shipSpecsUI[3].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = " +0";

        shipManager.shipSpecsUI[4].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            transform.parent.parent.GetComponent<BuyManager>().slotShipData.shipData.shields.ToString();
        ModShipSpecs(4, 2);

        shipManager.shipSpecsUI[5].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            transform.parent.parent.GetComponent<BuyManager>().slotShipData.shipData.evasion.ToString();
        ModShipSpecs(5, 1);

        shipManager.shipSpecsUI[6].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
            transform.parent.parent.GetComponent<BuyManager>().slotShipData.shipData.maxMiscPods.ToString();
        shipManager.shipSpecsUI[6].transform.GetChild(2).GetComponent<TextMeshProUGUI>().color = Color.white;
        shipManager.shipSpecsUI[6].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = " +0";


        shipManager.shipSpecsUI[7].GetComponent<Image>().sprite =
            transform.parent.parent.GetComponent<BuyManager>().slotShipData.shipData.shipAvatar;
        shipManager.shipSpecsUI[7].GetComponent<Image>().color = new(1, 1, 1, 1);

        shipManager.shipSpecsUI[8].GetComponent<TextMeshProUGUI>().text =
            GameManager.Instance.shipTotalPrice.ToString();

        for (var i = 0; i < GameManager.Instance.highlightedShip.maxMiscPods; i++)
        {
            GameManager.Instance.allPodSlots[i].GetComponent<Image>().sprite = GameManager.Instance.canSelectPod_WeaponSprite;
            if(i < buyManager.slotShipData.podData.Count && buyManager.slotShipData.podData[i].podAvatar != null)
            {
                GameManager.Instance.allPodSlots[i].transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1);
                GameManager.Instance.allPodSlots[i].transform.GetChild(0).GetComponent<Image>().sprite =
                    buyManager.slotShipData.podData[i].podAvatar;
                GameManager.Instance.allPodSlots[i].GetComponent<Button>().interactable = true;
            }
        }

        for(var i = GameManager.Instance.highlightedShip.maxMiscPods; i < GameManager.Instance.allPodSlots.Count; i++)
        {
            GameManager.Instance.allPodSlots[i].GetComponent<Image>().sprite =
                GameManager.Instance.cannotSelectPod_WeaponSprite;
        }

        for (var i = 0; i < GameManager.Instance.highlightedShip.maxGuns; i++)
        {
            GameManager.Instance.allWeaponSlots[i].GetComponent<Image>().sprite = GameManager.Instance.canSelectPod_WeaponSprite;
            if(i < buyManager.slotShipData.weaponData.Count && buyManager.slotShipData.weaponData[i].weaponAvatar != null)
            {
                GameManager.Instance.allWeaponSlots[i].transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1);
                GameManager.Instance.allWeaponSlots[i].transform.GetChild(0).GetComponent<Image>().sprite =
                    buyManager.slotShipData.weaponData[i].weaponAvatar;
                GameManager.Instance.allWeaponSlots[i].GetComponent<Button>().interactable = true;
            }
        }

        for (var i = GameManager.Instance.highlightedShip.maxGuns; i < GameManager.Instance.allWeaponSlots.Count; i++)
        {
            GameManager.Instance.allWeaponSlots[i].GetComponent<Image>().sprite =
                GameManager.Instance.cannotSelectPod_WeaponSprite;
        }
    }


    /// <summary>
    /// This function will set the text color and text of the Specs of ship we are modifying.
    /// </summary>
    private void ModShipSpecs(int index,int extraShipSpecsIndex)
    {
        string baseAndPilotSpecs = "";
        transform.parent.parent.GetComponent<BuyManager>().extraSpecsData[extraShipSpecsIndex].value = 0;

        switch(extraShipSpecsIndex)
        {
            case 3:
                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.hull, Skill_Type.Hull);
                foreach(PodData pod in GameManager.Instance.selectedPods)
                {
                    if(pod.data.data_Type == PodsData.Data_Type.Hull)
                        transform.parent.parent.GetComponent<BuyManager>().extraSpecsData[extraShipSpecsIndex].value += pod.data.value;
                }
                break;
            case 2:
                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.shields, Skill_Type.Shields);
                foreach(PodData pod in GameManager.Instance.selectedPods)
                {
                    if(pod.data.data_Type == PodsData.Data_Type.Shields)
                        transform.parent.parent.GetComponent<BuyManager>().extraSpecsData[extraShipSpecsIndex].value += pod.data.value;
                }
                break;
            case 0:
                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.jumpSpeed, Skill_Type.Speed);
                foreach(PodData pod in GameManager.Instance.selectedPods)
                {
                    if(pod.data.data_Type == PodsData.Data_Type.Speed)
                        transform.parent.parent.GetComponent<BuyManager>().extraSpecsData[extraShipSpecsIndex].value += pod.data.value;
                }
                break;
            case 1:
                baseAndPilotSpecs = GetSpecText(GameManager.Instance.highlightedShip.evasion, Skill_Type.Evasion);
                foreach(PodData pod in GameManager.Instance.selectedPods)
                {
                    if(pod.data.data_Type == PodsData.Data_Type.Evasion)
                        transform.parent.parent.GetComponent<BuyManager>().extraSpecsData[extraShipSpecsIndex].value += pod.data.value;
                }
                break;
        }



        if (transform.parent.parent.GetComponent<BuyManager>().extraSpecsData[extraShipSpecsIndex].value > 0)
        {
            shipManager.shipSpecsUI[index].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = baseAndPilotSpecs + " <color=green>+" +
                transform.parent.parent.GetComponent<BuyManager>().extraSpecsData[extraShipSpecsIndex].value.ToString() + "</color>";
        }
        else
        {
            shipManager.shipSpecsUI[index].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = baseAndPilotSpecs;
        }
    }
}