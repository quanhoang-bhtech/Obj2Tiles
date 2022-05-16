﻿using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Obj2Tiles.Library.Geometry;
using Obj2Tiles.Stages.Model;
using Obj2Tiles.Tiles;
using SilentWave;
using SilentWave.Obj2Gltf;

namespace Obj2Tiles.Stages;

public static partial class StagesFacade
{
    public static void Tile(string sourcePath, string destPath, int lods, GpsCoords? coords = null)
    {
        coords ??= DefaultGpsCoords;

        ConvertAllB3dm(sourcePath, destPath, lods);

        const int baseError = 1024;

        // Generate tileset.json
        var tileset = new Tileset
        {
            Asset = new Asset { Version = "1.0" },
            GeometricError = baseError,
            Root = new TileElement
            {
                GeometricError = baseError,
                Refine = "ADD",
                Transform = new[]
                {
                    0.9686356343768792,
                    0.24848542777253735,
                    0,
                    0,
                    -0.15986460744966327,
                    0.623177611820219,
                    0.765567091384559,
                    0,
                    0.19023226619126932,
                    -0.7415555652213445,
                    0.6433560667227647,
                    0,
                    1215011.9317263428,
                    -4736309.3434217675,
                    4081602.0044800863,
                    1
                },
                Content = null,
                Children = new List<TileElement>()
            }
        };

        var masterDescriptors = Directory.GetFiles(Path.Combine(destPath, "LOD-0"), "*.json");

        var maxX = double.MinValue;
        var minX = double.MaxValue;
        var maxY = double.MinValue;
        var minY = double.MaxValue;
        var maxZ = double.MinValue;
        var minZ = double.MaxValue;
        
        foreach (var descriptor in masterDescriptors)
        {
            var currentTileElement = tileset.Root;

            for (var lod = lods - 1; lod >= 0; lod--)
            {
                var box3 = JsonConvert.DeserializeObject<BoxDTO>(File.ReadAllText(descriptor));
                
                if (box3.Min.X < minX)
                    minX = box3.Min.X;

                if (box3.Max.X > maxX)
                    maxX = box3.Max.X;
                
                if (box3.Min.Y < minY)
                    minY = box3.Min.Y;
                
                if (box3.Max.Y > maxY)
                    maxY = box3.Max.Y;
                
                if (box3.Min.Z < minZ)
                    minZ = box3.Min.Z;
                
                if (box3.Max.Z > maxZ)
                    maxZ = box3.Max.Z;
                
                var tile = new TileElement
                {
                    GeometricError = baseError / (1 << lod),
                    Refine = "REPLACE",
                    Children = new List<TileElement>(),
                    Content = new Content
                    {
                        Uri = $"LOD-{lod}/{Path.GetFileNameWithoutExtension(descriptor)}.b3dm"
                    },
                    BoundingVolume = box3.ToBoundingVolume()
                };

                currentTileElement.Children.Add(tile);
                currentTileElement = tile;
            }
        }

        var globalBox = new BoxDTO(new Box3(minX, minY, minZ, maxX, maxY, maxZ));

        tileset.Root.BoundingVolume = globalBox.ToBoundingVolume();

        File.WriteAllText(Path.Combine(destPath, "tileset.json"),
            JsonConvert.SerializeObject(tileset, Formatting.Indented));
    }

    private static void ConvertAllB3dm(string sourcePath, string destPath, int lods)
    {
        var filesToConvert = new List<Tuple<string, string>>();

        for (var lod = 0; lod < lods; lod++)
        {
            var files = Directory.GetFiles(Path.Combine(sourcePath, "LOD-" + lod), "*.obj");

            foreach (var file in files)
            {
                var outputFolder = Path.Combine(destPath, "LOD-" + lod);
                Directory.CreateDirectory(outputFolder);

                var outputFile = Path.Combine(outputFolder, Path.ChangeExtension(Path.GetFileName(file), ".b3dm"));
                filesToConvert.Add(new Tuple<string, string>(file, outputFile));
                File.Copy(Path.ChangeExtension(file, ".json"), Path.ChangeExtension(outputFile, ".json"), true);
            }
        }

        Parallel.ForEach(filesToConvert, (file) => { ConvertB3dm(file.Item1, file.Item2); });
    }

    private static readonly GpsCoords DefaultGpsCoords = new()
    {
        Altitude = 0, // 120
        Latitude = 45.479,
        Longitude = 9.155
    };


    private static void ConvertB3dm(string objPath, string destPath)
    {
        var dir = Path.GetDirectoryName(objPath);
        var name = Path.GetFileNameWithoutExtension(objPath);

        var converter = Converter.MakeDefault();
        var outputFile = dir != null ? Path.Combine(dir, $"{name}.gltf") : $"{name}.gltf";

        converter.Convert(objPath, outputFile);

        var glbConv = new Gltf2GlbConverter();
        glbConv.Convert(new Gltf2GlbOptions(outputFile));

        File.Delete(outputFile);

        var glbFile = Path.ChangeExtension(outputFile, ".glb");

        var b3dm = new B3dm(File.ReadAllBytes(glbFile));

        File.WriteAllBytes(destPath, b3dm.ToBytes());
    }
}

public class GpsCoords
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Altitude { get; set; }
}