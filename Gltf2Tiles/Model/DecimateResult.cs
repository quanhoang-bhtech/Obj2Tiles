﻿using Obj2Tiles.Library.Geometry;

namespace Gltf2Tiles.Stages.Model
{
    public class DecimateResult
    {
        public string[] DestFiles { get; set; }
        public Box3 Bounds { get; set; }
    }
}