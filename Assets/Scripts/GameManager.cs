using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LootLocker.Requests;
using System;
using Unity.VisualScripting;
using System.Linq;
using System.Globalization;
using UnityEngine.EventSystems;
using System.Reflection;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("SO")]
    public PilotData selectedPilot;
    public PilotData highlightedPilot;

    public ShipData highlightedShip;
    public ShipData selectedShip;

    private ShipManager shipManager;
    private PilotManager pilotManager;

    public Data data;

    [Header("Lists")]
    public List<ShipData> OwnedShips;
    public List<PodData> selectedPods;
    public List<WeaponData> selectedWeapons;

    [Header("Buttons")]
    public List<Button> mainMenuButtons;
    public List<Button> shipClassesButtons;
    public List<Button> shipTypeButtons;

    [Header("GameObjects")]
    public List<GameObject> panels;
    public List<GameObject> allPodSlots;
    public List<GameObject> allWeaponSlots;
    public GameObject[] ownedShipSlots = new GameObject[4];
    public GameObject confirmShipButton;
    public GameObject sliderContent;
    public GameObject yourShipsPanel;
    public GameObject buyAndArmoryPanel;
    public GameObject scoutClass;
    public GameObject spectreClass;
    public GameObject transportClass;
    public GameObject navigationButtons;
    public GameObject buy_modShipPopup;
    public GameObject modCredit;
    public GameObject selectAndUpgradeYourShipPanel;
    public GameObject canvases;
    public GameObject ship;
    public GameObject welcomePanel;
    public GameObject maximizeButton;
    public GameObject minimizeButton;
    public GameObject notifications;
    [SerializeField] private GameObject quickRepair;
    public GameObject quickRepairPopup;
    public GameObject notificationsFrame;
    public GameObject twitterButton;
    public GameObject twitterHint;

    [Header("Images")]
    public Image pilotDisplayArea;
    public Image shipDisplayArea;
    public Image mythLogo;

    [Header("Texture")]
    public Texture2D arrowCursor;

    [Header("Sprite")]
    public Sprite canSelectPod_WeaponSprite;
    public Sprite cannotSelectPod_WeaponSprite;
    public Sprite ownedShipPanel;
    public Sprite notOwnedShipPanel;
    public Sprite frameShip;
    public Sprite buyPanelSprite;
    public Sprite modPanelSprite;
    public Sprite class2ShipNotAvailable;
    public Sprite class3ShipNotAvailable;
    public Sprite whiteMythLogo;
    public Sprite pinkMythLogo;

    [SerializeField] private Sprite shipClassDisable;
    [SerializeField] private Sprite shipClassEnabled;

    [Header("InputField")]
    [SerializeField] private TMP_InputField playerName;
    [SerializeField] private TMP_InputField playerWelcomeName;
    
    [Header("TextMeshProUGUI")]
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI quickRepairPriceText;
    public TextMeshProUGUI quickRepairDiscountPriceText;
    public RectTransform logoRectTransform;

    [Header("bool")]
    [SerializeField] private bool isBuyShipToArmory = false;
    [SerializeField] private bool isArmoryToBuyShip = false;
    public bool[] slotSelectedForBuy = new bool[4];
    public bool[] slotSelectedForMod = new bool[4];
    public bool[] slotSelectedForRepair = new bool[4];
    [SerializeField] private bool canBuyShipOrNot;
    private bool manualScroll;

    [Header("int/floats")]
    public int podsAdded = 0;
    public int weaponsAdded = 0;
    public int currency;
    public int shipTotalPrice;
    public int whichSlotShipToSell;
    public int shipSelectedIndex;
    public float repairPrice = 10f;
    public float GAME_REPAIR_PRICE = 10f;
    public int repairShipTotalHealth = 0;
    public int totalPrice = 0; // Ship + pods + guns price
    public int repairTotalPrice = 0;
    private readonly string key = "FirstTimeLogin";

    [Header("RectTransform")]
    private RectTransform sliderContentRectTransform;

    public PlayerSessionManager playerSessionManager;
    public List<Mission> missionsData;

    // This value sets the percentage of Myth received when selling a ship.
    // Set to 0.5f to return the 50% of the ship value + pods + guns when selling.
    public float DISCOUNT = 0.2f;
    public float REPAIR_PERCETAGE = 0.1f;
    [SerializeField] private ScrollRect[] scrollRectsForKills;

    [SerializeField] private Scrollbar scrollbarForKills;

    [SerializeField] private ScrollRect[] scrollRectsForMyths;

    [SerializeField] private Scrollbar scrollbarForMyths;

    [SerializeField] private ScrollRect[] scrollRectsForHours;

    [SerializeField] private Scrollbar scrollbarForHours;

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (data.whichScene.Equals("GamePlay"))
        {
            canvases.transform.GetChild(0).gameObject.SetActive(false);
            canvases.transform.GetChild(1).gameObject.SetActive(true);
            data.whichScene = "Scene1";
        }

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        shipManager = FindAnyObjectByType<ShipManager>();
        pilotManager = FindAnyObjectByType<PilotManager>();

        data.weaponDatas.Clear();
        data.podDatas.Clear();

        Cursor.lockState = CursorLockMode.Confined;

        scrollbarForKills.value = 1.0f;
        scrollbarForKills.value = 0.15f;
    }

    private void Start()
    {
        Cursor.SetCursor(arrowCursor, Vector2.zero, CursorMode.Auto);
        sliderContentRectTransform = sliderContent.GetComponent<RectTransform>();

        data.shipCombatStatsAllData.hull = 0;
        data.shipCombatStatsAllData.maxHull = 0;
        data.shipCombatStatsAllData.shields = 0;
        data.shipCombatStatsAllData.evasion = 0;
        data.shipCombatStatsAllData.jumpSpeed = 0;
    }

    private void Update()
    {
        // Update the position of the logo every frame in case the text changes
        PositionLogo();

        if (sliderContentRectTransform.anchoredPosition.x <= -1500)
        {
            navigationButtons.transform.GetChild(0).gameObject.SetActive(false);
            navigationButtons.transform.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            navigationButtons.transform.GetChild(0).gameObject.SetActive(true);
            navigationButtons.transform.GetChild(1).gameObject.SetActive(false);
        }

        if(isBuyShipToArmory)
        {
            sliderContentRectTransform.anchoredPosition = 
                Vector2.MoveTowards(sliderContentRectTransform.anchoredPosition, new(-1740, 384), 1000f * Time.deltaTime);
            if (sliderContentRectTransform.anchoredPosition.x <= -1740)
            {
                isBuyShipToArmory = false;
            }
        }

        if (isArmoryToBuyShip)
        {
            sliderContentRectTransform.anchoredPosition =
                Vector2.MoveTowards(sliderContentRectTransform.anchoredPosition, new(-769.5f, 384), 1000f * Time.deltaTime);
            if (sliderContentRectTransform.anchoredPosition.x >= -769.5)
            {
                isArmoryToBuyShip = false;
            }
        }

        if(canBuyShipOrNot && currency >= shipTotalPrice)
        {
            if (slotSelectedForBuy[0] || slotSelectedForBuy[1] || slotSelectedForBuy[2] || slotSelectedForBuy[3])
            {
                if (weaponsAdded > 0)
                    navigationButtons.transform.GetChild(1).GetChild(2).GetComponent<Button>().interactable = true;
                else
                    navigationButtons.transform.GetChild(1).GetChild(2).GetComponent<Button>().interactable = false;
            }
            else if((slotSelectedForMod[0] || slotSelectedForMod[1] || slotSelectedForMod[2] || slotSelectedForMod[3]) & shipTotalPrice != 0)
            {
                if (weaponsAdded > 0)
                    navigationButtons.transform.GetChild(1).GetChild(2).GetComponent<Button>().interactable = true;
                else
                    navigationButtons.transform.GetChild(1).GetChild(2).GetComponent<Button>().interactable = false;
            }else
                navigationButtons.transform.GetChild(1).GetChild(2).GetComponent<Button>().interactable = false;
                
        }else
            navigationButtons.transform.GetChild(1).GetChild(2).GetComponent<Button>().interactable = false;


        if (selectedPilot == null || selectedShip == null)
            mainMenuButtons[3].GetComponent<Button>().interactable = false;
        else
            mainMenuButtons[3].GetComponent<Button>().interactable = true;

        if(currency < 500000)
           {
            // Not interactable and on pointer enter disabled
            twitterButton.GetComponent<Button>().interactable = true;
           }
        else
        {
            // Interactable and on pointer enter enabled
            twitterButton.GetComponent<Button>().interactable = false;
        }
    }

    public void twitterButtonOnEnter()
    {
        if(twitterButton.GetComponent<Button>().interactable == true)
            twitterHint.SetActive(true);
        else
            twitterHint.SetActive(false);
    }

    /// <summary>
    /// To change the cursor when hovering/unhovering an UI.
    /// </summary>
    public void CrossHairChanger(Texture2D crosshair) => Cursor.SetCursor(crosshair, new Vector2(20, 4), CursorMode.Auto);

    public Texture2D defaultCursor;
    public Texture2D hoverCursor;

    public void ChangeCursorOnHover(Button button)
    {
        if (button.interactable && button.enabled)
        {
            // Makes sound and change cursor
            AudioManager.Instance.PlayMusic("Button Hover");
            Cursor.SetCursor(hoverCursor, new Vector2(20, 4), CursorMode.Auto);
        }
    }

    public void TriggerFullscreen()
    {
        AudioManager.Instance.PlayMusic("Button Click");
        if(Screen.fullScreen)
        {
            Screen.fullScreen = false;
            maximizeButton.SetActive(true);
            minimizeButton.SetActive(false);
        }
        else
        {
            Screen.fullScreen = true;
            maximizeButton.SetActive(false);
            minimizeButton.SetActive(true);
        }
    }

    public void SetPlanetPlans()
    {
       if(data.actualNode == "A5" && !data.shipPlans.Contains("plan1"))
            playerSessionManager.planetPlansPanel.transform.Find("Download").gameObject.SetActive(true);

       if(data.actualNode == "A9" && !data.shipPlans.Contains("plan2"))
            playerSessionManager.planetPlansPanel.transform.Find("Download").gameObject.SetActive(true);
    }

    public void CheckifCanBuy_ModShipOrNot()
    {
        // if (OwnedShips.Count > 0)
        // {
        //     for (var j = 0; j < OwnedShips.Count; j++)
        //     {
        //         // Check if highlightedShip exists before using it. If not using this, I get a reference error.
        //         if (highlightedShip)
        //         {
        //             if (highlightedShip.name.Equals(OwnedShips[j].name))
        //             {
        //                 canBuyShipOrNot = false;
        //                 navigationButtons.transform.GetChild(1).GetChild(2).GetComponent<Button>().interactable = false;
        //                 break;
        //             }
        //             else
        //             {
        //                 canBuyShipOrNot = true;
        //                 navigationButtons.transform.GetChild(1).GetChild(2).GetComponent<Button>().interactable = true;
        //             }
        //         }
        //     }
        // }
    }

    /// <summary>
    /// This will Reset all the main menu buttons when a new button is clicked, except the selected one.
    /// </summary>
    /// <param name="buttonNumber"></param>
    public void ResetMainMenuButtons(int buttonNumber)
    {
        for (int i = 0; i < mainMenuButtons.Count; i++)
        {
            if (i != buttonNumber)
            {
                mainMenuButtons[i].GetComponent<ButtonManager>().defaultState = true;
                mainMenuButtons[i].GetComponent<ButtonManager>().ChangeHighlightedSprite();
                mainMenuButtons[i].GetComponent<Image>().sprite = mainMenuButtons[i].GetComponent<ButtonManager>().defaultStateImage;
            }
        }
    }

    /// <summary>
    /// This will Reset specific ship classes buttons when a new button is clicked, except the selected one.
    /// </summary>
    /// <param name="buttonNumber"></param>
    public void ResetShipClassesButtons(int buttonNumber)
    {
        // When passing -1 which means reset all of them.
        if(buttonNumber == -1)
        {
            for (int i = 0; i < shipClassesButtons.Count; i++)
            {
                shipClassesButtons[i].GetComponent<ButtonManager>().defaultState = true;
                shipClassesButtons[i].GetComponent<ButtonManager>().ChangeHighlightedSprite();
                shipClassesButtons[i].GetComponent<Image>().sprite = shipClassesButtons[i].GetComponent<ButtonManager>().defaultStateImage;
            }
        }
        else
        {
            for (int i = 0; i < shipClassesButtons.Count; i++)
            {
                if (i != buttonNumber)
                {
                    shipClassesButtons[i].GetComponent<ButtonManager>().defaultState = true;
                    shipClassesButtons[i].GetComponent<ButtonManager>().ChangeHighlightedSprite();
                    shipClassesButtons[i].GetComponent<Image>().sprite = shipClassesButtons[i].GetComponent<ButtonManager>().defaultStateImage;
                }
            }
        }
    }

    /// <summary>
    /// This will Reset all the ship types buttons when the confirm button is clicked. 
    /// </summary>
    /// <param name="buttonNumber"></param>
    public void ResetShipTypesButtons(int buttonNumber)
    {
        // When passing -1 which means reset all of them.
        if (buttonNumber == -1)
        {
            for (int i = 0; i < shipTypeButtons.Count; i++)
            {
                shipTypeButtons[i].GetComponent<ButtonManager>().defaultState = true;
                shipTypeButtons[i].GetComponent<ButtonManager>().ChangeHighlightedSprite();
                shipTypeButtons[i].GetComponent<Image>().sprite = shipTypeButtons[i].GetComponent<ButtonManager>().defaultStateImage;
            }
        }
        else
        {
            for (int i = 0; i < shipTypeButtons.Count; i++)
            {
                // I added the interactable condition for the ship to change state.
                if (i != buttonNumber && shipTypeButtons[i].interactable == true)
                {
                    shipTypeButtons[i].GetComponent<ButtonManager>().defaultState = true;
                    shipTypeButtons[i].GetComponent<ButtonManager>().ChangeHighlightedSprite();
                    shipTypeButtons[i].GetComponent<Image>().sprite = shipTypeButtons[i].GetComponent<ButtonManager>().defaultStateImage;
                }
            }
        }
        // Reset pod and guns slots
        allPodSlots.ForEach(a => {
            a.GetComponent<Button>().interactable = false;
        });
        allWeaponSlots.ForEach(a => {
            a.GetComponent<Button>().interactable = false;
        });
    }

    /// <summary>
    /// This will disable all the panels except the one of which the button is clicked.
    /// </summary>
    /// <param name="panelNumber"></param>
    public void DisablePanels(int panelNumber)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            if (i != panelNumber)
                panels[i].SetActive(false);
        }
    }

    /// <summary>
    /// This will handle the notifications on each planet.
    /// </summary>
    public void HandleNotifications()
    {
        if(notifications.transform.Find("Notifications Container").gameObject.activeSelf)
            notifications.transform.Find("Notifications Container").gameObject.SetActive(false);
        else
            notifications.transform.Find("Notifications Container").gameObject.SetActive(true);
    }

    /// <summary>
    /// Sets the pilot when player confirm pilot and when player starts session.
    /// </summary>
    /// <param name="currentPilot"></param>
    public void SetPilot(PilotData currentPilot)
    {
        selectedPilot = currentPilot;
    }

    /// <summary>
    /// Highlights the pilot when player is clicking on all the pilot buttons.
    /// </summary>
    /// <param name="currentPilot"></param>
    public void HighlightPilot(PilotData currentPilot)
    {
        highlightedPilot = currentPilot;
    }

    /// <summary>
    /// This button is called to open the pilotCofirm Popup.
    /// </summary>
    public void ConfirmPilot()
    {
        if (selectedPilot != null)
        {
            pilotManager.changePilotPopup.SetActive(true);
        }
        else
        {
            pilotManager.changePilotPopup.SetActive(false);
            selectedPilot = highlightedPilot; 
            pilotDisplayArea.sprite = selectedPilot.pilotAvatar;
            pilotDisplayArea.GetComponent<Image>().enabled = true;
            pilotManager.UpdateShipsSpecsDependingOnPilot();
            playerSessionManager.UpdatePlayerSelectedPilot(selectedPilot.pilotName);
            panels[0].SetActive(false);
        }
    }

    /// <summary>
    /// This button shows the popup to buy ship plans
    /// </summary>
    public void DownloadShipPlan()
    {
        int planPrice = 9000;
        if(currency < planPrice)
        {
            playerSessionManager.planetPlansPanel.transform.Find("Download Popup/Popup Frame/Yes&No/Yes").GetComponent<Button>().interactable = false;
        }
    }

    /// <summary>
    /// This button buys ship plans from planets. It sets the playerSession in LootLocker.
    /// </summary>
    public void BuyShipPlan()
    {
        int planPrice = 9000;
        Mission removeMission = null;
        if(currency >= planPrice)
        {
            if(data.actualNode == "A5")
            {
                Leaderboard.Instance.SetLeaderboardCurrency(currency - planPrice);
                currency -= planPrice;
                currencyText.text = currency.ToString("#,#",CultureInfo.InvariantCulture);
                playerSessionManager.UpdatePlayerShipPlans("plan1");
                playerSessionManager.UpdatePlayerMissions(1);
                foreach(Mission mission in missionsData)
                {
                    if(mission.target.name == "A5")
                        removeMission = mission;
                    if(mission.target.name == "A9")
                        mission.enabled = true;
                }
                missionsData.Remove(removeMission);
            }
            else if(data.actualNode == "A9")
            {
                Leaderboard.Instance.SetLeaderboardCurrency(currency - planPrice);
                currency -= planPrice;
                currencyText.text = currency.ToString("#,#",CultureInfo.InvariantCulture);
                playerSessionManager.UpdatePlayerShipPlans("plan2");
                playerSessionManager.UpdatePlayerMissions(2);
                foreach(Mission mission in missionsData)
                {
                    if(mission.target.name == "A9")
                    {
                        missionsData.Remove(mission);
                        break;
                    }
                }
            }
            RefreshMissions();
        }
    }

    public void RefreshMissions()
    {
        int x = 0;
        // Destroy all missions
        int missionsCount = playerSessionManager.planetPlansPanel.transform.Find("Notification/Notifications Container").childCount;
        if(missionsCount > 0)
        {
            for(int i = 0;i <= missionsCount - 1;i++)
            {
                Destroy(playerSessionManager.planetPlansPanel.transform.Find("Notification/Notifications Container").GetChild(i).gameObject);
            }
        }
        // Show available missions
        foreach(Mission mission in missionsData)
        {
            if(mission.enabled)
            {
                Vector2 desiredPosition = new Vector2(54, -5 * (1 + (x * 25)));
                GameObject notificationFrame = Instantiate(notificationsFrame);
                // Set the parent canvas
                notificationFrame.transform.SetParent(playerSessionManager.planetPlansPanel.transform.Find("Notification/Notifications Container"), false);
                if(data.actualNode == mission.target.name)
                    notificationFrame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Buy ship plans here now";
                else
                    notificationFrame.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = mission.description;
                
                if(mission.target.type == "plan" && mission.target.name == data.actualNode)
                {
                    notificationFrame.transform.GetComponent<Button>().enabled = true;
                    notificationFrame.transform.GetComponent<Animator>().enabled = true;
                    // notificationFrame.transform.GetComponent<Image>().color = new(1f, 0.92f, 0.016f);
                }
                notificationFrame.SetActive(true);
                // Set the image position
                RectTransform newImageRectTransform = notificationFrame.GetComponent<RectTransform>();
                newImageRectTransform.anchoredPosition = desiredPosition;
                x++;
            }
        }
        
        if(x > 0)
            playerSessionManager.planetPlansPanel.transform.Find("Notification/Notification Button").GetChild(0).GetComponent<TextMeshProUGUI>().text = x.ToString();
        else
            playerSessionManager.planetPlansPanel.transform.Find("Notification/Notification Button").GetChild(0).gameObject.SetActive(false);
    }

    /// <summary>
    /// This function will be called on the yes button when changing pilot.
    /// </summary>
    public void YesChangePilot()
    {
        // Return if not enough currency.
        if (currency < 5000) return;

        AudioManager.Instance.PlayMusic("Button Click");
        
        selectedPilot = highlightedPilot;
        pilotDisplayArea.sprite = selectedPilot.pilotAvatar;
        pilotDisplayArea.GetComponent<Image>().enabled = true;

        pilotManager.UpdateShipsSpecsDependingOnPilot();
        playerSessionManager.UpdatePlayerSelectedPilot(selectedPilot.pilotName);
        currency -= 5000;
        Leaderboard.Instance.SetLeaderboardCurrency(currency - 5000);
    }

    /// <summary>
    /// This is set the ship when the player either selects from a bought ship and buys his first one.
    /// </summary>
    /// <param name="currentShip"></param>
    public void HighLightShip(ShipData currentShip)
    {
        highlightedShip = currentShip;

        selectedPods.Clear();
        selectedWeapons.Clear();

        DisableExtraPodSlots();
        DisableExtraWeaponSlots();
    }

    /// <summary>
    /// This Function will be called when you will click the any slot button of bought ships.
    /// </summary>
    public void SelectShipForBattle(int index)
    {
        if (ownedShipSlots[index].GetComponent<BuyManager>().hasShip && 
            ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipData != null)
        {
            highlightedShip = ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipData;
            shipSelectedIndex = index;
        }
    }

    /// <summary>
    /// This function will be called when player will click the confirm button when the ships and all its pods and guns are selected.
    /// </summary>
    public void ConfirmShip()
    {
        // Prevent from buying without the money
        // TODO: Modal saying you dont have enough currency
        if (currency - shipTotalPrice < 0) return;

        int index = 0;
        // These lists are for updating LootLocker selected Pods and Weapons
        List<string> podsList = new(){};
        List<string> weaponsList = new(){};

        for (int i = 0; i < 4; i++)
        {
            if (slotSelectedForBuy[i] || slotSelectedForMod[i])
            {
                index = i;
                
                shipSelectedIndex = i;
                // Do this if you are in Buy section
                if(slotSelectedForBuy[i])
                {
                    OwnedShips.Add(highlightedShip);
                    ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.finalPrice = Mathf.RoundToInt(shipTotalPrice * (1 - DISCOUNT));
                    // Setting the new ship to be the selected one.
                    selectedShip = highlightedShip;
                    // Update ship display sprite
                    shipDisplayArea.sprite = highlightedShip.shipAvatar;
                    shipDisplayArea.GetComponent<Image>().enabled = true;
                    playerSessionManager.shipDestroyed.GetComponent<Image>().enabled = false;
                    quickRepair.transform.Find("HealthBar").GetChild(1).GetComponent<Image>().fillAmount = 1;
                    quickRepair.SetActive(true);
                    ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipHealth = 1;
                    ownedShipSlots[index].GetComponent<BuyManager>().RefreshData();
                    // Set Ship slot health to 1 in LootLocker
                    Leaderboard.Instance.SetLeaderboardShipSlotHealth(i, 10000);
                }
                // Do this if you are in Mod section
                else
                {
                    ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.finalPrice = CalculateShipSellingPrice(ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipData.price, selectedPods, selectedWeapons);
                    // Turn mod mode to false
                    // slotSelectedForMod[i] = false;
                }

                ownedShipSlots[index].GetComponent<BuyManager>().hasShip = true;
                currency -= shipTotalPrice;
                if(currency == 0)
                {
                    currencyText.text = "0";
                }else{
                    currencyText.text = currency.ToString("#,#",CultureInfo.InvariantCulture);
                }
                // Update LootLocker Currency Leaderboard
                Leaderboard.Instance.SetLeaderboardCurrency(currency);
                break;
            }
        }

        ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipData = highlightedShip;
        ownedShipSlots[index].GetComponent<BuyManager>().RefreshData();

        // Clear pods list before adding new ones
        ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData.Clear();
        for (int i = 0; i < selectedPods.Count; i++)
        {
            ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData.Add(selectedPods[i]);
            // Get pod names to store in LootLocker
            podsList.Add(selectedPods[i].pod_Name);
        }

        // Clear weapons list before adding new ones
        ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.weaponData.Clear();
        for (int i = 0; i < selectedWeapons.Count; i++)
        {
            ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.weaponData.Add(selectedWeapons[i]);
            // Get gun names to store in LootLocker
            weaponsList.Add(selectedWeapons[i].weapon_Name);
        }

        // Added reset here because i changed it to Off mode in the UI config.
        // Needed to do this for the not enough currency condition when clicking Yes button.
        ResetBuyShipAndArmoryPanel();
        yourShipsPanel.SetActive(true);
        buyAndArmoryPanel.SetActive(false);

        selectedPods.Clear();
        selectedWeapons.Clear();
        // // Update LootLocker with the selected Ship, Pods and Guns
        // playerSessionManager.UpdatePlayerSelectedShipPodsGuns(index);
        // Stores the ship, pods and guns as owned
        playerSessionManager.UpdatePlayerOwnedShipsFile(index, highlightedShip.id, podsList, weaponsList, true);
        shipManager.AutoSelect();
        ResetSlotSelectedForBuyAndMod();
    }

    /// <summary>
    /// This will calculate the ship price with the DISCOUNT applied.
    /// <summary>
    private int CalculateShipSellingPrice(int shipPrice, List<PodData> pods, List<WeaponData> weapons)
    {
        int podsTotalPrice = 0;
        int weaponsTotalPrice = 0;

        foreach(PodData pod in pods)
        {
            podsTotalPrice += pod.price;
        }
        foreach(WeaponData weapon in weapons)
        {
            weaponsTotalPrice += weapon.price;
        }

        return Mathf.RoundToInt((shipPrice + podsTotalPrice + weaponsTotalPrice) * (1 - DISCOUNT));
    }

    /// <summary>
    /// Call this when the ship is selected and we are on the armory screen.
    /// </summary>
    public void DisableExtraPodSlots()
    {
        for (int i = 0; i < allPodSlots.Count; i++)
        {
            allPodSlots[i].GetComponent<Image>().sprite = canSelectPod_WeaponSprite;
            allPodSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
            allPodSlots[i].transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
        }

        for (int i = highlightedShip.maxMiscPods; i < allPodSlots.Count; i++)
        {
            allPodSlots[i].GetComponent<Image>().sprite = cannotSelectPod_WeaponSprite;
        }
    }

    /// <summary>
    /// Call this when the ship is selected and we are on the armory screen.
    /// </summary>
    public void DisableExtraWeaponSlots()
    {
        for (int i = 0; i < allWeaponSlots.Count; i++)
        {
            allWeaponSlots[i].GetComponent<Image>().sprite = canSelectPod_WeaponSprite;
            allWeaponSlots[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
            allWeaponSlots[i].transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
        }

        for (int i = highlightedShip.maxGuns; i < allWeaponSlots.Count; i++)
        {
            allWeaponSlots[i].GetComponent<Image>().sprite = cannotSelectPod_WeaponSprite;
        }
    }

    /// <summary>
    /// This is to empty the slots of weapon and pods on sell ship panel.
    /// </summary>
    public void EmptySellSlots()
    {
        shipManager.shipSellPods.ForEach(a => {
            a.transform.GetChild(0).GetComponent<Image>().sprite = null;
            a.transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
            a.GetComponent<Image>().sprite = cannotSelectPod_WeaponSprite;
        });
        shipManager.shipSellWeapons.ForEach(b => {
            b.transform.GetChild(0).GetComponent<Image>().sprite = null;
            b.transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
            b.GetComponent<Image>().sprite = cannotSelectPod_WeaponSprite;
            });
    }

    /// <summary>
    /// This is used to sell the ships.
    /// </summary>
    public void SellShip()
    {
        for(var i = 0; i < ownedShipSlots.Length; i++)
        {
            if(i == whichSlotShipToSell)
            {
                ownedShipSlots[i].GetComponent<Image>().sprite = notOwnedShipPanel;
                // Final price is already calculated with the DISCOUNT
                ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.finalPrice = CalculateShipSellingPrice(ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipData.price, ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.podData, ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.weaponData);
                currency += ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.finalPrice;

                ownedShipSlots[i].GetComponent<BuyManager>().hasShip = false;

                for (int x = 0; x < OwnedShips.Count; x++)
                {
                    // Remove selected ship if its sold
                    if(selectedShip != null && OwnedShips[x].id == selectedShip.id)
                    {
                        selectedShip = null;
                        // shipDisplayArea.sprite = frameShip;
                        shipDisplayArea.GetComponent<Image>().enabled = false;
                        quickRepair.SetActive(false);
                    }
                    // Remove the sold ship from OwnedShips
                    if(OwnedShips[x].id == ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipData.id)
                    {
                        OwnedShips.RemoveAt(x);
                    }
                }
                ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipData = null;
                ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.weaponData.Clear();
                ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.podData.Clear();
                Array.ForEach(ownedShipSlots[i].GetComponent<BuyManager>().extraSpecsData, x => x.value = 0);
                ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.finalPrice = 0;
                ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipHealth = 0;
                // Update Ship Health to 99999 which means there is no ship in that slot.
                Leaderboard.Instance.SetLeaderboardShipSlotHealth(i, 99999);
                currencyText.text = currency.ToString("#,#", CultureInfo.InvariantCulture);
                ownedShipSlots[i].GetComponent<BuyManager>().RefreshData();
                EmptySellSlots();
                // Update owned ships in LootLocker by removing the sold ship
                playerSessionManager.UpdatePlayerOwnedShipsFile(i, "", new List<string>(), new List<string>(), false);
                // Update currency on LootLocker
                Leaderboard.Instance.SetLeaderboardCurrency(currency);
                shipManager.AutoSelect();
                break;
            }
        }
    }

    public void ResetBuyShipSlot(int i, bool resetShipDisplayArea)
    {
        ownedShipSlots[i].GetComponent<BuyManager>().hasShip = false;
        ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipData = null;
        ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.weaponData.Clear();
        ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.podData.Clear();
        Array.ForEach(ownedShipSlots[i].GetComponent<BuyManager>().extraSpecsData, x => x.value = 0);
        ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.finalPrice = 0;
        ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipHealth = 0;
        
        if(resetShipDisplayArea)
        {
            playerSessionManager.shipDisplayArea.GetComponent<Image>().enabled = false;
            playerSessionManager.shipDestroyed.GetComponent<Image>().enabled = true;
            playerSessionManager.quickRepair.SetActive(false);

            selectedShip = null;
            data.shipData = null;
        }
        
        ownedShipSlots[i].GetComponent<BuyManager>().RefreshData();
    }

    /// <summary>
    /// This function is necessary to reset the slotSelectedForBuy and slotSelectedForMod when player exits any of those modes.
    /// </summary>
    public void ResetSlotSelectedForBuyAndMod()
    {
        for (int i = 0; i < 4; i++)
        {
            slotSelectedForBuy[i] = false;
            slotSelectedForMod[i] = false;
        }
        canBuyShipOrNot = true;
    }

    /// <summary>
    /// Sliding of slider.
    /// </summary>
    public void BuyShipToArmory()
    {
        navigationButtons.transform.GetChild(1).GetChild(0).GetComponent<Button>().interactable = true;
        selectAndUpgradeYourShipPanel.GetComponent<Image>().sprite = modPanelSprite;
        isBuyShipToArmory = true;
    }

    /// <summary>
    /// Sliding of slider.
    /// </summary>
    public void ArmoryToBuyShip()
    {
        // shipManager.shipSpecsUI[8].GetComponent<TextMeshProUGUI>().gameObject.SetActive(false);
        selectAndUpgradeYourShipPanel.GetComponent<Image>().sprite = buyPanelSprite;
        isArmoryToBuyShip = true;
    }

    /// <summary>
    /// Call this to set player name.
    /// </summary>
    public void SetPlayerName()
    {
        string newName = "";
        if(playerWelcomeName.text != "")
        {
            newName = playerWelcomeName.text;
            LootLockerSDKManager.UpdateOrCreateKeyValue(key, "1", true, (getPersistentStoragResponse) =>
            {
                Debug.Log("First Time Login set to 1");
                // Show mission notifications only on first time login
                HandleNotifications();
            });
        }
        else
            newName = playerName.text;
        
        if(newName == "") return;

        LootLockerSDKManager.SetPlayerName(newName, (response) => 
        {
            if (response.success)
            {
                Debug.Log("Player name succesfully updated");
                playerName.text = response.name;
                data.playerName = playerName.text;
                welcomePanel.SetActive(false);
            }
        });
    }

    public void SetShip()
    {
        List<string> pods = new List<string>();
        List<string> weapons = new List<string>();

        selectedShip = highlightedShip;

        playerSessionManager.UpdatePlayerSelectedShipPodsGuns(shipSelectedIndex);

        shipDisplayArea.sprite = selectedShip.shipAvatar;
        if(selectedShip == null)
        {
            shipDisplayArea.GetComponent<Image>().enabled = false;
            quickRepair.SetActive(false);
        }
        else
        {
            shipDisplayArea.GetComponent<Image>().enabled = true;
            playerSessionManager.shipDestroyed.GetComponent<Image>().enabled = false;
            quickRepair.transform.Find("HealthBar").GetChild(1).GetComponent<Image>().fillAmount = ownedShipSlots[shipSelectedIndex].GetComponent<BuyManager>().slotShipData.shipHealth;
            quickRepair.SetActive(true);
        }
        
        // Reset pods and guns
        // selectedPods.Clear();
        // selectedWeapons.Clear();
        data.podDatas.Clear();
        data.weaponDatas.Clear();

        // for (var i = 0; i < 4; i++)
        // {
            if (ownedShipSlots[shipSelectedIndex].GetComponent<BuyManager>().hasShip)
            {
                // if (highlightedShip.id == ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipData.id)
                // {
                    foreach (PodData pod in ownedShipSlots[shipSelectedIndex].GetComponent<BuyManager>().slotShipData.podData)
                    {
                        pods.Add(pod.pod_Name);
                        // selectedPods.Add(pod);
                        data.podDatas.Add(pod);
                    }
                    foreach (WeaponData weapon in ownedShipSlots[shipSelectedIndex].GetComponent<BuyManager>().slotShipData.weaponData)
                    {
                        weapons.Add(weapon.weapon_Name);
                        // selectedWeapons.Add(weapon);
                        data.weaponDatas.Add(weapon);
                    }
                    // break;
                // }
            }
        // }
        // playerSessionManager.UpdatePlayerSelectedShipPodsGuns(highlightedShip.id.ToString(), pods, weapons, true);
    }

    /// <summary>
    /// This is used to disable extra ship classes button, which have price more than left myth coins.
    /// </summary>
    /// <param name="index"> 0 = Scout, 1 = Spectre, 2 = Transport </param>
    public void DisableExtraShipClasses(int index)
    {
        if(index == 0)
        {
            for (int i = 0; i < scoutClass.transform.childCount; i++)
            {
                scoutClass.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 1f);
                scoutClass.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 1f);
                scoutClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1f);
                scoutClass.transform.GetChild(i).GetComponent<Image>().sprite = shipClassEnabled;
                scoutClass.transform.GetChild(i).GetComponent<Button>().interactable = true;
                if(Convert.ToInt32(scoutClass.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text) > currency)
                {
                    scoutClass.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 0.5f);
                    scoutClass.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 0.5f);
                    scoutClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0.5f);
                    scoutClass.transform.GetChild(i).GetComponent<Image>().sprite = shipClassDisable;
                    scoutClass.transform.GetChild(i).GetComponent<Button>().interactable = false;
                }
            }
        }

        if (index == 1)
        {
            for (int i = 0; i < spectreClass.transform.childCount; i++)
            {
                spectreClass.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 1f);
                spectreClass.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 1f);
                spectreClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1f);
                spectreClass.transform.GetChild(i).GetComponent<Image>().sprite = shipClassEnabled;
                spectreClass.transform.GetChild(i).GetComponent<Button>().interactable = true;
                if (Convert.ToInt32(spectreClass.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text) > currency)
                {
                    spectreClass.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 0.5f);
                    spectreClass.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 0.5f);
                    spectreClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0.5f);
                    spectreClass.transform.GetChild(i).GetComponent<Image>().sprite = shipClassDisable;
                    spectreClass.transform.GetChild(i).GetComponent<Button>().interactable = false;
                }
                if (!data.shipPlans.Contains("plan1"))
                {
                    spectreClass.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 0.5f);
                    spectreClass.transform.GetChild(i).GetChild(2).gameObject.SetActive(false);
                    spectreClass.transform.GetChild(i).GetChild(3).gameObject.SetActive(true);
                    spectreClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0.5f);
                    spectreClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = class2ShipNotAvailable;
                    spectreClass.transform.GetChild(i).GetChild(4).gameObject.SetActive(false);
                    spectreClass.transform.GetChild(i).GetComponent<Image>().sprite = shipClassDisable;
                    spectreClass.transform.GetChild(i).GetComponent<Button>().interactable = false;
                }
            }
        }

        if (index == 2)
        {
            for (int i = 0; i < transportClass.transform.childCount; i++)
            {
                transportClass.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 1f);
                transportClass.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 1f);
                transportClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1f);
                transportClass.transform.GetChild(i).GetComponent<Image>().sprite = shipClassEnabled;
                transportClass.transform.GetChild(i).GetComponent<Button>().interactable = true;
                if (Convert.ToInt32(transportClass.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text) > currency)
                {
                    transportClass.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 0.5f);
                    transportClass.transform.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 0.5f);
                    transportClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0.5f);
                    transportClass.transform.GetChild(i).GetComponent<Image>().sprite = shipClassDisable;
                    transportClass.transform.GetChild(i).GetComponent<Button>().interactable = false;
                }
                if (!data.shipPlans.Contains("plan2"))
                {
                    transportClass.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().color = new(1, 1, 1, 0.5f);
                    transportClass.transform.GetChild(i).GetChild(2).gameObject.SetActive(false);
                    transportClass.transform.GetChild(i).GetChild(3).gameObject.SetActive(true);
                    transportClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0.5f);
                    transportClass.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = class3ShipNotAvailable;
                    transportClass.transform.GetChild(i).GetChild(4).gameObject.SetActive(false);
                    transportClass.transform.GetChild(i).GetComponent<Image>().sprite = shipClassDisable;
                    transportClass.transform.GetChild(i).GetComponent<Button>().interactable = false;
                }

            }
        }
    }

    /// <summary>
    /// Reset State of BuyAndArmory Panel when player leaves BuyAndArmory Panel.
    /// </summary>
    public void ResetBuyShipAndArmoryPanel()
    {
        sliderContentRectTransform.anchoredPosition = new(-769.5f,384f);

        shipManager.shipSpecsUI[0].GetComponent<TextMeshProUGUI>().text = null;
        for (var i = 1; i < 7; i++)
        {
            shipManager.shipSpecsUI[i].transform.GetChild(0).gameObject.SetActive(false);
            shipManager.shipSpecsUI[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = null;
            shipManager.shipSpecsUI[i].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = " +0";
            shipManager.shipSpecsUI[i].transform.GetChild(2).gameObject.SetActive(false);
        }
        shipManager.shipSpecsUI[7].GetComponent<Image>().sprite = null;
        shipManager.shipSpecsUI[7].GetComponent<Image>().color = new(1, 1, 1, 0);
        shipManager.shipSpecsUI[8].GetComponent<TextMeshProUGUI>().text = null;

        allPodSlots.ForEach(a => {
            a.transform.GetChild(0).GetComponent<Image>().sprite = null;
            a.transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
            a.GetComponent<Image>().sprite = cannotSelectPod_WeaponSprite;
            a.GetComponent<Button>().interactable = false;
        });
        allWeaponSlots.ForEach(a => {
            a.transform.GetChild(0).GetComponent<Image>().sprite = null;
            a.transform.GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
            a.GetComponent<Image>().sprite = cannotSelectPod_WeaponSprite;
            a.GetComponent<Button>().interactable = false;
        });

        if (scoutClass.activeSelf)
            scoutClass.SetActive(false);

        if (spectreClass.activeSelf)
            spectreClass.SetActive(false);

        if (transportClass.activeSelf)
            transportClass.SetActive(false);

        ResetShipClassesButtons(-1);
        ResetShipTypesButtons(-1);

        ResetPodAndWeaponCount();
    }

    /// <summary>
    /// This is used to repair a ship.
    /// </summary>
    public void RepairShip()
    {
        for (int i = 0; i < 4; i++)
        {
            if (slotSelectedForRepair[i])
            {
                if(currency >= Convert.ToInt32(repairTotalPrice))
                {
                    Debug.Log("totalPrice: " + totalPrice);
                    Debug.Log("currency: " + currency);
                    Debug.Log("repairTotalPrice: " + repairTotalPrice);
                    
                    ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipHealth = 1;
                    quickRepair.transform.Find("HealthBar").GetChild(1).GetComponent<Image>().fillAmount = 1;
                    Leaderboard.Instance.SetLeaderboardShipSlotHealth(i, 10000);
                    currency -= Convert.ToInt32(totalPrice);
                    Leaderboard.Instance.SetLeaderboardCurrency(currency);
                    currencyText.text = currency.ToString("#,#",CultureInfo.InvariantCulture);
                    ownedShipSlots[i].GetComponent<BuyManager>().RefreshData();
                }
                else
                {
                    float repairAmount = totalPrice / GAME_REPAIR_PRICE / repairShipTotalHealth;
                    Debug.Log("repairShipTotalHealth: " + repairShipTotalHealth);
                    ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipHealth += repairAmount;
                    quickRepair.transform.Find("HealthBar").GetChild(1).GetComponent<Image>().fillAmount += repairAmount;
                    Leaderboard.Instance.SetLeaderboardShipSlotHealth(i, Convert.ToInt32(ownedShipSlots[i].GetComponent<BuyManager>().slotShipData.shipHealth * 10000));
                    currency = 0;
                    Leaderboard.Instance.SetLeaderboardCurrency(currency);
                    currencyText.text = "0";
                    ownedShipSlots[i].GetComponent<BuyManager>().RefreshData();
                }
                return;
            }
        }
    }

    /// <summary>
    /// This is used to reset the value of number of pods and weapons added in ship.
    /// </summary>
    public void ResetPodAndWeaponCount()
    {
        for(int x = 0; x < 4; x++)
        {
            if(slotSelectedForBuy[x] == true)
            for (int i = 0; i < 4; i++)
                ownedShipSlots[x].GetComponent<BuyManager>().extraSpecsData[i].value = 0;
        }
        weaponsAdded = 0; 
        podsAdded = 0;
    }

    public void ResetPilotSelection() => selectedPilot = null;

    public void SceneChanger()
    {
        if(selectedShip == null || selectedPilot == null || ownedShipSlots[shipSelectedIndex].GetComponent<BuyManager>().slotShipData.shipHealth == 0) 
            return;

        data.previousScene = "MainMenu";

        data.podDatas.Clear();
        data.weaponDatas.Clear();

        data.pilotData = selectedPilot;
        data.shipData = selectedShip;

        data.slot = shipSelectedIndex;
        data.shipHealth = ownedShipSlots[shipSelectedIndex].GetComponent<BuyManager>().slotShipData.shipHealth;
        ownedShipSlots[shipSelectedIndex].
            GetComponent<BuyManager>().slotShipData.podData.ForEach(pod =>
            {
                data.podDatas.Add(pod);
            });
        ownedShipSlots[shipSelectedIndex].
            GetComponent<BuyManager>().slotShipData.weaponData.ForEach(pod =>
            {
                data.weaponDatas.Add(pod);
            });

        data.currency = currency;
        StartCoroutine(ChangeScene());
    }

    private void PositionLogo()
    {
        // Get the rendered width of the TextMeshPro text
        float textWidth = quickRepairDiscountPriceText.renderedWidth;

        // Calculate the new position for the logo
        // You might need to adjust the offset value to get the exact positioning you want
        float offset = 60f; // Space between the end of the text and the start of the logo
        Vector3 newPosition = quickRepairDiscountPriceText.rectTransform.localPosition;
        newPosition.x += textWidth - offset;

        // Set the logo's position
        logoRectTransform.localPosition = newPosition;

    }

    /// <summary>
    /// This is used to quick repair selected ship.
    /// </summary>
    /// <param name="index"></param>
    public void QuickRepair()
    {
        int pilotDiscount = 0;
        quickRepairDiscountPriceText.text = "";
        mythLogo.enabled = true;
        
        // Calculate additional values from pilot's skills
        if(selectedPilot)
        {
            foreach(Skill skill in selectedPilot.skills)
            {
                if(skill.skill_Type == Skill.Skill_Type.RepairCost)
                {
                    pilotDiscount += skill.value;
                    break;
                }
            }
        }
        
        if(currency > 0)
        {
            for (int i = 0; i < 4; i++)
                slotSelectedForRepair[i] = false;
            
            int index = shipSelectedIndex;
            slotSelectedForRepair[index] = true;

            // // Total Price = Ship price + Pods price + Guns price
            int totalHealth = ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipData.hull;
            int hullPodsValue = 0;
            for(int x = 0;x < ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData.Count;x++)
            {
                // totalHealth += ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData[x].data.data_Type.ToString().Equals("Hull");
                if(ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData[x].data.data_Type.ToString() == "Hull")
                {
                    hullPodsValue += ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.podData[x].data.value;
                }
            }
            // for(int x = 0;x < ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.weaponData.Count;x++)
            //     totalPrice += ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.weaponData[x].price;

            // Calculate Repair Price
            repairTotalPrice = Convert.ToInt32(
                (1 - ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipHealth)
                    * (totalHealth + hullPodsValue) * GAME_REPAIR_PRICE
                );
            
            repairShipTotalHealth = totalHealth + hullPodsValue;

            float repairAmount;
            // Set popup price text
            if (repairTotalPrice > currency)
            {
                quickRepairPriceText.text = currency.ToString();
                repairAmount = currency / GAME_REPAIR_PRICE;
                // repairTotalPrice = currency;

                totalPrice = currency;
            }
            else if(repairTotalPrice != 0)
            {
                repairAmount = (1 - ownedShipSlots[index].GetComponent<BuyManager>().slotShipData.shipHealth)
                    * (totalHealth + hullPodsValue);
                
                totalPrice = repairTotalPrice;
                quickRepairPriceText.text = repairTotalPrice.ToString();
            }
            else
                return;

            if(pilotDiscount < 0)
            {
                mythLogo.enabled = false;
                quickRepairPriceText.text = "<u color=#DB60D8>" + repairTotalPrice.ToString() + "</u>";

                // Calculate Repair Price
                int repairDiscountPrice = Convert.ToInt32(
                    repairTotalPrice * ((float)(100 + pilotDiscount) / 100)
                    );
                
                totalPrice = repairDiscountPrice;
                
                quickRepairDiscountPriceText.text = repairDiscountPrice.ToString();
            }

            quickRepairPopup.SetActive(true);
            quickRepairPopup.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Are you sure you want to repair " + repairAmount.ToString("F1") + " hull for:";
        }
    }

    private IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(1);
    }

    public void SyncScrollingForKills(float value)
    {
        if (manualScroll) return;

        manualScroll = true;

        foreach (var scrollRect in scrollRectsForKills)
        {
            scrollRect.verticalNormalizedPosition = value;
        }

        manualScroll = false;
    }

    public void SyncScrollingForMyths(float value)
    {
        if (manualScroll) return;

        manualScroll = true;

        foreach (var scrollRect in scrollRectsForKills)
        {
            scrollRect.verticalNormalizedPosition = value;
        }

        manualScroll = false;
    }

    public void SyncScrollingForHours(float value)
    {
        if (manualScroll) return;

        manualScroll = true;

        foreach (var scrollRect in scrollRectsForKills)
        {
            scrollRect.verticalNormalizedPosition = value;
        }

        manualScroll = false;
    }

    private void OnApplicationQuit()
    {
        data.whichScene = " ";
        data.volumeValue = -1;
        data.previousScene = "";
        data.discoveredNodes.Clear();
        data.actualNode = "";
        data.shipPlans.Clear();
        data.shipData = null;
        data.missionsData.Clear();
        data.nodesData.Clear();
        data.shipFunNames.Clear();
    }
}