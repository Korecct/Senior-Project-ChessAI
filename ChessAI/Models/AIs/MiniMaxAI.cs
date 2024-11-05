﻿using System;
using System.Collections.Generic;
using System.Linq;
using ChessAI.Models;
using ChessAI.Controllers;
using Microsoft.Extensions.Logging;
using static ChessAI.Controllers.HomeController;

namespace ChessAI.Models.AIs
{
    public class MinimaxAI : IAIPlayer
    {
        private const int MaxDepth = 3;  // Depth of the search tree for Minimax

        // Positional value arrays for each piece, adjusted for Black's perspective
        private static readonly int[,] PawnPositionValues =
        {
            { 0, 0, 0, 0, 0, 0, 0, 0 },
            { 5, -5, -10, 0, 0, -10, -5, 5 },
            { 5, 10, 10, -20, -20, 10, 10, 5 },
            { 0, 0, 0, 20, 20, 0, 0, 0 },
            { 5, 5, 10, 25, 25, 10, 5, 5 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 0, 0, 0, 0, 0, 0, 0, 0 }
        };

        private static readonly int[,] KnightPositionValues =
        {
            { -50, -40, -30, -30, -30, -30, -40, -50 },
            { -40, -20, 0, 5, 5, 0, -20, -40 },
            { -30, 5, 10, 15, 15, 10, 5, -30 },
            { -30, 0, 15, 20, 20, 15, 0, -30 },
            { -30, 5, 15, 20, 20, 15, 5, -30 },
            { -30, 0, 10, 15, 15, 10, 0, -30 },
            { -40, -20, 0, 5, 5, 0, -20, -40 },
            { -50, -40, -30, -30, -30, -30, -40, -50 }
        };

        private static readonly int[,] BishopPositionValues =
        {
            { -20, -10, -10, -10, -10, -10, -10, -20 },
            { -10, 0, 5, 10, 10, 5, 0, -10 },
            { -10, 5, 10, 15, 15, 10, 5, -10 },
            { -10, 10, 15, 20, 20, 15, 10, -10 },
            { -10, 5, 10, 15, 15, 10, 5, -10 },
            { -10, 0, 5, 10, 10, 5, 0, -10 },
            { -20, -10, -10, -10, -10, -10, -10, -20 },
            { -20, -10, -10, -10, -10, -10, -10, -20 }
        };

        private static readonly int[,] RookPositionValues =
        {
            { 0, 0, 0, 5, 5, 0, 0, 0 },
            { 0, 0, 0, 10, 10, 0, 0, 0 },
            { 0, 0, 5, 10, 10, 5, 0, 0 },
            { 0, 0, 10, 15, 15, 10, 0, 0 },
            { 0, 0, 10, 15, 15, 10, 0, 0 },
            { 0, 0, 5, 10, 10, 5, 0, 0 },
            { 0, 0, 0, 10, 10, 0, 0, 0 },
            { 0, 0, 0, 5, 5, 0, 0, 0 }
        };

        private static readonly int[,] QueenPositionValues =
        {
            { -20, -10, -10, -5, -5, -10, -10, -20 },
            { -10, 0, 5, 0, 0, 5, 0, -10 },
            { -10, 5, 10, 5, 5, 10, 5, -10 },
            { -5, 0, 5, 10, 10, 5, 0, -5 },
            { -5, 0, 5, 10, 10, 5, 0, -5 },
            { -10, 5, 10, 5, 5, 10, 5, -10 },
            { -10, 0, 5, 0, 0, 5, 0, -10 },
            { -20, -10, -10, -5, -5, -10, -10, -20 }
        };

        private static readonly int[,] KingPositionValues =
        {
            { 20, 20, 10, 0, 0, 10, 20, 20 },
            { 10, 10, 0, 0, 0, 0, 10, 10 },
            { -10, -20, -30, -30, -30, -30, -20, -10 },
            { -20, -30, -40, -40, -40, -40, -30, -20 },
            { -30, -40, -50, -50, -50, -50, -40, -30 },
            { -30, -40, -50, -50, -50, -50, -40, -30 },
            { -30, -40, -50, -50, -50, -50, -40, -30 },
            { -30, -40, -50, -50, -50, -50, -40, -30 }
        };

        public string Name => "Minimax AI";

        public (PositionModel From, PositionModel To) GetNextMove(Game game)
        {
            var board = game.Board;
            var isWhiteTurn = game.IsWhiteTurn;

            // Call Minimax algorithm to get the best move
            var bestMove = GetBestMove(game, MaxDepth, isWhiteTurn, int.MinValue, int.MaxValue);

            return bestMove;
        }

