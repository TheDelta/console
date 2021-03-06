﻿using Popcron.Console;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

[Category("Built in commands")]
public class CommandsBuiltin
{
    private static string Indent = "    ";

    [Command("info", "Prints system information.")]
    public static string PrintSystemInfo()
    {
        string deviceName = SystemInfo.deviceName;
        string deviceModel = SystemInfo.deviceModel;
        DeviceType deviceType = SystemInfo.deviceType;
        string id = SystemInfo.deviceUniqueIdentifier;

        string os = SystemInfo.operatingSystem;
        OperatingSystemFamily osFamily = SystemInfo.operatingSystemFamily;
        string ram = (SystemInfo.systemMemorySize / 1024f) + " GiB";

        string cpuName = SystemInfo.processorType;
        int cpuCount = SystemInfo.processorCount;
        int cpuFrequency = SystemInfo.processorFrequency;

        GraphicsDeviceType gpuType = SystemInfo.graphicsDeviceType;
        string gpuName = SystemInfo.graphicsDeviceName;
        string gpuVendor = SystemInfo.graphicsDeviceVendor;
        string gpuRam = (SystemInfo.graphicsMemorySize / 1024f) + " GiB";

        int padding = 23;
        StringBuilder text = new StringBuilder();
        text.AppendLine("<b>Device</b>");
        text.AppendLine("\t<b>OS</b>".PadRight(padding) + os + "(" + osFamily + ")");
        text.AppendLine("\t<b>RAM</b>".PadRight(padding) + ram);
        text.AppendLine("\t<b>Name</b>".PadRight(padding) + deviceName);
        text.AppendLine("\t<b>Model</b>".PadRight(padding) + deviceModel);
        text.AppendLine("\t<b>Type</b>".PadRight(padding) + deviceType);
        text.AppendLine("\t<b>Unique ID</b>".PadRight(padding) + id);

        text.AppendLine("<b>CPU</b>");
        text.AppendLine("\t<b>Name</b>".PadRight(padding) + cpuName);
        text.AppendLine("\t<b>Processors</b>".PadRight(padding) + cpuCount);
        text.AppendLine("\t<b>Frequency</b>".PadRight(padding) + cpuFrequency);

        text.AppendLine("<b>GPU</b>");
        text.AppendLine("\t<b>Name</b>".PadRight(padding) + gpuName);
        text.AppendLine("\t<b>Type</b>".PadRight(padding) + gpuType);
        text.AppendLine("\t<b>Vendor</b>".PadRight(padding) + gpuVendor);
        text.AppendLine("\t<b>Memory</b>".PadRight(padding) + gpuRam);
        return text.ToString();
    }

    [Command("show fps")]
    public static bool ShowFPS
    {
        get
        {
            string key = $"{Application.companyName}.{Application.productName}.Popcron.Console.ShowFPS";
            return PlayerPrefs.GetInt(key, 0) == 1;
        }
        set
        {
            string key = $"{Application.companyName}.{Application.productName}.Popcron.Console.ShowFPS";
            PlayerPrefs.SetInt(key, value ? 1 : 0);
        }
    }

    [Command("clear", "Clears the console window.")]
    public static void ClearConsole()
    {
        Console.Clear();
        Debug.ClearDeveloperConsole();
    }

    [Command("owners", "Lists all instance command owners.")]
    public static string Owners()
    {
        StringBuilder text = new StringBuilder();
        foreach (Owner owner in Parser.Owners)
        {
            Console.Print(Indent + owner.id + " = " + owner.owner);
            foreach (Owner.OwnerMember method in owner.methods)
            {
                string parameters = "";
                text.AppendLine(Indent + Indent + method.name + parameters);
            }

            foreach (Owner.OwnerMember property in owner.properties)
            {
                string extra = "";
                if (property.canSetProperty)
                {
                    extra += " [value]";
                }

                text.AppendLine(Indent + Indent + property.name + extra);
            }

            foreach (Owner.OwnerMember field in owner.fields)
            {
                text.AppendLine(Indent + Indent + field.name + " = " + field.Value);
            }
        }
        return text.ToString();
    }

    [Command("converters", "Lists all type converters.")]
    public static string Converters()
    {
        StringBuilder text = new StringBuilder();
        foreach (Converter converter in Converter.Converters)
        {
            text.AppendLine(Indent + converter.GetType() + " (" + converter.Type + ")");
        }

        return text.ToString();
    }

    [Command("help", "Outputs a list of all commands")]
    public static string Help()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("All commands registered: ");
        foreach (Category category in Library.Categories)
        {
            builder.AppendLine(Indent + category.Name);
            foreach (Command command in category.Commands)
            {
                string text = string.Join("/", command.Names);
                if (command.Member is MethodInfo method)
                {
                    foreach (string parameter in command.Parameters)
                    {
                        text += " ˂" + parameter + "˃";
                    }
                }
                else if (command.Member is PropertyInfo property)
                {
                    MethodInfo set = property.GetSetMethod();
                    if (set != null)
                    {
                        text += " [value]";
                    }
                }
                else if (command.Member is FieldInfo field)
                {
                    text += " [value]";
                }

                if (command.Description != "")
                {
                    text += " = " + command.Description;
                }

                if (!command.IsStatic)
                {
                    text = "@id " + text;
                }

                builder.AppendLine(Indent + Indent + text);
            }
        }
        return builder.ToString();
    }

    [Command("echo")]
    public static string Echo(string text)
    {
        return text;
    }
}
