using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwonsJukebox.Services
{
    internal class TokenService
    {
        public string Token { get; private set; }
        public TokenService() => Token = File.ReadAllText($@"D:\DiscordToken.txt");
    }
}
