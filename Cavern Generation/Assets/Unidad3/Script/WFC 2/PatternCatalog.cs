using System;
using System.Collections.Generic;

/// Catálogo de patrones (n×n) con sus pesos y compatibilidades por dirección.
/// Trabaja con una matriz de ints (IDs de contenido/tile).
public class PatternCatalog
{
    public readonly int N;                       // Tamaño de patrón n×n
    public readonly bool PeriodicInput;         // Si envuelve al extraer del input
    public int PatternCount => _patterns.Count;

    // Datos por patrón
    private readonly List<int[]> _patterns = new();   // patrón linealizado n*n (row-major)
    private readonly List<int> _weights = new();      // conteos de ocurrencia
    private readonly Dictionary<string, int> _hashToIndex = new();

    // Compatibilidad: por dirección (0:Up,1:Down,2:Left,3:Right) => para cada patrón p: conjunto de q compatibles
    private readonly List<HashSet<int>>[] _compatible; // tamaño 4, cada uno lista de sets (uno por patrón)

    public PatternCatalog(int n, bool periodicInput = true)
    {
        N = Math.Max(1, n);
        PeriodicInput = periodicInput;
        _compatible = new List<HashSet<int>>[4];
        for (int d = 0; d < 4; d++) _compatible[d] = new List<HashSet<int>>();
    }

    /// Extrae patrones de un input y construye compatibilidades.
    public void BuildFromInput(int[,] input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));
        int w = input.GetLength(0);
        int h = input.GetLength(1);

        // 1) Extraer todos los parches n×n (con o sin “wrap”)
        for (int y = 0; y < (PeriodicInput ? h : h - N + 1); y++)
        {
            for (int x = 0; x < (PeriodicInput ? w : w - N + 1); x++)
            {
                int[] patch = ExtractPatch(input, x, y, w, h);
                string key = HashPattern(patch);
                if (_hashToIndex.TryGetValue(key, out int idx))
                {
                    _weights[idx]++;
                }
                else
                {
                    int newIdx = _patterns.Count;
                    _hashToIndex[key] = newIdx;
                    _patterns.Add(patch);
                    _weights.Add(1);
                }
            }
        }

        // 2) Inicializar compatibilidades
        for (int d = 0; d < 4; d++)
        {
            _compatible[d].Clear();
            for (int p = 0; p < PatternCount; p++) _compatible[d].Add(new HashSet<int>());
        }

        // 3) Construir compatibilidades por solapamiento (n-1) para cada par de patrones
        for (int a = 0; a < PatternCount; a++)
        {
            for (int b = 0; b < PatternCount; b++)
            {
                if (AreCompatible(a, b, 0)) _compatible[0][a].Add(b); // Up: b arriba de a
                if (AreCompatible(a, b, 1)) _compatible[1][a].Add(b); // Down: b abajo de a
                if (AreCompatible(a, b, 2)) _compatible[2][a].Add(b); // Left: b a la izquierda de a
                if (AreCompatible(a, b, 3)) _compatible[3][a].Add(b); // Right: b a la derecha de a
            }
        }
    }

    // Devuelve pesos normalizados
    public double[] GetWeightsNormalized()
    {
        double sum = 0;
        foreach (var w in _weights) sum += w;
        double[] wnorm = new double[_weights.Count];
        for (int i = 0; i < wnorm.Length; i++) wnorm[i] = _weights[i] / sum;
        return wnorm;
    }

    public int GetWeight(int patternIndex) => _weights[patternIndex];
    public int[] GetPattern(int patternIndex) => _patterns[patternIndex];
    public IEnumerable<int> GetCompatible(int patternIndex, int direction) => _compatible[direction][patternIndex];

    /// Reconstruye una salida de (W+N-1)×(H+N-1) a partir de una asignación de patrones por celda (W×H).
    public int[,] ReconstructFromPatternGrid(int width, int height, int[,] patternGrid)
    {
        int outW = width + N - 1;
        int outH = height + N - 1;
        var output = new int[outW, outH];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int p = patternGrid[x, y];
                var pat = _patterns[p];
                // Escribir bloque n×n en (x,y)
                for (int dy = 0; dy < N; dy++)
                {
                    for (int dx = 0; dx < N; dx++)
                    {
                        output[x + dx, y + dy] = pat[dy * N + dx];
                    }
                }
            }
        }
        return output;
    }

    // ---------- Helpers ----------
    private int[] ExtractPatch(int[,] input, int sx, int sy, int w, int h)
    {
        int[] patch = new int[N * N];
        for (int dy = 0; dy < N; dy++)
        {
            int yy = sy + dy;
            if (PeriodicInput) yy = ((yy % h) + h) % h;
            for (int dx = 0; dx < N; dx++)
            {
                int xx = sx + dx;
                if (PeriodicInput) xx = ((xx % w) + w) % w;
                patch[dy * N + dx] = input[xx, yy];
            }
        }
        return patch;
    }

    private string HashPattern(int[] pat)
    {
        // Hash estable y simple.
        unchecked
        {
            int h = 17;
            for (int i = 0; i < pat.Length; i++) h = h * 31 + pat[i];
            return h.ToString();
        }
    }

    // Direcciones: 0 Up, 1 Down, 2 Left, 3 Right
    private bool AreCompatible(int a, int b, int direction)
    {
        var A = _patterns[a];
        var B = _patterns[b];
        int n = N;
        switch (direction)
        {
            case 0: // B arriba de A: comparar franja superior de A con franja inferior de B
                for (int x = 0; x < n; x++)
                    for (int k = 0; k < n - 1; k++)
                        if (A[(k + 1) * n + x] != B[k * n + x]) return false;
                return true;

            case 1: // B abajo de A
                for (int x = 0; x < n; x++)
                    for (int k = 0; k < n - 1; k++)
                        if (A[k * n + x] != B[(k + 1) * n + x]) return false;
                return true;

            case 2: // B a la izquierda de A
                for (int y = 0; y < n; y++)
                    for (int k = 0; k < n - 1; k++)
                        if (A[y * n + (k + 1)] != B[y * n + k]) return false;
                return true;

            case 3: // B a la derecha de A
                for (int y = 0; y < n; y++)
                    for (int k = 0; k < n - 1; k++)
                        if (A[y * n + k] != B[y * n + (k + 1)]) return false;
                return true;
        }
        return false;
    }
}
