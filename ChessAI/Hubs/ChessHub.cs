using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ChessAI.Models;

namespace ChessAI.Hubs
{
    public class ChessHub : Hub
    {
        private readonly ILogger<ChessHub> _logger;

        public ChessHub(ILogger<ChessHub> logger)
        {
            _logger = logger;
        }

        private static readonly ConcurrentDictionary<string, GameLobby> GameLobbies = new ConcurrentDictionary<string, GameLobby>();

        public async Task CreateLobby()
        {
            var connectionId = Context.ConnectionId;
            string lobbyCode;
            do
            {
                lobbyCode = GenerateLobbyCode();
            } while (GameLobbies.ContainsKey(lobbyCode));

            var lobby = new GameLobby
            {
                LobbyCode = lobbyCode,
                HostConnectionId = connectionId
            };

            if (GameLobbies.TryAdd(lobbyCode, lobby))
            {
                _logger.LogInformation($"Lobby created with code {lobbyCode} by connection {connectionId}.");
                await Groups.AddToGroupAsync(connectionId, lobbyCode);
                await Clients.Caller.SendAsync("LobbyCreated", lobbyCode);
            }
            else
            {
                _logger.LogWarning($"Failed to create lobby with code {lobbyCode} by connection {connectionId}.");
                await Clients.Caller.SendAsync("LobbyCreationFailed", "Failed to create lobby.");
            }
        }

        public async Task JoinLobby(string lobbyCode)
        {
            var connectionId = Context.ConnectionId;

            if (GameLobbies.TryGetValue(lobbyCode, out var lobby))
            {
                if (lobby.GuestConnectionId == null)
                {
                    lobby.GuestConnectionId = connectionId;
                    _logger.LogInformation($"Connection {connectionId} joined lobby {lobbyCode} as guest.");
                    await Groups.AddToGroupAsync(connectionId, lobbyCode);
                    await Clients.Group(lobbyCode).SendAsync("StartGame", lobbyCode);
                }
                else
                {
                    _logger.LogWarning($"Connection {connectionId} failed to join lobby {lobbyCode} because it is full.");
                    await Clients.Caller.SendAsync("LobbyJoinFailed", "Lobby is full.");
                }
            }
            else
            {
                _logger.LogWarning($"Connection {connectionId} attempted to join non-existent lobby {lobbyCode}.");
                await Clients.Caller.SendAsync("LobbyJoinFailed", "Lobby not found.");
            }
        }

        public async Task ResignGame(string lobbyCode)
        {
            _logger.LogInformation($"Resignation received in lobby {lobbyCode} from connection {Context.ConnectionId}.");

            if (GameLobbies.TryGetValue(lobbyCode, out var lobby))
            {
                var game = lobby.Game;
                if (game.IsGameOver)
                {
                    _logger.LogWarning($"ResignGame called in lobby {lobbyCode}, but the game is already over.");
                    await Clients.Caller.SendAsync("InvalidMove", "The game is already over.");
                    return;
                }

                // Determine which player is resigning
                bool resigningIsWhite = Context.ConnectionId == lobby.HostConnectionId;
                game.IsGameOver = true;

                if (resigningIsWhite)
                {
                    game.GameResult = "Black wins by resignation.";
                }
                else
                {
                    game.GameResult = "White wins by resignation.";
                }

                // Update the game in the lobby
                lobby.Game = game;

                _logger.LogInformation($"Player {(resigningIsWhite ? "White" : "Black")} resigned in lobby {lobbyCode}. Result: {game.GameResult}");

                // Notify all players in the lobby
                await Clients.Group(lobbyCode).SendAsync("GameOver", game.GameResult);
            }
            else
            {
                _logger.LogWarning($"ResignGame called with invalid lobby code: {lobbyCode}");
                await Clients.Caller.SendAsync("InvalidMove", "Lobby not found.");
            }
        }

