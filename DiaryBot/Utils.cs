using FireSharp;
using FireSharp.Config;
using FireSharp.Response;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Timers;
using Telegram.Bot.Types.ReplyMarkups;
using Timer = System.Timers.Timer;

namespace DiaryBot;

public class Utils
{
    
    public static async Task<long> GetChannelId(string botToken, string channelName)
    {
        var channelid = -1L;
        using var httpClient = new HttpClient();
        var response =
            await httpClient.GetAsync($"https://api.telegram.org/bot{botToken}/getChat?chat_id={channelName}");
        var responseContent = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(responseContent);
        if (json.ContainsKey("result"))
        {
            channelid = json["result"]["id"].Value<long>();
            return channelid;
        }

        Console.WriteLine("Ошибка при получении ID канала");
        return -1;
    }

    public static FirebaseClient GetFirebaseClient()
    {
        var config = new FirebaseConfig();
        config.BasePath = "https://diarybot-36fce-default-rtdb.firebaseio.com";
        config.AuthSecret = "qVVVLDZUGu7ITL5z2Va0ibKPR3q5AnUgODwKanln";
        var firebaseClient = new FirebaseClient(config);
        return firebaseClient;
    }

    public static string GetBotToken()
    {
        return "5995864096:AAFvvRBUzfgmGuUeI0CMA10W1FMq2Ec72iQ";
    }

    public static async Task<string> GetRegisterStatus(FirebaseClient firebaseClient, Message message)
    {
        var registeredField = "registered";
        var responseRegisterStatus =
            await firebaseClient.GetAsync($"Users/{message.Chat.Id}/{registeredField}");
        var registerStatus = responseRegisterStatus.ResultAs<string>();
        return registerStatus;
    }
    public static async Task<string> GetUserField(long userId, string field)
    {
        var firebaseClient = GetFirebaseClient();
        var responseRegisterStatus =
            await firebaseClient.GetAsync($"Users/{userId}/{field}");
        var registerStatus = responseRegisterStatus.ResultAs<string>();
        return registerStatus;
    }
    public static async Task<string> GetStartStatus(FirebaseClient firebaseClient, Message message)
    {
        string isStartedField = "isStarted";;
        var response = await firebaseClient.GetAsync($"Users/{message.Chat.Id}/{isStartedField}");
        var responseString = response.ResultAs<string>();
        return responseString;
    }

    public static async Task SetUserField(long userId,string field, dynamic data)
    {
        var firebaseClient = GetFirebaseClient();
        await firebaseClient.SetAsync($"Users/{userId}/{field}", data);
    }
    public static  void SetTimerToAllUsers(FirebaseClient firebaseClient, TelegramBotClient botClient)
    {
        const string isPostedTodayField = "isPostedToday";
        
        FirebaseResponse response = firebaseClient.Get("Users/");
        Dictionary<string, User> getUsers = response.ResultAs<Dictionary<string, User>>();
        var timer = new Timer(1000 * 60 * 60);
        timer.AutoReset = true;
        timer.Enabled = true;
        timer.Elapsed += async (sender, e) =>
        {
            if (DateTime.Now.Hour == 21)
            {
                foreach (var get in getUsers)
                {
                    if (GetUserField(long.Parse(get.Key), isPostedTodayField).Result == "false")
                    {
                        ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                        {
                            new KeyboardButton[] { "Конечно, пора подвести итоги дня", "Напомни через час" }
                        })
                        {
                            ResizeKeyboard = true
                        };
                        await botClient.SendTextMessageAsync(get.Key, "Готов писать пост?",replyMarkup: replyKeyboardMarkup);
                    }
                    
                }
            }
            if (DateTime.Now.Hour == 3)
            {
                foreach (var get in getUsers)
                {
                    await SetUserField(long.Parse(get.Key), isPostedTodayField, false);
                }
            }
        };
       
    }
    public static async void SetBotCommands(TelegramBotClient client)
    {
        BotCommand botCommand = new BotCommand{Command = "write_post", Description = "Написать пост в каннал"};
        IEnumerable<BotCommand> s = new[] {botCommand};
        await client.SetMyCommandsAsync(s);
    }

    public static async void IncreaseDayWithBot(long userId)
    {
        const string daysWithBotField = "daysWithBot";
        var currentDaysWithBot = await GetUserField(userId, daysWithBotField);
        await SetUserField(userId, daysWithBotField, currentDaysWithBot + 1);
    }
    public static string FormatPost(long userID, string postText)
    {
        const string daysWithBotField = "daysWithBot";
        string date = DateTime.Now.ToShortDateString();
        string emodjiDay = GetNumberEmoji(int.Parse(GetUserField(userID, daysWithBotField).Result));
        return $"{date}   #️⃣{emodjiDay}\n \n" +
               $"{postText}" +
               $"\n \n✅Написано с помощью DiaryBot🤖";
    }

    public static string GetNumberEmoji(int number)
    {
        var numsArray = SplitToDigits(number);
        var outputString = "";
        for (int i = 0; i < numsArray.Length; i++)
        {
            switch (numsArray[i])
            {
                case 1 : outputString += "1️⃣"; break;
                case 2 : outputString += "2️⃣"; break;
                case 3 : outputString += "3️⃣"; break;
                case 4 : outputString += "4️⃣"; break;
                case 5 : outputString += "5️⃣"; break;
                case 6 : outputString += "6️⃣"; break;
                case 7 : outputString += "7️⃣"; break; 
                case 8 : outputString += "8️⃣"; break; 
                case 9 : outputString += "9️⃣"; break;
                case 0 : outputString += "0️⃣"; break;
            }
        }

        return outputString;
    }
    /// <summary>
    /// Разбивка числа на цифры
    /// </summary>
    /// <param name="number">Исходное число</param>
    /// <returns>Массив однозначных чисел</returns>
    static int[] SplitToDigits(int number)
    {
        number = Math.Abs(number);
        //Если число меньше 10
        if (number < 10)
        {
            return new int[] { number };
        }
        //Результирующий массив размером в количество цифр в числе
        var result = new int[(int)Math.Log10(number) + 1];
        for (int i = 0; i < result.Length; i++)
        {
            //Последняя цифра числа как остаток от деления на 10
            result[result.Length - i - 1] = number % 10;
            //уменьшаем исходное число в 10 раз
            number /= 10;
        }
        return result;
    }
}