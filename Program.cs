using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Settings.Configuration;
using StreamDeckLib;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using StreamDeckLib.Models;
using Microsoft.Extensions.Options;

namespace BluetoothController
{
    class Program
    {

        static async Task Main(string[] args)
        {





            using var config = StreamDeckLib.Config.ConfigurationBuilder.BuildDefaultConfiguration(args);

            var actionManagerLogger = config.LoggerFactory.CreateLogger<ActionManager>();


            using CommandLineApplication commandLineApplication = new();
            commandLineApplication.HelpOption();
            CommandOption<int> commandOption = commandLineApplication.Option<int>("-port|--port <PORT>", "The port the Elgato StreamDeck software is listening on", CommandOptionType.SingleValue);
            CommandOption<string> commandOption2 = commandLineApplication.Option<string>("-pluginUUID <UUID>", "The UUID that the Elgato StreamDeck software knows this plugin as.", CommandOptionType.SingleValue);
            CommandOption<string> commandOption3 = commandLineApplication.Option<string>("-registerEvent <REGEVENT>", "The registration event", CommandOptionType.SingleValue);
            CommandOption<string> commandOption4 = commandLineApplication.Option<string>("-info <INFO>", "Some information", CommandOptionType.SingleValue);
            commandLineApplication.Parse(args);
            StreamDeckToolkitOptions options = new()
            {
                Info = commandOption4.ParsedValue,
                PluginUUID = commandOption2.ParsedValue,
                Port = commandOption.ParsedValue,
                RegisterEvent = commandOption3.ParsedValue
            };


            var actionManager = new ActionManager(actionManagerLogger);
            var connectionManager = new ConnectionManager(new OptionsWrapper<StreamDeckToolkitOptions>(options), actionManager, config.LoggerFactory.CreateLogger<ConnectionManager>());


            await connectionManager.RegisterAllActions(typeof(Program).Assembly)
                                                         .StartAsync();

        }

    }
}
