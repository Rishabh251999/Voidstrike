using UnityEngine;

[CreateAssetMenu(fileName = "NodeDataManager", menuName = "Singletons/Node Data Manager")]
public class NodeDataManager : ScriptableObject
{
    private static NodeDataManager instance;

    public static NodeDataManager Instance
    {
        get
        {
            if (!instance)
            {
                instance = Resources.Load<NodeDataManager>("NodeDataManager");
            }
            return instance;
        }
    }

    public NodeData[] allNodes;
}