using System.Text.Json.Serialization;
using CommandLine;

namespace Gltf2Tiles;

public sealed class Options
{
    [Value(0, MetaName = "Input", Required = true, HelpText = "Input gltf file.")]
    public string Input { get; set; }

    [Value(1, MetaName = "Output", Required = true, HelpText = "Output folder.")]
    public string Output { get; set; }

    [Option('l', "lods", Required = false, HelpText = "How many levels of details", Default = 1)]
    public int LODs { get; set; }

    [Option("lat", Required = false, HelpText = "Latitude of the mesh", Default = null)]
    public double? Latitude { get; set; }

    [Option("lon", Required = false, HelpText = "Longitude of the mesh", Default = null)]
    public double? Longitude { get; set; }

    [Option("alt", Required = false, HelpText = "Altitude of the mesh (meters)", Default = 0)]
    public double Altitude { get; set; }

    [Option("use-system-temp", Required = false, HelpText = "Uses the system temp folder", Default = false)]
    public bool UseSystemTempFolder { get; set; }

    [Option("keep-intermediate", Required = false, HelpText = "Keeps the intermediate files (do not cleanup)", Default = true)]
    public bool KeepIntermediateFiles { get; set; }
}