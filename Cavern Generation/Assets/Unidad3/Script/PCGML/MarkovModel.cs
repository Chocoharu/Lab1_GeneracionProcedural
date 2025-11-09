using System.Collections.Generic;
using UnityEngine;

public class MarkovModel : MonoBehaviour
{

    int N;
    private Dictionary<string, List<char>> transitions;

    public MarkovModel(int n)
    {

        N = n;
        transitions = new Dictionary<string, List<char>>();
    }
    

    public void Train(string example)
    {
        example = example.Replace("\n", "").Replace("\r", "");
        transitions.Clear();

        for(int i = 0; i <= example.Length - N; i++)
        {

            string key = example.Substring(i, N - 1);
            char next = example[i + (N - 1)];

            if (!transitions.ContainsKey(key))
                transitions[key] = new List<char>();

            transitions[key].Add(next);
        }   
    }

    public string Generate(int length)
    {

        if (transitions.Count == 0)
            return " ";

        string current = GetRandomKey();
        string result = current;

        for(int i = 0; i < length; i++)
        {

            if (!transitions.ContainsKey(current))
                current = GetRandomKey();

            List<char> possibleNext = transitions[current];
            char next = possibleNext[Random.Range(0, possibleNext.Count)];
            result += next;

            current = result.Substring(result.Length - (N - 1), N - 1);
        }

        return result;
    }

    private string GetRandomKey()
    {

        List<string> keys = new List<string>(transitions.Keys);
        return keys[Random.Range(0, keys.Count)];
    }
}
