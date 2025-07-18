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
using RestSharp;

namespace InternetTestCLI.Commands;

[Command("request", Description = "对指定资源发起请求。")]
public class RequestCommand() : ICommand
{
    [CommandParameter(0, Name = "method", Description = "请求方法。")]
    public required Method Method { get; init; } = Method.Get;

    [CommandParameter(1, Name = "url", Description = "请求的URL地址。")]
    public required string URL { get; init; }

    [CommandOption("content", 'c', Description = "只输出响应内容。", IsRequired = false)]
    public bool ContentOnly { get; init; } = false;

    public async ValueTask ExecuteAsync(IConsole Console)
    {
        try
        {
            if (!ContentOnly) Console.Output.WriteLine($"正在对 {URL} 执行 {Method} 请求，请稍候...");
            await ExecuteRequest(Method, URL);


        }
        catch (Exception ex)
        {
            throw new CommandException("发生错误：" + ex.Message);
        }
    }

    public async Task ExecuteRequest(Method method, string url)
    {
        var options = new RestClientOptions(url);
        var client = new RestClient(options);
        var request = new RestRequest("", method);

        var response = await client.ExecuteAsync(request);
        if (!ContentOnly)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n响应内容");
            Console.WriteLine("========\n");
            Console.ResetColor();
        }
        Console.WriteLine(response.Content);

        if (ContentOnly) return;

        string headers = "";
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n响应头");
        Console.WriteLine("======\n");

        foreach (var item in response.Headers)
        {
            headers += item.ToString() + "\n";
            var header = item.ToString().Split("=", 2);
            if (header.Length < 2) continue;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(header[0]);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(header[1] + "\n");
            Console.ResetColor();
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n状态");
        Console.WriteLine("====\n");
        Console.ResetColor();


        var color = (int)response.StatusCode switch
        {
            var code when code >= 100 && code < 200 => ConsoleColor.Blue,// Informational
            var code when code >= 200 && code < 300 => ConsoleColor.Green,// Success
            var code when code >= 300 && code < 400 => ConsoleColor.Yellow,// Redirection
            var code when code >= 400 && code < 500 => ConsoleColor.Red,// ClientError
            var code when code >= 500 && code < 600 => ConsoleColor.DarkRed,// ServerError
            _ => Console.ForegroundColor,// Keep the default color for unexpected values
        };


        Console.Write("状态码: "); Console.ForegroundColor = color; Console.Write((int)response.StatusCode); Console.ResetColor();
        Console.WriteLine("");
        Console.Write($"状态描述: "); Console.ForegroundColor = color; Console.Write(response.StatusDescription + "\n"); Console.ResetColor();

    }
}