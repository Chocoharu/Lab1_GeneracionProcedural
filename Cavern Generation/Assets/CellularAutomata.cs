using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject cellPrefab;

    private GameObject[,] cells;
    private bool[,] grid;

    void Start()
    {
        cells = new GameObject[width, height];
        grid = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 position = new Vector2(x, y);
                cells[x, y] = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                grid[x, y] = Random.value > 0.5f; // Estado inicial aleatorio
                UpdateCellVisual(x, y);
            }
        }
    }

    void Update()
    {
        // Aquí irá la lógica de actualización del autómata
    }

    void UpdateCellVisual(int x, int y)
    {
        var renderer = cells[x, y].GetComponent<SpriteRenderer>();
        renderer.color = grid[x, y] ? Color.black : Color.white;
    }
}
