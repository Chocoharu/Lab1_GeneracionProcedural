using UnityEngine;
using UnityEngine.UI;
using System;

public class SudokuGenerator : MonoBehaviour
{
    private const int Size = 9;
    private int[,] board = new int[Size, Size];

    public GameObject cellPrefab;
    public Transform gridParent;

    void Start()
    {
        GenerateFullSolution();
        RemoveNumbers(40); // Por ejemplo, elimina 40 celdas
        DrawBoard();
    }

    // Genera una solución completa de Sudoku
    private bool GenerateFullSolution()
    {
        return FillBoard(0, 0);
    }


    // Rellena el tablero recursivamente
    private bool FillBoard(int row, int col)
    {
        if (row == Size)
            return true;
        if (col == Size)
            return FillBoard(row + 1, 0);

        var nums = GetShuffledNumbers();
        foreach (var num in nums)
        {
            if (IsSafe(row, col, num))
            {
                board[row, col] = num;
                if (FillBoard(row, col + 1))
                    return true;
                board[row, col] = 0;
            }
        }
        return false;
    }

    // Devuelve un array de números del 1 al 9 en orden aleatorio
    private int[] GetShuffledNumbers()
    {
        int[] nums = new int[Size];
        for (int i = 0; i < Size; i++) nums[i] = i + 1;
        System.Random rand = new System.Random();
        for (int i = 0; i < Size; i++)
        {
            int j = rand.Next(i, Size);
            (nums[i], nums[j]) = (nums[j], nums[i]);
        }
        return nums;
    }

    // Verifica si es seguro colocar un número en una posición dada
    private bool IsSafe(int row, int col, int num)
    {
        for (int i = 0; i < Size; i++)
        {
            if (board[row, i] == num || board[i, col] == num)
                return false;
        }
        int startRow = row / 3 * 3, startCol = col / 3 * 3;
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (board[startRow + i, startCol + j] == num)
                    return false;
        return true;
    }

    // Elimina números del tablero para crear el puzzle
    private void RemoveNumbers(int count)
    {
        System.Random rand = new System.Random();
        int removed = 0;
        while (removed < count)
        {
            int row = rand.Next(Size);
            int col = rand.Next(Size);
            if (board[row, col] != 0)
            {
                int backup = board[row, col];
                board[row, col] = 0;
                removed++;
            }
        }
    }
    
    private void DrawBoard()
    {
        float cellSize = 40f; // Ajusta según tu diseño
        float spacing = 2f;   // Espacio entre celdas
        for (int row = 0; row < Size; row++)
        {
            for (int col = 0; col < Size; col++)
            {
                GameObject cell = Instantiate(cellPrefab, gridParent);
                var rect = cell.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0, 1);
                rect.anchorMax = new Vector2(0, 1);
                rect.pivot = new Vector2(0, 1);
                rect.sizeDelta = new Vector2(cellSize, cellSize);
                rect.anchoredPosition = new Vector2(col * (cellSize + spacing), -row * (cellSize + spacing));

                // Busca el componente TextMeshProUGUI en el hijo
                var text = cell.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = board[row, col] == 0 ? "" : board[row, col].ToString();
                    text.alignment = TMPro.TextAlignmentOptions.Center;
                    text.fontSize = 28; // Ajusta el tamaño de fuente si es necesario
                }
            }
        }
    }

    public void Solver()
    {
        // Ejemplo de uso en SudokuGenerator
        var solver = new SudokuSolver(board);
        bool solved = solver.Solve();
        if (solved)
        {
            board = solver.GetSolution();
            DrawBoard();
        }
    }
}
