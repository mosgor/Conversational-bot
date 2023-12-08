using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

string token = System.IO.File.ReadAllText("./PrivateData/TOKEN.txt");
var botClient = new TelegramBotClient(token);

using CancellationTokenSource cts = new();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

Console.ReadLine();

cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Message is not { } message)
        return;
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;

    int chance = Globals.rnd.Next(1, 100);

    string log = "";

    foreach(string a in Globals.messages){
        log += a + " ";
    }

    readMessages(message.Chat.Id.ToString());

    Globals.messages.Add(messageText);

    writeMessage(message.Chat.Id.ToString());

    Console.WriteLine(chance + " " + log);

    // Echo received message text
    if (chance > 60)
    {
        Message sentMessage = await botClient.SendTextMessageAsync(
            chatId: chatId,
            text: Globals.messages[Globals.rnd.Next(0, Globals.messages.Count - 1)],
            cancellationToken: cancellationToken);
    }
    else return;
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

void writeMessage(string chatID)
{
    string data = JsonConvert.SerializeObject(Globals.messages);
    System.IO.File.WriteAllText("./PrivateData/" + chatID + ".json", data);
}

void readMessages(string chatID)
{
    if (!System.IO.File.Exists("./PrivateData/" + chatID + ".json"))
    {
        System.IO.File.Create("./PrivateData/" + chatID + ".json");
    }
    string json = System.IO.File.ReadAllText("./PrivateData/" + chatID + ".json");
    if (json == "") {
        json = "[]";
    }
    Globals.messages = JsonConvert.DeserializeObject<List<string>>(json);
}

public static class Globals
{
    public static Random rnd = new Random();
    public static List<string> messages = new List<string>();
}
