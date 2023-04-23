using Discord.Commands;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiwonsJukebox.Modules
{
    public class JukeboxModule : ModuleBase<SocketCommandContext>
    {
        [Command("안녕")]
        public async Task HelloCommand()
        {
            await Context.Channel.SendMessageAsync("안녕 못해 임마");
        }
    }
}
