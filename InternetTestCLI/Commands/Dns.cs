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


using System.Diagnostics;
using System.Net;
using CliFx;
using CliFx.Attributes;
using CliFx.Exceptions;
using CliFx.Infrastructure;
using DnsClient;
using InternetTestCLI.Classes;
using Whois;
using BetterConsoleTables;
using InternetTestCLI.Enums;
using System.Drawing;

namespace InternetTestCLI.Commands;

[Command("dns", Description = "获取域名的DNS信息。")]
public class DnsCommand() : ICommand
{
	[CommandParameter(0, Name = "site", Description = "网站URL。")]
	public required string Site { get; init; }

	[CommandOption("advanced", 'a', Description = "显示Whois的所有信息。", IsRequired = false)]
	public bool Advanced { get; init; } = false;

	[CommandOption("record-types", 'r', Description = "仅显示指定的记录类型。", IsRequired = false)]
	public DnsClient.Protocol.ResourceRecordType[] RecordTypes { get; init; }

	public async ValueTask ExecuteAsync(IConsole Console)
	{
		try
		{
			Console.Output.WriteLine($"正在获取 {Site} 的DNS信息，请稍候...");

			GetDnsInfo(Site);

		}
		catch (Exception ex)
		{
			throw new CommandException("发生错误：" + ex.Message);
		}
	}

	private void GetDnsInfo(string website)
	{
		// Get WHOIS
		var whois = new WhoisLookup();
		var response = whois.Lookup(website);

		// Get IP
		IPHostEntry host = Dns.GetHostEntry(website);
		IPAddress ip = host.AddressList[0];

		Console.WriteLine($"IP地址: {ip}");
		Console.WriteLine("");

		// Get DNS records
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine($"DNS记录");
		Console.WriteLine($"==========");
		Console.ResetColor();
		var lookup = new LookupClient();
		var result = lookup.QueryAsync(website, QueryType.ANY).Result;
		foreach (var record in result.AllRecords)
		{
			if (RecordTypes is not null && !RecordTypes.Contains(record.RecordType)) continue;
			Console.WriteLine($"{record.RecordType} - {record}");
		}

		// Display WHOIS
		Console.WriteLine("");
		Console.ForegroundColor = ConsoleColor.Cyan;
		Console.WriteLine($"WHOIS信息");
		Console.WriteLine($"========");
		Console.ResetColor();

		if (Advanced)
		{
			Console.WriteLine(response.Content);
		}
		else
		{
			Console.WriteLine($"注册时间: {response.Registered}");
			Console.WriteLine($"到期时间: {response.Expiration}");
			Console.WriteLine($"状态:\n\t{string.Join("\n\t", response.DomainStatus)}");
			string regInfo = "";
			foreach (var prop in response.Registrant.GetType().GetProperties())
			{
				var val = prop.GetValue(response.Registrant, null);
				if (val is null || prop.Name == "Address") continue;
				regInfo += $"\t{prop.Name} - {val}\n";
			}
			Console.WriteLine("注册人信息:");
			Console.WriteLine(regInfo);
		}

	}

}

#if _WINDOWS
[Command("dns cache", Description = "获取DNS缓存。")]
public class DnsCacheCommand() : ICommand
{
	[CommandOption("clear", 'c', Description = "清除DNS缓存。", IsRequired = false)]
	public bool Clear { get; init; } = false;
	public async ValueTask ExecuteAsync(IConsole Console)
	{

		if (Clear)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Output.WriteLine("确定要清除DNS缓存吗？(y/n): ");
			var key = Console.ReadKey().Key;
			if (key != ConsoleKey.Y)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Output.WriteLine("\n操作已取消。");
				Console.ResetColor();
				return;
			}

			Console.Output.WriteLine("\n正在清除DNS缓存，请稍候...");
			ProcessStartInfo processInfo = new()
			{
				FileName = "ipconfig",
				Arguments = "/flushdns",
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using Process process = Process.Start(processInfo);
			// Read the output from the command
			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			// Wait for the process to exit
			process.WaitForExit();
			Console.ForegroundColor = ConsoleColor.Green;
			Console.Output.WriteLine("DNS缓存已成功清除。");
			Console.ResetColor();
		}

		try
		{
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.Output.WriteLine("正在获取DNS缓存，请稍候...");
			Console.ForegroundColor = ConsoleColor.Yellow;

			var cache = await GetDnsCache();

			if (cache != null)
			{
				// Define the headers with alignment for each column
				ColumnHeader[] headers = [
					new ColumnHeader("条目", Alignment.Left),
					new ColumnHeader("记录名", Alignment.Left),
					new ColumnHeader("类型", Alignment.Center),
					new ColumnHeader("状态", Alignment.Center),
					new ColumnHeader("数据", Alignment.Left)
				];

				// Initialize the table with the headers
				Table table = new(headers)
				{
					Config = TableConfiguration.MySqlSimple() // Simple MySQL-style table formatting
				};

				// Add rows to the table
				foreach (var item in cache)
				{
					if (item is null) continue;

					table.AddRow(
						item.Entry ?? "",
						item.Name ?? "",
						(Types)item.Type,
						(Status)item.Status,
						item.Data ?? ""
					);
				}

				// Print the table
				Console.Output.WriteLine(table.ToString());
				Console.ForegroundColor = ConsoleColor.White;
			}
		}
		catch (Exception ex)
		{
			throw new CommandException("发生错误：" + ex.Message + ex.StackTrace + ex.InnerException + ex.Source);
		}
	}
	public static async Task<DnsCacheInfo[]> GetDnsCache()
	{
		// The PowerShell command to execute
		string psCommand = "Get-DnsClientCache | ConvertTo-Json -Depth 4";

		// Capture the JSON output asynchronously
		string json = await RunPowerShellCommandAsync(psCommand);
		return DnsCacheInfo.FromJson(json);
	}

	public static async Task<string> RunPowerShellCommandAsync(string psCommand)
	{
		// Create a new process to run PowerShell
		ProcessStartInfo processInfo = new()
		{
			FileName = "powershell.exe",
			Arguments = $"-Command \"{psCommand}\"",
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		// Start the PowerShell process
		using Process process = Process.Start(processInfo);

		// Asynchronously read the output from the process
		string output = await process.StandardOutput.ReadToEndAsync();

		// Wait for the process to complete asynchronously
		await process.WaitForExitAsync();

		// Return the JSON output
		return output;
	}
}
#endif