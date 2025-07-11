﻿/*
 *                     GNU AFFERO GENERAL PUBLIC LICENSE
 *                       Version 3, 19 November 2007
 *  Copyright (C) 2007 Free Software Foundation, Inc. <https://fsf.org/>
 *  Everyone is permitted to copy and distribute verbatim copies
 *  of this license document, but changing it is not allowed.
 */

using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UVtools.Cmd.Symbols;
using UVtools.Core;
using UVtools.Core.FileFormats;
using UVtools.Core.Operations;

namespace UVtools.Cmd;

internal class Program
{
    internal const string EndOperationText = "Done in {0:F2}s";
    internal static OperationProgress Progress { get; } = new();
    internal static Stopwatch StopWatch { get; } = new();
    internal static bool Quiet { get; private set; }
    internal static bool NoProgress { get; private set; }
    internal static bool DummyMode { get; private set; }

    internal static string[] Args { get; private set; } = null!;

    public static async Task<int> Main(params string[] args)
    {

        Thread.CurrentThread.CurrentCulture = CoreSettings.OptimalCultureInfo;
        Args = args;

        var rootCommand = new RootCommand("MSLA/DLP, file analysis, repair, conversion and manipulation")
        {
            SetPropertiesCommand.CreateCommand(),
            RunCommand.CreateCommand(),
            ConvertCommand.CreateCommand(),
            ExtractCommand.CreateCommand(),
            CopyParametersCommand.CreateCommand(),
            SetThumbnailCommand.CreateCommand(),

            CompareCommand.CreateCommand(),

            PrintIssuesCommand.CreateCommand(),
            PrintPropertiesCommand.CreateCommand(),
            //PrintLayersCommand.CreateCommand(),
            PrintGCodeCommand.CreateCommand(),
            PrintMachinesCommand.CreateCommand(),
            PrintFormatsCommand.CreateCommand(),

            GlobalOptions.QuietOption,
            GlobalOptions.NoProgressOption,
            GlobalOptions.DummyOption,
            GlobalOptions.CoreVersionOption,
        };

        HandleGlobals();
        await rootCommand.Parse(args).InvokeAsync();

        return 1;

    }

    internal static void HandleGlobals()
    {
        foreach (var arg in Args)
        {
            if (arg[0] != '-') continue;

            if (GlobalOptions.QuietOption.Name == arg || GlobalOptions.QuietOption.Aliases.Any(alias => arg == alias))
            {
                Quiet = true;
                continue;
            }

            if (GlobalOptions.NoProgressOption.Name == arg || GlobalOptions.NoProgressOption.Aliases.Any(alias => arg == alias))
            {
                NoProgress = true;
                continue;
            }

            if (GlobalOptions.DummyOption.Name == arg || GlobalOptions.DummyOption.Aliases.Any(alias => arg == alias))
            {
                DummyMode = true;
                continue;
            }

            if (GlobalOptions.CoreVersionOption.Name == arg)
            {
                Console.WriteLine(About.VersionString);
                Environment.Exit(0);
            }
        }

        if (DummyMode)
        {
            Console.WriteLine("> Dummy mode active!");
        }
    }

    internal static FileFormat OpenInputFile(FileInfo inputFile, FileFormat.FileDecodeType decodeType = FileFormat.FileDecodeType.Full)
    {
        return ProgressBarWork($"Opening file {inputFile.Name}", () =>
        {
            var slicerFile = FileFormat.Open(inputFile.FullName, decodeType, Progress);
            if (slicerFile is null)
            {
                throw new IOException($"Invalid file: {inputFile.Name}");
            }

            return slicerFile;
        });
    }

    internal static void SaveFile(FileFormat slicerFile, FileInfo? outputFile) => SaveFile(slicerFile, outputFile?.FullName);

    internal static void SaveFile(FileFormat slicerFile, string? outputFile = null)
    {
        if (DummyMode) return;
        var fileName = outputFile is null ? slicerFile.Filename : Path.GetFileName(outputFile);
        ProgressBarWork($"Saving file {fileName}", () =>
        {
            slicerFile.SaveAs(outputFile, Progress);
        });
    }

    #region ProgressBar

    internal static T ProgressBarWork<T>(string title, Func<T> action)
    {
        T result;
        Progress.Title = title;

        StopWatch.Restart();
        Write($"{title}: ");
        if (Quiet || NoProgress)
        {
            result = action.Invoke();
        }
        else
        {
            using var progressBar = new ProgressBar();
            result = action.Invoke();
        }
        StopWatch.Stop();


        WriteLine(string.Format(EndOperationText, StopWatch.ElapsedMilliseconds / 1000.0));
        return result;
    }

    internal static void ProgressBarWork(string title, Action action)
    {
        ProgressBarWork(title, () =>
        {
            action.Invoke();
            return true;
        });
    }

    #endregion

    #region Write to console methods
    internal static void Write(object? obj)
    {
        if (Quiet) return;
        Console.Write(obj);
    }

    internal static void WriteLine(object? obj)
    {
        if (Quiet) return;
        Console.WriteLine(obj);
    }

    internal static void WriteWarning(object? obj)
    {
        if (Quiet) return;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(obj);
        Console.ResetColor();
    }

    internal static void WriteLineWarning(object? obj)
    {
        if (Quiet) return;

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(obj);
        Console.ResetColor();
    }

    internal static void WriteError(object? obj, bool exit = false)
    {
        if (Quiet)
        {
            if (exit) Environment.Exit(-1);
            return;
        }
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write(obj);
        Console.ResetColor();
        if (exit) Environment.Exit(-1);
    }

    internal static void WriteLineError(object? obj, bool exit = true, bool prependError = true)
    {
        if (Quiet)
        {
            if (exit) Environment.Exit(-1);
            return;
        }

        var str = obj?.ToString();
        if (str is not null && prependError && !str.StartsWith("Error:")) str = $"Error: {str}";

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(str);
        Console.ResetColor();
        if (exit) Environment.Exit(-1);
    }
    #endregion
}