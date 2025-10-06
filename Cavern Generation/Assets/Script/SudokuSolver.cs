using System;
using System.Collections.Generic;
using UnityEngine;

public class SudokuSolver
{
    private const int Size = 9;
    private int[,] board;
    private bool[,] fixedCells;

    public SudokuSolver(int[,] initialBoard)
    {
        board = (int[,])initialBoard.Clone();
        fixedCells = new bool[Size, Size];
        for (int i = 0; i < Size; i++)
            for (int j = 0; j < Size; j++)
                fixedCells[i, j] = board[i, j] != 0;
    }

    public bool Solve(int maxIterations = 100000, double initialTemp = 2.0, double coolingRate = 0.999)
    {
        System.Random rand = new System.Random();

        // Rellenar aleatoriamente las celdas vacías en cada bloque 3x3
        for (int blockRow = 0; blockRow < 3; blockRow++)
        {
            for (int blockCol = 0; blockCol < 3; blockCol++)
            {
                bool[] present = new bool[Size + 1];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if (board[blockRow * 3 + i, blockCol * 3 + j] != 0)
                            present[board[blockRow * 3 + i, blockCol * 3 + j]] = true;

                var missing = new List<int>();
                for (int n = 1; n <= Size; n++)
                    if (!present[n]) missing.Add(n);

                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        if (board[blockRow * 3 + i, blockCol * 3 + j] == 0)
                        {
                            int idx = rand.Next(missing.Count);
                            board[blockRow * 3 + i, blockCol * 3 + j] = missing[idx];
                            missing.RemoveAt(idx);
                        }
            }
        }

        // Recoge las posiciones editables por bloque
        var editableCells = new List<(int, int)>[9];
        for (int b = 0; b < 9; b++)
        {
            editableCells[b] = new List<(int, int)>();
        }

        for (int r = 0; r < Size; r++)
            for (int c = 0; c < Size; c++)
                if (!fixedCells[r, c])
                    editableCells[(r / 3) * 3 + (c / 3)].Add((r, c));

        

        double temp = initialTemp;
        int currentEnergy = Energy();
        for (int iter = 0; iter < maxIterations && currentEnergy > 0; iter++)
        {
            int block;
            do { block = rand.Next(9); } while (editableCells[block].Count < 2);

            int idx1 = rand.Next(editableCells[block].Count);
            int idx2;
            do { idx2 = rand.Next(editableCells[block].Count); } while (idx2 == idx1);

            var (r1, c1) = editableCells[block][idx1];
            var (r2, c2) = editableCells[block][idx2];

            int tmp = board[r1, c1];
            board[r1, c1] = board[r2, c2];
            board[r2, c2] = tmp;

            int newEnergy = Energy();
            int delta = newEnergy - currentEnergy;

            if (delta < 0 || rand.NextDouble() < Math.Exp(-delta / temp))
            {
                currentEnergy = newEnergy;
            }
            else
            {
                board[r2, c2] = board[r1, c1];
                board[r1, c1] = tmp;
            }

            temp *= coolingRate;
        }

        return currentEnergy == 0;
    }

    int Energy()
    {
        int conflicts = 0;
        for (int i = 0; i < Size; i++)
        {
            var rowCount = new int[Size + 1];
            var colCount = new int[Size + 1];
            for (int j = 0; j < Size; j++)
            {
                rowCount[board[i, j]]++;
                colCount[board[j, i]]++;
            }
            for (int n = 1; n <= Size; n++)
            {
                if (rowCount[n] > 1) conflicts += rowCount[n] - 1;
                if (colCount[n] > 1) conflicts += colCount[n] - 1;
            }
        }
        return conflicts;
    }

    public int[,] GetSolution()
    {
        return (int[,])board.Clone();
    }
}