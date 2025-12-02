using UnityEngine;

public static class MapLabeler
{
    public static int GetDifficulty(int[,] map)
    {
        int count = 0;
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                if (map[x, y] == 2) count++;

        if (count <= 2) return 0;     // fácil
        if (count <= 6) return 1;     // medio
        return 2;                     // difícil
    }
}