using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace VanuCommand
{
    public class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Config.json")
            .Build();

            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) =>
            services
            .AddSingleton(config)
            .AddSingleton(x => new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = Discord.GatewayIntents.AllUnprivileged,
                LogGatewayIntentWarnings = false,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Debug,
            }))
            .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<InteractionHandler>()
            .AddSingleton(x => new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                DefaultRunMode = Discord.Commands.RunMode.Async
            })))
            .Build();

            await RunAsync(host);
            await Task.Delay(-1);
        }

        public async Task RunAsync(IHost host)
        {
            using IServiceScope serviceScope = host.Services.CreateScope();
            IServiceProvider provider = serviceScope.ServiceProvider;

            var commands = provider.GetRequiredService<InteractionService>();
            var _client = provider.GetRequiredService<DiscordSocketClient>();
            var config = provider.GetRequiredService<IConfigurationRoot>();

            await provider.GetRequiredService<InteractionHandler>().InititaliseAsync();

            var token = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("Config.json")).token;

            _client.Ready += async () =>
            {
                Console.WriteLine("Bot is ready");
                await commands.RegisterCommandsGloballyAsync();
            };

            await _client.LoginAsync(Discord.TokenType.Bot, token);
            await _client.StartAsync();
        }
    }

    public struct Configuration
    {
        public string token { get; set; }
        public string channel { get; set; }
        public string email { get; set; }

        public string id { get; set; }
    }

}