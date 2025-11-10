using UnityEngine;

public class OriginalImage : MonoBehaviour
{
    [Header("CSV Map")]
    [SerializeField] TextAsset inputCSV;

    [Header("Prefabs")]
    [SerializeField] GameObject sandPrefab;  // ID 0
    [SerializeField] GameObject grassPrefab; // ID 1
    [SerializeField] GameObject waterPrefab; // ID 2

    [Header("Tile Settings")]
    public float cellSize = 1f;

    void Start()
    {
        GenerateImage();
    }

    private void GenerateImage()
    {
        string[] rows = inputCSV.text.Split('\n');

        for (int y = 0; y < rows.Length; y++)
        {
            string[] cols = rows[y].Trim().Split(',');

            for (int x = 0; x < cols.Length; x++)
            {
                int tileID = int.Parse(cols[x]);
                Vector3 pos = new Vector3(x * cellSize, -y * cellSize, 0);

                switch (tileID)
                {
                    case 0: Instantiate(sandPrefab, pos, Quaternion.identity, transform); break;
                    case 1: Instantiate(grassPrefab, pos, Quaternion.identity, transform); break;
                    case 2: Instantiate(waterPrefab, pos, Quaternion.identity, transform); break;
                }
            }
        }
    }
}
