/*
 * Greenshot - a free and open source screenshot tool
 * Copyright (C) 2007-2026 Thomas Braun, Jens Klingen, Robin Krom
 *
 * For more information see: https://getgreenshot.org/
 * The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Text.Json;
using Greenshot.Mcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;

// greenshot-mcp.exe: MCP server over stdio. Stdout carries the protocol, so all logging goes to stderr.
var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// Tool parameters and results: the MCP protocol types plus our own (source generated, for Native AOT)
var jsonOptions = new JsonSerializerOptions(McpJsonUtilities.DefaultOptions);
jsonOptions.TypeInfoResolverChain.Insert(0, GreenshotMcpJsonContext.Default);

builder.Services
    .AddMcpServer(options =>
    {
        options.ServerInfo = new Implementation
        {
            Name = "buckshot-markup",
            Title = "BuckShot Markup",
            Version = GreenshotConnection.McpServerVersion
        };
        options.ServerInstructions = "BuckShot Markup captures images for engineering markup. Use the available capture tools; public recipe authoring is not supported.";
    })
    .WithStdioServerTransport()
    .WithTools<GreenshotTools>(jsonOptions);

// BuckShot exposes capture tools only; public recipe discovery/authoring is not registered.
// Greenshot's events: tools changed, Greenshot exits (wait for it, or exit for an update)
builder.Services.AddHostedService<GreenshotWatch>();

await builder.Build().RunAsync().ConfigureAwait(false);
