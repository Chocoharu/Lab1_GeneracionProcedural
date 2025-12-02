using System;
using System.Collections.Generic;
using UnityEngine;

public class WFCGenerator : MonoBehaviour
{
    // Tamaño de los patrones extraídos (N x N)
    [Header("Aprendizaje de patrones")]
    [SerializeField] int n = 3;
    // Si al extraer patrones desde la entrada se considera envoltura (torus)
    [SerializeField] bool periodicInput = true;
    // Semilla para el generador aleatorio (-1 significa usar una semilla aleatoria)
    [SerializeField] int rngSeed = 12345;

    // Dimensiones de la salida en celdas de patrón
    [Header("Tamaño de salida (en celdas de patrón)")]
    [SerializeField] int outWidth = 20;
    [SerializeField] int outHeight = 20;

    // Mapeo opcional de ID de celda a Prefab de Unity para instanciado
    [Header("Mapeo opcional ID->Prefab (para instanciar)")]
    [SerializeField] List<TilePrefab> tilePrefabs = new();
    // Tamaño de cada celda en unidades de escena
    [SerializeField] float cellSize = 1f;

    // Fuente de ejemplo: CSV de enteros (opcional)
    [Header("Fuente simple de ejemplo (matriz de enteros)")]
    [SerializeField] string inputFolder = "InputMaps";
    private List<int[,]> trainingMaps;

    // Diccionario interno que asocia un ID de celda a un prefab
    private Dictionary<int, GameObject> _idToPrefab;

    void Start()
    {
        // Cargar múltiples mapas
        trainingMaps = LoadMaps.LoadAllMaps(inputFolder);

        if (trainingMaps.Count == 0)
        {
            Debug.LogError("No se encontraron mapas en la carpeta: " + inputFolder);
            return;
        }

        // Construir catálogo con múltiples inputs
        var catalog = new PatternCatalog(n, periodicInput);

        foreach (var m in trainingMaps)
            catalog.BuildFromInput(m);

        int? seed = rngSeed >= 0 ? rngSeed : (int?)null;
        var model = new WaveModel(catalog, outWidth, outHeight, seed);

        if (!model.Run(out int[,] patternGrid))
        {
            Debug.LogError("WFC falló (contradicción)");
            return;
        }

        int[,] result = catalog.ReconstructFromPatternGrid(outWidth, outHeight, patternGrid);

        // Exportar CON etiqueta
        MapExporter.SaveLabeled(result, "Generated/WFC", "map_wfc_" + UnityEngine.Random.Range(0, 9999) + ".csv");

        BuildPrefabMap();
        InstantiateResult(result);
    }

    // ---------- Utilidades ----------

    // Construye el diccionario de mapeo ID->Prefab a partir de la lista serializada
    private void BuildPrefabMap()
    {
        _idToPrefab = new Dictionary<int, GameObject>();
        foreach (var tp in tilePrefabs)
        {
            if (!_idToPrefab.ContainsKey(tp.id) && tp.prefab != null)
                _idToPrefab.Add(tp.id, tp.prefab);
        }
    }

    // Instancia los prefabs en la escena según la matriz de IDs; si no hay mapeo, solo informa por consola
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

    // Carga una matriz de enteros desde un TextAsset CSV; devuelve null si no se proporciona
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
    // Identificador que corresponde al valor en la matriz de salida
    public int id;
    // Prefab a instanciar para el id correspondiente
    public GameObject prefab;
}
