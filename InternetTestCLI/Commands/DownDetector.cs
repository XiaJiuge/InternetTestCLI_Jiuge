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

using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using PeyrSharp.Core;
using System;

namespace InternetTestCLI.Commands;

[Command("downdetector", Description = "检测网站是否无法访问。")]
public class DownDetectorTestCommand() : ICommand
{
	[CommandParameter(0, Name = "sites", Description = "网站URL。", IsRequired = false)]
	public string[] Sites { get; init; } = [];

	[CommandOption("filepath", 'f', Description = "指定你提供的是文件而不是URL列表。", IsRequired = false)]
	public bool FilePath { get; init; } = false;

	public async ValueTask ExecuteAsync(IConsole Console)
	{
		try
		{
			string[] websites = Sites;
			if (FilePath && File.Exists(Sites[0]))
			{
				websites = File.ReadAllLines(Sites[0]);
			}

			foreach (var site in websites)
			{
				string url = site;
				if (!(url.Contains("https://") || url.Contains("http://")))
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Output.WriteLine("未指定协议，默认使用HTTPS。");
					url = "https://" + url;
				}
				Console.Output.WriteLine($"正在测试 {url}，请稍候...");

				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Output.WriteLine($"\n{url}");
				Console.Output.WriteLine(new string('=', url.Length));

				Console.ResetColor();
				var statusInfo = await Internet.GetStatusInfoAsync(url); // Makes a request to the specified website and saves the status code and message
				var color = statusInfo.StatusCode switch
				{
					var code when code >= 100 && code < 200 => ConsoleColor.Blue,// Informational
					var code when code >= 200 && code < 300 => ConsoleColor.Green,// Success
					var code when code >= 300 && code < 400 => ConsoleColor.Yellow,// Redirection
					var code when code >= 400 && code < 500 => ConsoleColor.Red,// ClientError
					var code when code >= 500 && code < 600 => ConsoleColor.DarkRed,// ServerError
					_ => Console.ForegroundColor,// Keep the default color for unexpected values
				};
				// Load Information
				Console.Output.Write("状态码: "); Console.ForegroundColor = color; Console.Output.Write(statusInfo.StatusCode); Console.ResetColor();
				Console.Output.WriteLine("");
				Console.Output.WriteLine($"状态类型: {statusInfo.StatusType}");
				Console.Output.Write($"状态描述: "); Console.ForegroundColor = color; Console.Output.Write(statusInfo.StatusDescription + "\n"); Console.ResetColor();
			}
		}
		catch (Exception ex)
		{
			throw new CommandException("发生错误：" + ex.Message);
		}
	}
}