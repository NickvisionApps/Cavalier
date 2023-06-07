using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace NickvisionCavalier.Shared.Models;

public class Cava
{
    private readonly Process _proc;

    public event EventHandler<float[]>? OutputReceived;
    
    public Cava()
    {
        var configPath = $"{Configuration.ConfigDir}{Path.DirectorySeparatorChar}cava_config";
        var config = @"[general]
            framerate = 30
            bars = 20
            [output]
            method = raw
            raw_target = /dev/stdout
            bit_format = 16bit";
        File.WriteAllText(configPath, config);
        _proc = new Process
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "cava",
                Arguments = $"-p \"{configPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            }
        };
    }
    
    public void Start()
    {
        _proc.Start();
        Task.Run(ReadCavaOutput);
    }
    
    private void ReadCavaOutput()
    {
        var br = new BinaryReader(_proc.StandardOutput.BaseStream);
        while(!_proc.HasExited)
        {
            var sample = new float[20];
            var len = 40;
            var ba = br.ReadBytes(len);
            for (var i = 0; i < len; i += 2)
            {
                sample[i/2] = BitConverter.ToUInt16(ba, i) / 65535.0f;
            }
            OutputReceived?.Invoke(this, sample);
        }
    }
}