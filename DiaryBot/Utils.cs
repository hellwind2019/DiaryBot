using Newtonsoft.Json.Linq;

namespace DiaryBot;

public class Utils
{
    public static async Task<long> GetChannelId(string botToken, string channelName)
    {
        var channelid = -1L;
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync($"https://api.telegram.org/bot{botToken}/getChat?chat_id={channelName}");
        var responseContent = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(responseContent);
        if (json.ContainsKey("result"))
        {
            channelid =  json["result"]["id"].Value<long>();
            return channelid;
        }
        else
        {
            Console.WriteLine("Ошибка при получении ID канала");
            return -1;
        }
        
    }
}