using System.Collections.Generic;
using UnityEngine;

public class LevelRenderer : MonoBehaviour
{
    public GameObject groundPrefab;
    public GameObject blockPrefab;
    public GameObject emptyPrefab;
    public GameObject CoinBlockPrefab;
    public GameObject QuestionBlockPrefab;
    public GameObject enemyPrefab;
    public GameObject leftTubPrefab;
    public GameObject rightTubPrefab;
    public GameObject leftTopTubPrefab;
    public GameObject rightTopTubPrefab;

    public void Render(List<string> columns)
    {

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        int width = columns.Count;
        int height = columns[0].Length;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                char symbol = columns[x][y];
                GameObject prefab = GetPrefab(symbol);

                if (prefab != null)
                    Instantiate(prefab, new Vector3(x, height - y, 0), Quaternion.identity, transform);
            }
        }
    }

    private GameObject GetPrefab(char c)
    {

        switch (c)
        {

            case 'X': return groundPrefab;
            case 'S': return blockPrefab;
            case '-': return emptyPrefab;
            case '?': return QuestionBlockPrefab;
            case 'Q': return CoinBlockPrefab;
            case 'E': return enemyPrefab;
            case '<': return leftTopTubPrefab;
            case '>': return rightTopTubPrefab;
            case '[': return leftTubPrefab;
            case ']': return rightTubPrefab;
            default: return null;
        }
    }
}
