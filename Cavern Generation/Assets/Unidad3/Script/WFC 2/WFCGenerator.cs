using System;
using System.Collections.Generic;
using UnityEngine;

/// Ejemplo de uso en Unity
/// - Define una matriz de entrada (int IDs) o constrúyela desde texturas/tiles
/// - Aprende patrones y pesos
/// - Corre WFC con tamaño deseado
/// - Reconstruye la salida y opcionalmente instancia prefabs por ID
public class WFCGenerator : MonoBehaviour
{
    [Header("Aprendizaje de patrones")]
    [SerializeField] int n = 3;                          // tamaño de patrón n×n
    [SerializeField] bool periodicInput = true;          // envolver al extraer
    [SerializeField] int rngSeed = 12345;                // semilla para reproducibilidad (-1 = aleatoria)

    [Header("Tamaño de salida (en celdas de patrón)")]
    [SerializeField] int outWidth = 20;
    [SerializeField] int outHeight = 20;

    [Header("Mapeo opcional ID->Prefab (para instanciar)")]
    [SerializeField] List<TilePrefab> tilePrefabs = new(); // lista de pares (id, prefab)
    [SerializeField] float cellSize = 1f;

    [Header("Fuente simple de ejemplo (matriz de enteros)")]
    [SerializeField] TextAsset inputCSV;                 // opcional: CSV de ints, separados por coma

    private Dictionary<int, GameObject> _idToPrefab;

    void Start()
    {
        // 1) Construye la matriz de entrada
        int[,] input = LoadInputFromCSV(inputCSV);
        if (input == null)
        {
            Debug.LogError("No se pudo cargar la matriz de entrada.");
            return;
        }

        // 2) Aprende patrones, pesos y reglas
        var catalog = new PatternCatalog(n, periodicInput);
        catalog.BuildFromInput(input);

        // 3) Ejecuta WFC en una rejilla de patrones outWidth x outHeight
        int? seed = rngSeed >= 0 ? rngSeed : (int?)null;
        var model = new WaveModel(catalog, outWidth, outHeight, seed);
        if (!model.Run(out int[,] patternGrid))
        {
            Debug.LogError("WFC falló (contradicción). Prueba otra semilla o ajusta parámetros.");
            return;
        }

        // 4) Reconstruye salida de IDs por celda: tamaño (outWidth+n-1) x (outHeight+n-1)
        int[,] result = catalog.ReconstructFromPatternGrid(outWidth, outHeight, patternGrid);

        // 5) Instanciar prefabs (opcional)
        BuildPrefabMap();
        InstantiateResult(result);
    }

    // ---------- Utilidades ----------

    private void BuildPrefabMap()
    {
        _idToPrefab = new Dictionary<int, GameObject>();
        foreach (var tp in tilePrefabs)
        {
            if (!_idToPrefab.ContainsKey(tp.id) && tp.prefab != null)
                _idToPrefab.Add(tp.id, tp.prefab);
        }
    }

    private void InstantiateResult(int[,] grid)
    {
        if (_idToPrefab == null || _idToPrefab.Count == 0)
        {
            Debug.Log("No hay mapeo ID->Prefab; sólo se generó la matriz de salida internamente.");
            return;
        }

        int w = grid.GetLength(0);
        int h = grid.GetLength(1);
        var parent = new GameObject("WFC_Output").transform;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int id = grid[x, y];
                if (_idToPrefab.TryGetValue(id, out GameObject prefab))
                {
                    Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);
                    Instantiate(prefab, pos, Quaternion.identity, parent);
                }
            }
        }
    }

    private int[,] LoadInputFromCSV(TextAsset csv)
    {
        if (csv == null) return null;
        string[] lines = csv.text.Trim().Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return null;

        var rows = new List<int[]>();
        foreach (var raw in lines)
        {
            string line = raw.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] toks = line.Split(',');
            int[] row = new int[toks.Length];
            for (int i = 0; i < toks.Length; i++)
                row[i] = int.Parse(toks[i].Trim());
            rows.Add(row);
        }

        int h = rows.Count;
        int w = rows[0].Length;
        var grid = new int[w, h];
        for (int y = 0; y < h; y++)
        {
            if (rows[y].Length != w) throw new Exception("CSV con filas de distinto largo.");
            for (int x = 0; x < w; x++) grid[x, y] = rows[y][x];
        }
        return grid;
    }
}

[Serializable]
public class TilePrefab
{
    public int id;
    public GameObject prefab;
}
