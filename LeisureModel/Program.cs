using System;
using System.Collections.Concurrent;
using System.Data;
using System.Runtime.CompilerServices;

namespace LeisureModel
{
    internal class Program
    {
        class Cell
        {
            public int kind = 0;
            public Cell() { }
        }
        class Piece
        {
            public int kind;
            public int w;
            public int h;
            public Piece(int kind, int w, int h)
            {
                this.kind = kind;
                this.w = w;
                this.h = h;
            }
        }
        class Field
        {
            public int size;
            public int bounds;
            public Cell[,] cells;

            public void Print()
            {
                for (int i = 0; i < this.size; i++)
                {
                    for (int j = 0; j < this.size; j++)
                        switch (cells[i, j].kind)
                        {
                            case 0:
                                Console.Write('0');
                                break;
                            case 1:
                                Console.Write('X');
                                break;
                            case 2:
                                Console.Write('H');
                                break;
                            case 3:
                                if (i == 0 && j == 0)
                                {
                                    Console.Write(i);
                                    break;
                                }
                                if (j == 0 || j == size - 1)
                                {
                                    Console.Write(i);
                                    break;
                                }
                                if (i == 0 || i == size - 1)
                                {
                                    Console.Write(j);
                                    break;
                                }
                                if (j == size - 1 && j == size - 1)
                                {
                                    Console.Write(j);
                                    break;
                                }
                                break;
                            default:
                                break;
                        }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            public bool CanPlaceOnCellByEmptiness(int kind, int i, int j)
            {
                if (this.cells[i, j].kind != 0)
                {
                    //Console.WriteLine("Cell [{1},{0}] is not empty", i, j);
                    return false;
                }

                return true;
            }

            public bool CanPlaceOnCellByNearCells(int kind, int i, int j)
            {

                if (this.cells[i + 1, j].kind != kind
                    &&
                    this.cells[i - 1, j].kind != kind
                    &&
                    this.cells[i, j + 1].kind != kind
                    &&
                    this.cells[i, j - 1].kind != kind)
                {
                   
                    return false;
                }

                return true;
            }

            public bool CanPlacePiece(Piece piece, int i, int j)
            {
                if (i > bounds || i < 1 || j > bounds || j < 1)
                {
                    //Console.WriteLine("Trying to place piece on cell out of field");
                    return false;
                }

                if (i + piece.h - 1 > bounds || j + piece.w - 1 > bounds)
                {
                    //Console.WriteLine("Piece is too big to be placed on this cell");
                    return false;
                }

                bool canPlace = true;

                for (int m = 0; m < piece.h; m++)
                {
                    for (int n = 0; n < piece.w; n++)
                    {
                        canPlace &= CanPlaceOnCellByEmptiness(piece.kind, i + m, j + n);
                        if (!canPlace)
                            return false;
                    }
                }

                canPlace = false;

                for (int m = 0; m < piece.h; m++)
                {
                    for (int n = 0; n < piece.w; n++)
                    {
                        canPlace |= CanPlaceOnCellByNearCells(piece.kind, i + m, j + n);
                        if (canPlace)
                            break;
                    }
                    if (canPlace)
                        break;
                }

                if (!canPlace)
                {
                    //Console.WriteLine("No similar cells nearby");
                    return false;
                }

                return true;
            }

            public void PlacePiece(Piece piece, int i, int j)
            {
                for (int m = 0; m < piece.h; m++)
                    for (int n = 0; n < piece.w; n++)
                        this.cells[i + m, j + n].kind = piece.kind;
            }

            public Field(int n)
            {
                this.size = n + 2;
                this.bounds = n;

                this.cells = new Cell[this.size, this.size];

                for (int i = 0; i < this.size; i++)
                    for (int j = 0; j < this.size; j++)
                        cells[i, j] = new Cell();

                for (int i = 0; i < this.size; i++)
                {
                    cells[i, 0].kind = 3;
                    cells[i, size - 1].kind = 3;

                    cells[0, i].kind = 3;
                    cells[size - 1, i].kind = 3;
                }

                cells[1, 1].kind = 1;
                cells[bounds, bounds].kind = 2;
            }
        }

        static void Main(string[] args)
        {
            Random rnd = new Random();
            int turn = 1;
            int i, j, firstPlayerScore = 0, secondPlayerScore = 0;
            bool isTurnPossible, isTurnProceed;
            byte impossibleTurnsInTheRow = 0;
            Field field = new Field(8);
            Piece firstPlayerPiece = new Piece(1, 0, 0);
            Piece secondPlayerPiece = new Piece(2, 0, 0);

            while (true)
            {
                if (impossibleTurnsInTheRow > 1)
                {
                    Console.WriteLine("Game is over");

                    foreach (Cell cell in field.cells)
                    {
                        if (cell.kind == 1)
                            firstPlayerScore++;
                        if (cell.kind == 2)
                            secondPlayerScore++;
                    }

                    Console.WriteLine("Scores: 1 - {0}, 2 - {1}", firstPlayerScore, secondPlayerScore);

                    if (firstPlayerScore > secondPlayerScore)
                        Console.WriteLine("First player win");
                    else if (firstPlayerScore < secondPlayerScore)
                        Console.WriteLine("Second player win");
                    else
                        Console.WriteLine("Draw");

                    return;
                }

                Console.WriteLine("Turn: {0}", turn);
                isTurnPossible = false;
                field.Print();

                if (turn % 2 == 0)
                {
                    Console.WriteLine("Player 2 turn");
                    secondPlayerPiece.w = rnd.Next(1, 6);
                    secondPlayerPiece.h = rnd.Next(1, 6);

                    Console.WriteLine("Piece is: w - {0}, h - {1}", secondPlayerPiece.w, secondPlayerPiece.h);
                    for (int m = 1; m < field.bounds; m++)
                    {
                        for (int n = 1; n < field.bounds; n++)
                        {
                            if (field.cells[m, n].kind == 0)
                                isTurnPossible |= field.CanPlacePiece(secondPlayerPiece, m, n);
                        }
                    }

                    if (isTurnPossible)
                    {
                        if (impossibleTurnsInTheRow > 0)
                            impossibleTurnsInTheRow--;
                        isTurnProceed = true;
                        while (isTurnProceed)
                        {
                            Console.WriteLine("X: ");
                            j = Convert.ToInt32(Console.ReadLine());

                            Console.WriteLine("Y: ");
                            i = Convert.ToInt32(Console.ReadLine());

                            if (field.CanPlacePiece(secondPlayerPiece, i, j))
                            {
                                field.PlacePiece(secondPlayerPiece, i, j);
                                isTurnProceed = false;
                            }
                            else
                            {
                                Console.WriteLine("Wrong place");
                            }
                        }
                    }
                    else
                    {
                        impossibleTurnsInTheRow++;
                        Console.WriteLine("Turn is unavailable");
                    }
                }
                else
                {
                    Console.WriteLine("Player 1 turn");
                    firstPlayerPiece.w = rnd.Next(1, 6);
                    firstPlayerPiece.h = rnd.Next(1, 6);

                    Console.WriteLine("Piece is: w - {0}, h - {1}", firstPlayerPiece.w, firstPlayerPiece.h);

                    for (int m = 1; m < field.bounds; m++)
                    { 
                        for (int n = 1; n < field.bounds; n++)
                        {
                            if (field.cells[m, n].kind == 0)
                                isTurnPossible |= field.CanPlacePiece(firstPlayerPiece, m, n);
                        }
                    }

                    if (isTurnPossible)
                    {
                        if (impossibleTurnsInTheRow > 0)
                            impossibleTurnsInTheRow--;
                        isTurnProceed = true;
                        while (isTurnProceed)
                        {
                            Console.WriteLine("X: ");
                            j = Convert.ToInt32(Console.ReadLine());

                            Console.WriteLine("Y: ");
                            i = Convert.ToInt32(Console.ReadLine());

                            if (field.CanPlacePiece(firstPlayerPiece, i, j))
                            {
                                field.PlacePiece(firstPlayerPiece, i, j);
                                isTurnProceed = false;
                            }
                            else
                            {
                                Console.WriteLine("Wrong place");
                            }
                        }
                    }
                    else
                    {
                        impossibleTurnsInTheRow++;
                        Console.WriteLine("Turn is unavailable");
                    }
                }

                turn++;
            }
        }
    }
}