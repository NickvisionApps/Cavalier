using Nickvision.Aura;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NickvisionCavalier.Shared.Models;

public class CAVA : IDisposable
{
    private bool _disposed;
    private readonly Process _proc;
    private readonly string _configPath;

    public event EventHandler<float[]>? OutputReceived;

    public CAVA()
    {
        _disposed = false;
        _configPath = $"{UserDirectories.ApplicationConfig}{Path.DirectorySeparatorChar}cava_config";
        UpdateConfig();
        _proc = new Process
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cava",
                Arguments = $"-p \"{_configPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };
    }

    /// <summary>
    /// Finalizes the CAVA object
    /// </summary>
    ~CAVA() => Dispose(false);

    /// <summary>
    /// Frees resources used by the CAVA object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the CAVA object
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        if (disposing)
        {
            _proc.Kill();
            _proc.WaitForExit();
        }
        _disposed = true;
    }

    /// <summary>
    /// Update CAVA configuration file
    /// </summary>
    private void UpdateConfig()
    {
        var config = @$"[general]
            framerate = {Configuration.Current.Framerate}
            bars = {Configuration.Current.BarPairs * 2}
            autosens = {(Configuration.Current.Autosens ? "1" : "0")}
            sensitivity = {Math.Pow(Configuration.Current.Sensitivity, 2)}
            [input]
            method = {Environment.GetEnvironmentVariable("CAVALIER_INPUT_METHOD") ?? "pulse"}
            source = {Environment.GetEnvironmentVariable("CAVALIER_INPUT_SOURCE") ?? "auto"}
            [output]
            method = raw
            raw_target = /dev/stdout
            bit_format = 16bit
            channels = {(Configuration.Current.Stereo ? "stereo" : "mono")}
            [smoothing]
            monstercat = {(Configuration.Current.Monstercat ? "1" : "0")}
            noise_reduction = {(int)(Configuration.Current.NoiseReduction * 100)}";
        File.WriteAllText(_configPath, config);
    }

    /// <summary>
    /// (Re)start CAVA
    /// </summary>
    public void Restart()
    {
        try
        {
            _proc.Kill();
        }
        catch (InvalidOperationException) { }
        UpdateConfig();
        _proc.Start();
        Task.Run(ReadCAVAOutput);
    }

    /// <summary>
    /// Read what CAVA prints to stdout
    /// </summary>
    private void ReadCAVAOutput()
    {
        var br = new BinaryReader(_proc.StandardOutput.BaseStream);
        while (!_proc.HasExited)
        {
            var sample = new float[Configuration.Current.BarPairs * 2];
            var len = (int)Configuration.Current.BarPairs * 4;
            var ba = br.ReadBytes(len);
            for (var i = 0; i < len; i += 2)
            {
                sample[i / 2] = BitConverter.ToUInt16(ba, i) / 65535.0f;
            }
            if (Configuration.Current.ReverseOrder)
            {
                if (Configuration.Current.Stereo)
                {
                    Array.Reverse(sample, 0, sample.Length / 2);
                    Array.Reverse(sample, sample.Length / 2, sample.Length / 2);
                }
                else
                {
                    Array.Reverse(sample, 0, sample.Length);
                }
            }
            OutputReceived?.Invoke(this, sample);
        }
    }
}
