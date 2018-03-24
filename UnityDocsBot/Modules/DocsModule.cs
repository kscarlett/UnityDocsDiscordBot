﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using UnityDocsBot.Services;

namespace UnityDocsBot.Modules
{
    public class DocsModule : ModuleBase<SocketCommandContext>
    {
        private readonly DocLookupService _docLookup;
        private readonly Dictionary<string, Dictionary<string, string[]>> _docs;
        private readonly string _currentVersion;
        private readonly List<CommandInfo> _commands;

        public DocsModule(DocLookupService docLookup, Dictionary<string, Dictionary<string, string[]>> docs, string currentVersion, CommandService service)
        {
            _docLookup = docLookup;
            _docs = docs;
            _currentVersion = currentVersion;
            _commands = service.Commands.ToList();
        }

        [Command("help"), Summary("Lists all bot commands.")]
        public async Task Help()
        {
            EmbedBuilder builder = new EmbedBuilder().WithAuthor("AVAILABLE COMMANDS");
            foreach (var command in _commands)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append(command.Name + " ");
                foreach (var commandParameter in command.Parameters)
                {
                    if (commandParameter.IsOptional)
                    {
                        sb.Append($"<OPTIONAL: {commandParameter.Name}> ");
                    }
                    else
                    {
                        sb.Append($"<{commandParameter.Name}> ");
                    }

                }
                builder.AddField(sb.ToString(), command.Summary);
            }
            builder.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl()).WithColor(Color.Green);
            await ReplyAsync("", embed: builder.Build());
        }

        [Command("about"), Summary("About this bot...")]
        public async Task Info()
        {
            SocketUser user = Context.Client.GetUser(223834565544902656);
            await ReplyAsync($"UNITY DOCS BOT V. {Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}, written by {user?.Username ?? "RubyNova"}#{user?.Discriminator ?? "0404"}. If I break, please let him know!");
        }

        [Command("docs", RunMode = RunMode.Async), Summary("Searches the Manual for the desired name.")]
        public async Task Docs(string name, string version = "")
        {
            Dictionary<string, string[]> docs;
            if (_docs.ContainsKey(version))
            {
                docs = _docs[version];
            }
            else
            {
                docs = _docs[_currentVersion];
            }
            if (docs.Keys.Any(x => x.ToUpper() == name.ToUpper()))
            {
                string key = docs.Keys.First(x => x.ToUpper() == name.ToUpper());
                EmbedBuilder builder = new EmbedBuilder().WithAuthor(new EmbedAuthorBuilder { Name = key })
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField("Description:", docs[key][0]).AddField("Full Reference:", docs[key][1])
                    .WithColor(Color.Green);
                await ReplyAsync("", embed: builder.Build());
            }
            else if (docs.Keys.Any(x => x.ToUpper().Contains($".{name.ToUpper()}")))
            {
                string key = docs.Keys.First(x => x.ToUpper().Contains($".{name.ToUpper()}"));
                EmbedBuilder builder = new EmbedBuilder().WithAuthor(new EmbedAuthorBuilder { Name = "(Probably meant) " + key })
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField("Description:", docs[key][0]).AddField("Full Reference:", docs[key][1])
                    .WithColor(Color.Green);
                await ReplyAsync("", embed: builder.Build());
            }
            else if (docs.Keys.Any(x => x.ToUpper().Contains($".{name.ToUpper()}.")))
            {
                string key = docs.Keys.First(x => x.ToUpper().Contains($".{name.ToUpper()}."));
                EmbedBuilder builder = new EmbedBuilder().WithAuthor(new EmbedAuthorBuilder { Name = "(Probably meant) " + key })
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField("Description:", docs[key][0]).AddField("Full Reference:", docs[key][1])
                    .WithColor(Color.Green);
                await ReplyAsync("", embed: builder.Build());
            }
            else if (docs.Keys.Any(x => x.ToUpper().Contains($"{name.ToUpper()}.")))
            {
                string key = docs.Keys.First(x => x.ToUpper().Contains($"{name.ToUpper()}."));
                EmbedBuilder builder = new EmbedBuilder().WithAuthor(new EmbedAuthorBuilder { Name = "(Probably meant) " + key })
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .AddField("Description:", docs[key][0]).AddField("Full Reference:", docs[key][1])
                    .WithColor(Color.Green);
                await ReplyAsync("", embed: builder.Build());
            }
            else
            {
                await ReplyAsync("No documentation found.");
            }
        }

        [Command("doctag"), Summary("Additional documentation-related tags that aren't in the script reference.")]
        public async Task DocTag(string tag)
        {
            if (_docLookup.TryGet(tag.ToUpper(), out string[] lookup))
            {
                await ReplyAsync($"<{lookup[0]}>");
            }
            else
            {
                await ReplyAsync("DocTag not found.");
            }
        }

        [Command("doctags"), Summary("Lists all available tags that have associated data.")]
        public async Task DocTags()
        {
            await ReplyAsync(_docLookup.GetAllTags().Aggregate((x, y) => $"{x}, {y}"));
        }

        [Command("versions"), Summary("Displays the current manuals supported by this bot.")]
        public async Task Versions()
        {
            EmbedBuilder builder = new EmbedBuilder().WithAuthor("CURRENT SUPPORTED VERSIONS");
            foreach (string version in _docs.Keys)
            {
                builder.AddField(version,
                    "https://docs.unity3d.com/" + version +
                    "/Documentation/ScriptReference/index.html");
            }
            builder.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl()).WithColor(Color.Green);
            await ReplyAsync("", embed: builder.Build());
        }
    }
}
