using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace cuahang.Helpers // Bạn có thể đổi tên namespace cho đúng với dự án
{
    public static class SessionExtensions
    {
        // Hàm dùng để LƯU một đối tượng vào Session (Biến Object thành Json String)
        public static void SetJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Hàm dùng để ĐỌC một đối tượng từ Session (Biến Json String thành Object)
        public static T? GetJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}