        // Minimax algorithm with alpha-beta pruning
        private (PositionModel From, PositionModel To) GetBestMove(Game game, int depth, bool isWhiteTurn, int alpha, int beta)
        {
            var board = game.Board;

            // Get all pieces belonging to the current player
            var aiPieces = board.Squares.SelectMany(row => row)
                                       .Where(piece => piece != null && piece.IsWhite == isWhiteTurn)
                                       .ToList();

            int bestScore = isWhiteTurn ? int.MinValue : int.MaxValue;
            (PositionModel From, PositionModel To) bestMove = (null, null);

            foreach (var piece in aiPieces)
            {
                var validMoves = piece.GetValidMoves(board).ToList();

                foreach (var move in validMoves)
                {
                    // Simulate the move
                    var (fromRow, fromCol) = piece.Position;
                    var (toRow, toCol) = move;

                    var capturedPiece = board.Squares[toRow][toCol];
                    board.Squares[fromRow][fromCol] = null;
                    board.Squares[toRow][toCol] = piece;
                    piece.Position = move;

                    // Recursively evaluate the position
                    int score = Minimax(game, depth - 1, !isWhiteTurn, alpha, beta);

                    // Undo the move
                    board.Squares[fromRow][fromCol] = piece;
                    board.Squares[toRow][toCol] = capturedPiece;
                    piece.Position = (fromRow, fromCol);

                    // If it's White's turn, maximize the score, otherwise minimize it
                    if (isWhiteTurn)
                    {
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMove = (new PositionModel { Row = fromRow, Col = fromCol }, new PositionModel { Row = toRow, Col = toCol });
                        }
                        alpha = Math.Max(alpha, score);
                    }
                    else
                    {
                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestMove = (new PositionModel { Row = fromRow, Col = fromCol }, new PositionModel { Row = toRow, Col = toCol });
                        }
                        beta = Math.Min(beta, score);
                    }

                    // Alpha-Beta Pruning
                    if (beta <= alpha)
                    {
                        break; // Prune the search tree
                    }
                }
            }

            return bestMove;
        }

        // Minimax evaluation function
        private int Minimax(Game game, int depth, bool isWhiteTurn, int alpha, int beta)
        {
            var board = game.Board;

            // Base case: if we have reached the maximum depth or the game is over
            if (depth == 0 || game.IsGameOver)
            {
                return EvaluateBoard(board, isWhiteTurn, game);
            }

            // Recursively evaluate the position for all valid moves
            var aiPieces = board.Squares.SelectMany(row => row)
                                       .Where(piece => piece != null && piece.IsWhite == isWhiteTurn)
                                       .ToList();

            int bestScore = isWhiteTurn ? int.MinValue : int.MaxValue;

            foreach (var piece in aiPieces)
            {
                var validMoves = piece.GetValidMoves(board).ToList();

                foreach (var move in validMoves)
                {
                    // Simulate the move
                    var (fromRow, fromCol) = piece.Position;
                    var (toRow, toCol) = move;

                    var capturedPiece = board.Squares[toRow][toCol];
                    board.Squares[fromRow][fromCol] = null;
                    board.Squares[toRow][toCol] = piece;
                    piece.Position = move;

                    // Recursively evaluate
                    int score = Minimax(game, depth - 1, !isWhiteTurn, alpha, beta);

                    // Undo the move
                    board.Squares[fromRow][fromCol] = piece;
                    board.Squares[toRow][toCol] = capturedPiece;
                    piece.Position = (fromRow, fromCol);

                    if (isWhiteTurn)
                    {
                        bestScore = Math.Max(bestScore, score);
                        alpha = Math.Max(alpha, bestScore);
                    }
                    else
                    {
                        bestScore = Math.Min(bestScore, score);
                        beta = Math.Min(beta, bestScore);
                    }

                    // Alpha-Beta Pruning
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
            }

            return bestScore;
        }

        // Basic evaluation function that evaluates the board based on piece values and positional values
        private int EvaluateBoard(Board board, bool isWhiteTurn, Game game)
        {
            // If the game is over, return a very high value for a win or a very low value for a loss
            if (game.IsGameOver)
            {
                if (game.GameResult.Contains("Checkmate"))
                {
                    return isWhiteTurn ? int.MaxValue : int.MinValue;  // Checkmate: a win for the current player
                }
                else if (game.GameResult.Contains("Draw"))
                {
                    return 0;  // Draw
                }
            }

            int evaluation = 0;

            // Define piece values
            var pieceValues = new Dictionary<Type, int>
            {
                { typeof(Pawn), 100 },
                { typeof(Knight), 320 },
                { typeof(Bishop), 330 },
                { typeof(Rook), 500 },
                { typeof(Queen), 900 },
                { typeof(King), 20000 }
            };

            // Define positional values for each piece type
            var piecePositionValues = new Dictionary<Type, int[,]>
            {
                { typeof(Pawn), PawnPositionValues },
                { typeof(Knight), KnightPositionValues },
                { typeof(Bishop), BishopPositionValues },
                { typeof(Rook), RookPositionValues },
                { typeof(Queen), QueenPositionValues },
                { typeof(King), KingPositionValues }
            };

            // Evaluate the board based on material balance and positional value
            foreach (var row in board.Squares)
            {
                foreach (var piece in row)
                {
                    if (piece != null)
                    {
                        int value = pieceValues[piece.GetType()];
                        int positionValue = piecePositionValues[piece.GetType()][piece.Position.Row, piece.Position.Col];

                        // Add positional value to the material value for the piece
                        int totalValue = value + positionValue;

                        // Adjust for AI's or opponent's pieces
                        if (piece.IsWhite == isWhiteTurn)
                        {
                            evaluation += totalValue;  // Add score for AI's pieces
                        }
                        else
                        {
                            evaluation -= totalValue;  // Subtract score for opponent's pieces
                        }
                    }
                }
            }

            return evaluation;
        }
    }
}
