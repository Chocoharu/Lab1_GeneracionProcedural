using System;
using System.Collections.Generic;

public class PatternCatalog
{
    // Tamaño del patrón (n x n)
    public readonly int N;

    // Si la extracción de parches usa envoltura (wrap) sobre los bordes del input
    public readonly bool PeriodicInput;

    // Número de patrones distintos detectados
    public int PatternCount => _patterns.Count;

    // Lista de patrones, cada patrón es un arreglo linealizado en orden row-major (n*n)
    private readonly List<int[]> _patterns = new();

    // Pesos (frecuencias) asociados a cada patrón en la misma posición que _patterns
    private readonly List<int> _weights = new();

    // Mapa de hash de patrón a índice en _patterns / _weights
    private readonly Dictionary<string, int> _hashToIndex = new();

    // Compatibilidades por dirección:
    // índice 0 = Up, 1 = Down, 2 = Left, 3 = Right
    // Cada entrada es una lista (por patrón) de conjuntos de índices compatibles
    private readonly List<HashSet<int>>[] _compatible;

    // Constructor: fija N mínimo 1 y configura el arreglo de compatibilidades
    public PatternCatalog(int n, bool periodicInput = true)
    {
        N = Math.Max(1, n);
        PeriodicInput = periodicInput;
        _compatible = new List<HashSet<int>>[4];
        for (int d = 0; d < 4; d++) _compatible[d] = new List<HashSet<int>>();
    }

    // Construye el catálogo de patrones y las relaciones de compatibilidad a partir de una imagen de entrada
    public void BuildFromInput(int[,] input)
    {
        if (input == null) throw new ArgumentNullException(nameof(input));

        int w = input.GetLength(0);
        int h = input.GetLength(1);

        // 1) Extraer todos los parches n x n del input.
        //    Si PeriodicInput es true se recorre todo el ancho/alto aplicando wrap; si no, se limita a posiciones válidas.
        for (int y = 0; y < (PeriodicInput ? h : h - N + 1); y++)
        {
            for (int x = 0; x < (PeriodicInput ? w : w - N + 1); x++)
            {
                int[] patch = ExtractPatch(input, x, y, w, h);
                string key = HashPattern(patch);

                // Si el parche ya existe, incrementar su peso; si no, añadirlo como nuevo patrón
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

        // 2) Inicializar las estructuras de compatibilidad: para cada dirección crear un HashSet por cada patrón
        for (int d = 0; d < 4; d++)
        {
            _compatible[d].Clear();
            for (int p = 0; p < PatternCount; p++) _compatible[d].Add(new HashSet<int>());
        }

        // 3) Rellenar compatibilidades comprobando solapamiento de (N-1) celdas entre pares de patrones
        for (int a = 0; a < PatternCount; a++)
        {
            for (int b = 0; b < PatternCount; b++)
            {
                if (AreCompatible(a, b, 0)) _compatible[0][a].Add(b); // b puede ir arriba de a
                if (AreCompatible(a, b, 1)) _compatible[1][a].Add(b); // b puede ir abajo de a
                if (AreCompatible(a, b, 2)) _compatible[2][a].Add(b); // b puede ir a la izquierda de a
                if (AreCompatible(a, b, 3)) _compatible[3][a].Add(b); // b puede ir a la derecha de a
            }
        }
    }

    // Devuelve los pesos normalizados (suman 1)
    public double[] GetWeightsNormalized()
    {
        double sum = 0;
        foreach (var w in _weights) sum += w;
        double[] wnorm = new double[_weights.Count];
        for (int i = 0; i < wnorm.Length; i++) wnorm[i] = _weights[i] / sum;
        return wnorm;
    }

    // Accesores simples
    public int GetWeight(int patternIndex) => _weights[patternIndex];
    public int[] GetPattern(int patternIndex) => _patterns[patternIndex];
    public IEnumerable<int> GetCompatible(int patternIndex, int direction) => _compatible[direction][patternIndex];

    // Reconstruye una imagen de salida a partir de una cuadrícula de índices de patrón.
    // La salida tiene tamaño (width+N-1) x (height+N-1) debido al solapamiento de patrones.
    public int[,] ReconstructFromPatternGrid(int width, int height, int[,] patternGrid)
    {
        int outW = width + N - 1;
        int outH = height + N - 1;
        var output = new int[outW, outH];

        // Para cada celda de la cuadrícula escribir el bloque n x n correspondiente en la salida
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int p = patternGrid[x, y];
                var pat = _patterns[p];
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

    // ---------- Métodos auxiliares ----------

    // Extrae un parche n x n desde la posición (sx,sy) aplicando wrap si PeriodicInput es true
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

    // Calcula un hash simple y estable para un patrón (devuelve string para usarlo como clave de diccionario)
    private string HashPattern(int[] pat)
    {
        unchecked
        {
            int h = 17;
            for (int i = 0; i < pat.Length; i++) h = h * 31 + pat[i];
            return h.ToString();
        }
    }

    // Comprueba compatibilidad entre dos patrones mediante el solapamiento de (N-1) celdas
    // Direcciones: 0 = Up (b arriba de a), 1 = Down (b abajo de a), 2 = Left (b izquierda de a), 3 = Right (b derecha de a)
    private bool AreCompatible(int a, int b, int direction)
    {
        var A = _patterns[a];
        var B = _patterns[b];
        int n = N;

        switch (direction)
        {
            case 0:
                // B arriba de A: la franja superior de A debe coincidir con la franja inferior de B
                for (int x = 0; x < n; x++)
                    for (int k = 0; k < n - 1; k++)
                        if (A[(k + 1) * n + x] != B[k * n + x]) return false;
                return true;

            case 1:
                // B abajo de A: la franja inferior de A debe coincidir con la franja superior de B
                for (int x = 0; x < n; x++)
                    for (int k = 0; k < n - 1; k++)
                        if (A[k * n + x] != B[(k + 1) * n + x]) return false;
                return true;

            case 2:
                // B a la izquierda de A: la franja izquierda de A debe coincidir con la franja derecha de B
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
