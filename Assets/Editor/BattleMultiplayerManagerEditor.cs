#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BattleMultiplayerManager))]
public class BattleMultiplayerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BattleMultiplayerManager myTarget = (BattleMultiplayerManager)target;

        // Displaying the onlinePlayerGuns
        if (myTarget.onlinePlayerGuns != null)
        {
            EditorGUILayout.LabelField("Online Player Guns:", EditorStyles.boldLabel);
            foreach (var gun in myTarget.onlinePlayerGuns)
            {
                EditorGUILayout.LabelField("Gun Name:", gun.gunName);
                EditorGUILayout.FloatField("Accuracy:", gun.accuracy);
                EditorGUILayout.FloatField("Firing Rate:", gun.firingRate);
                EditorGUILayout.FloatField("Damage:", gun.damage);
                EditorGUILayout.Space(); // Adds a little space between items for clarity
            }
        }

        // Displaying the attackingPlayer SyncList
        EditorGUILayout.LabelField("Attacking Players:", EditorStyles.boldLabel);
        if (myTarget.attackingPlayer != null)
        {
            for (int i = 0; i < myTarget.attackingPlayer.Count; i++)
            {
                // Assuming BattleMultiplayerManager is a MonoBehaviour attached to a GameObject
                myTarget.attackingPlayer[i] = (BattleMultiplayerManager)EditorGUILayout.ObjectField(myTarget.attackingPlayer[i], typeof(BattleMultiplayerManager), true);
            }
        }
    }
}
#endif
