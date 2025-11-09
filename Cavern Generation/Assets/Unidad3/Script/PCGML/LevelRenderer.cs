using UnityEngine;

public class LevelRenderer : MonoBehaviour
{
    public GameObject grassPrefab;
    public GameObject sandPrefab;
    public GameObject waterPrefab;

    public void Render(string data, int width)
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

    private GameObject GetPrefab(char c)
    {

        switch (c)
        {
            case 'G': return grassPrefab;
            case 'S': return sandPrefab;
            case 'W': return waterPrefab;
            default: return null;
        }
    }
}
