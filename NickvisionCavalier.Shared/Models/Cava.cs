using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace NickvisionCavalier.Shared.Models;

public class Cava : IDisposable
{
    private bool _disposed;
    private readonly Process _proc;
    private readonly string _configPath;

    public event EventHandler<float[]>? OutputReceived;
    
    public Cava()
    {
        _disposed = false;
        _configPath = $"{Configuration.ConfigDir}{Path.DirectorySeparatorChar}cava_config";
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
    /// Finalizes the Cava object
    /// </summary>
    ~Cava() => Dispose(false);

    /// <summary>
    /// Frees resources used by the Cava object
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by the Cava object
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
        }
        _disposed = true;
    }

    private void UpdateConfig()
    {
        var config = @$"[general]
            framerate = {Configuration.Current.Framerate}
            bars = {Configuration.Current.BarPairs * 2}
            autosens = {(Configuration.Current.Autosens ? "1" : "0")}
            sensitivity = {Math.Pow(Configuration.Current.Sensitivity, 2)}
            [output]
            method = raw
            raw_target = /dev/stdout
            bit_format = 16bit
            channels = {(Configuration.Current.Stereo ? "stereo" : "mono")}
            [smoothing]
            monstercat = {(Configuration.Current.Monstercat ? "1" : "0")}
            noise_reduction = {Configuration.Current.NoiseReduction.ToString("G2", CultureInfo.InvariantCulture)}";
        File.WriteAllText(_configPath, config);
    }

    public void Start()
    {
        _proc.Start();
        Task.Run(ReadCavaOutput);
    }
    
    public void Restart()
    {
        _proc.Kill();
        UpdateConfig();
        Start();
    }

    private void ReadCavaOutput()
    {
        var br = new BinaryReader(_proc.StandardOutput.BaseStream);
        while(!_proc.HasExited)
        {
            var sample = new float[Configuration.Current.BarPairs * 2];
            var len = (int)Configuration.Current.BarPairs * 4;
            var ba = br.ReadBytes(len);
            for (var i = 0; i < len; i += 2)
            {
                sample[i/2] = BitConverter.ToUInt16(ba, i) / 65535.0f;
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