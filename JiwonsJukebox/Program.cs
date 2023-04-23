using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using JiwonsJukebox.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace JiwonsJukebox
{
    internal class Program
    {
        // 봇 클라이언트
        private readonly DiscordSocketClient _client;
        // 명령어 수신 클라이언트
        private readonly CommandService _commands;
        private readonly CommandHandler _handler;
        private readonly IServiceProvider _services;

        private TokenService _tokenService = new TokenService();

        static void Main(string[] args)
        {
            new Program().BotMain().GetAwaiter().GetResult();
        }

        public Program()
        {

            
            // * GatewayIntents 필수임.
            _client = new DiscordSocketClient(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.All })
            {
            };
            
            _commands = new CommandService(new CommandServiceConfig())
            {
            };

            // 로그 수신시 로그 출력 함수에서 출력되도록 설정
            _client.Log += OnClientLogReceived;
            _commands.Log += OnClientLogReceived;

            // 슬래시 명령
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;
        }

        private async Task Client_Ready()
        {
            var guild = _client.GetGuild(1099630896052715540);
            // Next, lets create our slash command builder. This is like the embed builder but for slash commands.
            var guildCommand = new SlashCommandBuilder();

            // Note: Names have to be all lowercase and match the regular expression ^[\w-]{3,32}$
            guildCommand.WithName("first-command");

            // Descriptions can have a max length of 100.
            guildCommand.WithDescription("This is my first guild slash command!");

            // Let's do our global command
            var globalCommand = new SlashCommandBuilder();
            globalCommand.WithName("first-global-command");
            globalCommand.WithDescription("This is my first global slash command");

            try
            {
                // Now that we have our builder, we can call the CreateApplicationCommandAsync method to make our slash command.
                await guild.CreateApplicationCommandAsync(guildCommand.Build());

                // With global commands we don't need the guild.
                await _client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
                // Using the ready event is a simple implementation for the sake of the example. Suitable for testing and development.
                // For a production bot, it is recommended to only run the CreateGlobalApplicationCommandAsync() once for each command.
            }
            catch (ApplicationCommandException exception)
            {
                // If our command was invalid, we should catch an ApplicationCommandException. This exception contains the path of the error as well as the error message. You can serialize the Error field in the exception to get a visual of where your error is.
                // var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);

                // You can send this error somewhere or just print it to the console, for this example we're just going to print it.
                // Console.WriteLine(json);
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            await command.RespondAsync($"You executed {command.Data.Name}");

        }

        /// <summary>
        /// 봇의 진입점
        /// </summary>
        private async Task BotMain()
        {
            try
            {
                // 메세지 수신
                await InitCommands();

                // 봇의 토큰을 사용해 서버에 로그인
                await _client.LoginAsync(TokenType.Bot, _tokenService.Token);

                // 봇이 이벤트를 수신하기 시작
                await _client.StartAsync();

                // 봇이 종료되지 않도록 블로킹
                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

        }

        private async Task InitCommands()
        {
            _client.MessageReceived += OnClientMessage;
        }

        /// <summary>
        /// 메세지 수신
        /// </summary>
        /// <param name="arg">메세지</param>
        /// <returns></returns>
        private async Task OnClientMessage(SocketMessage arg)
        {
            // 수신한 메세지가 사용자가 보낸게 아닐 때 취소
            var message = arg as SocketUserMessage;
            if (message == null)
            {
                return;
            }


            int position = 0;
            // 메세지 앞에 !가 달려있지 않고, 자신이 호출된게 아니거다 다른 봇이 호출했다면 취소
            if (!(message.HasCharPrefix('!', ref position) ||
                message.HasMentionPrefix(_client.CurrentUser, ref position)) ||
                message.Author.IsBot)
            {
                return;
            }

            // 수신된 메세지에 대한 Context 생성
            var context = new SocketCommandContext(_client, message);

            // 수신된 명령어를 다시 보낸다.
            await context.Channel.SendMessageAsync("명령어 수신됨 - " + message.Content);

        }



        /// <summary>
        /// 로그 수신
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        private Task OnClientLogReceived(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}
