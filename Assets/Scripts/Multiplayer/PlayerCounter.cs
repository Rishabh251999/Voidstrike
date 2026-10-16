using UnityEngine;
using TMPro;
using Mirror;

public class PlayerCounter : MonoBehaviour
{
    public TMP_Text playeCount;

    public GameObject[] playersArray;

    void Update()
    {
        FindPlayersByTag();
    }

    public void FindPlayersByTag()
    {
        playersArray = GameObject.FindGameObjectsWithTag("PlayerController");
        playeCount.text = string.Format("Players Online: {0}", playersArray.Length.ToString());
    }
}