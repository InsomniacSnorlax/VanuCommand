using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace VanuCommand.Modules
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        #region Slash Commands
        [SlashCommand("schedulesetup", "Sets up the schedules")]
        public async Task SetupRankChannel()
        {
            var close = new EmbedBuilder
            {
                Title = "**Platoon Schedule**",
                Description = "Schedule of all platoons",
                Color = Color.Blue
            };

            var button = new ButtonBuilder()
            {
                Label = "Refresh",
                CustomId = "Refresh",
                Style = ButtonStyle.Primary
            };

            var button1 = new ButtonBuilder()
            {
                Label = "Aussie Dates",
                CustomId = "AusTime",
                Style = ButtonStyle.Secondary
            };

            var button2 = new ButtonBuilder()
            {
                Label = "Spreadsheet",
                Url = "https://docs.google.com/spreadsheets/d/1dcWVNu3HCDR_EoTF6-85_W56jG9_QxVdkQeRmlfZcoI/edit?usp=sharing",
                Style = ButtonStyle.Link
            };

            await ReplyAsync(embed: close.Build(), components: new ComponentBuilder().WithButton(button).WithButton(button1).WithButton(button2).Build());
        }
        #endregion
    }
}
