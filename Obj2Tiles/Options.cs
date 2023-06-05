using System.Text.Json.Serialization;
using CommandLine;

namespace Obj2Tiles;

public sealed class Options
{
    [Value(0, MetaName = "Input", Required = true, HelpText = "Input OBJ file.")]
    public string Input { get; set; }

    [Value(1, MetaName = "Output", Required = true, HelpText = "Output folder.")]
    public string Output { get; set; }

    [Option('s', "stage", Required = false, HelpText = "Stage to stop at (Decimation, Splitting, Tiling)", Default = Stage.Splitting)]
    public Stage StopAt { get; set; }

    [Option('d', "divisions", Required = false, HelpText = "How many tiles divisions", Default = 2)]
    public int Divisions { get; set; }
    
    [Option('z', "zsplit", Required = false, HelpText = "Splits along z-axis too", Default = false)]
    public bool ZSplit { get; set; }    
    
    [Option('l', "lods", Required = false, HelpText = "How many levels of details", Default = 1)]
    public int LODs { get; set; }

    [Option('k', "keeptextures", Required = false, HelpText = "Keeps original textures", Default = true)]
    public bool KeepOriginalTextures { get; set; }
    
    [Option("lat", Required = false, HelpText = "Latitude of the mesh", Default = null)]
    public double? Latitude { get; set; }
    
    [Option("lon", Required = false, HelpText = "Longitude of the mesh", Default = null)]
    public double? Longitude { get; set; }
    
    [Option("alt", Required = false, HelpText = "Altitude of the mesh (meters)", Default = 0)]
    public double Altitude { get; set; }
    
    [Option("use-system-temp", Required = false, HelpText = "Uses the system temp folder", Default = false)]
    public bool UseSystemTempFolder { get; set; }
    
    [Option("keep-intermediate", Required = false, HelpText = "Keeps the intermediate files (do not cleanup)", Default = false)]
    public bool KeepIntermediateFiles { get; set; }
}

public enum Stage
{
    Decimation,
    Splitting,
    Tiling
}