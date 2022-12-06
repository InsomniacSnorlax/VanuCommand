using Discord.Interactions;
using Discord.WebSocket;
using Discord;
using System;
using System.Reflection;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4.Data;

namespace VanuCommand
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private readonly IServiceProvider _services;

        static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string AppplicationName = "Schedules";
        static readonly string SpreadsheetID = "1dcWVNu3HCDR_EoTF6-85_W56jG9_QxVdkQeRmlfZcoI";
        static readonly string sheet = "Schedule";
        static SheetsService service;
        private string[] Days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
        private List<DaySchedule> daySchedules = new List<DaySchedule>();
        public InteractionHandler(DiscordSocketClient client, InteractionService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
        }

        public async Task InititaliseAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.InteractionCreated += HandleInteraction;

            _client.ButtonExecuted += SelectButton;

            _client.ModalSubmitted += SelectModal;
        }

        private async Task SelectModal(SocketModal arg)
        {

        }


        private async Task SelectButton(SocketMessageComponent arg)
        {
            switch (arg.Data.CustomId)
            {
                case "Refresh":
                    await GoogleSheets(arg);
                    break;
                case "AusTime":
                    await GoogleSheets(arg, true);
                    break;
            }
        }

        private async Task HandleInteraction(SocketInteraction arg)
        {
            try
            {
                var ctx = new SocketInteractionContext(_client, arg);
                await _commands.ExecuteCommandAsync(ctx, _services);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task GoogleSheets(SocketMessageComponent arg, bool AussieDate = false)
        {
            const int offset = 57600;

            GoogleCredential credential;

            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Scopes);
            }

            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = AppplicationName
            });
            var range = $"{sheet}!B3:J12";
            var request = service.Spreadsheets.Values.Get(SpreadsheetID, range);

            var reponse = request.Execute();
            var values = reponse.Values;

            var embed = new EmbedBuilder()
            .WithTitle("Outfit Schedules")
            .WithDescription("List of schedules from every outfit participating")
            .WithColor(new Discord.Color(68, 14, 98));

            var year = values[0][0].ToString();

            int numberOfOutfits = values.Count;
            daySchedules = new List<DaySchedule>();
            int dayIndex = 1;
            Console.WriteLine(values[3].Count);
            foreach (string day in Days)
            {
                Console.WriteLine(day);
                DaySchedule daySchedule = new DaySchedule() { Schedules = new List<string>() };
                daySchedule.Day = !AussieDate ? day : Days[dayIndex % Days.Length];

                string DM = values[0][dayIndex].ToString();
                string date = DM.Split(" ")[0];
                int month = ReturnMonth(DM);

                for (int rows = 2; rows < numberOfOutfits; rows++)
                {

                    if (String.IsNullOrEmpty(values[rows][dayIndex].ToString())) continue;


                    var time = values[rows][dayIndex].ToString();

                    var timeSplit = time.ToString().Split("-");

                    var minSecSplit1 = timeSplit[0].Split(":");

                    var minSecSplit2 = timeSplit[1].Split(":");

                    var outfit = values[rows][0].ToString();

                    //string unix1 = $"{year}-{month}-{date} {timeSplit[0]}";
                    DateTime dateTime1 = new DateTime(int.Parse(year), month, int.Parse(date), int.Parse(minSecSplit1[0]), int.Parse(minSecSplit1[1]), 0);
                    DateTime dateTime2 = new DateTime(int.Parse(year), month, int.Parse(date), int.Parse(minSecSplit2[0]), int.Parse(minSecSplit2[1]), 0);
                    var debug = ($"{outfit} <t:{ToUnixTimestamp(dateTime1) + offset}:t> - <t:{ToUnixTimestamp(dateTime2) + offset}:t> {rows} {dayIndex}");
                    Console.WriteLine(debug);
                    daySchedule.Schedules.Add($"{outfit} <t:{ToUnixTimestamp(dateTime1) + offset}:t> - <t:{ToUnixTimestamp(dateTime2) + offset}:t>");
                }
                dayIndex++;
                daySchedules.Add(daySchedule);
            }

            foreach (DaySchedule Days in daySchedules)
            {
                string description = "";
                foreach (string outfits in Days.Schedules)
                {
                    description += $"{outfits} \n";
                }

                embed.AddField(Days.Day, description, inline: true);
            }

            if (!AussieDate)
                await arg.UpdateAsync(x => x.Embed = embed.Build());
            else
                await arg.RespondAsync("This just shift the dates by one so its less aids", embed: embed.Build(), ephemeral: true);

        }

        #region Helper 
        public int ReturnMonth(string value)
        {
            if (value.Contains("Jan")) return 1;
            if (value.Contains("Feb")) return 2;
            if (value.Contains("Mar")) return 3;
            if (value.Contains("Apr")) return 4;
            if (value.Contains("May")) return 5;
            if (value.Contains("Jun")) return 6;
            if (value.Contains("Jul")) return 7;
            if (value.Contains("Aug")) return 8;
            if (value.Contains("Sep")) return 9;
            if (value.Contains("Oct")) return 10;
            if (value.Contains("Nov")) return 11;
            if (value.Contains("Dec")) return 12;

            return 0;
        }

        public static int ToUnixTimestamp(DateTime value)
        {
            return (int)Math.Truncate((value.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }
    }

    public struct DaySchedule
    {
        public string Day;
        public string Date;
        public List<string> Schedules;
    }
    #endregion
}
