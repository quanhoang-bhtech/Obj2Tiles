﻿namespace Obj2Tiles.Library.Geometry;

public interface IMesh
{
    string Name { get; set; }
    Box3 Bounds { get; }

    int Split(IVertexUtils utils, double q, out IMesh left,
        out IMesh right);

    Vertex3 GetVertexBaricenter();
    void WriteObj(string path, bool removeUnused = false);
    
    int FacesCount { get; }
    int VertexCount { get; }
}