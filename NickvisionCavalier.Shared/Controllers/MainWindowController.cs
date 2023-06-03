using NickvisionCavalier.Shared.Events;
using NickvisionCavalier.Shared.Models;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using static NickvisionCavalier.Shared.Helpers.Gettext;

namespace NickvisionCavalier.Shared.Controllers;

/// <summary>
/// A controller for a MainWindow
/// </summary>
public class MainWindowController
{
    private readonly Renderer _renderer;
    public readonly Cava Cava;
    /// <summary>
    /// The path of the folder opened
    /// </summary>
    public string FolderPath { get; private set; }

    /// <summary>
    /// Gets the AppInfo object
    /// </summary>
    public AppInfo AppInfo => AppInfo.Current;
    /// <summary>
    /// Whether or not the version is a development version or not
    /// </summary>
    public bool IsDevVersion => AppInfo.Current.Version.IndexOf('-') != -1;
    /// <summary>
    /// The preferred theme of the application
    /// </summary>
    public Theme Theme => Configuration.Current.Theme;

    /// <summary>
    /// Constructs a MainWindowController
    /// </summary>
    public MainWindowController()
    {
        FolderPath = "No Folder Opened";
        _renderer = new Renderer();
        Cava = new Cava();
        Cava.Start();
    }

    public void SetCanvas(SKCanvas canvas)
    {
        _renderer.Canvas = canvas;
    }
    
    public void Render(float[] sample, float width, float height) => _renderer.Draw(sample, width, height);

    /// <summary>
    /// Creates a new PreferencesViewController
    /// </summary>
    /// <returns>The PreferencesViewController</returns>
    public PreferencesViewController CreatePreferencesViewController() => new PreferencesViewController();
}
