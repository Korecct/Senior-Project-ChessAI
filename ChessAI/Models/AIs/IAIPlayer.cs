using static ChessAI.Controllers.HomeController;

namespace ChessAI.Models.AIs
{
    public interface IAIPlayer
    {
        (PositionModel From, PositionModel To) GetNextMove(Game game);

        string Name { get; }
    }
}