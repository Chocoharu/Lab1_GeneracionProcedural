using System.Collections.Generic;
using UnityEngine;

public class MarkovModel : MonoBehaviour
{

    int N;
    private Dictionary<string, List<string>> transitions;

    public MarkovModel(int n)
    {
        N = Mathf.Max(2, n);
        transitions = new Dictionary<string, List<string>>();
    }

    public void Train(List<string> columns)
    {
        transitions.Clear();

        int contextSize = N - 1;

        if (columns.Count <= contextSize)
        {
            Debug.LogError("No hay suficientes columnas para entrenar con N=" + N);
            return;
        }

        for (int i = 0; i < columns.Count - contextSize; i++)
        {
            string key = BuildKey(columns, i, contextSize);
            string next = columns[i + contextSize];

            if (!transitions.ContainsKey(key))
                transitions[key] = new List<string>();

            transitions[key].Add(next);
        }
    }

    public List<string> Generate(int count, List<string> initialColumns)
    {
        List<string> result = new List<string>();

        int contextSize = N - 1;

        for (int i = 0; i < contextSize; i++)
            result.Add(initialColumns[i]);

        for (int i = contextSize; i < count; i++)
        {
            string key = BuildKey(result, i - contextSize, contextSize);

            if (!transitions.ContainsKey(key))
                key = GetRandomKey();

            List<string> options = transitions[key];
            string next = options[Random.Range(0, options.Count)];
            result.Add(next);
        }

        return result;
    }

    private string BuildKey(List<string> columns, int start, int count)
    {
        string key = "";

        for (int i = 0; i < count; i++)
        {
            if (i > 0) key += "|";
            key += columns[start + i];
        }

        return key;
    }

    private string GetRandomKey()
    {
        List<string> keys = new List<string>(transitions.Keys);
        return keys[Random.Range(0, keys.Count)];
    }
}
