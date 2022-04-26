﻿namespace Obj2Tiles.Library.Geometry
{
    public class Box3D
    {
        public readonly Vertex3D Min;
        public readonly Vertex3D Max;
    
        public Box3D(Vertex3D min, Vertex3D max)
        {
            Min = min;
            Max = max;
        }
    
        public Box3D(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
        {
            Min = new Vertex3D(minX, minY, minZ);
            Max = new Vertex3D(maxX, maxY, maxZ);
        }
    
        public double Width => Max.x - Min.x;
        public double Height => Max.y - Min.y;
        public double Depth => Max.z - Min.z;
    
        public Vertex3D Center => new((Min.x + Max.x) / 2, (Min.y + Max.y) / 2, (Min.z + Max.z) / 2);
    
    }
}