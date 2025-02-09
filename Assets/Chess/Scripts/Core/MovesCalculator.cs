using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chess.Scripts.Core
{
    public class MovesCalculator : MonoBehaviour
    {
        private Transform selectedPiece;
        private GameObject tile;
        private ChessPlayerPlacementHandler selectedHandler;
        private ChessBoardPlacementHandler chessBoard;
        private Dictionary<Vector2Int, string> piecePositions = new Dictionary<Vector2Int, string>();

        void Start()
        {
            PopulatePiecePositions();
            chessBoard = ChessBoardPlacementHandler.Instance;

        }

        void PopulatePiecePositions()
        {
            piecePositions.Clear();
            ChessPlayerPlacementHandler[] pieces = FindObjectsByType<ChessPlayerPlacementHandler>(FindObjectsSortMode.None);

            foreach (var piece in pieces)
            {
                Vector2Int pos = new Vector2Int(piece.row, piece.column);
                piecePositions[pos] = piece.tag;
            }
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                SelectPiece();
            }
        }

        void SelectPiece()
        {
            // Clear previous state
            piecePositions.Clear();
            chessBoard.ClearHighlights();

            // Update current board state
            PopulatePiecePositions();

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.GetComponent<SpriteRenderer>() != null)
            {
                selectedPiece = hit.transform;
                selectedHandler = selectedPiece.GetComponent<ChessPlayerPlacementHandler>();

                if (selectedHandler != null)
                {
                    // Remove the currently selected piece from positions
                    Vector2Int currentPos = new Vector2Int(selectedHandler.row, selectedHandler.column);
                    if (piecePositions.ContainsKey(currentPos))
                    {
                        piecePositions.Remove(currentPos);
                    }

                    HighlightValidMoves(selectedPiece.tag, selectedHandler.row, selectedHandler.column);
                }
            }
        }


        void HighlightValidMoves(string pieceTag, int row, int column)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            string pieceType = pieceTag.Replace("Black", "").Replace("White", "");

            // Store current piece position
            Vector2Int currentPos = new Vector2Int(row, column);
            string currentPieceTag = null;
            if (piecePositions.ContainsKey(currentPos))
            {
                currentPieceTag = piecePositions[currentPos];
                piecePositions.Remove(currentPos);
            }

            // Calculate moves based on piece type
            switch (pieceType)
            {
                case "Pawn":
                    moves = GetPawnMoves(row, column);
                    break;
                case "Knight":
                    moves = GetKnightMoves(row, column);
                    break;
                case "Bishop":
                    moves = GetBishopMoves(row, column);
                    break;
                case "Rook":
                    moves = GetRookMoves(row, column);
                    break;
                case "Queen":
                    moves = GetQueenMoves(row, column);
                    break;
                case "King":
                    moves = GetKingMoves(row, column);
                    break;
            }

            // Restore the piece position
            if (currentPieceTag != null)
            {
                piecePositions[currentPos] = currentPieceTag;
            }
        }

        //Pawn Movment Calculation
        List<Vector2Int> GetPawnMoves(int row, int column)
        {
            List<Vector2Int> moves = new List<Vector2Int>();

            bool isWhite = selectedPiece.tag.StartsWith("White");
            int direction = isWhite ? -1 : 1; // White moves down, Black moves up
            int startRow = isWhite ? 6 : 1;   // White starts at row 6, Black starts at row 1

            //Check normal one-step move
            Vector2Int forwardMove = new Vector2Int(row + direction, column);
            if (IsValid(forwardMove.x, forwardMove.y) && !piecePositions.ContainsKey(forwardMove))
            {
                moves.Add(forwardMove);

                // Check two-step move if it's the first move
                Vector2Int doubleMove = new Vector2Int(row + 2 * direction, column);
                Vector2Int intermediateMove = new Vector2Int(row + direction, column); // Ensure the middle square is empty
                if (row == startRow && !piecePositions.ContainsKey(doubleMove) && !piecePositions.ContainsKey(intermediateMove))
                {
                    moves.Add(doubleMove);
                }
            }

            //Capture diagonally left and right
            Vector2Int leftCapture = new Vector2Int(row + direction, column - 1);
            Vector2Int rightCapture = new Vector2Int(row + direction, column + 1);

            if (IsValid(leftCapture.x, leftCapture.y) && piecePositions.ContainsKey(leftCapture) && !IsSameTeam(piecePositions[leftCapture], selectedPiece.tag))
            {
               HighlightRed(leftCapture.x, leftCapture.y);
            }

            if (IsValid(rightCapture.x, rightCapture.y) && piecePositions.ContainsKey(rightCapture) && !IsSameTeam(piecePositions[rightCapture], selectedPiece.tag))
            {
                HighlightRed(rightCapture.x, rightCapture.y);
            }

            for (int i = 0; i < moves.Count; i++)
            {
                chessBoard.Highlight(moves[i].x, moves[i].y);
            }
            return moves;
        }


        //Knight Movement Calculation
        List<Vector2Int> GetKnightMoves(int row, int column)
        {
            var moves = new List<Vector2Int>();
            int[] dRow = { -2, -1, 1, 2, 2, 1, -1, -2 };
            int[] dCol = { 1, 2, 2, 1, -1, -2, -2, -1 };

            for (int i = 0; i < 8; i++)
            {
                int newRow = row + dRow[i];
                int newCol = column + dCol[i];

                if (IsValid(newRow, newCol))
                {
                    Vector2Int newPos = new Vector2Int(newRow, newCol);
                    if (piecePositions.ContainsKey(newPos))
                    {
                        if (!IsSameTeam(piecePositions[newPos], selectedPiece.tag))
                        {
                            HighlightRed(newRow, newCol);
                        }
                    }
                    else
                    {
                        moves.Add(newPos);
                    }
                }
            }

            for (int i = 0; i < moves.Count; i++)
            {
                chessBoard.Highlight(moves[i].x, moves[i].y);
            }


            return moves;
        }


        //Bishop Movement Calculation
        List<Vector2Int> GetBishopMoves(int row, int column)
        {
            return GetDiagonalMoves(row, column);
        }

        //Rook Movement Calculation
        List<Vector2Int> GetRookMoves(int row, int column)
        {
            return GetStraightMoves(row, column);
        }
 
         //Queen Movement Calculation
        List<Vector2Int> GetQueenMoves(int row, int column)
        {
            List<Vector2Int> moves = GetStraightMoves(row, column);
            moves.AddRange(GetDiagonalMoves(row, column));

            return moves;
        }

        //King Movement Calculation
        List<Vector2Int> GetKingMoves(int row, int column)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            int[] dRow = { -1, -1, -1, 0, 1, 1, 1, 0 };
            int[] dCol = { -1, 0, 1, 1, 1, 0, -1, -1 };

            for (int i = 0; i < 8; i++)
            {
                int newRow = row + dRow[i], newCol = column + dCol[i];

                if (IsValid(newRow, newCol))
                {
                    Vector2Int newPosition = new Vector2Int(newRow, newCol);

                    // Check if the square is occupied
                    if (piecePositions.ContainsKey(newPosition))
                    {
                        // Ensure it's an opponent's piece before adding
                        if (!IsSameTeam(piecePositions[newPosition], selectedPiece.tag))
                        {
                            HighlightRed(newRow, newCol);
                        }
                    }
                    else
                    {
                        moves.Add(newPosition); // The square is empty, so it's a valid move
                    }
                }
            }
            for (int i = 0; i < moves.Count; i++)
            {
                chessBoard.Highlight(moves[i].x, moves[i].y);
            }

            return moves;
        }


        List<Vector2Int> GetStraightMoves(int row, int column)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            int[][] directions = new int[][]
            {
              new int[] {1, 0},  // Down
              new int[] {-1, 0}, // Up
              new int[] {0, 1},  // Right
              new int[] {0, -1}  // Left
            };

            foreach (var dir in directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    int newRow = row + dir[0] * i;
                    int newCol = column + dir[1] * i;
                    Vector2Int newPos = new Vector2Int(newRow, newCol);

                    if (!IsValid(newRow, newCol)) break;

                    if (piecePositions.ContainsKey(newPos))
                    {
                        // If there's an enemy piece, allow capturing
                        if (!IsSameTeam(piecePositions[newPos], selectedPiece.tag))
                        {
                            HighlightRed(newRow, newCol);
                        }

                        // Stop at the first piece (can't jump over)
                        break;
                    }

                    moves.Add(newPos);
                }
            }

            for (int i = 0; i < moves.Count; i++)
            {
                chessBoard.Highlight(moves[i].x, moves[i].y);
            }

            for (int i = 0; i < moves.Count; i++)
            {
                chessBoard.Highlight(moves[i].x, moves[i].y);
            }

            return moves;
        }


        List<Vector2Int> GetDiagonalMoves(int row, int column)
        {
            List<Vector2Int> moves = new List<Vector2Int>();
            int[][] directions = new int[][]
            {
               new int[] {1, 1},   // Bottom-right
               new int[] {1, -1},  // Bottom-left
               new int[] {-1, 1},  // Top-right
               new int[] {-1, -1}  // Top-left
            };

            foreach (var dir in directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    int newRow = row + dir[0] * i;
                    int newCol = column + dir[1] * i;
                    Vector2Int newPos = new Vector2Int(newRow, newCol);

                    if (!IsValid(newRow, newCol)) break;

                    if (piecePositions.ContainsKey(newPos))
                    {
                        // Allow capture if the piece is an opponent
                        if (!IsSameTeam(piecePositions[newPos], selectedPiece.tag))
                        {
                            HighlightRed(newRow, newCol);
                        }


                        break; // Stop at first encountered piece
                    }

                    moves.Add(newPos);
                }
            }
            for (int i = 0; i < moves.Count; i++)
            {
                chessBoard.Highlight(moves[i].x, moves[i].y);
            }


            return moves;
        }


        bool IsValid(int row, int column)
        {
            return row >= 0 && row < 8 && column >= 0 && column < 8;
        }

        bool IsSameTeam(string piece1, string piece2)
        {
            if (string.IsNullOrEmpty(piece1) || string.IsNullOrEmpty(piece2))
                return false; // If either piece is null or empty, assume they're not the same team.

            bool isPiece1White = piece1.StartsWith("White");
            bool isPiece2White = piece2.StartsWith("White");


            return isPiece1White == isPiece2White; // Return true if both are the same color.
        }

        private void HighlightRed(int i, int j)
        {
            tile = chessBoard.GetTile(i, j);
            if (tile != null)
            {
                SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.red;
                }
            }
        }
    }
}

