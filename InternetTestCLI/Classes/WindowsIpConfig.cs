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

namespace InternetTestCLI.Classes;
public record WindowsIpConfig(
    string? Name,
    string? IPv4Address,
    string? IPv4Mask,
    string? IPv4Gateway,
    string? IPv6Address,
    string? IPv6Gateway,
    string? DNSSuffix,
    OperationalStatus? Status)
{
    public override string ToString() =>
        $"IPv4地址: {IPv4Address}\n" +
        $"子网掩码: {IPv4Mask}\n" +
        $"IPv4网关: {IPv4Gateway ?? "无"}\n" +
        $"IPv6地址: {IPv6Address}\n" +
        $"IPv6网关: {IPv6Gateway ?? "无"}\n" +
        $"DNS后缀: {DNSSuffix ?? "无"}\n" +
        $"状态: {(Status == OperationalStatus.Up ? "已连接" : "未连接")}";
}