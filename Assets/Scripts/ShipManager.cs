using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipManager : MonoBehaviour
{
    [Header("List")]
    public List<PodData> podData;
    public List<WeaponData> weaponData;
    public List<ShipData> shipData;
    public List<ShipData> allShipsData;
    public List<GameObject> shipSpecsUI;
    public List<GameObject> shipSellSpecs;
    public List<GameObject> shipSellPods;
    public List<GameObject> shipSellWeapons;

    /// <summary>
    /// This is used when we open the Hangar Panel it keeps the ship selected.
    /// </summary>
    public void AutoSelect()
    {
        if(GameManager.Instance.selectedShip == null)
            return;

        GameManager.Instance.highlightedShip = GameManager.Instance.ownedShipSlots[GameManager.Instance.shipSelectedIndex].GetComponent<BuyManager>().slotShipData.shipData;
        GameManager.Instance.ownedShipSlots[GameManager.Instance.shipSelectedIndex].GetComponent<Button>().Select();
    }

    public Sprite noShipSprite;
}