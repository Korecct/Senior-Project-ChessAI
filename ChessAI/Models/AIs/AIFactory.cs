using System.Reflection;

namespace ChessAI.Models.AIs
{
    public static class AIFactory
    {
        private static readonly List<IAIPlayer> _aiPlayers = [];

        static AIFactory()
        {
            LoadAIPlayers();
        }

        private static void LoadAIPlayers()
        {
            var aiInterface = typeof(IAIPlayer);
            var aiTypes = Assembly.GetExecutingAssembly().GetTypes()
                                   .Where(t => aiInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            foreach (var type in aiTypes)
            {
                if (Activator.CreateInstance(type) is IAIPlayer aiPlayer)
                {
                    _aiPlayers.Add(aiPlayer);
                }
            }
        }

        public static IEnumerable<IAIPlayer> GetAllAIPlayers()
        {
            return _aiPlayers;
        }

        public static IAIPlayer? GetAIByName(string name)
        {
            return _aiPlayers.FirstOrDefault(ai => ai.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}