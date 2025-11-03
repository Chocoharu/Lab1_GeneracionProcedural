using System;
using System.Collections.Generic;
public class WaveModel
{
    private readonly PatternCatalog _catalog;
    private readonly int _W, _H;
    private readonly bool[,,] _wave; // [x,y,pattern] => permitido
    private readonly double[] _weights;
    private readonly double[] _weightLog; // w*log(w)
    private readonly Random _rng;

    // Para reconstrucción final (índice de patrón colapsado por celda)
    private readonly int[,] _chosen;

    // Vecindad 4-dir
    private static readonly int[] DX = { 0, 0, -1, 1 };
    private static readonly int[] DY = { 1, -1, 0, 0 };
    private const int UP = 0, DOWN = 1, LEFT = 2, RIGHT = 3;

    // Constructor: inicializa estado, marca todas las opciones como permitidas y carga pesos
    public WaveModel(PatternCatalog catalog, int width, int height, int? seed = null)
    {
        _catalog = catalog;
        _W = Math.Max(1, width);
        _H = Math.Max(1, height);
        _wave = new bool[_W, _H, catalog.PatternCount];
        _chosen = new int[_W, _H];
        _weights = new double[catalog.PatternCount];
        _weightLog = new double[catalog.PatternCount];
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();

        // Inicializar todas las celdas con todos los patrones permitidos
        for (int y = 0; y < _H; y++)
            for (int x = 0; x < _W; x++)
                for (int p = 0; p < catalog.PatternCount; p++)
                    _wave[x, y, p] = true;

        // Cargar pesos normalizados desde el catálogo y calcular w*log(w)
        double[] wn = catalog.GetWeightsNormalized();
        for (int p = 0; p < wn.Length; p++)
        {
            _weights[p] = wn[p];
            _weightLog[p] = wn[p] > 0 ? wn[p] * Math.Log(wn[p]) : 0.0;
        }
    }

    // Ejecuta el algoritmo WFC. Devuelve true y la grilla de patrones si tiene éxito.
    public bool Run(out int[,] patternGrid)
    {
        while (true)
        {
            var next = FindMinEntropyCell(out double entropy);
            if (next == null)
            {
                // Todas las celdas están colapsadas: devolver copia del resultado
                patternGrid = (int[,])_chosen.Clone();
                return true;
            }

            (int x, int y) = next.Value;

            // Muestrear un patrón válido en la celda seleccionada
            int picked = SamplePattern(x, y);
            if (picked < 0)
            {
                patternGrid = null;
                return false; // contradicción: sin patrones posibles
            }

            // Banear todos los patrones distintos del escogido en la celda
            for (int p = 0; p < _catalog.PatternCount; p++)
                if (p != picked && _wave[x, y, p])
                    Ban(x, y, p);

            _chosen[x, y] = picked;

            // Propagar restricciones a través de la cuadrícula
            if (!Propagate())
            {
                patternGrid = null;
                return false;
            }

            // Si toda la cuadrícula está colapsada, devolver resultado
            if (IsFullyCollapsed())
            {
                patternGrid = (int[,])_chosen.Clone();
                return true;
            }
        }
    }

    // Busca la celda no colapsada con mínima entropía y devuelve entropía usada para desempate.
    private (int x, int y)? FindMinEntropyCell(out double bestEntropy)
    {
        bestEntropy = double.MaxValue;
        (int x, int y)? best = null;

        for (int y = 0; y < _H; y++)
        {
            for (int x = 0; x < _W; x++)
            {
                if (IsCollapsed(x, y)) continue;

                double sumW = 0, sumWLog = 0;
                int count = 0;
                for (int p = 0; p < _catalog.PatternCount; p++)
                {
                    if (_wave[x, y, p])
                    {
                        double w = _weights[p];
                        sumW += w;
                        sumWLog += _weightLog[p];
                        count++;
                    }
                }

                // Si no hay opciones, retornar inmediatamente señalando contradicción
                if (count == 0) return (x, y);

                // Entropía de Shannon modificada por los pesos
                double H = Math.Log(sumW) - (sumW > 0 ? sumWLog / sumW : 0.0);

                // Añadir un ruido pequeño para evitar empates deterministas
                double noise = 1e-6 * _rng.NextDouble();
                double key = H + noise;

                if (key < bestEntropy)
                {
                    bestEntropy = key;
                    best = (x, y);
                }
            }
        }
        return best;
    }

