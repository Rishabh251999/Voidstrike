using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CockPitMultiplayerManager : MonoBehaviour
{
    [Header("Singleton")]
    public static CockPitMultiplayerManager Instance;

    [Header("GameObjects")]
    public GameObject blueLaser;
    public GameObject redLaser;
    public GameObject engageButton;
    public GameObject disEngageButton;
    public GameObject enemyShipsParent;
    public GameObject connectionLoading;
    public GameObject playerDiedPanel;
    public GameObject damagePanel;
    public GameObject mapView;
    public GameObject chatPanel;
    public GameObject chatScrollBar;
    public GameObject chatScroller;
    public GameObject controlPanel;
    public GameObject enemyShipFrame;
    public GameObject musicPlayer;

    [Header("Sprites")]
    public Sprite defaultChatAttacked;
    public Sprite universalChatAttacked;
    public Sprite chatDefault;
    public Sprite chatUniversal;
    public Sprite chatScrollAttacked;
    public Sprite chatScrollDefault;
    public Sprite musicBarDefault;
    public Sprite musicBarAttacked;

    [Header("Transform")]
    public Transform redLaserReceiver;

    [Header("Classes")]
    // [HideInInspector]
    public BattleMultiplayerManager localPlayer;
    [SerializeField] private PlayersTracker playerTracker;

    [Header("List")]
    [SerializeField] private List<GameObject> shieldBar;
    [SerializeField] private List<Sprite> shield;
    [SerializeField] private List<Sprite> shieldDrag;
    public List<RuntimeAnimatorController> shipAnimatorController;
    public List<int> availableNumbers;
    public List<GameObject> enemyShipSpawnPoints;

    [Header("Button")]
    public Button jumpButton;

    [Header("ScrollBar")]
    [SerializeField] private Scrollbar shieldBarSlider;
    public Scrollbar engagedWithShipHealthBar;

    [Header("Image")]
    public Image engagedWithShipImage;
    public Image playerHP;
    public Image jumpBar;
    public Image playerShipImage;

    [Header("Text")]
    public TextMeshProUGUI engagedWithShipName;
    public TextMeshProUGUI engagedWithShipClassName;
    public TextMeshProUGUI playerHealth;
    public TextMeshProUGUI shieldBarValue;
    public TextMeshProUGUI playerName;

    [Header("float")]
    private float k;
    private float newFillTime;
    private float currentTime = 0f;
    private float previousNormalizedTime = 0f;
    public float shieldPercentage = 0.0f;

    [Header("Texture2D")]
    public Texture2D blueCrosshair;
    public Texture2D redCursor;
    public Data data;
    public int processesToFinish = 0;
    public VisitedPlanetsManager visitedPlanetsManager;

    [Header("Chat")]
    [SerializeField] private GameObject chatUI;
    [SerializeField] private GameObject chatContent;
    [SerializeField] private GameObject chatLog;
    [SerializeField] private Scrollbar scrollbarVertical;

    [Header("Bool")]
    public bool useAllDataTest;
    public bool ifAmIDead;
    public bool canScrollChatMessages;
    Color redColor = Color.red;
    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);

        else
            Instance = this;
    }


    private void Start()
    {
        Leaderboard.Instance.SetLeaderboardLastLogin();
        
        int nodesDataIndex = data.discoveredNodes.FindIndex(nodeData => nodeData == "A13");
        if (nodesDataIndex == -1)
        {
            data.discoveredNodes.Add("A13");
            visitedPlanetsManager.UpdateVisitedPlanets("A13");
        }

        Cursor.SetCursor(blueCrosshair, new Vector2(10, 4), CursorMode.Auto);
        Cursor.lockState = CursorLockMode.Confined;

        InitializeAvailableNumbers();
        CalculateConstant();

        playerTracker.ChildrenChanged += CheckForOtherPlayersBattle;
    }


    private void OnDestroy() => playerTracker.ChildrenChanged -= CheckForOtherPlayersBattle;


    private void Update()
    {
        if (currentTime < newFillTime)
        {
            float normalizedTime = previousNormalizedTime + (Time.deltaTime / newFillTime);

            jumpBar.fillAmount = normalizedTime;

            currentTime = Time.deltaTime;

            previousNormalizedTime = normalizedTime;
        }

        if (jumpBar.fillAmount == 1)
            jumpButton.interactable = true;
        else
            jumpButton.interactable = false;

        if(!canScrollChatMessages)
            scrollbarVertical.value = 0;
    }

    /// <summary>
    /// This will allow ius to scroll back the messages.
    /// </summary>
    public void CanScrollChatMessages(bool value) => canScrollChatMessages = value;

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
                    localPlayer.GetComponent<BattleMultiplayerManager>().shieldSliderValue = 0f;
                    shieldBarValue.text = Math.Round(CalculateShield(0), MidpointRounding.AwayFromZero).ToString();
                    localPlayer.CmdSyncShieldDeatils(0, CalculateShield(0));
                }

                else if (i == 1)
                {
                    newFillTime = CalculateTimeToFill(CalculateJumpSpeed(25));
                    localPlayer.GetComponent<BattleMultiplayerManager>().shieldSliderValue = 0.25f;
                    shieldBarValue.text = Math.Round(CalculateShield(25), MidpointRounding.AwayFromZero).ToString();
                    localPlayer.CmdSyncShieldDeatils(0.25f, CalculateShield(25));
                }

                else if (i == 2)
                {
                    newFillTime = CalculateTimeToFill(CalculateJumpSpeed(50));
                    localPlayer.GetComponent<BattleMultiplayerManager>().shieldSliderValue = 0.50f;
                    shieldBarValue.text = Math.Round(CalculateShield(50), MidpointRounding.AwayFromZero).ToString();
                    localPlayer.CmdSyncShieldDeatils(0.50f, CalculateShield(50));
                }

                else if (i == 3)
                {
                    newFillTime = CalculateTimeToFill(CalculateJumpSpeed(75));
                    localPlayer.GetComponent<BattleMultiplayerManager>().shieldSliderValue = 0.75f;
                    shieldBarValue.text = Math.Round(CalculateShield(75), MidpointRounding.AwayFromZero).ToString();
                    localPlayer.CmdSyncShieldDeatils(0.75f, CalculateShield(75));
                }

                else if (i == 4)
                {
                    newFillTime = CalculateTimeToFill(CalculateJumpSpeed(99));
                    localPlayer.GetComponent<BattleMultiplayerManager>().shieldSliderValue = 1.0f;
                    shieldBarValue.text = Math.Round(CalculateShield(100), MidpointRounding.AwayFromZero).ToString();
                    localPlayer.CmdSyncShieldDeatils(1.0f, CalculateShield(100));
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
    /// This refreshes the jump speed depending on what is the shield value which gets changed by the slider.
    /// </summary>
    public float CalculateJumpSpeed(float shieldPercentage) =>
    localPlayer.GetComponent<BattleMultiplayerManager>().jumpSpeed * (1f - (shieldPercentage / 100));


    /// <summary>
    /// Depeding on where your slider is, it updates the shield value.
    /// </summary>
    public float CalculateShield(float shieldPercentage) => localPlayer.GetComponent<BattleMultiplayerManager>().shields * (shieldPercentage / 100);


    /// <summary>
    /// This is to calculate the constant at which the the JumpSpeed bar will change.
    /// </summary>
    private void CalculateConstant()
    {
        float maxJumpSpeed = 26f;
        float timeToFill = 9f;    

        k = maxJumpSpeed * timeToFill;
    }


    /// <summary>
    /// This is to calculate the time to fill the JumpSpeed Bar.
    /// </summary>
    public float CalculateTimeToFill(float jumpSpeed)
    {
        float timeToFill = k / jumpSpeed;
        return Mathf.Max(timeToFill, 0f);
    }


    /// <summary>
    /// This sets us the local player for each client in the cockpitMultiplayerManager script.
    /// </summary>
    public void SetLocalPlayer(BattleMultiplayerManager player)
    {
        localPlayer = player;

        StartCoroutine(nameof(StartFillingJumpBar));
    }


    /// <summary>
    /// This function is called on the engage button click.
    /// </summary>
    public void EngageButtonClicked()
    {
        if (localPlayer != null)
        {
            localPlayer.StopBattle();
            localPlayer.OnDisEngageButtonClicked();

            StartCoroutine(IEEnableDisableEngageDisEngageButton(1));

            StartLaserRotator("Firing");
        }
    }


    /// <summary>
    /// This function is called on the event trigger of the engaged button.
    /// </summary>
    public void DisEngageButtonClicked()
    {
        if (localPlayer != null)
        {
            localPlayer.StopBattle();
            localPlayer.OnDisEngageButtonClicked();

            StartCoroutine(IEEnableDisableEngageDisEngageButton(0));
        }
    }


    /// <summary>
    /// This handles everything related to the lasers when battling or receiving damage.
    /// </summary>
    public void StartLaserRotator(string direction) => StartCoroutine(LaserRotator(direction));


    /// <summary>
    /// This is used to rotate the our laser towards the direction of enemy which we are attacking.
    /// </summary>
    private IEnumerator LaserRotator(string direction)
    {
        if (direction == "Firing")
        {
            localPlayer.OnEngageButtonClicked();
            localPlayer.StartBattle();

            // Wait until valueToCheck is not null
            yield return new WaitUntil(() => localPlayer.engagedWith != null);

            blueLaser.SetActive(true);

            Vector2 playerFireDirection = localPlayer.engagedWith.transform.GetChild(2).position - blueLaser.transform.position;

            blueLaser.GetComponent<RectTransform>().sizeDelta =
                new(170, Vector3.Distance(blueLaser.transform.localPosition, localPlayer.engagedWith.transform.localPosition));

            blueLaser.transform.rotation = Quaternion.LookRotation(Vector3.forward, playerFireDirection);
        }

        else if (direction == "Receiving")
        {
            yield return new WaitForSeconds(0.1f);

            for (int j = 0; j < localPlayer.allPlayers.Count; j++)
            {
                if (localPlayer.allPlayers[j].engagedWith != null)
                {
                    if (localPlayer.netId == localPlayer.allPlayers[j].engagedWith.netId)
                    {
                        if(localPlayer.allPlayers[j].transform.childCount <= 5)
                        {
                            GameObject redLaserPrefab = Instantiate(redLaser, localPlayer.allPlayers[j].transform.position, Quaternion.identity);

                            redLaserPrefab.transform.SetParent(localPlayer.allPlayers[j].transform);

                            Vector2 enemyFireDirection = localPlayer.allPlayers[j].transform.GetChild(2).position - redLaserReceiver.transform.position;

                            redLaserPrefab.GetComponent<RectTransform>().sizeDelta =
                                new(170, Vector3.Distance(redLaserReceiver.transform.localPosition,
                                localPlayer.allPlayers[j].transform.localPosition));

                            redLaserPrefab.transform.rotation = Quaternion.LookRotation(Vector3.forward,
                                enemyFireDirection);
                        }
                    }
                }
            }
        }

        else if (direction == "Disable")
        {
            yield return new WaitForSeconds(0.01f);

            for (int j = 0; j < localPlayer.allPlayers.Count; j++)
            {
                if (localPlayer.allPlayers[j].engagedWith != null)
                {
                    if (localPlayer.netId == localPlayer.allPlayers[j].engagedWith.netId && localPlayer.allPlayers[j].battleMode == false)
                    {
                        if(localPlayer.allPlayers[j].transform.GetChild(5)!=null)
                        {
                            Destroy(localPlayer.allPlayers[j].transform.GetChild(5).gameObject);
                        }
                    }
                }
            }
        }
    }

        
    /// <summary>
    /// When the game starts, this lets the jump bar to start filling up. 
    /// </summary>
    private IEnumerator StartFillingJumpBar() 
    {
        yield return new WaitForSeconds(1f);

        localPlayer.JumpBarStart();
    }


    /// <summary>
    /// This will start checking who are in battle with each other.
    /// </summary>
    public void CheckForOtherPlayersBattle() => StartCoroutine(nameof(IECheckForOtherPlayersBattle));


    /// <summary>
    /// Just the coroutine that does the players battling checks
    /// </summary>
    private IEnumerator IECheckForOtherPlayersBattle()
    {
        yield return new WaitForSeconds(0.1f);

        localPlayer?.GetAllPlayers();
    }


    /// <summary>
    /// when engaging or disengaging, this changes which button needed to be enabled or disabled.
    /// </summary>
    private IEnumerator IEEnableDisableEngageDisEngageButton(int index)
    {
        yield return new WaitForSeconds(0.5f);

        if(index == 0)
        {
            engageButton.SetActive(true);
            disEngageButton.SetActive(false);
        }
        else if (index == 1)
        {
            engageButton.SetActive(false);
            disEngageButton.SetActive(true);
        }
    }


    /// <summary>
    /// Initializes the list so that we can keep track of which variables we have to remove.
    /// </summary>
    private void InitializeAvailableNumbers()
    {
        availableNumbers = new List<int>();

        for (int i = 1; i < enemyShipSpawnPoints.Count; i++)
            availableNumbers.Add(i);
    }


    /// <summary>
    /// Generate a random number and then remove it from it list so that it doesn't comes again.
    /// </summary>
    public int GenerateRandomNumber()
    {
        if (availableNumbers.Count == 0)
        {
            // Handle the error, e.g., by returning a special value or throwing an exception
            throw new InvalidOperationException("No more available numbers");
        }

        int index = UnityEngine.Random.Range(0, availableNumbers.Count);
        int randomNumber = availableNumbers[index];
        availableNumbers.RemoveAt(index);

        return randomNumber;
    }

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

        damagePanel.SetActive(true);
        chatPanel.transform.GetChild(0).GetComponent<Image>().sprite = defaultChatAttacked;
        chatPanel.transform.GetChild(1).GetComponent<Image>().sprite = universalChatAttacked;
        chatScrollBar.GetComponent<Image>().sprite = chatScrollAttacked;
        musicPlayer.GetComponent<Image>().sprite = musicBarAttacked;
        chatScroller.GetComponent<Image>().color = redColor;
        controlPanel.GetComponent<Image>().color = redColor;
        enemyShipFrame.GetComponent<Image>().color = redColor;

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
        damagePanel.SetActive(false);
        chatPanel.transform.GetChild(0).GetComponent<Image>().sprite = chatDefault;
        chatPanel.transform.GetChild(1).GetComponent<Image>().sprite = chatUniversal;
        chatScrollBar.GetComponent<Image>().sprite = chatScrollDefault;
        musicPlayer.GetComponent<Image>().sprite = musicBarDefault;
        Color transparent = new Color(1f, 1f, 1f, 1f);
        chatScroller.GetComponent<Image>().color = transparent;
        controlPanel.GetComponent<Image>().color = transparent;
        enemyShipFrame.GetComponent<Image>().color = transparent;
    }

    // /// <summary>
    // /// This is to display the damage panel when a player get hits.
    // /// </summary>
    // public void DisplayDamage() => StartCoroutine(IEDisplayDamage());


    // /// <summary>
    // /// This is to IEnumerator to display the damage panel when a player get hits.
    // /// </summary>
    // private IEnumerator IEDisplayDamage()
    // {
    //     damagePanel.SetActive(true);
    //     chatPanel.transform.GetChild(0).GetComponent<Image>().sprite = defaultChatAttacked;
    //     chatPanel.transform.GetChild(1).GetComponent<Image>().sprite = universalChatAttacked;
    //     chatScrollBar.GetComponent<Image>().sprite = chatScrollAttacked;
    //     musicPlayer.GetComponent<Image>().sprite = musicBarAttacked;
    //     Color newColor = Color.red;
    //     chatScroller.GetComponent<Image>().color = newColor;
    //     controlPanel.GetComponent<Image>().color = newColor;
    //     enemyShipFrame.GetComponent<Image>().color = newColor;

    //     yield return new WaitForSeconds(0.5f);

    //     damagePanel.SetActive(false);
    //     chatPanel.transform.GetChild(0).GetComponent<Image>().sprite = chatDefault;
    //     chatPanel.transform.GetChild(1).GetComponent<Image>().sprite = chatUniversal;
    //     chatScrollBar.GetComponent<Image>().sprite = chatScrollDefault;
    //     musicPlayer.GetComponent<Image>().sprite = musicBarDefault;
    //     Color transparent = new Color(1f, 1f, 1f, 1f);
    //     chatScroller.GetComponent<Image>().color = transparent;
    //     controlPanel.GetComponent<Image>().color = transparent;
    //     enemyShipFrame.GetComponent<Image>().color = transparent;
    // }


    /// <summary>
    /// Return to Home Planet.
    /// </summary>
    public void HomePlanet()
    {
        if(processesToFinish >= 3 || playerHP.fillAmount > 0)
        {
            localPlayer.Disconnect();

            data.volumeValue = MusicPlayerManager.Instance.volumeController.value;
            // data.shipHealth = playerHP.fillAmount;
            data.previousScene = "GamePlay";

            SceneManager.LoadScene(0);
        }
    }


    /// <summary>
    /// To change the crosshair on the basis of UI.
    /// </summary>
    public void CrossHairChanger(Texture2D crosshair)
    {
        if (crosshair.name.Equals("Blue") || crosshair.name.Equals("Red"))
            Cursor.SetCursor(crosshair, new Vector2(crosshair.width / 2, crosshair.height / 2), CursorMode.Auto);

        else if (crosshair.name.Equals("Hand"))
            Cursor.SetCursor(crosshair, new Vector2(20, 4), CursorMode.Auto);

        else
            Cursor.SetCursor(crosshair, new Vector2(10, 4), CursorMode.Auto);
    }

    /// <summary>
    /// Changes to MapView
    /// </summary>
    public void MapView()
    {
        MapViewManager.Instance.RenderMapView();
        mapView.SetActive(true);
    }

    public void KilledEnemyChat(string name, float reward, string shipClass)
    {
        GameObject chat = Instantiate(chatLog);

        string pattern = @"^(.*?)_NetID";
        Match match = Regex.Match(name, pattern);

        chat.GetComponent<TextMeshProUGUI>().text = "Chippy: You killed " + match.Groups[1].Value +
        ", a " + shipClass + ", and received " + reward + " M! Your total is now: " + data.currency.ToString("#,#",CultureInfo.InvariantCulture) + " M";
        chat.transform.SetParent(chatContent.transform, false);
    }

    /// <summary>
    /// This is to disconnect the player when player goes to another node from Multiplayer.
    /// </summary>
    public void DisconnectGame() => localPlayer.Disconnect();
}