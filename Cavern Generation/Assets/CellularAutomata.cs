using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    public int width;
    public int height;
    public GameObject cellPrefab;
    public float simulationInterval;

    private GameObject[,] cells;
    private bool[,] grid;
    private float timer = 0;

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
        timer += Time.deltaTime;
        if (timer >= simulationInterval)
        {
            Simulate(5);
            timer = 0f;
        }
    }

    public void Simulate(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            bool[,] newGrid = (bool[,])grid.Clone();
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int neighbors = CountAliveNeighbors(x, y);
                    if (neighbors > 4)
                        newGrid[x, y] = true;
                    else if (neighbors < 4)
                        newGrid[x, y] = false;
                    // Si neighbors == 4, mantiene el estado actual
                }
            }
            grid = newGrid;

            for (int x = 0; x < width; x++)
            {
                newGrid[x, 0] = false; // Borde superior
                newGrid[x, height - 1] = false; // Borde inferior
            }

            for (int y = 0; y < height; y++)
            {
                newGrid[0, y] = false; // Borde izquierdo
                newGrid[width - 1, y] = false; // Borde derecho
            }
        }
        // Actualiza la visualización después de simular
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                UpdateCellVisual(x, y);
    }

    private int CountAliveNeighbors(int x, int y)
    {
        int count = 0;
        for (int nx = x - 1; nx <= x + 1; nx++)
        {
            for (int ny = y - 1; ny <= y + 1; ny++)
            {
                if (nx == x && ny == y) continue;
                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                if (grid[nx, ny]) count++;
            }
        }
        return count;
    }

    void UpdateCellVisual(int x, int y)
    {
        var renderer = cells[x, y].GetComponent<SpriteRenderer>();
        renderer.color = grid[x, y] ? Color.black : Color.white;
    }
}
