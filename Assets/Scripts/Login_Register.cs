using LootLocker.Requests;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(TimeStampAPI))]
public class Login_Register : MonoBehaviour
{
    [Header("InputField")]
    [SerializeField] private TMP_InputField newUserEmailInputField;
    [SerializeField] private TMP_InputField newUserPasswordInputField;
    [SerializeField] private TMP_InputField existingUserEmailInputField;
    [SerializeField] private TMP_InputField existingUserPasswordInputField;
    [SerializeField] private TMP_InputField forgotpasswordEmail;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("Canvas")]
    [SerializeField] private Canvas currentOnePanel;
    [SerializeField] private Canvas afterLoginPanel;

    [Header("GameObject")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Strings")]
    [SerializeField] private string firstTime;
    private readonly string key = "FirstTimeLogin";
    [Header("Images")]
    [SerializeField] private Image passwordVisibilityImageLogIn;
    [SerializeField] private Image passwordVisibilityImageSignUp;

    [Header("Sprites")]
    [SerializeField] private Sprite passwordVisible;
    [SerializeField] private Sprite passwordHide;
    [SerializeField] private Sprite defaultLoginButton;
    [SerializeField] private Sprite pressedLoginButton;

    [Header("Buttons")]
    [SerializeField] private Button loginButton;
    [SerializeField] private Button signUpButton;

    [Header("Others")]
    public Leaderboard leaderboard;
    public VisitedPlanetsManager visitedPlanetsManager;
    public PlayerSessionManager playerSessionManager;
    

    private int initialCurrency = 20000;

    [Header("SO")]
    public Data data;
    // private WhitelistAPI whitelisteAPI;
    private TimeStampAPI timeAPI;
    // private GoogleSheetsAPIConnector googleSheetsAPIConnector;

    private void Awake()
    {
        existingUserPasswordInputField.contentType = TMP_InputField.ContentType.Password;
        newUserPasswordInputField.contentType = TMP_InputField.ContentType.Password;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (EventSystem.current.currentSelectedGameObject == newUserEmailInputField.gameObject)
            {
                SelectNextInputField(newUserPasswordInputField);
            }
            else if (EventSystem.current.currentSelectedGameObject == newUserPasswordInputField.gameObject)
            {
                SelectNextInputField(newUserEmailInputField);
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (EventSystem.current.currentSelectedGameObject == existingUserEmailInputField.gameObject)
            {
                SelectNextInputField(existingUserPasswordInputField);
            }
            else if (EventSystem.current.currentSelectedGameObject == existingUserPasswordInputField.gameObject)
            {
                SelectNextInputField(existingUserEmailInputField);
            }
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (EventSystem.current.currentSelectedGameObject == existingUserEmailInputField.gameObject || EventSystem.current.currentSelectedGameObject == existingUserPasswordInputField.gameObject)
            {
                Login();
                loginButton.GetComponent<Image>().sprite = pressedLoginButton;
            }

            if (EventSystem.current.currentSelectedGameObject == newUserEmailInputField.gameObject ||
            EventSystem.current.currentSelectedGameObject == newUserPasswordInputField.gameObject)
            {
                SignUp();
                signUpButton.GetComponent<Image>().sprite = pressedLoginButton;
            }
            
        }
    }

    private void SelectNextInputField(TMP_InputField nextInputField)
    {
        if (nextInputField != null)
        {
            nextInputField.Select();
            nextInputField.ActivateInputField();
        }
    }

    private void Start()
    {
        StartProcess();
    }

    public void StartProcess()
    {
        if (data.previousScene == "GamePlay")
        {
            // Sets MainMenu background, selected ship and selected pilot
            playerSessionManager.SetMainMenu();
            mainMenuPanel.gameObject.SetActive(true);
            playerSessionManager.StartPlayerSession(data.playerId, () => {
                EnableMainMenu();
            });
        }
        else
        {
            timeAPI = GetComponent<TimeStampAPI>();
            // googleSheetsAPIConnector = GetComponent<GoogleSheetsAPIConnector>();
            // Remember to use the LootLocker namespace in the top of your file.
            LootLockerSDKManager.CheckWhiteLabelSession(response =>
            {
                if (response)
                {
                    infoText.text = "Session found";
                    // Start a new session
                    LootLockerSDKManager.StartWhiteLabelSession(resp =>
                    {
                        if (resp.success)
                        {
                            StartCoroutine(CheckAlreadyLogged(resp.player_id.ToString(), (alreadyLoggedin) =>
                            {
                                if (alreadyLoggedin)
                                {
                                    infoText.text = "Wait up to 60 seconds before logging in to allow the server to reset your other session.";
                                }
                                else
                                {
                                    StartCoroutine(CheckFirstTimeLogin(() =>
                                    {
                                        // Code here will run after the CheckFirstTimeLogin coroutine completes.
                                        infoText.text = "Loading...";
                                        // Get and Set LootLocker init player's data
                                        playerSessionManager.StartPlayerSession(resp.player_id.ToString(), () =>
                                        {
                                            EnableMainMenu();
                                        });
                                    }));
                                }
                            }));
                        }
                        else
                        {
                            Debug.Log("Error starting session");
                        }
                    });

                }
                else
                {
                    // Show login form here
                    Debug.Log("session is NOT valid, we should show the login form");
                    loginPanel.SetActive(true);
                }
            });
        }
    }

    public void Login()
    {
        string email = existingUserEmailInputField.text;
        string password = existingUserPasswordInputField.text;

        LootLockerSDKManager.WhiteLabelLoginAndStartSession(email, password, true, loginResponse =>
        {
            if (!loginResponse.success)
            {
                // Login error
                infoText.color = Color.red;
                infoText.text = "Username or password is wrong!";
            }
            else
            {
                // Login successful
                infoText.color = Color.white;
                infoText.text = "Player was logged in successfully.";

                StartCoroutine(CheckAlreadyLogged(loginResponse.SessionResponse.player_id.ToString(), (alreadyLoggedin) => {
                    if (alreadyLoggedin)
                    {
                        // Player is already logged in elsewhere
                        infoText.text = "Wait up to 60 seconds before logging in to allow the server to reset your other session.";
                    }
                    else
                    {
                        // New login
                        StartCoroutine(CheckFirstTimeLogin(() =>
                        {
                            // Successful first-time login actions
                            infoText.text = "Loading...";
                            playerSessionManager.StartPlayerSession(loginResponse.SessionResponse.player_id.ToString(), () => {
                                EnableMainMenu();
                            });

                        }));
                    }
                }));
            }
            loginButton.GetComponent<Image>().sprite = defaultLoginButton;
        });
    }

    private IEnumerator CheckAlreadyLogged(string playerID, Action<bool> callback)
    {
        bool hasFinishedAsyncOperation = false;
        bool alreadyLoggedin = false;
        DateTime currentTime = DateTime.MinValue;

        if (timeAPI != null)
        {
            timeAPI.FetchTimestamp((apiTime) => {
                currentTime = apiTime;
                hasFinishedAsyncOperation = true;
                Debug.Log("CheckAlreadyLogged currentTime: " + currentTime);
            });
        }
        else
        {
            // Handle the null case, maybe set a default time or abort the operation.
        }

        // Wait until the timestamp is fetched
        yield return new WaitUntil(() => hasFinishedAsyncOperation);

        // Reset for the next async operation
        hasFinishedAsyncOperation = false;

        // Now check the last login time
        leaderboard.GetPlayerLastLogin(playerID, (lastLoginScore) => {
            
            DateTime timestamp = DateTimeOffset.FromUnixTimeSeconds(lastLoginScore).UtcDateTime;

            TimeSpan timeDifference = currentTime - timestamp;

            if (timeDifference.TotalMinutes < 1)
            {
                alreadyLoggedin = true;
            }
            else
            {
                playerSessionManager.UpdateLoginTimestamp();
            }

            hasFinishedAsyncOperation = true;
        });

        // Wait until the leaderboard check is done
        yield return new WaitUntil(() => hasFinishedAsyncOperation);

        callback.Invoke(alreadyLoggedin);
    }
    
    private IEnumerator CheckFirstTimeLogin(Action callback)
    {
        bool hasFinishedAsyncOperation = false;

        LootLockerSDKManager.GetSingleKeyPersistentStorage(key, (response) =>
                {
                    if (response.success)
                    {
                        if (response.payload != null)
                        {
                            if(response.payload.value == "0")
                            {
                                welcomePanel.SetActive(true);
                                // Set initial currency to its value
                                GameManager.Instance.currency = initialCurrency;
                            }
                            hasFinishedAsyncOperation = true;
                        }
                        else
                        {
                            LootLockerSDKManager.UpdateOrCreateKeyValue(key, "0", true, (getPersistentStoragResponse) =>
                            {
                                if (getPersistentStoragResponse.success)
                                {
                                    Debug.Log("Successfully created first login key");
                                    welcomePanel.SetActive(true);
                                    // Set the initial amount of Player's Myth (Currency)
                                    leaderboard.SetLeaderboardCurrency(initialCurrency);
                                    // Set initial currency to its value
                                    GameManager.Instance.currency = initialCurrency;
                                    // Create game files in LootLocker
                                    playerSessionManager.CreateInitialPlayerFiles(() => {
                                        hasFinishedAsyncOperation = true;
                                    });
                                }
                            });
                        }
                    }
                    else
                    {
                        Debug.Log("Error getting player storage");
                    }
                });
                yield return new WaitUntil(() => hasFinishedAsyncOperation);
                // Notify that CheckFirstTimeLogin is complete
                callback.Invoke();
    }
    private void EnableMainMenu()
    {
        currentOnePanel.gameObject.SetActive(false);
        afterLoginPanel.gameObject.SetActive(true);
    }

    public void SignUp()
    {
        string email = newUserEmailInputField.text;
        string password = newUserPasswordInputField.text;

        existingUserEmailInputField.text = email;
        existingUserPasswordInputField.text = password;

        LootLockerSDKManager.WhiteLabelSignUp(email, password, (response) =>
        {
            if (!response.success)
            {
                infoText.color = Color.red;
                infoText.text = "Error signing up!";
            }
            else
            {
                infoText.color = Color.white;
                infoText.text = "Verification link sent. Check Email.";
            }
        });
    }

    public void SignOut()
    {
        LootLockerSDKManager.EndSession((response) =>
        {
            data.previousScene = "Scene1";
            if(response.success)
            {
                print("Signed Out");
                data.shipPlans.Clear();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            else
            {
                print("Error Signing Out");
            }
        });
    }
    public void ResetPassword()
    {
        string email = forgotpasswordEmail.text;
        
        LootLockerSDKManager.WhiteLabelRequestPassword(email, (response) =>
        {
            if (response.success)
            {
                infoText.color = Color.white;
                infoText.text = "Password reset link sent.";
            }
            else
            {
                infoText.color = Color.red;
                infoText.text = "Error sending password-reset-link!";
            }
        });
    }

    public void togglePasswordVisibility()
    {
        if (existingUserPasswordInputField.contentType == TMP_InputField.ContentType.Password)
        {
            existingUserPasswordInputField.contentType = TMP_InputField.ContentType.Standard;
            newUserPasswordInputField.contentType = TMP_InputField.ContentType.Standard;
            passwordVisibilityImageLogIn.sprite = passwordVisible;
            passwordVisibilityImageSignUp.sprite = passwordVisible;
        } else {
            existingUserPasswordInputField.contentType = TMP_InputField.ContentType.Password;
            newUserPasswordInputField.contentType = TMP_InputField.ContentType.Password;
            passwordVisibilityImageLogIn.sprite = passwordHide;
            passwordVisibilityImageSignUp.sprite = passwordHide;
        }

        existingUserPasswordInputField.ForceLabelUpdate();
        newUserPasswordInputField.ForceLabelUpdate();
    }

    public void SetCursor()
    {
        // Set the cursor in the input field
        newUserEmailInputField.Select();
        newUserEmailInputField.ActivateInputField();
    }

}