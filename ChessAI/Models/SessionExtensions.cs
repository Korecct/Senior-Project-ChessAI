using Newtonsoft.Json;

namespace ChessAI.Models
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            var json = JsonConvert.SerializeObject(value, _settings);
            session.SetString(key, json);
        }

        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var json = session.GetString(key);
            return json == null ? default : JsonConvert.DeserializeObject<T>(json, _settings);
        }
    }
}
