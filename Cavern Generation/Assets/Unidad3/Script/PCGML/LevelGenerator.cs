using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    [SerializeField] string inputFolder = "InputMaps";

    public int N = 2;
    public int outputLenght = 20;
    public int width = 20;

    public LevelRenderer renderer;

    private MarkovModel model;

    void Start()
    {
        GenerateLevelMarkov();
    }

    public void GenerateLevelMarkov()
    {

<<<<<<< Updated upstream
        model = new MarkovModel(N);
        model.Train(inputExample);

        string generated = model.Generate(outputLenght);
        Debug.Log("Resultado Esperado: " + generated);

        renderer.Render(generated, width);
    }
=======
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

        // Generar columnas nuevas
        List<string> generated = model.Generate(outputColumns, allColumns);

        renderer.Render(generated);

        // Exportar con etiqueta
        string filename = "map_markov_" + Random.Range(0, 9999) + ".csv";
        MapExporter.SaveMarkovLabeled(generated, "Generated/Markov", filename);

        Debug.Log("Mapa Markov exportado como: " + filename);
    }

    List<string> ProcessInput(string raw)
    {

        string[] split = raw.Split('\n');
        List<string> lines = new List<string>();

        foreach (string line in split)
        {

            string clean = line.TrimEnd('\r');
            lines.Add(clean);
        }

        return lines;
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

    List<string> ConvertToColumns(List<string> lines)
    {

        int height = lines.Count;
        int widht = lines[0].Length;

        List<string> columns = new List<string>();

        for(int x = 0; x < widht; x++)
        {
            string col = "";
            for(int y = 0; y < height; y++)
            {

                col += lines[y][x];
            }

            columns.Add(col);
        }

        return columns;
    }

>>>>>>> Stashed changes
}
