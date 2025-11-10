using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{

    [TextArea]
    public string inputExample;

    public int outputColumns;

    public int N;

    public LevelRenderer renderer;

    private MarkovModel model;

    void Start()
    {
        GenerateLevelMarkov();
    }

    public void GenerateLevelMarkov()
    {

        List<string> lines = ProcessInput(inputExample);
        List<string> columns = ConvertToColumns(lines);

        model = new MarkovModel(N);
        model.Train(columns);

        string initialColumn = columns[0];

        List<string> generated = model.Generate(outputColumns, columns);

        renderer.Render(generated);
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

}
