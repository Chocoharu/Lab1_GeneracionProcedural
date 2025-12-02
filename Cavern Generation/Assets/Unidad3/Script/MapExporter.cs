using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class MapExporter 
{
    // Exporta un grid de enteros (WFC)
    public static void SaveGridCSV(int[,] grid, string filename)
    {
        string folder = Application.dataPath + "/ExportedMaps/";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string path = folder + filename;

        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        List<string> lines = new List<string>();

        for (int y = 0; y < h; y++)
        {
            string row = "";
            for (int x = 0; x < w; x++)
            {
                row += grid[x, y];
                if (x < w - 1) row += ",";
            }
            lines.Add(row);
        }

        File.WriteAllLines(path, lines);
        Debug.Log("CSV guardado en: " + path);
    }

    // Exporta un mapa de Markov a CSV 
    public static void SaveMarkovColumns(List<string> columns, string filename)
    {
        int width = columns.Count;
        int height = columns[0].Length;

        int[,] grid = new int[width, height];

        // convertir char a id
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = CharToID(columns[x][y]);

        SaveGridCSV(grid, filename);
    }

    // Puedes editar este mapeo a tu gusto
    public static int CharToID(char c)
    {
        switch (c)
        {
            case 'X': return 1;   // suelo
            case '-': return 0;   // vacío
            case 'S': return 2;   // bloque sólido
            case '?': return 3;   // bloque pregunta
            case 'Q': return 4;   // bloque moneda
            case 'E': return 5;   // enemigo
            default: return 0;
        }
    }

    public static void SaveLabeled(int[,] grid, string folder, string filename)
    {
        string path = Application.dataPath + "/" + folder + "/";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        int difficulty = MapLabeler.GetDifficulty(grid);

        List<string> lines = new List<string>();
        lines.Add(difficulty.ToString()); // etiqueta primera línea

        int w = grid.GetLength(0);
        int h = grid.GetLength(1);

        for (int y = 0; y < h; y++)
        {
            string row = "";
            for (int x = 0; x < w; x++)
            {
                row += grid[x, y];
                if (x < w - 1) row += ",";
            }
            lines.Add(row);
        }

        File.WriteAllLines(path + filename, lines);
    }

    public static void SaveMarkovLabeled(List<string> columns, string folder, string filename)
    {
        int w = columns.Count;
        int h = columns[0].Length;

        int[,] grid = new int[w, h];

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                grid[x, y] = columns[x][y] - '0'; // convierte char '0'  0

        SaveLabeled(grid, folder, filename);
    }
}
