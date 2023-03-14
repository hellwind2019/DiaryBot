using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http;
using System.Threading.Tasks;
using DiaryBot;
using FireSharp.Config;
using FireSharp;
using FireSharp.Response;
using Newtonsoft.Json.Linq;

var botToken = "5995864096:AAFvvRBUzfgmGuUeI0CMA10W1FMq2Ec72iQ";
var client = new TelegramBotClient(botToken);
client.StartReceiving(Update, Error);

Console.ReadLine();

static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
{
    FirebaseConfig config = new FirebaseConfig();
    config.BasePath = "https://diarybot-36fce-default-rtdb.firebaseio.com";
    config.AuthSecret = "qVVVLDZUGu7ITL5z2Va0ibKPR3q5AnUgODwKanln";
    FirebaseClient firebaseClient = new FirebaseClient(config);
    
    var message = update.Message;
    var botToken = "5995864096:AAFvvRBUzfgmGuUeI0CMA10W1FMq2Ec72iQ";
    var isStartedField = "isStarted";
    var channelIDField = "channelID";
    var registeredField = "registered";
    
    
    if (message?.Text != null)
    {
        FirebaseResponse responseRegisterStatus = await firebaseClient.GetAsync($"{message.Chat.Id}/{registeredField}");
        var registerStatus = responseRegisterStatus.ResultAs<string>();
        if (registerStatus!="true")
        {
             FirebaseResponse response = await firebaseClient.GetAsync($"{message.Chat.Id}/{isStartedField}");
        var responseString = response.ResultAs<string>();
        if (message.Text == "/start")
        {
            if (responseString == "true")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, $"Welcome back, {message.Chat.FirstName}");
            }
            else
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Bot started...");
                await firebaseClient.SetAsync($"{message.Chat.Id}/{isStartedField}", "true");
                await botClient.SendTextMessageAsync(message.Chat.Id, "Send your channel name like\"@channel_name\"");
            }
            
        }
        if (message.Text.Contains("@") && responseString == "true" )
        {
            var channelId = Utils.GetChannelId(botToken, message.Text).Result;
            Console.WriteLine(channelId);
            await firebaseClient.SetAsync($"{message.Chat.Id}/{channelIDField}", channelId);
            await botClient.SendTextMessageAsync(message.Chat.Id, "Channel registered");
            await botClient.SendTextMessageAsync(message.Chat.Id, "Now add bot to this channel as admin");
            
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { "Done" },
            })
            {
                ResizeKeyboard = true
            };
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Click this button, when you added bot to channel",
                replyMarkup: replyKeyboardMarkup,
                cancellationToken: token
                );
        }

        if (message.Text == "Done")
        {
            FirebaseResponse firebaseResponse = await firebaseClient.GetAsync($"{message.Chat.Id}/{channelIDField}");
            var responseId = firebaseResponse.ResultAs<long>();
            
            ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
            {
                new KeyboardButton[] { "Yes, i see", "No, i can't see the message"},
            })
            {
                ResizeKeyboard = true
            };
            
            await botClient.SendTextMessageAsync(message.Chat.Id, "Now bot will send a message to you channel", replyMarkup: new ReplyKeyboardRemove());
            await botClient.SendTextMessageAsync(responseId, "Test message in channel");
            await botClient.SendTextMessageAsync(message.Chat.Id, "See the message from the bot in the channel?", replyMarkup: replyKeyboardMarkup);
        }

        if (message.Text == "Yes, i see")
        {
            await botClient.SendTextMessageAsync(message.Chat.Id, "Congratulations! Now your channel is registered", replyMarkup: new ReplyKeyboardRemove());
            await firebaseClient.SetAsync($"{message.Chat.Id}/{registeredField}", "true");
        }
        }
    } 
  
}
static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken agr3)
{
    Console.WriteLine(arg2);
    throw new NotImplementedException();
}


