using System;
using System.Collections.Generic;

/// Implementación del Wave Function Collapse con:
/// - Selección por mínima entropía (Shannon) usando pesos del catálogo
/// - Muestreo ponderado (probabilidades)
/// - Propagación de restricciones hasta converger o fallar
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

        // Inicializar wave: todo permitido al inicio
        for (int y = 0; y < _H; y++)
            for (int x = 0; x < _W; x++)
                for (int p = 0; p < catalog.PatternCount; p++)
                    _wave[x, y, p] = true;

        // Pesos
        double[] wn = catalog.GetWeightsNormalized();
        for (int p = 0; p < wn.Length; p++)
        {
            _weights[p] = wn[p];
            _weightLog[p] = wn[p] > 0 ? wn[p] * Math.Log(wn[p]) : 0.0;
        }
    }

    public bool Run(out int[,] patternGrid)
    {
        // Bucle principal: seleccionar celda con mínima entropía, colapsar y propagar
        while (true)
        {
            var next = FindMinEntropyCell(out double entropy);
            if (next == null)
            {
                // No hay celdas con superposición: todo colapsado
                patternGrid = (int[,])_chosen.Clone();
                return true;
            }

            (int x, int y) = next.Value;

            // Muestrear un patrón ponderado por los pesos restantes en esa celda
            int picked = SamplePattern(x, y);
            if (picked < 0)
            {
                patternGrid = null;
                return false; // contradicción (sin patrones posibles)
            }

            // Banear todos los demás patrones en (x,y)
            for (int p = 0; p < _catalog.PatternCount; p++)
                if (p != picked && _wave[x, y, p])
                    Ban(x, y, p);

            _chosen[x, y] = picked;

            // Propagar restricciones
            if (!Propagate())
            {
                patternGrid = null;
                return false;
            }

            // Continuar hasta colapsar todo
            if (IsFullyCollapsed())
            {
                patternGrid = (int[,])_chosen.Clone();
                return true;
            }
        }
    }

    // ---------- Núcleo WFC ----------
    private (int x, int y)? FindMinEntropyCell(out double bestEntropy)
    {
        bestEntropy = double.MaxValue;
        (int x, int y)? best = null;

        for (int y = 0; y < _H; y++)
        {
            for (int x = 0; x < _W; x++)
            {
                if (IsCollapsed(x, y)) continue;

                // Calcular entropía de la celda
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

                if (count == 0) return (x, y); // sin opciones -> entropía mínima/contradicción

                double H = Math.Log(sumW) - (sumW > 0 ? sumWLog / sumW : 0.0);
                // desempate aleatorio ligero
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

    private int SamplePattern(int x, int y)
    {
        // Construir distribución de probabilidad restringida a patrones aún válidos
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
        // Debería haber retornado, pero por redondeo:
        for (int p = _catalog.PatternCount - 1; p >= 0; p--)
            if (_wave[x, y, p]) return p;

        return -1;
    }

    private void Ban(int x, int y, int p)
    {
        _wave[x, y, p] = false;
    }

    private bool Propagate()
    {
        var q = new Queue<(int x, int y)>();
        // Inicial: encolar todas las celdas
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

                // Para cada patrón q en el vecino, verificar si sigue teniendo soporte desde (x,y)
                for (int qpat = 0; qpat < _catalog.PatternCount; qpat++)
                {
                    if (!_wave[nx, ny, qpat]) continue;

                    // Debe existir al menos un patrón p en (x,y) compatible con qpat según dir inversa
                    bool supported = false;
                    int invDir = Inverse(dir);
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
                    // Si el vecino quedó sin opciones, contradicción
                    bool any = false;
                    for (int qp = 0; qp < _catalog.PatternCount; qp++)
                        if (_wave[nx, ny, qp]) { any = true; break; }
                    if (!any) return false;

                    q.Enqueue((nx, ny));
                }
            }
        }
        return true;
    }

    private int Inverse(int dir) => dir switch
    {
        UP => DOWN,
        DOWN => UP,
        LEFT => RIGHT,
        RIGHT => LEFT,
        _ => 0
    };

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

    private bool IsFullyCollapsed()
    {
        for (int y = 0; y < _H; y++)
            for (int x = 0; x < _W; x++)
                if (!IsCollapsed(x, y)) return false;
        return true;
    }
}
