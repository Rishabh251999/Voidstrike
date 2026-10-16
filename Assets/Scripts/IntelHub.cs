using UnityEngine;
using LootLocker.Requests;

public class IntelHub : MonoBehaviour
{
    public Leaderboard leaderboard;

    private void OnEnable()
    {
        leaderboard.GetLeaderboardKills();
        leaderboard.GetLeaderboardCurrency();
        leaderboard.GetLeaderboardPvpKills();
    }
}