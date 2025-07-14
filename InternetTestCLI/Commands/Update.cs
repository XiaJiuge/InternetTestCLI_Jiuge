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
using InternetTestCLI.Classes;
using PeyrSharp.Env;

namespace InternetTestCLI.Commands;

[Command("update", Description = "检查InternetTest CLI是否有新版本。")]
public class UpdateCommand() : ICommand
{
    public async ValueTask ExecuteAsync(IConsole Console)
    {
        try
        {
            Console.Output.WriteLine($"正在检查更新，请稍候...");
            string last = await Update.GetLastVersionAsync("https://raw.githubusercontent.com/Leo-Corporation/LeoCorp-Docs/master/Liens/Update%20System/InternetTest/CLI/Version.txt");
            string download = await Update.GetLastVersionAsync("https://raw.githubusercontent.com/Leo-Corporation/LeoCorp-Docs/master/Liens/Update%20System/InternetTest/CLI/Download.txt");

            if (Update.IsAvailable(Global.Version, last))
            {
                Console.Output.WriteLine("发现新版本可用。");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Output.WriteLine($"\t版本: {last}");
                Console.Output.WriteLine($"\t下载地址: {download}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Output.WriteLine("已是最新版本！");
            }
            Console.ResetColor();

        }
        catch (Exception ex)
        {
            throw new CommandException("发生错误：" + ex.Message);
        }
    }
}