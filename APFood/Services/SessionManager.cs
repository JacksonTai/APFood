using System.Text.Json;

namespace APFood.Services
{
    public class SessionManager(IHttpContextAccessor httpContextAccessor)
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public void Set<T>(string key, T value)
        {
            if (_httpContextAccessor.HttpContext?.Session != null)
            {
                var serializedObject = JsonSerializer.Serialize(value);
                _httpContextAccessor.HttpContext.Session.SetString(key, serializedObject);
            }
        }

        public T? Get<T>(string key)
        {
            if (_httpContextAccessor.HttpContext?.Session != null)
            {
                var serializedObject = _httpContextAccessor.HttpContext.Session.GetString(key);
                if (serializedObject != null)
                {
                    return JsonSerializer.Deserialize<T>(serializedObject);
                }
            }
            return default;
        }

        public void Remove(string key)
        {
            if (_httpContextAccessor.HttpContext?.Session != null)
            {
                _httpContextAccessor.HttpContext.Session.Remove(key);
            }
        }   
    }
}
