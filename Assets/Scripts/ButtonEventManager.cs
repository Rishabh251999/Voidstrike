using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonEventManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Textures")]
    [SerializeField] private Texture2D onEnterTexture;
    [SerializeField] private Texture2D onExitTexture;

    public enum GameMode
    {
        SinglePlayer,
        Multiplayer
    }

    public GameMode currentGameMode;

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Check if the button is interactable before executing the code
        if (GetComponent<Button>().interactable)
        {
            if (currentGameMode == GameMode.SinglePlayer)
            {
                CockPitManager.Instance.CrossHairChanger(onEnterTexture);
            }
            else if (currentGameMode == GameMode.Multiplayer)
            {
                CockPitMultiplayerManager.Instance.CrossHairChanger(onEnterTexture);
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Check if the button is interactable before executing the code
        if (GetComponent<Button>().interactable)
        {
            if (currentGameMode == GameMode.SinglePlayer)
            {
                CockPitManager.Instance.CrossHairChanger(onExitTexture);
            }
            else if (currentGameMode == GameMode.Multiplayer)
            {
                CockPitMultiplayerManager.Instance.CrossHairChanger(onExitTexture);
            }
        }
    }
}
