﻿using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MeepleBot.database;

namespace MeepleBot.commands;

public class ApplicationCommand : ApplicationCommandModule
{
    private static RealmDatabaseService _databaseService;
    public ApplicationCommand(RealmDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }
    [SlashCommand("apply", "Apply for a game using this command")]
    public static async Task Apply(
        InteractionContext context,
        [Option("username", "What your IGN is")]
        string username,
        [Option("game", "What game you want to be whitelisted on")] [Choice("Astroneer", "astroneer")]
        string game
    )
    {
        await context.DeferAsync();
        try
        {
            if (!await _databaseService.ApplicationExists(context.User.Id
                    .ToString())) // Checks if the application already exists
            {
                await _databaseService.CreateApplication(context.User.Id.ToString(), game, username);
            }
            else
            {
                var failEmbed = new DiscordEmbedBuilder()
                    .WithTitle("Application")
                    .WithDescription("You have already applied.")
                    .WithColor(DiscordColor.Red);
                await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(failEmbed));
                return;
            }

            var successEmbed = new DiscordEmbedBuilder()
                .WithTitle("Application")
                .WithDescription("You have successfully applied")
                .WithColor(DiscordColor.Blurple);
            await context.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(successEmbed));
            Logging.Logger.LogInfo(Logs.Command,
                $"{context.User.Username} ran the /apply command. \nParams: {username}, {game}");
        }
        catch (Exception ex)
        {
            Logging.Logger.LogError(Logs.Command, $"Error processing /apply command: {ex.Message}");
        }
    }
}