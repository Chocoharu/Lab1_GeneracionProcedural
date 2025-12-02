using System.Collections.Generic;
using UnityEngine;

public class MarkovModel
{
    public int N { get; private set; }

    // key = contexto (ej: "01320|11100"), value = posibles columnas siguientes
    private Dictionary<string, List<string>> transitions;

    public MarkovModel(int n)
    {
        // Orden mínimo 2 (contexto de al menos 1 columna)
        N = Mathf.Max(2, n);
        transitions = new Dictionary<string, List<string>>();
    }

    public void Train(List<string> columns)
    {
        transitions.Clear();

        int contextSize = N - 1;

        // Validación: necesitamos al menos contextSize+1 columnas para aprender una transición
        if (columns == null || columns.Count <= contextSize)
        {
            Debug.LogError($"No hay suficientes columnas para entrenar con N={N}. Columnas: {columns?.Count ?? 0}");
            return;
        }

        // Recorre todas las posiciones posibles para contexto + siguiente
        // Ej: N=2 → contexto de 1 columna, siguiente = la columna después
        for (int i = 0; i < columns.Count - contextSize; i++)
        {
            // Construye la llave del contexto con las N-1 columnas a partir de i
            string key = BuildKey(columns, i, contextSize);   // contexto
            // Toma la columna que sigue inmediatamente al contexto
            string next = columns[i + contextSize];           // columna siguiente

            // Si no existe la lista para este contexto, créala
            if (!transitions.ContainsKey(key))
                transitions[key] = new List<string>();

            // Añade la observación 'next' a la lista (puede repetirse para contar frecuencia)
            transitions[key].Add(next);
        }

        Debug.Log($"Markov entrenado. Contextos aprendidos: {transitions.Count}");
    }

    /// Genera 'count' columnas nuevas usando 'initialColumns' como semilla
    /// (las primeras N-1 columnas). Si initialColumns es demasiado pequeño,
    /// se usa un contexto aleatorio del modelo.
    /// <param name="count">Número total de columnas a generar</param>
    /// <param name="initialColumns">Semilla inicial (se usan las primeras N-1 entradas)</param>
    /// <returns>Lista de columnas generadas (strings)</returns>
    public List<string> Generate(int count, List<string> initialColumns)
    {
        List<string> result = new List<string>();

        int contextSize = N - 1;

        // Validación: el modelo debe estar entrenado (tener transiciones)
        if (transitions.Count == 0)
        {
            Debug.LogError("MarkovModel: No hay transiciones entrenadas. Llama a Train() antes de Generate().");
            return result;
        }

        // Preparar la semilla inicial
        if (initialColumns == null || initialColumns.Count < contextSize)
        {
            // Si la semilla es insuficiente, elegir un contexto aleatorio del modelo
            Debug.LogWarning("InitialColumns es muy pequeño, se usará un contexto aleatorio del modelo.");
            string randomKey = GetRandomKey();
            // Dividir la llave por '|' para recuperar las columnas del contexto
            string[] parts = randomKey.Split('|');
            result.AddRange(parts);
        }
        else
        {
            // Usar las primeras N-1 columnas de initialColumns como semilla
            for (int i = 0; i < contextSize; i++)
                result.Add(initialColumns[i]);
        }

        // Generación iterativa hasta alcanzar 'count' columnas
        for (int i = result.Count; i < count; i++)
        {
            // Construir la llave usando las últimas 'contextSize' columnas de 'result'
            // start = i - contextSize (índice del primer elemento del contexto)
            string key = BuildKey(result, i - contextSize, contextSize);

            // Si no tenemos transiciones para este contexto (nunca visto),
            // sustituimos por un contexto aleatorio conocido
            if (!transitions.ContainsKey(key))
            {
                key = GetRandomKey();
            }

            // Obtener opciones posibles y elegir una aleatoriamente
            List<string> options = transitions[key];
            string next = options[Random.Range(0, options.Count)];
            result.Add(next);
        }

        return result;
    }

    /// Construye la llave uniendo 'count' columnas a partir de 'start' con separador '|'
    /// Se asume que las entradas en 'columns' existen en los índices solicitados
    private string BuildKey(List<string> columns, int start, int count)
    {
        // Construcción simple y eficiente de la llave
        // Usamos concatenación básica porque los strings de contexto suelen ser cortos.
        string key = "";

        for (int i = 0; i < count; i++)
        {
            if (i > 0) key += "|";
            key += columns[start + i];
        }

        return key;
    }

    /// Devuelve una llave aleatoria del diccionario 'transitions'
    /// Regresa cadena vacía y log de error si no hay llaves
    private string GetRandomKey()
    {
        List<string> keys = new List<string>(transitions.Keys);

        if (keys.Count == 0)
        {
            Debug.LogError("MarkovModel: No hay llaves en transitions.");
            return "";
        }

        return keys[Random.Range(0, keys.Count)];
    }
}
