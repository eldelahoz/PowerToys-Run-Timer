using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;

namespace Community.PowerToys.Run.Plugin.Timers;

/// <summary>
/// Service for managing timer alarm sounds.
/// </summary>
public class SoundService : IDisposable
{
    private SoundPlayer? _currentPlayer;
    private CancellationTokenSource? _playbackCancellation;
    private Task? _playbackTask;
    private readonly string _soundsDirectory;

    public SoundService(string pluginDirectory)
    {
        _soundsDirectory = Path.Combine(pluginDirectory, "Sounds");
    }

    /// <summary>
    /// Gets available sound files from the Sounds directory.
    /// </summary>
    public List<string> GetAvailableSounds()
    {
        if (!Directory.Exists(_soundsDirectory))
        {
            return new List<string> { "default.wav" };
        }

        var soundFiles = Directory
            .GetFiles(_soundsDirectory, "*.wav")
            .Select(Path.GetFileName)
            .OfType<string>()
            .OrderBy(x => x)
            .ToList();

        return soundFiles.Any() ? soundFiles : new List<string> { "default.wav" };
    }

    /// <summary>
    /// Plays the selected sound repeatedly for the specified duration.
    /// </summary>
    public void PlayAlarm(string soundFileName, int durationSeconds)
    {
        StopAlarm();

        var soundPath = Path.Combine(_soundsDirectory, soundFileName);

        if (!File.Exists(soundPath))
        {
            soundPath = Path.Combine(_soundsDirectory, "default.wav");
        }

        if (!File.Exists(soundPath))
        {
            return;
        }

        _playbackCancellation = new CancellationTokenSource();
        _playbackTask = PlaySoundLoopAsync(soundPath, durationSeconds, _playbackCancellation.Token);
    }

    /// <summary>
    /// Stops the current alarm sound.
    /// </summary>
    public void StopAlarm()
    {
        _playbackCancellation?.Cancel();
        _currentPlayer?.Stop();
        _currentPlayer?.Dispose();
        _currentPlayer = null;

        try
        {
            _playbackTask?.Wait(TimeSpan.FromSeconds(1));
        }
        catch (OperationCanceledException) { }
    }

    private async Task PlaySoundLoopAsync(
        string soundPath,
        int durationSeconds,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var endTime = DateTime.Now.AddSeconds(durationSeconds);

            while (DateTime.Now < endTime && !cancellationToken.IsCancellationRequested)
            {
                _currentPlayer = new SoundPlayer(soundPath);
                _currentPlayer.PlaySync();

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await Task.Delay(100, cancellationToken);
            }
        }
        catch (OperationCanceledException) { }
        finally
        {
            _currentPlayer?.Dispose();
            _currentPlayer = null;
        }
    }

    public void Dispose()
    {
        StopAlarm();
        _playbackCancellation?.Dispose();
    }
}
