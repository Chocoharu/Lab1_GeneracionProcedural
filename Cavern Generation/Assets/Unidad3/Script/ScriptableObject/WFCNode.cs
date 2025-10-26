using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WFCNode", menuName = "WFC/Node")]
[System.Serializable]
public class WFCNode : ScriptableObject
{
    public string nodeName;
    public GameObject prefab;
    public WFC_Conection top;
    public WFC_Conection bottom;
    public WFC_Conection left;
    public WFC_Conection right;

}

[System.Serializable]
public class WFC_Conection
{
    public List<WFCNode> compatibleNodes = new List<WFCNode>();
}
