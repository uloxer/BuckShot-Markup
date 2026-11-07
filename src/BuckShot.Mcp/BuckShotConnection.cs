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

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace BuckShot.Mcp
{
    /// <summary>
    /// A failure talking to BuckShot (not running, connection closed, invalid reply).
    /// </summary>
    public sealed class BuckShotConnectionException : Exception
    {
        public BuckShotConnectionException(string message, Exception? innerException = null) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// One request to the running BuckShot over its named pipe (BuckShot_&lt;user SID&gt;), using the protocol of
    /// buckshot-cli.exe and the browser extension: frames with a 4-byte little-endian length and UTF-8 JSON. The first frame is
    /// HELLO with source "mcp", which selects the command whitelist and consent rules for AI tools in BuckShot.
    /// </summary>
    public static class BuckShotConnection
    {
        private const string PipePrefix = "BuckShot-Markup_";
        private const int ConnectTimeoutMilliseconds = 2000;
        private const int StartupTimeoutMilliseconds = 20000;
        // A capture of several 4K displays as base64 PNG can be large
        private const int MaxReplySize = 256 * 1024 * 1024;

        public static string McpServerVersion => new Version(System.Reflection.CustomAttributeExtensions
            .GetCustomAttribute<System.Reflection.AssemblyFileVersionAttribute>(typeof(BuckShotConnection).Assembly)!.Version).ToString(3);

        /// <summary>
        /// Sends the request and returns BuckShot's final reply, with streamed stdout / stderr text collected in
        /// "streamed_stdout" / "streamed_stderr".
        /// </summary>
        /// <param name="request">Envelope with at least "command"</param>
        /// <param name="clientName">Name of the AI tool, shown to the user when BuckShot asks for consent</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <param name="startBuckShot">Start BuckShot when it isn't running (false for background requests)</param>
        public static async Task<JsonObject> SendAsync(JsonObject request, string? clientName, CancellationToken cancellationToken, bool startBuckShot = true)
        {
            // The user closed BuckShot: it isn't started again behind the user's back, it has to be started by the user
            await using var pipe = await ConnectAsync(startBuckShot && !BuckShotState.IsClosedByUser, cancellationToken).ConfigureAwait(false);
            await SendHelloAsync(pipe, clientName, cancellationToken).ConfigureAwait(false);

            request["version"] = 1;
            request["source"] = "mcp";
            await WriteFrameAsync(pipe, request, cancellationToken).ConfigureAwait(false);

            var streamedStdout = new StringBuilder();
            var streamedStderr = new StringBuilder();
            while (true)
            {
                var frame = await ReadFrameAsync(pipe, cancellationToken).ConfigureAwait(false)
                            ?? throw new BuckShotConnectionException("BuckShot closed the connection without a reply.");

                string? buckshotEvent = (string?)frame["event"];
                if (buckshotEvent != null)
                {
                    // BuckShot's own messages can arrive on any connection
                    if (string.Equals(buckshotEvent, "shutdown", StringComparison.OrdinalIgnoreCase))
                    {
                        BuckShotState.OnShutdown((string?)frame["reason"]);
                        throw new BuckShotConnectionException("BuckShot is exiting, the request was not finished.");
                    }
                    continue;
                }

                string? stream = (string?)frame["stream"];
                if (stream != null)
                {
                    var target = string.Equals(stream, "stderr", StringComparison.OrdinalIgnoreCase) ? streamedStderr : streamedStdout;
                    target.AppendLine((string?)frame["text"]);
                    continue;
                }

                if (streamedStdout.Length > 0)
                {
                    frame["streamed_stdout"] = streamedStdout.ToString().TrimEnd();
                }
                if (streamedStderr.Length > 0)
                {
                    frame["streamed_stderr"] = streamedStderr.ToString().TrimEnd();
                }
                return frame;
            }
        }

        /// <summary>
        /// Opens a connection which stays open for BuckShot's events (WATCH), without starting BuckShot.
        /// The pipe is null when BuckShot isn't running, or doesn't know WATCH (an older version, IsRunning is true).
        /// </summary>
        internal static async Task<(NamedPipeClientStream? Pipe, bool IsRunning)> OpenWatchAsync(CancellationToken cancellationToken)
        {
            NamedPipeClientStream pipe;
            try
            {
                pipe = await ConnectAsync(false, cancellationToken).ConfigureAwait(false);
            }
            catch (BuckShotConnectionException)
            {
                return (null, false);
            }

            try
            {
                await SendHelloAsync(pipe, "buckshot-mcp", cancellationToken).ConfigureAwait(false);
                await WriteFrameAsync(pipe, new JsonObject { ["version"] = 1, ["command"] = "WATCH", ["source"] = "mcp" }, cancellationToken).ConfigureAwait(false);
                var reply = await ReadFrameAsync(pipe, cancellationToken).ConfigureAwait(false);
                if (reply != null && reply["watching"] is JsonValue watching && watching.TryGetValue(out bool isWatching) && isWatching)
                {
                    return (pipe, true);
                }
            }
            catch (Exception ex) when (ex is BuckShotConnectionException || ex is IOException)
            {
                // Treated like an older BuckShot
            }
            await pipe.DisposeAsync().ConfigureAwait(false);
            return (null, true);
        }

        private static Task SendHelloAsync(Stream pipe, string? clientName, CancellationToken cancellationToken)
        {
            var hello = new JsonObject
            {
                ["version"] = 1,
                ["command"] = "HELLO",
                ["source"] = "mcp",
                ["origin"] = string.IsNullOrWhiteSpace(clientName) ? "AI tool" : clientName,
                ["reply_format"] = "json"
            };
            return WriteFrameAsync(pipe, hello, cancellationToken);
        }

        private static string GetPipeName()
        {
            var user = WindowsIdentity.GetCurrent().User
                       ?? throw new BuckShotConnectionException("Could not get the Windows user SID.");
            return PipePrefix + user.Value;
        }

        /// <summary>
        /// Connects to BuckShot; starts BuckShot.exe from the same directory when it isn't running (and startBuckShot is true).
        /// </summary>
        private static async Task<NamedPipeClientStream> ConnectAsync(bool startBuckShot, CancellationToken cancellationToken)
        {
            string pipeName = GetPipeName();
            var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                await pipe.ConnectAsync(ConnectTimeoutMilliseconds, cancellationToken).ConfigureAwait(false);
                return pipe;
            }
            catch (TimeoutException)
            {
                await pipe.DisposeAsync().ConfigureAwait(false);
            }

            string buckshotExe = Path.Combine(AppContext.BaseDirectory, "BuckShot.exe");
            if (!startBuckShot || !File.Exists(buckshotExe))
            {
                throw new BuckShotConnectionException(BuckShotState.IsClosedByUser
                    ? "BuckShot was closed by the user. Ask the user to start BuckShot to use these tools."
                    : "BuckShot is not running. Please start BuckShot and try again.");
            }

            try
            {
                using var process = Process.Start(new ProcessStartInfo(buckshotExe) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                throw new BuckShotConnectionException($"BuckShot is not running and could not be started: {ex.Message}", ex);
            }

            pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            try
            {
                await pipe.ConnectAsync(StartupTimeoutMilliseconds, cancellationToken).ConfigureAwait(false);
                return pipe;
            }
            catch (TimeoutException ex)
            {
                await pipe.DisposeAsync().ConfigureAwait(false);
                throw new BuckShotConnectionException("BuckShot was started, but did not accept the connection in time. Please try again.", ex);
            }
        }

        internal static async Task WriteFrameAsync(Stream stream, JsonObject message, CancellationToken cancellationToken)
        {
            byte[] payload = Encoding.UTF8.GetBytes(message.ToJsonString());
            byte[] frame = new byte[4 + payload.Length];
            BinaryPrimitives.WriteInt32LittleEndian(frame, payload.Length);
            payload.CopyTo(frame, 4);
            await stream.WriteAsync(frame, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads one frame, null when the connection was closed before a frame started.
        /// </summary>
        internal static async Task<JsonObject?> ReadFrameAsync(Stream stream, CancellationToken cancellationToken)
        {
            byte[] lengthBytes = new byte[4];
            int read = await ReadExactlyOrEndAsync(stream, lengthBytes, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                return null;
            }
            if (read < 4)
            {
                throw new BuckShotConnectionException("BuckShot closed the connection in the middle of a reply.");
            }

            int length = BinaryPrimitives.ReadInt32LittleEndian(lengthBytes);
            if (length <= 0 || length > MaxReplySize)
            {
                throw new BuckShotConnectionException($"BuckShot sent an invalid reply length ({length}).");
            }

            byte[] payload = new byte[length];
            if (await ReadExactlyOrEndAsync(stream, payload, cancellationToken).ConfigureAwait(false) < length)
            {
                throw new BuckShotConnectionException("BuckShot closed the connection in the middle of a reply.");
            }

            return JsonNode.Parse(payload) as JsonObject
                   ?? throw new BuckShotConnectionException("BuckShot sent a reply which is not a JSON object.");
        }

        private static async Task<int> ReadExactlyOrEndAsync(Stream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            int total = 0;
            while (total < buffer.Length)
            {
                int read = await stream.ReadAsync(buffer.AsMemory(total), cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }
                total += read;
            }
            return total;
        }
    }
}
