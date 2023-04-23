using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JiwonsJukebox.Services;
using Microsoft.Extensions.DependencyInjection;

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

        }

        /// <summary>
        /// 봇의 진입점
        /// </summary>
        private async Task BotMain()
        {
            try
            {
                // 메세지 수신
                //await InitCommands();

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
