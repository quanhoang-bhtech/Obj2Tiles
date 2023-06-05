using System.Diagnostics;
using Gltf2Tiles.Model;
using Gltf2Tiles.Stages.Model;
using Obj2Tiles.Library.Geometry;
using SilentWave.Obj2Gltf;

namespace Gltf2Tiles;

public static class Utils
{

    public static BoundingVolume ToBoundingVolume(this Box3 box)
    {
        return new BoundingVolume
        {
            Box = new[] { box.Center.X, -box.Center.Z, box.Center.Y, box.Width / 2, 0, 0, 0, -box.Depth / 2, 0, 0, 0, box.Height / 2 }
        };
    }
    
    public static void ConvertGltf2B3dm(string objPath, string destPath)
    {
        var dir = Path.GetDirectoryName(objPath);
        var name = Path.GetFileNameWithoutExtension(objPath);

        var outputFile = dir != null ? Path.Combine(dir, $"{name}.gltf") : $"{name}.gltf";

        var glbConv = new Gltf2GlbConverter();
        glbConv.Convert(new Gltf2GlbOptions(outputFile));

        var glbFile = Path.ChangeExtension(outputFile, ".glb");

        var b3dm = new B3dm(File.ReadAllBytes(glbFile));

        File.WriteAllBytes(destPath, b3dm.ToBytes());
    }
}