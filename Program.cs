using Discord.WebSocket;
using DiscordBot.Commands;

public class Program
{
    public static void Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        var commands = new List<Command>()
        {
            new Ping()
        };

        var bot = new DiscordSocketClient();

        bot.MessageReceived += HandleMessageAsync;

        await bot.LoginAsync(Discord.TokenType.Bot,"");

        await bot.StartAsync();

        Console.ReadLine();

        async Task HandleMessageAsync(SocketMessage arg)
        {
            arg.Channel.SendMessageAsync("Recieved");
            Console.WriteLine($"Message content{arg.Content}");
            // var words = arg.Content.Split(' ');
            // commands.Find(x => x.Alias == words[0]).Handle(bot, arg);
        }
    }
}


