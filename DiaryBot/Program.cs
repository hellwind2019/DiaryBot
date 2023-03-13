using Telegram.Bot;
using Telegram.Bot.Types;


var client = new TelegramBotClient("5856433122:AAFbKRNQ3Sb1FKUxTYCOVBB-AwCSlcwLCcE");
client.StartReceiving(Update, Error);


Console.ReadLine();

static Task Error(ITelegramBotClient arg1, Exception arg2, CancellationToken agr3)
{
    throw new NotImplementedException();
}

async static Task Update(ITelegramBotClient botClient, Update update, CancellationToken token)
{
    var message = update.Message;
    if (message.Text != null)
    {
        Console.WriteLine($"{message.Chat.FirstName}     |   {message.Text}   ");
        if (message.Text.ToLower().Contains("hello"))
        {
           await botClient.SendTextMessageAsync(message.Chat.Id, "Здоровее видали");
           return;
        }
    }
    if (message.Photo != null)
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Лучше документом");
        return;
    }
    if (message.Photo != null)
    {
        await botClient.SendTextMessageAsync(message.Chat.Id, "Ща погодь, сделаю лучше");
        return;
    }
}