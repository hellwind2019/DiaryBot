using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DiaryBot;

public class UpdateBot
{
    public static async Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        var message = update.Message;
        var botToken = "5995864096:AAFvvRBUzfgmGuUeI0CMA10W1FMq2Ec72iQ";
        var channelName = "";
        var channelId = 1L;
        channelId = await Utils.GetChannelId(botToken, channelName);

        Console.WriteLine($"ID канала {channelName} = {channelId}");
    
        if (message.Text != null)
        {
            if (message.Text == "/start")
            {
                await botClient.SendTextMessageAsync(message.Chat.Id, "Bot started...");
                await botClient.SendTextMessageAsync(message.Chat.Id,
                    "Print your channel name like \"@channel_name\" ");
            }

            if (message.Text.Contains("@"))
            {
                
            }
        } 
  
    }

    
}