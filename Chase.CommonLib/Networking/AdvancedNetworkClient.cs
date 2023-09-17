/*
    Chase CommonLib - LFInteractive LLC. 2021-2024
    CommonLib is a library of common functions and classes for .NET 6.0+.
    Licensed under GPL-3.0
    https://www.gnu.org/licenses/gpl-3.0.en.html#license-text
*/

using Chase.CommonLib.Events;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace Chase.CommonLib.Networking;

/// <summary>
/// A modified <seealso cref="HttpClient">HttpClient</seealso> that adds additional features.
/// </summary>
public class AdvancedNetworkClient : HttpClient
{
    /// <summary>
    /// Downloads a file using an address and output file
    /// </summary>
    /// <param name="address">the direct download link</param>
    /// <param name="path">the directory or absolute file path</param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public Task<string> DownloadFileAsync(string address, string path, DownloadProgressEvent? progress = null) => DownloadFileAsync(new Uri(address), path, progress);

    /// <summary>
    /// Downloads a file using an address and output file
    /// </summary>
    /// <param name="address">the direct download uri</param>
    /// <param name="path">the directory or absolute file path</param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task<string> DownloadFileAsync(Uri address, string path, DownloadProgressEvent? progress = null)
    {
        bool guessFileName = new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory) && Directory.Exists(path);
        using HttpResponseMessage response = await GetAsync(address, HttpCompletionOption.ResponseHeadersRead);
        string? ContentDisposition = response.Content.Headers.ContentDisposition?.FileName;
        string file = guessFileName ? Path.Combine(path, Strings.GetValidFileName(string.IsNullOrWhiteSpace(ContentDisposition) ? Path.GetRandomFileName() : ContentDisposition)) : path;

        using HttpContent content = response.Content;
        long contentLength = content.Headers.ContentLength.GetValueOrDefault();

        using Stream contentStream = await content.ReadAsStreamAsync();
        byte[] buffer = new byte[8192];
        int bytesRead;
        long totalBytesRead = 0;
        double bytesPerSecond = 0;
        long currentBytes = 0;

        System.Timers.Timer dataPerSecondTimer = new(1000) { Enabled = true, AutoReset = true };
        System.Timers.Timer updateProgressTimer = new(100) { Enabled = true, AutoReset = true };
        dataPerSecondTimer.Elapsed += (s, e) =>
        {
            bytesPerSecond = totalBytesRead - currentBytes;
            currentBytes = totalBytesRead;
        };

        updateProgressTimer.Elapsed += (s, e) =>
        {
            progress?.Invoke(this, new DownloadProgressEventArgs(Path.GetFileName(file), (double)totalBytesRead / contentLength, totalBytesRead, contentLength, bytesPerSecond));
        };
        dataPerSecondTimer.Start();
        updateProgressTimer.Start();
        using (FileStream fs = new(file, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
        {
            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fs.WriteAsync(buffer.AsMemory(0, bytesRead));
                totalBytesRead += bytesRead;
            }
        }
        dataPerSecondTimer.Stop();
        updateProgressTimer.Stop();
        progress?.Invoke(this, new DownloadProgressEventArgs(Path.GetFileName(file), (double)totalBytesRead / contentLength, totalBytesRead, contentLength, bytesPerSecond));
        return file;
    }

    /// <summary>
    /// Uploads a file using an address and input file
    /// </summary>
    /// <param name="address"></param>
    /// <param name="file"></param>
    /// <param name="progress"></param>
    /// <returns></returns>
    public async Task<HttpResponseMessage> UploadFileAsync(string address, string file, DownloadProgressEvent? progress = null)
    {
        using FileStream fs = File.OpenRead(file); // Opens the file for reading
        using StreamContent content = new(fs); // Creates a new StreamContent object with the file stream

        // Sets the content headers
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileName = Path.GetFileName(file)
        };

        // Gets the total bytes of the file
        long totalBytes = new FileInfo(file).Length;

