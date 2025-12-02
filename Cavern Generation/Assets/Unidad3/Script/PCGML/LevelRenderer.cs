using UnityEngine;

public class LevelRenderer : MonoBehaviour
{
<<<<<<< Updated upstream
    public GameObject grassPrefab;
    public GameObject sandPrefab;
    public GameObject waterPrefab;

    public void Render(string data, int width)
=======
    public GameObject groundPrefab;
    public GameObject waterPrefab;
    public GameObject enemyPrefab;
    public GameObject grassPrefab;
    public void Render(List<string> columns)
>>>>>>> Stashed changes
    {

        foreach (Transform child in transform)
            GameObject.Destroy(child.gameObject);

        for(int i = 0; i < data.Length; i++)
        {

            int x = i % width;
            int y = i / width;

            char symbol = data[i];
            GameObject prefab = GetPrefab(symbol);

            if (prefab != null)
                Instantiate(prefab, new Vector3(x, -y, 0), Quaternion.identity, transform);
        }
    }

    private GameObject GetPrefab(int id)
    {
        switch (id)
        {
<<<<<<< Updated upstream
            case 'G': return grassPrefab;
            case 'S': return sandPrefab;
            case 'W': return waterPrefab;
=======
            case 0: return waterPrefab;
            case 1: return groundPrefab;
            case 2: return enemyPrefab;
            case 3: return grassPrefab;
>>>>>>> Stashed changes
            default: return null;
        }
    }
}