        public async Task SendMove(string lobbyCode, MoveRequest move)
        {
            _logger.LogInformation($"Move received in lobby {lobbyCode}: from ({move.FromRow}, {move.FromCol}) to ({move.ToRow}, {move.ToCol}).");
            if (GameLobbies.TryGetValue(lobbyCode, out var lobby))
            {
                var game = lobby.Game;
                var playerConnectionId = Context.ConnectionId;
                var isWhite = playerConnectionId == lobby.HostConnectionId;


                if (game.IsWhiteTurn != isWhite)
                {
                    _logger.LogWarning($"Invalid move by {(isWhite ? "White" : "Black")} in lobby {lobbyCode}: Not their turn.");
                    await Clients.Caller.SendAsync("InvalidMove", "It's not your turn.");
                    return;
                }

                var fromPosition = (move.FromRow, move.FromCol);
                var toPosition = (move.ToRow, move.ToCol);
                var piece = game.Board.Squares[move.FromRow][move.FromCol];

                if (piece == null || piece.IsWhite != isWhite)
                {
                    _logger.LogWarning($"Invalid move in lobby {lobbyCode}: No valid piece at ({move.FromRow}, {move.FromCol}) for {(isWhite ? "White" : "Black")}.");
                    await Clients.Caller.SendAsync("InvalidMove", "Invalid piece selected.");
                    return;
                }

                var validMoves = piece.GetValidMoves(game.Board);
                validMoves = validMoves.Where(m =>
                {
                    var boardClone = game.Board.Clone();
                    var pieceClone = boardClone.Squares[move.FromRow][move.FromCol];
                    var capturedPiece = boardClone.Squares[m.Row][m.Col];
                    boardClone.Squares[move.FromRow][move.FromCol] = null;
                    boardClone.Squares[m.Row][m.Col] = pieceClone;
                    pieceClone.Position = (m.Row, m.Col);

                    Piece enPassantCapturedPawn = null;
                    if (pieceClone is Pawn pawnClone && capturedPiece == null && m.Col != move.FromCol)
                    {
                        int capturedPawnRow = pieceClone.IsWhite ? m.Row + 1 : m.Row - 1;
                        enPassantCapturedPawn = boardClone.Squares[capturedPawnRow][m.Col];
                        boardClone.Squares[capturedPawnRow][m.Col] = null;
                    }

                    bool isInCheck = boardClone.IsKingInCheck(pieceClone.IsWhite);
                    return !isInCheck;
                }).ToList();

                if (validMoves.Any(m => m.Row == move.ToRow && m.Col == move.ToCol))
                {
                    bool isCapture = game.Board.Squares[move.ToRow][move.ToCol] != null;
                    bool isPromotion = false;
                    bool isCastle = false;
                    bool isEnPassantCapture = false;

                    if (piece is Pawn)
                    {
                        if (move.ToCol != move.FromCol && game.Board.Squares[move.ToRow][move.ToCol] == null)
                        {
                            isEnPassantCapture = true;
                        }
                    }

                    if (piece is King && Math.Abs(move.ToCol - move.FromCol) == 2)
                    {
                        isCastle = true;
                    }

                    if (piece is Pawn && ((piece.IsWhite && move.ToRow == 0) || (!piece.IsWhite && move.ToRow == 7)))
                    {
                        isPromotion = true;
                    }

                    var success = game.MakeMove(fromPosition, toPosition, _logger);
                    if (!success)
                    {
                        _logger.LogWarning($"Invalid move execution in lobby {lobbyCode}: Move from ({move.FromRow}, {move.FromCol}) to ({move.ToRow}, {move.ToCol}) failed.");
                        await Clients.Caller.SendAsync("InvalidMove", "Invalid move.");
                        return;
                    }

                    _logger.LogInformation($"Move executed in lobby {lobbyCode}: {(isWhite ? "White" : "Black")} moved from ({move.FromRow}, {move.FromCol}) to ({move.ToRow}, {move.ToCol}). Next turn: {(game.IsWhiteTurn ? "White" : "Black")}.");

                    var moveDetails = new MoveDetails
                    {
                        FromRow = move.FromRow,
                        FromCol = move.FromCol,
                        ToRow = move.ToRow,
                        ToCol = move.ToCol,
                        IsCapture = isCapture || isEnPassantCapture,
                        IsPromotion = isPromotion,
                        IsCastle = isCastle,
                        IsEnPassantCapture = isEnPassantCapture,
                        IsCheck = game.Board.IsKingInCheck(!isWhite),
                        IsCheckmate = game.IsGameOver && game.GameResult.Contains("wins"),
                        IsGameOver = game.IsGameOver,
                        GameResult = game.GameResult,
                        IsWhiteTurn = game.IsWhiteTurn,
                        IsWhiteKingInCheck = game.Board.IsKingInCheck(true),
                        IsBlackKingInCheck = game.Board.IsKingInCheck(false),
                        HalfMoveClock = game.HalfMoveClock,
                        FullMoveNumber = game.FullMoveNumber
                    };

                    await Clients.Group(lobbyCode).SendAsync("ReceiveMove", moveDetails);

                    if (game.IsGameOver)
                    {
                        _logger.LogInformation($"Game over in lobby {lobbyCode}: {game.GameResult}");
                        await Clients.Group(lobbyCode).SendAsync("GameOver", game.GameResult);
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid move in lobby {lobbyCode}: Move from ({move.FromRow}, {move.FromCol}) to ({move.ToRow}, {move.ToCol}) is not allowed.");
                    await Clients.Caller.SendAsync("InvalidMove", "Invalid move.");
                }
            }
        }

        public async Task GetValidMoves(string lobbyCode, int row, int col)
        {
            _logger.LogInformation($"GetValidMoves called in lobby {lobbyCode} for position ({row}, {col}).");
            if (GameLobbies.TryGetValue(lobbyCode, out var lobby))
            {
                var game = lobby.Game;
                var piece = game.Board.Squares[row][col];
                var playerConnectionId = Context.ConnectionId;
                var isWhite = playerConnectionId == lobby.HostConnectionId;

                if (piece == null || piece.IsWhite != isWhite || piece.IsWhite != game.IsWhiteTurn)
                {
                    _logger.LogInformation($"No valid moves for connection {playerConnectionId} in lobby {lobbyCode} at position ({row}, {col}).");
                    await Clients.Caller.SendAsync("ReceiveValidMoves", new List<PositionModel>());
                    return;
                }

                var validMoves = piece.GetValidMoves(game.Board);
                var safeMoves = validMoves.Where(move =>
                {
                    var boardClone = game.Board.Clone();
                    var pieceClone = boardClone.Squares[piece.Position.Row][piece.Position.Col];
                    var capturedPiece = boardClone.Squares[move.Row][move.Col];
                    boardClone.Squares[pieceClone.Position.Row][pieceClone.Position.Col] = null;
                    boardClone.Squares[move.Row][move.Col] = pieceClone;
                    pieceClone.Position = (move.Row, move.Col);

                    if (pieceClone is Pawn pawnClone)
                    {
                        if (capturedPiece == null && Math.Abs(move.Col - piece.Position.Col) == 1)
                        {
                            int capturedPawnRow = piece.IsWhite ? move.Row + 1 : move.Row - 1;
                            var enPassantPawn = boardClone.Squares[capturedPawnRow][move.Col];
                            if (enPassantPawn is Pawn enPassantPawnClone && enPassantPawnClone.EnPassantEligible)
                            {
                                boardClone.Squares[capturedPawnRow][move.Col] = null;
                            }
                        }
                    }

                    bool isInCheck = boardClone.IsKingInCheck(piece.IsWhite);
                    return !isInCheck;
                }).Select(m => new PositionModel { Row = m.Row, Col = m.Col }).ToList();

                _logger.LogInformation($"Valid moves for connection {playerConnectionId} in lobby {lobbyCode} at position ({row}, {col}): {string.Join(", ", safeMoves.Select(m => $"({m.Row}, {m.Col})"))}");
                await Clients.Caller.SendAsync("ReceiveValidMoves", safeMoves);
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            _logger.LogInformation($"Connection {connectionId} disconnected.");
            foreach (var kvp in GameLobbies)
            {
                var lobby = kvp.Value;
                if (lobby.HostConnectionId == connectionId || lobby.GuestConnectionId == connectionId)
                {
                    if (!lobby.Game.IsGameOver)
                    {
                        // Determine who disconnected
                        bool disconnectingIsWhite = connectionId == lobby.HostConnectionId;
                        string result = disconnectingIsWhite ? "Black wins by opponent leaving." : "White wins by opponent leaving.";

                        lobby.Game.IsGameOver = true;
                        lobby.Game.GameResult = result;

                        _logger.LogInformation($"Player {(disconnectingIsWhite ? "White" : "Black")} disconnected in lobby {lobby.LobbyCode}. Result: {result}");

                        // Notify the remaining player about the win due to disconnect
                        await Clients.Group(lobby.LobbyCode).SendAsync("GameOver", result);
                    }
                    else
                    {
                        _logger.LogInformation($"Player disconnected in lobby {lobby.LobbyCode}, but the game is already over.");

                        await Clients.Group(lobby.LobbyCode).SendAsync("OpponentDisconnected", "Opponent has disconnected, but the game has already concluded.");
                    }

                    // Remove the lobby from the active lobbies
                    await Groups.RemoveFromGroupAsync(connectionId, lobby.LobbyCode);
                    GameLobbies.TryRemove(kvp.Key, out _);
                    break;
                }
            }
            await base.OnDisconnectedAsync(exception);
        }


        private string GenerateLobbyCode()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        }
    }

    public class GameLobby
    {
        public string LobbyCode { get; set; }
        public string HostConnectionId { get; set; }
        public string GuestConnectionId { get; set; }
        public Game Game { get; set; } = new Game();
    }

    public class MoveRequest
    {
        public int FromRow { get; set; }
        public int FromCol { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }
    }

    public class PositionModel
    {
        public int Row { get; set; }
        public int Col { get; set; }
    }

    public class MoveDetails
    {
        public int FromRow { get; set; }
        public int FromCol { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }
        public bool IsCapture { get; set; }
        public bool IsPromotion { get; set; }
        public bool IsCastle { get; set; }
        public bool IsEnPassantCapture { get; set; }
        public bool IsCheck { get; set; }
        public bool IsCheckmate { get; set; }
        public bool IsGameOver { get; set; }
        public string GameResult { get; set; }
        public bool IsWhiteTurn { get; set; }
        public bool IsWhiteKingInCheck { get; set; }
        public bool IsBlackKingInCheck { get; set; }
        public int HalfMoveClock { get; set; }
        public int FullMoveNumber { get; set; }
    }
}
