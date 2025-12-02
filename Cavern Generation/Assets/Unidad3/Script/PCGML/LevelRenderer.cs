using System.Collections.Generic;
using UnityEngine;

public class LevelRenderer : MonoBehaviour
{
    public GameObject waterPrefab;
    public GameObject groundPrefab;
    public GameObject enemyPrefab;
    public GameObject grassPrefab;

    public float cellSize = 1f;

    public void Render(List<string> columns)
    {
        // Limpiar
        foreach (Transform child in transform)
            GameObject.Destroy(child.gameObject);

        int width = columns.Count;
        int height = columns[0].Length;

        // Dibujar columnas
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                char c = columns[x][y];
                GameObject prefab = GetPrefab(c);

                if (prefab != null)
                {
                    Vector3 pos = new Vector3(x * cellSize, -y * cellSize, 0);
                    Instantiate(prefab, pos, Quaternion.identity, transform);
                }
            }
        }
    }

    private GameObject GetPrefab(char c)
    {
        switch (c)
        {
            case '0': return waterPrefab;
            case '1': return groundPrefab;
            case '2': return enemyPrefab;
            case '3': return grassPrefab;
            default: return null;
        }
    }
}
