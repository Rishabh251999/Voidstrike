using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Enemy : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    #region Attributes
    public ShipData shipdata;

    [Header("bool")]
    public bool canShowDamagePanel = true;

    [Header("Sprite")]
    [SerializeField] private Sprite rightSprite;
    [SerializeField] private Sprite leftSprite;
    public Sprite frontSprite;

    [Header("Classes")]
    [SerializeField] private BattleManager battleManager;

    [Header("int")]
    public int aggressionFactor;
    public int rewardMyth;
    private int shipfacingIndex;

    [Header("float")]
    public float battleDuration = 0f;
    private float playerDamageTaken;
    public float randomPlayerAttackTime = 0;
    public float permanentMaxHull = 0.0f;
    public bool attackingPlayer = false;
    private readonly float NPC_DAMAGE_TO_PLAYER = 0.55f; //This is the percentage of the total damage that npcs make to players.
    public string enemyFunName;
    #endregion

    private void Awake()
    {
        battleManager = GetComponent<BattleManager>();

        // Create a random number between 0 and 100 to check if the ship will start attacking
        float attackProbability = UnityEngine.Random.Range(0, 100);
        // If the ship's aggression factor is bigger than the random number, it will start attacking
        // If not, it will never attack
        if(aggressionFactor > attackProbability)
            CalculateRandomPlayerAttackTime();
    }


    private void Start()
    {
        shipfacingIndex = GetRandomValue(1, 3);

        if(shipfacingIndex == 1)
            GetComponent<Image>().sprite = leftSprite;

        if (shipfacingIndex == 2)
            GetComponent<Image>().sprite = frontSprite;

        if (shipfacingIndex == 3)
            GetComponent<Image>().sprite = rightSprite;
    }


    private void Update()
    {
        if (randomPlayerAttackTime > 0)
        {
            randomPlayerAttackTime -= Time.deltaTime;

            if (randomPlayerAttackTime < 0)
                StartCoroutine("AttackPlayer");
        }

        if(CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>().playerStats.hull <= 0)
            StopCoroutine("AttackPlayer");
    }


    /// <summary>
    /// This is to calculate the time after which NPC will attack.
    /// </summary>
    public void CalculateRandomPlayerAttackTime()
    {
        // If launch from a planet, there is a 3 second extra delay to wait for the animation to finish
        if(CockPitManager.Instance.cockpitAnimation.transform.GetChild(2).GetComponent<Image>().enabled == true)
            randomPlayerAttackTime = UnityEngine.Random.Range(3, 13);
        else
            randomPlayerAttackTime = UnityEngine.Random.Range(1, 10);
    }

    /// <summary>
    /// This is to disengage the auto attack.
    /// </summary>
    public void DisengageFromPlayer()
    {
        StopCoroutine("AttackPlayer");
    }

    /// <summary>
    /// This is to disengage the auto attack.
    /// </summary>
    public void EngageToPlayer()
    {
        StartCoroutine("AttackPlayer");
    }

    /// <summary>
    /// This is for attacking Player from NPC.
    /// </summary>
    private IEnumerator AttackPlayer()
    {
        attackingPlayer = true;
        float playerTotalFiringRate = CalculateTotalFiringRate(GetComponent<BattleManager>().playerShip);

        float playerBattleDuration = CalculateBattleDuration(playerTotalFiringRate);

        if (!GetComponent<BattleManager>().isEngaged)
        {
            NpcLaserRotator();
            transform.GetChild(3).gameObject.SetActive(true);
            while (CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>().playerStats.hull >= 0)
            {
                if(transform.GetChild(1).GetChild(0).GetComponent<Scrollbar>().size > 0.0f)
                {
                    for (int i = 0; i < GetComponent<BattleManager>().playerShip.guns.Count; i++)
                    {
                        // Added this because sometimes CockPitManager.Instance.shieldBarValue.text = "" and creates an error.
                        float enemyTotalShield = GetComponent<BattleManager>().enemyStats.shields;

                        playerDamageTaken +=
                            NPC_DAMAGE_TO_PLAYER * CockPitManager.Instance.CalculateDamageTakenForPlayer(
                                GetComponent<BattleManager>().playerShip.guns[i].accuracy,
                                GetComponent<BattleManager>().enemyStats.evasion,
                                GetComponent<BattleManager>().playerShip.guns[i].damage,
                                enemyTotalShield, 
                                CockPitManager.Instance.shieldPercentage);
                    }

                    if (playerDamageTaken > 0 && !CockPitManager.Instance.isInvinsible)
                    {
                        CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>().playerStats.hull -= playerDamageTaken;

                        CockPitManager.Instance.SyncPlayerHealthAcrossAllEnemy();

                        // // Save health in LootLocker.
                        // battleManager.SetLootlockerShipHP(CockPitManager.Instance.data.slot, CockPitManager.Instance.playerHP.fillAmount);

                        CockPitManager.Instance.ScrollBarValueCalculator(CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>().playerStats.maxHull,
                                                CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>().playerStats.hull);

                        // Save health in LootLocker.
                        battleManager.SetLootlockerShipHP(CockPitManager.Instance.data.slot, CockPitManager.Instance.playerHP.fillAmount);

                        CockPitManager.Instance.playerHealth.text = MathF.Floor(CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>().playerStats.hull).ToString();

                        // if (canShowDamagePanel)
                        CockPitManager.Instance.DisplayDamage();

                        //For playing Player Taking Damage.
                        AudioManager.Instance.PlayMusic("Getting Damage");
                    }
                    else
                    {
                        //For playing Player Damade Evade Sound.
                        AudioManager.Instance.PlayMusic("Evaded");
                    }

                    playerDamageTaken = 0; // Reset playerDameTaken
                }

                yield return new WaitForSeconds(playerBattleDuration);
            }
        }
    }

    float CalculateTotalFiringRate(Ship ship)
    {
        if (ship.guns.Count == 0)
            return 0;

        float totalFiringRate = 0;
        ship.guns.ForEach(gun => totalFiringRate += gun.firingRate - 50); // 50 is to make the seconds between each shot range wider

        // Calculate the average by dividing the total by the number of guns.
        float averageFiringRate = totalFiringRate / ship.guns.Count;

        return averageFiringRate;
    }


    float CalculateBattleDuration(float totalFiringRate) => battleDuration = 35f / totalFiringRate;


    /// <summary>
    /// Just a function which take a random value which 
    /// </summary>
    private int GetRandomValue(int minValue, int maxValue) => Mathf.FloorToInt(UnityEngine.Random.Range(minValue, maxValue + 1));


    /// <summary>
    /// Called when the UI element is selected. In future check if this can be called in enable UI.
    /// </summary>
    public void OnSelect(BaseEventData eventData)
    {
        EnableUI();

        CockPitManager.Instance.EnableDisableEngageButton(true);

        battleManager.FillEnemyDataForPlayerManager(true);
    }


    /// <summary>
    /// Called when the UI element is deselected.  In future check if this can be called in enable UI.
    /// </summary>
    public void OnDeselect(BaseEventData eventData)
    {
        DisableUI();
        battleManager.FillEnemyDataForPlayerManager(false);
    }

    /// <summary>
    /// Called when Mouse hover over the UI.
    /// </summary>
    public void OnHover()
    {
        Cursor.SetCursor(CockPitManager.Instance.redCursor,
            new Vector2(CockPitManager.Instance.redCursor.width / 2, CockPitManager.Instance.redCursor.height / 2), CursorMode.Auto);
    }

    /// <summary>
    /// Called when Mouse un-hover over the UI.
    /// </summary>
    public void OnUnHover()
    {
        Cursor.SetCursor(CockPitManager.Instance.defaultCursor,
            new Vector2(CockPitManager.Instance.defaultCursor.width / 2, CockPitManager.Instance.defaultCursor.height / 2), CursorMode.Auto);
    }


    /// <summary>
    /// Called in event trigger functions when we select a ship.
    /// </summary>
    public void EnableUI()
    {
        MusicPlayerManager.Instance.PlayMusic("Button Click");
        CockPitManager.Instance.enemyShipObject = transform.gameObject;

        if (CockPitManager.Instance.enemyShipObject.GetComponent<BattleManager>().isEngaged)
        {
            CockPitManager.Instance.engageButton.SetActive(false);
            CockPitManager.Instance.disengageButton.SetActive(true);
        }

        else if (!CockPitManager.Instance.enemyShipObject.GetComponent<BattleManager>().isEngaged)
        {
            CockPitManager.Instance.engageButton.SetActive(true);
            CockPitManager.Instance.disengageButton.SetActive(false);
        }

        if (CockPitManager.Instance.selectedEnemyShip != null)
            CockPitManager.Instance.selectedEnemyShip.transform.GetChild(0).gameObject.SetActive(false);

        transform.GetComponent<Image>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);
        transform.GetChild(4).gameObject.SetActive(true);
        transform.GetChild(4).GetComponent<Image>().preserveAspect = true;

        if (battleManager.isEngaged)
        {
            transform.GetChild(4).gameObject.SetActive(false);
            transform.GetComponent<Image>().enabled = true;
        }

        else if (battleManager.isEngaged || battleManager.alreadyEngaged)
            transform.GetChild(4).GetComponent<Animator>().Play("Scan " + 2, 0);

        else
            transform.GetChild(4).GetComponent<Animator>().Play("Scan " + shipfacingIndex, 0);

        CockPitManager.Instance.enemyShipImage.color = new(1, 1, 1, 1);
        CockPitManager.Instance.enemyShipImage.sprite = shipdata.shipAvatar;
        CockPitManager.Instance.enemyShipImage.preserveAspect = true;
        CockPitManager.Instance.enemyShipName.text = enemyFunName;
        CockPitManager.Instance.enemyShipClassName.text = "Class " + shipdata.shipClass + " " + shipdata.shipType;
        CockPitManager.Instance.enemyHealthBarFill.transform.parent.gameObject.SetActive(true);
        CockPitManager.Instance.enemyHealthBarFill.size = transform.GetChild(1).GetChild(0).GetComponent<Scrollbar>().size;
    }


    /// <summary>
    /// Called in event trigger functions when we deselect a ship.
    /// </summary>
    public void DisableUI()
    {
        transform.GetComponent<Image>().enabled = true;
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
        transform.GetChild(4).GetComponent<Animator>().Play("Idle", 0);
        transform.GetChild(4).gameObject.SetActive(false);


        CockPitManager.Instance.enemyShipImage.color = new(1, 1, 1, 0);
        CockPitManager.Instance.enemyShipImage.sprite = null;
        CockPitManager.Instance.enemyShipName.text = " ";
        CockPitManager.Instance.enemyShipClassName.text = " ";
        CockPitManager.Instance.enemyHealthBarFill.transform.parent.gameObject.SetActive(false);
    }


    /// <summary>
    /// This is used to rotate the our laser towards the direction of Plahyer which we are attacking.
    /// </summary>
    public void NpcLaserRotator()
    {
        Vector2 NpcFireDirection = transform.GetChild(3).position - CockPitManager.Instance.enemyFireReceiver.transform.position;

        transform.GetChild(3).GetComponent<RectTransform>().sizeDelta =
            new(transform.GetComponent<RectTransform>().sizeDelta.y,
                Vector3.Distance(transform.GetChild(3).position, CockPitManager.Instance.enemyFireReceiver.transform.position));

        transform.GetChild(3).rotation = Quaternion.LookRotation(Vector3.forward, NpcFireDirection);
    }
}