        // Variables for tracking progress
        long currentBytes = 0;
        double bytesPerSecond = 0;
        byte[] buffer = new byte[8192];
        int bytesRead;

        // Sets the content length header
        content.Headers.ContentLength = totalBytes;

        // Creates a new request message with the content
        using HttpRequestMessage request = new(HttpMethod.Post, address)
        {
            Content = content
        };

        // Sends the request and gets the response
        using HttpResponseMessage response = await SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        // Gets the response stream
        using Stream stream = await response.Content.ReadAsStreamAsync();

        // Creates timers for tracking progress
        System.Timers.Timer dataPerSecondTimer = new(1000) { Enabled = true, AutoReset = true };
        System.Timers.Timer updateProgressTimer = new(100) { Enabled = true, AutoReset = true };

        dataPerSecondTimer.Elapsed += (s, e) =>
        {
            // Calculates the bytes per second
            bytesPerSecond = totalBytes - currentBytes;
            currentBytes = totalBytes;
        };

        updateProgressTimer.Elapsed += (s, e) =>
        {
            // Updates the progress
            progress?.Invoke(this, new DownloadProgressEventArgs(Path.GetFileName(file), (double)currentBytes / totalBytes, currentBytes, totalBytes, bytesPerSecond));
        };

        // Starts the timers
        dataPerSecondTimer.Start();
        updateProgressTimer.Start();

        // Reads the response stream
        while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
        {
            currentBytes += bytesRead;
        }

        // Stops the timers
        dataPerSecondTimer.Stop();
        updateProgressTimer.Stop();

        // Updates the progress one last time
        progress?.Invoke(this, new DownloadProgressEventArgs(Path.GetFileName(file), (double)currentBytes / totalBytes, currentBytes, totalBytes, bytesPerSecond));

        return response;
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified URI and parses the response content as JSON
    /// object (JObject).
    /// </summary>
    /// <param name="uri">The URI to send the GET request to.</param>
    /// <returns>
    /// A task representing the asynchronous operation that yields a JObject representing the parsed
    /// JSON response, or null if the response is null.
    /// </returns>
    public Task<JObject?> GetAsJson(string uri) => GetAsJson(new HttpRequestMessage() { Method = HttpMethod.Get, RequestUri = new(uri) });

    /// <summary>
    /// Sends an HTTP request and parses the response content as JSON object (JObject).
    /// </summary>
    /// <param name="requestMessage">The HttpRequestMessage to send.</param>
    /// <returns>
    /// A task representing the asynchronous operation that yields a JObject representing the parsed
    /// JSON response, or null if the response is null.
    /// </returns>
    public async Task<JObject?> GetAsJson(HttpRequestMessage requestMessage)
    {
        using HttpResponseMessage response = await SendAsync(requestMessage);

        if (response != null)
        {
            return JObject.Parse(await response.Content.ReadAsStringAsync());
        }

        return null;
    }

    /// <summary>
    /// Sends an HTTP GET request to the specified URI and parses the response content as JSON array (JArray).
    /// </summary>
    /// <param name="uri">The URI to send the GET request to.</param>
    /// <returns>
    /// A task representing the asynchronous operation that yields a JArray representing the parsed
    /// JSON response, or null if the response is null.
    /// </returns>
    public Task<JArray?> GetAsJsonArray(string uri) => GetAsJsonArray(new HttpRequestMessage() { Method = HttpMethod.Get, RequestUri = new(uri) });

    /// <summary>
    /// Sends an HTTP request and parses the response content as JSON array (JArray).
    /// </summary>
    /// <param name="requestMessage">The HttpRequestMessage to send.</param>
    /// <returns>
    /// A task representing the asynchronous operation that yields a JArray representing the parsed
    /// JSON response, or null if the response is null.
    /// </returns>
    public async Task<JArray?> GetAsJsonArray(HttpRequestMessage requestMessage)
    {
        using HttpResponseMessage response = await SendAsync(requestMessage);

        if (response != null)
        {
            return JArray.Parse(await response.Content.ReadAsStringAsync());
        }

        return null;
    }
}