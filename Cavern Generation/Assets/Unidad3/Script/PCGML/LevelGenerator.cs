using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("Mapas de entrada (CSV)")]
    [SerializeField] private List<TextAsset> inputCsvMaps;

    public int N = 2;
    public int outputColumns = 20;

    public LevelRenderer renderer;

    private MarkovModel model;

    // Inicio: lanzar la generación al comenzar la escena
    void Start()
    {
        GenerateLevelMarkov();
    }

    // Paso principal: orquesta la generación de niveles usando un modelo de Markov
    public void GenerateLevelMarkov()
    {
        // Paso 1: cargar mapas de entrada
        List<int[,]> maps = new List<int[,]>();

        foreach (var csv in inputCsvMaps)
        {
            if (csv == null) continue;
            maps.Add(ParseCsvToMap(csv));
        }

        if (maps.Count == 0)
        {
            Debug.LogError("No se encontraron mapas en la lista inputCsvMaps");
            return;
        }

        // Paso 2: convertir mapas a columnas
        List<string> allColumns = new List<string>();

        foreach (var map in maps)
            allColumns.AddRange(MapToColumns(map));

        // Paso 3: entrenar modelo Markov
        model = new MarkovModel(N);
        model.Train(allColumns);

        //  GENERAR 10 MAPAS
        for (int i = 0; i < 10; i++)
        {
            List<string> generated = model.Generate(outputColumns, allColumns);

            // Exportar
            string filename = $"map_markov_{i}.csv";
            MapExporter.SaveMarkovLabeled(generated, "Generated/Markov", filename);

            Debug.Log($"Markov: mapa {i} exportado como {filename}");

            // Render en Unity SOLO el último
            if (i == 9)
                renderer.Render(generated);
        }
    }

    // Convierte una matriz 2D de enteros en una lista de cadenas donde cada cadena es una columna
    List<string> MapToColumns(int[,] map)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        List<string> columns = new List<string>();

        for (int x = 0; x < w; x++)
        {
            string col = "";
            for (int y = 0; y < h; y++)
                col += map[x, y].ToString();

            columns.Add(col);
        }

        return columns;
    }

    // Parseo de CSV a matriz int[,]: maneja separación por comas y filas por líneas
    private int[,] ParseCsvToMap(TextAsset csv)
    {
        string[] lines = csv.text.Trim().Split('\n');

        int height = lines.Length;
        string[] firstRow = lines[0].Trim().Split(',');
        int width = firstRow.Length;

        int[,] map = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            string[] cells = lines[y].Trim().Split(',');

            for (int x = 0; x < width; x++)
            {
                if (int.TryParse(cells[x], out int val))
                    map[x, y] = val;
                else
                    map[x, y] = 0;
            }
        }

        return map;
    }
}
