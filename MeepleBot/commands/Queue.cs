﻿using System.Text;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MeepleBot.database;
using MeepleBot.objects;

namespace MeepleBot.commands;

public class QueueCommand : ApplicationCommandModule
{
    private readonly RealmDatabaseService _databaseService;

    public QueueCommand(RealmDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [SlashCommand("queue", "Returns the queue for a game")]
    public async Task Queue(
        InteractionContext context,
        [Option("game", "The game you want to view the queue of")] [Choice("Astroneer", "astroneer")]
        string game
    )
    {
        var hasRole = context.Member.Roles.Any(role => role.Name.Equals("bot"));
        if (!hasRole)
        {
            var failedEmbed = new DiscordEmbedBuilder()
                .WithTitle("Permissions")
                .WithDescription("You can't do this lol")
                .WithColor(DiscordColor.Red);
            await context.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(failedEmbed));
            return;
        }
        await context.DeferAsync(ephemeral: true);
        IQueryable<ApplicationObject> applications;
        try
        {
            applications = await _databaseService.GetApplications(game);
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
            return;
        }

        var responseBuilder = new StringBuilder();
        foreach (var application in applications)
        {
            responseBuilder.AppendLine(
                $"<@{application.DiscordId}> applied at <t:{Convert.ToInt64(application.Time) / 1000}:t>```{application.Username}```"); // <t: x :t> is discord timestamp, this accounts for different timezones
        }

        var successEmbed = new DiscordEmbedBuilder()
            .WithTitle($"Queue for {game}")
            .WithDescription(responseBuilder.ToString())
            .WithColor(DiscordColor.Blurple);
        await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(successEmbed));
        Logging.Logger.LogInfo(Logs.Discord, $"{context.User.Username} ran the /queue command. \nParams: {game}");
    }
}