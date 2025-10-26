using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class WFCBuilder : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;

    private WFCNode[,] grid;

    public List<WFCNode> Nodes = new List<WFCNode>();

    public List<Vector2Int> _toCollapse = new List<Vector2Int>();

    public Vector2Int[] offsets = new Vector2Int[]
    {
        new Vector2Int(0,1), // Up
        new Vector2Int(1,0), // Right
        new Vector2Int(0,-1), // Down
        new Vector2Int(-1,0) // Left
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grid = new WFCNode[width, height];

        WaveFunctionCollapse();

    }

    private void WaveFunctionCollapse()
    {
        _toCollapse.Clear();
        _toCollapse.Add(new Vector2Int(width/2, height/2));

        while(_toCollapse.Count > 0)
        {
            int x = _toCollapse[0].x;
            int y = _toCollapse[0].y;

            List<WFCNode> potentialNodes = new List<WFCNode>(Nodes);

            for(int i = 0; i < offsets.Length; i++)
            {
                Vector2Int neighbour = new Vector2Int(x + offsets[i].x, y + offsets[i].y);
             
                if (IsInsideGrid(neighbour))
                {
                    WFCNode neighbourNode = grid[neighbour.x, neighbour.y];

                    if(neighbourNode != null)
                    {
                        switch(i)
                        {
                            case 0:
                                WhittleNodes(potentialNodes, neighbourNode.bottom.compatibleNodes);
                                break;
                            case 1:
                                WhittleNodes(potentialNodes, neighbourNode.top.compatibleNodes);
                                break;
                            case 2:
                                WhittleNodes(potentialNodes, neighbourNode.left.compatibleNodes);
                                break;
                            case 3:
                                WhittleNodes(potentialNodes, neighbourNode.right.compatibleNodes);
                                break;
                        }
                    }
                    else 
                    {
                        if (!_toCollapse.Contains(neighbour)) _toCollapse.Add(neighbour);
                    }
                }
            }

            if (potentialNodes.Count < 1)
            {
                grid[x, y] = Nodes[0];
                Debug.LogWarning("Attemted to collapse warw on " + x + ", " + y + " but found no compatible node");
            }
            else
            {
                grid[x, y] = potentialNodes[Random.Range(0, potentialNodes.Count)];
            }

            GameObject newNode = Instantiate(grid[x, y].prefab, new Vector3(x, y, 0f), Quaternion.identity);

            _toCollapse.RemoveAt(0);
        }
    }
    private void WhittleNodes(List<WFCNode> potentialNodes, List<WFCNode> validNodes)
    {
        for(int i = potentialNodes.Count - 1; i > -1; i--)
        {
            if (!validNodes.Contains(potentialNodes[i]))
            {
                potentialNodes.RemoveAt(i);
            }
        }
    }

    private bool IsInsideGrid(Vector2Int position)
    {
        return position.x >  -1 && position.x < width && position.y > -1 && position.y < height;
    }
}
