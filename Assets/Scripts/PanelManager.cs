using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    private PilotManager pilotManager;
    private ShipManager shipManager;

    private void Awake()
    {
        pilotManager = FindAnyObjectByType<PilotManager>();
        shipManager = FindAnyObjectByType<ShipManager>();
    }

    /// <summary>
    /// This is used to change the sprite state of panel buttons.
    /// </summary>
    /// <param name="panelNumber"></param>
    public void ChangeState(int panelNumber)
    {
        switch(panelNumber)
        {
            case 0:
                pilotManager.AutoSelect();
                break;
            case 1:
                shipManager.AutoSelect();
                break;
        }
        GameManager.Instance.DisablePanels(panelNumber);
        GameManager.Instance.ResetSlotSelectedForBuyAndMod();
    }
}