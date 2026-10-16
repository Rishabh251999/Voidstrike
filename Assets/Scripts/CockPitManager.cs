using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CockPitManager : MonoBehaviour
{
    #region Attributes
    public static CockPitManager Instance;

    [Header("List")]
    [SerializeField] private List<GameObject> enemyShipSpawnPoints;
    [SerializeField] private List<GameObject> shieldBar;
    [SerializeField] private List<Sprite> shield;
    [SerializeField] private List<Sprite> shieldDrag;
    public List<Sprite> enemyShipAttackSprite;
    public List<Sprite> enemyShipSprite;
    public List<PodData> podDatas;
    public List<WeaponData> weaponDatas;
    private List<int> availableNumbers;

    [Header("Array")]
    public int[] NPCShipsAggressionRanges = new int[6] { 3, 6, 7, 10, 11, 14 };

    [Header("System")]
    private System.Random random;

    [Header("GameObject")]
    [SerializeField] private GameObject mapView;
    public GameObject enemyShipsParent;
    public GameObject engageButton;
    public GameObject disengageButton;
    public GameObject fireLaser;
    public GameObject enemyShipObject; // The one with you have stored as a temp variable for reference. 
    public GameObject selectedEnemyShip; // The one with which you have started battling.
    public GameObject homePlanet;
    public GameObject NPCAttackDamagePanel;
    public GameObject playerDiedPanel;
    public GameObject enemyFireReceiver;
    public GameObject playerBattleManager;
    public GameObject fullScreenButtons;
    public GameObject cockpitAnimation;
    private GameObject shipToBeDestroyed;
    public GameObject debugButtons;
    public GameObject chatPanel;
    public GameObject chatScrollBar;
    public GameObject chatScroller;
    public GameObject controlPanel;
    public GameObject enemyShipFrame;
    public GameObject musicPlayer;

    [Header("Text")]
    public TextMeshProUGUI playerHealth;
    public TextMeshProUGUI enemyShipName;
    public TextMeshProUGUI enemyShipClassName;
    public TextMeshProUGUI playerShipName;
    public TextMeshProUGUI shieldBarValue;

    [Header("SO")]
    public Data data;

    [Header("Texture")]
    public Texture2D defaultCursor;
    public Texture2D redCursor;

    [Header("UI")]
    [SerializeField] private Scrollbar shieldBarSlider;
    public Scrollbar enemyHealthBarFill;
    public Button jumpButton;

    [Header("Image")]
    public Image enemyShipImage;
    public Image playerShipImage;
    public Image playerHP;
    public Image jumpBar;

    [Header("Sprite")]
    public Sprite planet1;
    public Sprite planet2;
    public Sprite planet3;
    public Sprite mapViewDamageFrame;
    public Sprite mapViewDefaultFrame;
    public Enemyshipdata enemyShipData;
    // public Sprite chatAttacked;
    public Sprite defaultChatAttacked;
    public Sprite universalChatAttacked;
    public Sprite chatDefault;
    public Sprite chatUniversal;
    public Sprite chatScrollAttacked;
    public Sprite chatScrollDefault;
    public Sprite musicBarDefault;
    public Sprite musicBarAttacked;
        
    [Header("float")]
    private float k;
    private float newFillTime;
    private float currentTime = 0f;
    private float previousNormalizedTime = 0f;
    public float shieldPercentage = 0.0f;

    [Header("int")]
    public int damageIncreaseForGuns = 0; // Debug system

    [Header("bool")]
    public bool isInvinsible = false; // Debug system
    public bool canScrollChatMessages;

    [Header("Managers")]
    public BattleManager battleManager;
    public VisitedPlanetsManager visitedPlanetsManager;
    public ShipManager shipManager;

    // This variable is the chance of a npc ship to appear in the cockpit view
    private readonly float PROBABILITY_OF_NPC_SHIP_APPEARANCE = 0.5f;

    private bool isDead = false;
    public int processesToFinish = 0;
    Color redColor = Color.red;

    [Header("Chat")]
    [SerializeField] private GameObject chatUI;
    [SerializeField] private GameObject chatContent;
    [SerializeField] private GameObject chatLog;
    [SerializeField] private Scrollbar scrollbarVertical;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField chatInput;
    #endregion

    private void Awake()
    {
        data.whichScene = "GamePlay";

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        Cursor.lockState = CursorLockMode.Confined;
    }

    private void Start()
    {
        //Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        Leaderboard.Instance.SetLeaderboardLastLogin();

        int nodesDataIndex = data.discoveredNodes.FindIndex(nodeData => nodeData == data.actualNode);
        if (nodesDataIndex == -1)
        {
            data.discoveredNodes.Add(data.actualNode);
            visitedPlanetsManager.UpdateVisitedPlanets(data.actualNode);
        }
        
        if (data.actualNode == "A1")
            homePlanet.GetComponent<Image>().sprite = planet1;
        else if(data.actualNode == "A5")
            homePlanet.GetComponent<Image>().sprite = planet2;
        else if(data.actualNode == "A9")
            homePlanet.GetComponent<Image>().sprite = planet3;
        else
        {
            cockpitAnimation.SetActive(false);
            homePlanet.GetComponent<Image>().enabled = false;
        }

        // Set player HP.
        playerHP.fillAmount = data.shipHealth;

        random = new System.Random();

        SpawnShips();

        playerShipImage.color = new(1,1,1,1);
        playerShipImage.sprite = data.shipData.shipAvatar;

        playerShipName.text = data.playerName;

        CalculateConstant();

        StartCoroutine(JumpBarStartDelay());

        FindingShortestEnemy();
    }

    private void SpawnShips()
    {
        InitializeAvailableNumbers();
        NodeData nodeData = data.nodesData.Find(obj => obj.name == data.actualNode);
        int nodeIndex = Array.FindIndex(MapViewManager.Instance.allNodes, nodeData => nodeData.name == data.actualNode);
        List<string> alreadyUsedNames = new();
        int counter = 0;

        if(nodeData && DateTime.Now < nodeData.resetTime)
        {
            foreach (NpcNodeData npcNodeData in nodeData.currentShips)
            {
                if(npcNodeData.shipCombatStats.hull > 0)
                {
                    foreach (GameObject nodeNpcShip in MapViewManager.Instance.allNodes[nodeIndex].npcShips)
                    {
                        if(nodeNpcShip.name == npcNodeData.shipName)
                        {
                            GameObject go = Instantiate(nodeNpcShip,
                                enemyShipSpawnPoints[npcNodeData.spawnPlace].transform.position, Quaternion.identity);

                            go.transform.SetParent(enemyShipsParent.transform);
                            go.transform.localScale = Vector3.one;
                            go.transform.GetChild(1).GetChild(0).GetComponent<Scrollbar>().size = npcNodeData.shipCombatStats.hull / npcNodeData.shipCombatStats.maxHull;
                            BattleManager battleManager = go.GetComponent<BattleManager>();
                            Enemy enemy = go.GetComponent<Enemy>();

                            enemy.enemyFunName = npcNodeData.shipFunName;
                            battleManager.spawnPlace = npcNodeData.spawnPlace;
                            battleManager.playerPilot = npcNodeData.pilot;
                            battleManager.playerShip = npcNodeData.ship;
                            battleManager.playerStats = npcNodeData.shipCombatStats;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            foreach (GameObject nodeNpcShip in data.nodeNpcShips)
            {
                float randomValue = UnityEngine.Random.Range(0f, 1f);

                if(randomValue >= PROBABILITY_OF_NPC_SHIP_APPEARANCE)
                {
                    int spawnPlace = GenerateRandomNumber();
                    GameObject go = Instantiate(nodeNpcShip,
                        enemyShipSpawnPoints[spawnPlace].transform.position, Quaternion.identity);
                    
                    go.transform.SetParent(enemyShipsParent.transform);
                    go.transform.localScale = Vector3.one;

                    BattleManager battleManager = go.GetComponent<BattleManager>();
                    Enemy enemy = go.GetComponent<Enemy>();
                    
                    foreach(ShipData gameShip in shipManager.allShipsData)
                    {
                        if(gameShip.id == enemy.shipdata.id)
                        {
                            // int random = 1;
                            // int index = alreadyUsedNames.FindIndex(item => item == gameShip.id);
                            // if(index != -1)
                            // {
                            //     alreadyUsedNames.Add(gameShip.id);
                            //     data.shipFunNames.Add()
                            // }
                            enemy.enemyFunName = data.shipFunNames[counter]; // enemy.shipdata.classShipNames[random];
                            break;
                        }
                    }

                    if (battleManager != null)
                    {
                        battleManager.spawnPlace = spawnPlace;
                    }

                }
                counter++;
            }
        }
    }

    /// <summary>
    /// This is to find the ShortestEnemy to sync the laser and the damage panel.
    /// </summary>
    private void FindingShortestEnemy()
    {
        if (enemyShipsParent.transform.childCount <= 0)
            return;

        Enemy shortestEnemy = enemyShipsParent.transform.GetChild(0).GetComponent<Enemy>();

        for (int i = 0; i < enemyShipsParent.transform.childCount; i++)
        {
            if (shortestEnemy.randomPlayerAttackTime < enemyShipsParent.transform.GetChild(i).GetComponent<Enemy>().randomPlayerAttackTime)
                shortestEnemy = enemyShipsParent.transform.GetChild(i).GetComponent<Enemy>();
        }

        shortestEnemy.canShowDamagePanel = true;
    }


    private void Update()
    {
        if (selectedEnemyShip != null)
        {
            if (selectedEnemyShip.transform.GetChild(1).GetChild(0).GetComponent<Scrollbar>().size <= 0.0f)
            {
                //selectedEnemyShip.GetComponent<Image>().enabled = false;
                selectedEnemyShip.transform.GetChild(0).gameObject.SetActive(false);
                selectedEnemyShip.transform.GetChild(1).gameObject.SetActive(false);
                selectedEnemyShip.transform.GetChild(2).gameObject.SetActive(false);
                selectedEnemyShip.transform.GetChild(3).gameObject.SetActive(false);
                selectedEnemyShip.GetComponent<BattleManager>().isEngaged = false;
                playerBattleManager.GetComponent<BattleManager>().isEngaged = false;
                selectedEnemyShip.GetComponent<Animator>().enabled = true;

                if(enemyShipObject == selectedEnemyShip)
                {
                    enemyShipImage.color = new(1, 1, 1, 0);
                    enemyShipImage.sprite = null;
                    enemyShipName.text = " ";
                    enemyShipClassName.text = " ";
                    enemyHealthBarFill.transform.parent.gameObject.SetActive(false);
                }

                // This is used for saving the reward and the kill in LootLocker. It also updates AllData currency and kills
                int reward = selectedEnemyShip.GetComponent<Enemy>().rewardMyth;
                Leaderboard.Instance.SetLeaderboardCurrency(data.currency + reward);
                data.currency += reward;
                Leaderboard.Instance.SetLeaderboardKills(data.kills + 1);

                CheckMissionCompleted("npc", selectedEnemyShip.name.Replace("(Clone)", ""));

                data.kills++;
                StartCoroutine(NPCDyingDelay());
                fireLaser.SetActive(false);
                MusicPlayerManager.Instance.PlayMusic("Ship Destroy");

                // Insert in chat
                GameObject chat = Instantiate(chatLog);
                chat.GetComponent<TextMeshProUGUI>().text = "Chippy: You killed " + selectedEnemyShip.GetComponent<Enemy>().enemyFunName + ", a " + selectedEnemyShip.name.Replace("(Clone)", "") + 
                " ship, and received " + reward + " M! Your total is now: " + data.currency.ToString("#,#",CultureInfo.InvariantCulture) + " M";
                chat.transform.SetParent(chatContent.transform, false);

                // Canvas.ForceUpdateCanvases(); // Update the Canvas so the new content is included in the layout
                chatPanel.transform.GetChild(0).GetChild(0).GetComponent<ScrollRect>().verticalNormalizedPosition = 0f; // Scroll to the bottom

                shipToBeDestroyed = selectedEnemyShip;
                battleManager.StopEngageCoroutine();
                selectedEnemyShip = null;

                engageButton.SetActive(true);
                disengageButton.SetActive(false);
            }
        }

        if (playerHP.fillAmount <= 0.01)
        {
            playerDiedPanel.SetActive(true);
            mapView.SetActive(false);
            fireLaser.SetActive(false);
            playerBattleManager.GetComponent<BattleManager>().isEngaged = false;
            playerHealth.text = "0";
            NPCAttackDamagePanel.SetActive(false);
            chatPanel.transform.GetChild(0).GetComponent<Image>().sprite = chatDefault;
            chatPanel.transform.GetChild(1).GetComponent<Image>().sprite = chatUniversal;
            chatScrollBar.GetComponent<Image>().sprite = chatScrollDefault;
            musicPlayer.GetComponent<Image>().sprite = musicBarDefault;
            Color transparent = new Color(1f, 1f, 1f, 1f);
            chatScroller.GetComponent<Image>().color = transparent;
            controlPanel.GetComponent<Image>().color = transparent;
            enemyShipFrame.GetComponent<Image>().color = transparent;

            mapView.transform.Find("Frame").GetComponent<Image>().sprite = mapViewDefaultFrame;

            if(!isDead)
            {
                processesToFinish = 0;
                // Set actualNode to Planet 1 when player dies so it will reappear there.
                data.lastVisitedPlanet = "A1";
                data.actualNode = "A1";
                visitedPlanetsManager.UpdateVisitedPlanets("A1");

                // Sets health in LootLocker to 99999 which means that the slot is empty.
                battleManager.SetLootlockerShipHP(data.slot, 99999);
                battleManager.RemoveFromOwnedShipsFile(data.slot);
                data.shipData = null;
                isDead = true;
            }

            for (int i = 0; i < enemyShipsParent.transform.childCount; i++)
            {
                enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().isEngaged = false;
                enemyShipsParent.transform.GetChild(i).GetChild(3).gameObject.SetActive(false);
            }
        }

        if (currentTime < newFillTime && jumpBar.fillAmount != 1)
        {
            float normalizedTime = previousNormalizedTime + (Time.deltaTime / newFillTime);

            jumpBar.fillAmount = normalizedTime;

            currentTime = Time.deltaTime;

            previousNormalizedTime = normalizedTime;
        } 

        // Added this to enable Jump bar button only when bar is full
        if(jumpBar.fillAmount == 1)
            jumpButton.interactable = true;

        // Send chat message
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!string.IsNullOrEmpty(chatInput.text))
            {
                // Insert in chat
                GameObject chat = Instantiate(chatLog);
                chat.GetComponent<TextMeshProUGUI>().text = data.playerName + ": " + chatInput.text;
                chat.transform.SetParent(chatContent.transform, false);
                chat = Instantiate(chatLog);
                chat.GetComponent<TextMeshProUGUI>().text = "Chippy: " + data.playerName + " you're talking to yourself.";
                chat.transform.SetParent(chatContent.transform, false);
                chatInput.text = "";

                // Refocus on the input field
                chatInput.Select();
                chatInput.ActivateInputField();
            }   
        }

        if(!canScrollChatMessages)
            scrollbarVertical.value = 0;
    }

    /// <summary>
    /// This will allow ius to scroll back the messages.
    /// </summary>
    public void CanScrollChatMessages(bool value) => canScrollChatMessages = value;


    /// <summary>
    /// To change the crosshair on the basis of UI.
    /// </summary>
    public void CrossHairChanger(Texture2D crosshair)
    {
        if (crosshair.name.Equals("Blue") || crosshair.name.Equals("Red"))
            Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);

        else if(crosshair.name.Equals("Hand"))
            Cursor.SetCursor(crosshair, new Vector2(20, 4), CursorMode.Auto);

        else
            Cursor.SetCursor(crosshair, new Vector2(10, 4), CursorMode.Auto);
    }
    

    public void SyncPlayerHealthAcrossAllEnemy()
    {
        for (int i = 0; i < enemyShipsParent.transform.childCount; i++)
            enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().enemyStats.hull = playerBattleManager.GetComponent<BattleManager>().playerStats.hull;
    }


    // private IEnumerator DisplayDamagePanel()
    // {
    //     NPCAttackDamagePanel.SetActive(true);
    //     chatPanel.transform.GetChild(0).GetComponent<Image>().sprite = defaultChatAttacked;
    //     chatPanel.transform.GetChild(1).GetComponent<Image>().sprite = universalChatAttacked;
    //     chatScrollBar.GetComponent<Image>().sprite = chatScrollAttacked;
    //     musicPlayer.GetComponent<Image>().sprite = musicBarAttacked;
    //     chatScroller.GetComponent<Image>().color = redColor;
    //     controlPanel.GetComponent<Image>().color = redColor;
    //     enemyShipFrame.GetComponent<Image>().color = redColor;

    //     mapView.transform.Find("Frame").GetComponent<Image>().sprite = mapViewDamageFrame;

    //     yield return new WaitForSeconds(0.5f);

    //     NPCAttackDamagePanel.SetActive(false);
    //     // chatPanel.GetComponent<Image>().sprite = chatDefault;
    //     chatPanel.transform.GetChild(0).GetComponent<Image>().sprite = chatDefault;
    //     chatPanel.transform.GetChild(1).GetComponent<Image>().sprite = chatUniversal;
    //     chatScrollBar.GetComponent<Image>().sprite = chatScrollDefault;
    //     musicPlayer.GetComponent<Image>().sprite = musicBarDefault;
    //     Color transparent = new Color(1f, 1f, 1f, 1f);
    //     chatScroller.GetComponent<Image>().color = transparent;
    //     controlPanel.GetComponent<Image>().color = transparent;
    //     enemyShipFrame.GetComponent<Image>().color = transparent;
        
    //     mapView.transform.Find("Frame").GetComponent<Image>().sprite = mapViewDefaultFrame;
    // }

    public void DisplayDamage() => StartCoroutine(DisplayDamagePanel());

    // Add a Coroutine variable to keep track of the currently running coroutine
    private Coroutine currentDisplayDamageCoroutine = null;

    private IEnumerator DisplayDamagePanel()
    {
        // If there is a currently running instance of the coroutine, stop it
        if (currentDisplayDamageCoroutine != null)
        {
            StopCoroutine(currentDisplayDamageCoroutine);
        }

        // Reset UI to default state before setting to attacked state
        ResetUIDefault();

        // Set UI elements to their "attacked" state
        NPCAttackDamagePanel.SetActive(true);
        chatPanel.transform.GetChild(0).GetComponent<Image>().sprite = defaultChatAttacked;
        chatPanel.transform.GetChild(1).GetComponent<Image>().sprite = universalChatAttacked;
        chatScrollBar.GetComponent<Image>().sprite = chatScrollAttacked;
        musicPlayer.GetComponent<Image>().sprite = musicBarAttacked;
        chatScroller.GetComponent<Image>().color = redColor;
        controlPanel.GetComponent<Image>().color = redColor;
        enemyShipFrame.GetComponent<Image>().color = redColor;
        mapView.transform.Find("Frame").GetComponent<Image>().sprite = mapViewDamageFrame;

        // Wait for a specified amount of time
        yield return new WaitForSeconds(0.5f);

        // Reset UI to its default state
        ResetUIDefault();

        // Set the current coroutine reference to null as it's now finished
        currentDisplayDamageCoroutine = null;
    }
    
    private void ResetUIDefault()
    {
        // Set UI elements back to their default state
        NPCAttackDamagePanel.SetActive(false);
        chatPanel.transform.GetChild(0).GetComponent<Image>().sprite = chatDefault;
        chatPanel.transform.GetChild(1).GetComponent<Image>().sprite = chatUniversal;
        chatScrollBar.GetComponent<Image>().sprite = chatScrollDefault;
        musicPlayer.GetComponent<Image>().sprite = musicBarDefault;
        Color transparent = new Color(1f, 1f, 1f, 1f);
        chatScroller.GetComponent<Image>().color = transparent;
        controlPanel.GetComponent<Image>().color = transparent;
        enemyShipFrame.GetComponent<Image>().color = transparent;
        mapView.transform.Find("Frame").GetComponent<Image>().sprite = mapViewDefaultFrame;
    }

    private IEnumerator NPCDyingDelay()
    {
        yield return new WaitForSeconds(2.133f);

        Destroy(shipToBeDestroyed);
    }


    /// <summary>
    /// This is to calculalte the damage taken by the player from NPC ships. This is only one sided, which means only player will get damaged.
    /// </summary>
    public float CalculateDamageTakenForPlayer(float accuracy, float evasion, float gunDamage, float shieldValue, float shieldSlider)
    {
        float damageTaken = 0f;
        // float damageAfterShield = 0f;
        float randAccuracy = UnityEngine.Random.Range(1, 101);
        float randEvasion = UnityEngine.Random.Range(1, 101);

        if (randAccuracy <= accuracy)
        {
            if (!(randEvasion <= evasion))
            {
                damageTaken = (gunDamage - (gunDamage * (0.7f * shieldValue * shieldSlider / 100))) * 6;
            }
        }
        return damageTaken;
    }


    /// <summary>
    /// This is to give a delay to get the JumpSpeed of player.
    /// </summary>
    private IEnumerator JumpBarStartDelay()
    {
        yield return new WaitForSeconds(1);

        playerHealth.text = MathF.Floor(data.shipMaxHealth * data.shipHealth).ToString();
        OnScrollbarValueChanged();
    }


    /// <summary>
    /// Set the player's HP scrollbar value.
    /// </summary>
    public void ScrollBarValueCalculator(float maxValue, float currentValue) => playerHP.fillAmount = currentValue / maxValue;


    /// <summary>
    /// Initializes the list so that we can keep track of which variables we have to remove.
    /// </summary>
    private void InitializeAvailableNumbers()
    {
        availableNumbers = new List<int>();
        for (int i =0; i < enemyShipSpawnPoints.Count; i++)
            availableNumbers.Add(i);
    }


    /// <summary>
    /// Generate a random number and then remove it from it list so that it doesn't comes again.
    /// </summary>
    private int GenerateRandomNumber()
    {
        int index = random.Next(0, availableNumbers.Count);
        int randomNumber = availableNumbers[index];
        availableNumbers.RemoveAt(index);

        return randomNumber;
    }


    /// <summary>
    /// This changes the image of the shield bar when scrolled.
    /// </summary>
    public void OnScrollbarValueChanged()
    {
        int index = Mathf.RoundToInt(shieldBarSlider.value * (shieldBar.Count - 1));

        for (int i = 0; i <= index; i++)
        {
            if (i == index)
            {
                shieldBar[i].GetComponent<Image>().sprite = shieldDrag[i];

                if (i == 0)
                {
                    newFillTime = CalculateTimeToFill(CalculateJumpSpeed(0));
                    shieldPercentage = 0;
                    shieldBarValue.text = Math.Round(CalculateShield(0), MidpointRounding.AwayFromZero).ToString();
                }

                else if (i == 1)
                {
                    newFillTime = CalculateTimeToFill(CalculateJumpSpeed(25));
                    shieldPercentage = 0.25f;
                    shieldBarValue.text = Math.Round(CalculateShield(25), MidpointRounding.AwayFromZero).ToString();
                }

                else if (i == 2)
                {
                    newFillTime = CalculateTimeToFill(CalculateJumpSpeed(50));
                    shieldPercentage = 0.50f;
                    shieldBarValue.text = Math.Round(CalculateShield(50), MidpointRounding.AwayFromZero).ToString();
                }

                else if (i == 3)
                {
                    newFillTime = CalculateTimeToFill(CalculateJumpSpeed(75));
                    shieldPercentage = 0.75f;
                    shieldBarValue.text = Math.Round(CalculateShield(75), MidpointRounding.AwayFromZero).ToString();
                }

                else if (i == 4)
                {
                    newFillTime = CalculateTimeToFill(CalculateJumpSpeed(99));
                    shieldPercentage = 1.0f;
                    shieldBarValue.text = Math.Round(CalculateShield(100), MidpointRounding.AwayFromZero).ToString();
                }
            }
               
            else
                shieldBar[i].GetComponent<Image>().sprite = shield[i];

            shieldBar[i].SetActive(true);
        }

        for (int i = index + 1; i < shieldBar.Count; i++)
        {
            shieldBar[i].SetActive(false);
        }
    }


    /// <summary>
    /// This is to calculate the JumpSpeed of Player on the basis of Shield Percentage.
    /// </summary>
    public float CalculateJumpSpeed(float shieldPercentage) => 
        playerBattleManager.GetComponent<BattleManager>().playerStats.jumpSpeed * (1f - (shieldPercentage / 100));


    /// <summary>
    /// This is to calculate the shield value of shield.
    /// </summary>
    public float CalculateShield(float shieldPercentage) => playerBattleManager.GetComponent<BattleManager>().playerStats.shields * (shieldPercentage / 100);


    /// <summary>
    /// This is to calculate the constant at which the the JumpSpeed bar will change.
    /// </summary>
    private void CalculateConstant()
    {
        float maxJumpSpeed = 26f; // You can set this value
        float timeToFill = 9f;    // You can set this value

        // Calculate the constant 'k'
        k = maxJumpSpeed * timeToFill;
    }


    /// <summary>
    /// This is to calculate the time to fill the JumpSpeed Bar.
    /// </summary>
    public float CalculateTimeToFill(float jumpSpeed)
    {
        // Calculate time using the formula
        float timeToFill = k / jumpSpeed;
        return Mathf.Max(timeToFill, 0f); // Ensure time is non-negative
    }


    /// <summary>
    /// This will make the engage button non-interatcable if no ship is selected. 
    /// </summary>
    public void EnableDisableEngageButton(bool value)
    {
        if(value)
            engageButton.GetComponent<Button>().interactable = true;
        else
            engageButton.GetComponent<Button>().interactable = false;
    }


    /// <summary>
    /// Return to Home Planet.
    /// </summary>
    public void HomePlanet()
    {
        MapViewManager.Instance.UpdateNpcNodes();
        if(processesToFinish >= 3 || playerHP.fillAmount > 0)
        {
            data.volumeValue = MusicPlayerManager.Instance.volumeController.value;
            // data.shipHealth = playerHP.fillAmount;
            data.previousScene = "GamePlay";
            SceneManager.LoadScene(0);
        }
    }

    /// <summary>
    /// Changes to MapView
    /// </summary>
    public void MapView()
    {
        // if(jumpBar.fillAmount == 1)
        // {
            if(data.volumeValue != MusicPlayerManager.Instance.volumeController.value)
                battleManager.UpdatePlayerSoundVolume();
                
            MapViewManager.Instance.RenderMapView();
            mapView.SetActive(true);
            fullScreenButtons.SetActive(false);
            fullScreenButtons.SetActive(true);
        // }
    }


    /// <summary>
    /// Refreshes the scene when entering CockPitView from MapView.
    /// </summary>
    public void RefreshScene()
    {
        jumpBar.fillAmount = 0;
        jumpButton.interactable = false;
        currentTime = 0f;
        previousNormalizedTime = 0f;
        
        string nodeName = data.actualNode;

        homePlanet.GetComponent<Image>().enabled = true;
        if(nodeName == "A1")
            homePlanet.GetComponent<Image>().sprite = planet1;
        else if(nodeName == "A5")
            homePlanet.GetComponent<Image>().sprite = planet2;
        else if(nodeName == "A9")
            homePlanet.GetComponent<Image>().sprite = planet3;
        else
            homePlanet.GetComponent<Image>().enabled = false;

        SpawnShips();
        FindingShortestEnemy();

        playerBattleManager.GetComponent<BattleManager>().DisEngage();

        enemyShipImage.color = new(1, 1, 1, 0);
        enemyShipImage.sprite = null;
        enemyShipName.text = " ";
        enemyShipClassName.text = " ";
        enemyHealthBarFill.transform.parent.gameObject.SetActive(false);
    }

    /// <summary>
    /// This is used to check if the mission is completed
    /// </summary>
    private void CheckMissionCompleted(string type, string name)
    {
        foreach(Mission mission in data.missionsData)
        {
            if(mission.target.type == type && mission.target.name == name)
            {
                data.currency += mission.reward;
                battleManager.UpdatePlayerMissions(mission.id);
                // Insert in chat
                GameObject chat = Instantiate(chatLog);
                chat.GetComponent<TextMeshProUGUI>().text = "Chippy: <color=#FF00FF>Congrats you’ve been awarded " + mission.reward.ToString("#,#",CultureInfo.InvariantCulture) + " M for accomplishing the Kill a " + mission.target.name + " mission!</color> Your total is now: " + data.currency.ToString("#,#",CultureInfo.InvariantCulture) + " M";
                chat.transform.SetParent(chatContent.transform, false);

                Leaderboard.Instance.SetLeaderboardCurrency(data.currency);

                data.missionsData.Remove(mission);
                break;
            }
        }
    }

    /// <summary>
    /// This is used to rotate the our laser towards the direction of enemy which we are attacking.
    /// </summary>
    public void LaserRotator()
    {
        Vector2 playerFireDirection = enemyShipObject.transform.GetChild(4).position - fireLaser.transform.position;

        fireLaser.GetComponent<RectTransform>().sizeDelta =
            new(170, Vector3.Distance(fireLaser.transform.localPosition, enemyShipObject.transform.localPosition));

        fireLaser.transform.rotation = Quaternion.LookRotation(Vector3.forward, playerFireDirection);
    }


    public void Invinsible(bool value)
    {
        isInvinsible = value;
    }


    public void IncreaseGunDamage()
    {
        damageIncreaseForGuns++;
    }

    public void ShowHideDebugButtons()
    {
        if(debugButtons.activeSelf)
            debugButtons.SetActive(false);
        else
            debugButtons.SetActive(true);
    }

    public void ChangeToMultiplayerScene()
    {
        SceneManager.LoadScene(2);
    }

    public void ChargeJumpbar()
    {
        jumpButton.interactable = true;
    }

    private void OnApplicationQuit()
    {
        data.whichScene = " ";
        data.volumeValue = -1;
    }
}

[Serializable]
public class Enemyshipdata
{
    public ShipData shipData;

    public float hull;
    public float shields;
    public float jumpSpeed;
    public float evasion;
    public int maxGuns;
    public int maxPods;

    public List<Pods> pod;
    public List<Guns> weapon;
    public List<EnemyPilotStats> pilot;
}

[Serializable]
public class Guns
{
    [HideInInspector] public string gunName;
    public float accuracy;
    public float firingRate;
    public float damage;
}

[Serializable]
public class Pods
{
    [HideInInspector] public string podName;
    public float increasedValue;
}

[Serializable]
public class EnemyPilotStats
{
    [HideInInspector] public string statName;
    public float statValue;
}