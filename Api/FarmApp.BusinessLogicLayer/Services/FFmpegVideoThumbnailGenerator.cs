using System.Diagnostics;
using System.Globalization;
using FarmApp.BusinessLogicLayer.Services.Interfaces;
using FarmApp.ViewModels.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FarmApp.BusinessLogicLayer.Services
{
    public class FFmpegVideoThumbnailGenerator : IVideoThumbnailGenerator
    {
        private readonly FFmpegOptions _options;
        private readonly ILogger<FFmpegVideoThumbnailGenerator> _logger;

        public FFmpegVideoThumbnailGenerator(
            IOptions<FFmpegOptions> options,
            ILogger<FFmpegVideoThumbnailGenerator> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task CreateThumbnailAsync(
            Stream input,
            Stream output,
            int size,
            string sourceFileName,
            CancellationToken ct = default)
        {
            var extension = Path.GetExtension(sourceFileName);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".mp4";

            var tempInput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{extension}");
            var tempOutput = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.jpg");

            try
            {
                if (input.CanSeek)
                    input.Position = 0;

                await using (var fs = File.Create(tempInput))
                {
                    await input.CopyToAsync(fs, ct);
                }

                await RunFfmpegAsync(tempInput, tempOutput, size, ct);

                await using var resultStream = File.OpenRead(tempOutput);
                await resultStream.CopyToAsync(output, ct);

                if (output.CanSeek)
                    output.Position = 0;
            }
            finally
            {
                TryDelete(tempInput);
                TryDelete(tempOutput);
            }
        }

        private async Task RunFfmpegAsync(string inputPath, string outputPath, int size, CancellationToken ct)
        {
            var sizeArg = size.ToString(CultureInfo.InvariantCulture);
            var filter = $"thumbnail,scale='min({sizeArg},iw)':-2";

            var psi = new ProcessStartInfo
            {
                FileName = _options.BinaryPath,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
            };

            psi.ArgumentList.Add("-y");
            psi.ArgumentList.Add("-i");
            psi.ArgumentList.Add(inputPath);
            psi.ArgumentList.Add("-vf");
            psi.ArgumentList.Add(filter);
            psi.ArgumentList.Add("-frames:v");
            psi.ArgumentList.Add("1");
            psi.ArgumentList.Add("-q:v");
            psi.ArgumentList.Add("4");
            psi.ArgumentList.Add(outputPath);

            using var process = new Process { StartInfo = psi };

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to start FFmpeg at '{_options.BinaryPath}'. Make sure the binary is installed and available.",
                    ex);
            }

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            var stderrTask = process.StandardError.ReadToEndAsync(linked.Token);

            try
            {
                await process.WaitForExitAsync(linked.Token);
            }
            catch (OperationCanceledException)
            {
                TryKill(process);
                throw;
            }

            var stderr = await stderrTask;

            if (process.ExitCode != 0)
            {
                _logger.LogError("FFmpeg failed with exit code {ExitCode}. stderr: {Stderr}", process.ExitCode, stderr);
                throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}.");
            }
        }

        private static void TryKill(Process process)
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
            catch
            {
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch
            {
            }
        }
    }
}