    // Muestrea un patrón entre los permitidos en la celda (x,y) de forma ponderada por _weights.
    private int SamplePattern(int x, int y)
    {
        double total = 0;
        for (int p = 0; p < _catalog.PatternCount; p++)
            if (_wave[x, y, p]) total += _weights[p];

        if (total <= 0) return -1;

        double r = _rng.NextDouble() * total;
        double acc = 0;
        for (int p = 0; p < _catalog.PatternCount; p++)
        {
            if (!_wave[x, y, p]) continue;
            acc += _weights[p];
            if (r <= acc) return p;
        }

        // Fallback por redondeo: devolver el último patrón permitido
        for (int p = _catalog.PatternCount - 1; p >= 0; p--)
            if (_wave[x, y, p]) return p;

        return -1;
    }

    // Marca el patrón p en la celda (x,y) como no permitido
    private void Ban(int x, int y, int p)
    {
        _wave[x, y, p] = false;
    }

    // Propaga las restricciones hasta que no haya más cambios o se detecte contradicción.
    private bool Propagate()
    {
        var q = new Queue<(int x, int y)>();

        // Encolar todas las celdas para garantizar que la consistencia se propague inicialmente
        for (int y = 0; y < _H; y++)
            for (int x = 0; x < _W; x++)
                q.Enqueue((x, y));

        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();

            for (int dir = 0; dir < 4; dir++)
            {
                int nx = x + DX[dir];
                int ny = y + DY[dir];
                if (nx < 0 || nx >= _W || ny < 0 || ny >= _H) continue;

                bool changed = false;
                int invDir = Inverse(dir);

                // Verificar para cada patrón del vecino si aún tiene soporte en la celda actual
                for (int qpat = 0; qpat < _catalog.PatternCount; qpat++)
                {
                    if (!_wave[nx, ny, qpat]) continue;

                    bool supported = false;
                    foreach (int p in _catalog.GetCompatible(qpat, invDir))
                    {
                        if (_wave[x, y, p]) { supported = true; break; }
                    }

                    if (!supported)
                    {
                        _wave[nx, ny, qpat] = false;
                        changed = true;
                    }
                }

                if (changed)
                {
                    // Si el vecino se quedó sin opciones, se detecta contradicción
                    bool any = false;
                    for (int qp = 0; qp < _catalog.PatternCount; qp++)
                        if (_wave[nx, ny, qp]) { any = true; break; }
                    if (!any) return false;

                    // Re-enqueue para propagar los efectos del cambio
                    q.Enqueue((nx, ny));
                }
            }
        }
        return true;
    }

    // Devuelve la dirección opuesta a la dada
    private int Inverse(int dir) => dir switch
    {
        UP => DOWN,
        DOWN => UP,
        LEFT => RIGHT,
        RIGHT => LEFT,
        _ => 0
    };

    // Determina si la celda (x,y) está colapsada (exactamente un patrón permitido).
    // Si lo está, actualiza `_chosen` con el índice del patrón.
    private bool IsCollapsed(int x, int y)
    {
        int count = 0, last = -1;
        for (int p = 0; p < _catalog.PatternCount; p++)
        {
            if (_wave[x, y, p]) { count++; last = p; }
        }
        if (count == 1) _chosen[x, y] = last;
        return count == 1;
    }

    // Comprueba si todas las celdas están colapsadas
    private bool IsFullyCollapsed()
    {
        for (int y = 0; y < _H; y++)
            for (int x = 0; x < _W; x++)
                if (!IsCollapsed(x, y)) return false;
        return true;
    }
}
