using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    [SerializeField] string inputFolder = "InputMaps";
    public int N = 2;
    public int outputColumns = 20;

    public LevelRenderer renderer;

    private MarkovModel model;

    void Start()
    {
        GenerateLevelMarkov();
    }

    public void GenerateLevelMarkov()
    {
        List<int[,]> maps = LoadMaps.LoadAllMaps(inputFolder);

        if (maps.Count == 0)
        {
            Debug.LogError("No se encontraron mapas en carpeta " + inputFolder);
            return;
        }

        // Convertir todos los mapas en columnas Markov
        List<string> allColumns = new List<string>();

        foreach (var map in maps)
            allColumns.AddRange(MapToColumns(map));

        model = new MarkovModel(N);
        model.Train(allColumns);

        // Generar columnas nuevas (usar primeras columnas como seed)
        List<string> generated = model.Generate(outputColumns, allColumns);

        renderer.Render(generated);

        // Exportar con etiqueta
        string filename = "map_markov_" + Random.Range(0, 9999) + ".csv";
        MapExporter.SaveMarkovLabeled(generated, "Generated/Markov", filename);

        Debug.Log("Mapa Markov exportado como: " + filename);
    }

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
}
