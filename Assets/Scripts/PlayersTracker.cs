using System;
using UnityEngine;

public class PlayersTracker : MonoBehaviour
{
    public event Action ChildrenChanged;

    /// <summary>
    /// Use the OnTransformChildrenChanged event to trigger the custom event
    /// </summary>
    private void OnTransformChildrenChanged() => ChildrenChanged?.Invoke();
}
