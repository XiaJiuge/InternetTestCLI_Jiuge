/*
MIT License

Copyright (c) Léo Corporation

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Net.NetworkInformation;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;

namespace InternetTestCLI.Commands;

[Command("ping", Description = "对指定网址进行Ping请求。")]
public class PingCommand() : ICommand
{
    [CommandParameter(0, Name = "site", Description = "要Ping的站点。")]
    public required string Site { get; init; }

    [CommandOption("amount", 'a', Description = "要发送的Ping请求次数。", IsRequired = false)]
    public int Amount { get; init; } = 4;

    public async ValueTask ExecuteAsync(IConsole Console)
    {
        try
        {
            Console.Output.WriteLine($"正在Ping {Site}...\n");
            int sent = 0, received = 0;

            long[] times = new long[Amount]; // Create an array
            for (int i = 0; i < Amount; i++)
            {
                var ping = await new Ping().SendPingAsync(Site); // Send a ping
                sent++;
                times[i] = ping.RoundtripTime; // Get the time of the ping

                string nl = $"{i + 1}/{Amount}"; // Add a new line if it's not the last ping
                Console.Output.WriteLine($"{nl}. Ping: {ping.Address} ({ping.RoundtripTime}毫秒)");

                if (ping.Status == IPStatus.Success)
                {
                    received++;
                }
            }

            Console.Output.WriteLine("");

            // Get the average, minimum, and maximum of the times and print them
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Output.WriteLine($"平均时间: {times.Average():0.00}毫秒");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Output.WriteLine($"最小时间: {times.Min()}毫秒");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Output.WriteLine($"最大时间: {times.Max()}毫秒");
            Console.ResetColor();

            Console.Output.WriteLine("");

            // Print the number of sent, received, and lost pings
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Output.WriteLine($"已发送: {sent}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Output.WriteLine($"已接收: {received}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Red;
            Console.Output.WriteLine($"丢失: {sent - received}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            throw new CommandException("发生错误：" + ex.Message);
        }
    }
}