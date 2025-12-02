using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class LoadMaps
{
    // Carga TODOS los CSV que estén en Assets/inputFolder/
    public static List<int[,]> LoadAllMaps(string folder)
    {
        List<int[,]> maps = new();

        string path = Path.Combine(Application.dataPath, folder);

        Debug.Log("Buscando CSV en ruta: " + path);

        if (!Directory.Exists(path))
        {
            Debug.LogError("La carpeta NO existe en disco: " + path);
            return maps;
        }

        // Usa EnumerateFiles que funciona mejor en rutas con espacios / OneDrive / GitHub
        var files = Directory.EnumerateFiles(path, "*.csv", SearchOption.TopDirectoryOnly);

        int count = 0;

        foreach (string file in files)
        {
            Debug.Log("Encontrado: " + file);
            int[,] map = LoadSingleCSV(file);

            if (map != null)
            {
                maps.Add(map);
                count++;
            }
        }

        Debug.Log("LoadMaps: Se cargaron " + count + " mapas desde: " + path);

        return maps;
    }

    // Cargar CSV individual (números separados por coma)
    private static int[,] LoadSingleCSV(string filepath)
    {
        string[] lines = File.ReadAllLines(filepath);

        List<int[]> rows = new List<int[]>();

        foreach (var raw in lines)
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            string[] parts = raw.Split(',');
            int[] row = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
                row[i] = int.Parse(parts[i]);

            rows.Add(row);
        }

        int height = rows.Count;
        int width = rows[0].Length;

        int[,] grid = new int[width, height];

        for (int y = 0; y < height; y++)
        {
            if (rows[y].Length != width)
            {
                Debug.LogError("CSV con filas de distinto largo: " + filepath);
                return null;
            }
            for (int x = 0; x < width; x++)
                grid[x, y] = rows[y][x];
        }

        return grid;
    }
}
