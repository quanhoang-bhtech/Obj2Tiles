using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using Obj2Tiles.Library.Materials;
using SixLabors.ImageSharp;

namespace Obj2Tiles.Library.Geometry;

public class Mesh : IMesh
{
    private List<Vertex3> _vertices;
    private readonly List<Face> _faces;
    private List<Vertex3> _verticesNormal;
    public IReadOnlyList<Vertex3> VerticesNormal => _verticesNormal;
    public IReadOnlyList<Vertex3> Vertices => _vertices;
    public IReadOnlyList<Face> Faces => _faces;

    public const string DefaultName = "Mesh";

    public string Name { get; set; } = DefaultName;

    public Mesh(IEnumerable<Vertex3> vertices, IEnumerable<Vertex3> verticesNormal, IEnumerable<Face> faces)
    {
        _vertices = new List<Vertex3>(vertices);
        _verticesNormal = new List<Vertex3>(verticesNormal);
        _faces = new List<Face>(faces);
    }

    public int Split(IVertexUtils utils, double q, out IMesh left,
        out IMesh right)
    {
        var leftVertices = new Dictionary<Vertex3, int>(_vertices.Count);
        var rightVertices = new Dictionary<Vertex3, int>(_vertices.Count);

        var leftVerticesNormal = new Dictionary<Vertex3, int>(_verticesNormal.Count);
        var rightVerticesNormal = new Dictionary<Vertex3, int>(_verticesNormal.Count);

        var leftFaces = new List<Face>(_faces.Count);
        var rightFaces = new List<Face>(_faces.Count);

        var count = 0;

        for (var index = 0; index < _faces.Count; index++)
        {
            var face = _faces[index];

            var vA = _vertices[face.IndexA];
            var vB = _vertices[face.IndexB];
            var vC = _vertices[face.IndexC];

            var aSide = utils.GetDimension(vA) < q;
            var bSide = utils.GetDimension(vB) < q;
            var cSide = utils.GetDimension(vC) < q;

            if (aSide)
            {
                if (bSide)
                {
                    if (cSide)
                    {
                        // All on the left

                        var indexALeft = leftVertices.AddIndex(vA);
                        var indexBLeft = leftVertices.AddIndex(vB);
                        var indexCLeft = leftVertices.AddIndex(vC);
                        if (face.HasNormal())
                        {
                            var vnA = _verticesNormal[face.NormalA];
                            var vnB = _verticesNormal[face.NormalB];
                            var vnC = _verticesNormal[face.NormalC];
                            var indexNormalALeft = leftVerticesNormal!.AddIndex(vnA);
                            var indexNormalBLeft = leftVerticesNormal!.AddIndex(vnB);
                            var indexNormalCLeft = leftVerticesNormal!.AddIndex(vnC);
                            leftFaces.Add(new Face(indexALeft, indexBLeft, indexCLeft, indexNormalALeft, indexNormalBLeft, indexNormalCLeft));
                        }
                        else
                        {
                            leftFaces.Add(new Face(indexALeft, indexBLeft, indexCLeft, 0, 0, 0));
                        }
                    }
                    else
                    {
                        IntersectRight2D(utils, q, face.IndexC, face.IndexA, face.IndexB,
                            face.NormalC, face.NormalA, face.NormalB
                            , leftVertices, rightVertices, leftVerticesNormal, rightVerticesNormal,
                            leftFaces, rightFaces);
                        count++;
                    }
                }
                else
                {
                    if (cSide)
                    {
                        IntersectRight2D(utils, q, face.IndexB, face.IndexC, face.IndexA,
                            face.NormalB, face.NormalC, face.NormalA,
                            leftVertices, rightVertices, leftVerticesNormal, rightVerticesNormal,
                            leftFaces, rightFaces);
                        count++;
                    }
                    else
                    {
                        IntersectLeft2D(utils, q, face.IndexA, face.IndexB, face.IndexC,
                            face.NormalA, face.NormalB, face.NormalC,
                            leftVertices, rightVertices, leftVerticesNormal, rightVerticesNormal,
                            leftFaces, rightFaces);
                        count++;
                    }
                }
            }
            else
            {
                if (bSide)
                {
                    if (cSide)
                    {
                        IntersectRight2D(utils, q, face.IndexA, face.IndexB, face.IndexC,
                            face.NormalA, face.NormalB, face.NormalC,
                            leftVertices, rightVertices, leftVerticesNormal, rightVerticesNormal,
                            leftFaces, rightFaces);
                        count++;
                    }
                    else
                    {
                        IntersectLeft2D(utils, q, face.IndexB, face.IndexC, face.IndexA,
                            face.NormalB, face.NormalC, face.NormalA,
                            leftVertices, rightVertices, leftVerticesNormal, rightVerticesNormal,
                            leftFaces, rightFaces);
                        count++;
                    }
                }
                else
                {
                    if (cSide)
                    {
                        IntersectLeft2D(utils, q, face.IndexC, face.IndexA, face.IndexB,
                            face.NormalB, face.NormalC, face.NormalA,
                            leftVertices, rightVertices, leftVerticesNormal, rightVerticesNormal,
                            leftFaces, rightFaces);
                        count++;
                    }
                    else
                    {
                        var indexARight = rightVertices.AddIndex(vA);
                        var indexBRight = rightVertices.AddIndex(vB);
                        var indexCRight = rightVertices.AddIndex(vC);
                        // All on the right
                        if (face.HasNormal())
                        {
                            var vnA = _verticesNormal[face.NormalA];
                            var vnB = _verticesNormal[face.NormalB];
                            var vnC = _verticesNormal[face.NormalC];
                            var indexNormalARight = rightVerticesNormal!.AddIndex(vnA);
                            var indexNormalBRight = rightVerticesNormal!.AddIndex(vnB);
                            var indexNormalCRight = rightVerticesNormal!.AddIndex(vnC);
                            rightFaces.Add(new Face(indexARight, indexBRight, indexCRight, indexNormalARight, indexNormalBRight, indexNormalCRight));
                        }
                        else
                        {
                            rightFaces.Add(new Face(indexARight, indexBRight, indexCRight, 0, 0, 0));
                        }
                    }
                }
            }
        }

        var orderedLeftVertices = leftVertices.OrderBy(x => x.Value).Select(x => x.Key);
        var orderedRightVertices = rightVertices.OrderBy(x => x.Value).Select(x => x.Key);
        var orderedLeftVerticesNormal = leftVerticesNormal.OrderBy(x => x.Value).Select(x => x.Key);
        var orderedRightVerticesNormal = rightVerticesNormal.OrderBy(x => x.Value).Select(x => x.Key);
        left = new Mesh(orderedLeftVertices, orderedLeftVerticesNormal, leftFaces)
        {
            Name = $"{Name}-{utils.Axis}L"
        };

        right = new Mesh(orderedRightVertices, orderedRightVerticesNormal, rightFaces)
        {
            Name = $"{Name}-{utils.Axis}R"
        };

        return count;
    }

    private void IntersectLeft2D(IVertexUtils utils, double q, int indexVL, int indexVR1, int indexVR2,
        int indexNormalVR, int indexNormalVL1, int indexNormalVL2,
        IDictionary<Vertex3, int> leftVertices,
        IDictionary<Vertex3, int> rightVertices,
        IDictionary<Vertex3, int> leftVerticesNormal,
        IDictionary<Vertex3, int> rightVerticesNormal, ICollection<Face> leftFaces,
        ICollection<Face> rightFaces)
    {
        var vL = _vertices[indexVL];
        var vR1 = _vertices[indexVR1];
        var vR2 = _vertices[indexVR2];

        var vnVR = _verticesNormal[indexNormalVR];
        var vnVL1 = _verticesNormal[indexNormalVL1];
        var vnVL2 = _verticesNormal[indexNormalVL2];
        var indexVLLeft = leftVertices.AddIndex(vL);

        if (Math.Abs(utils.GetDimension(vR1) - q) < Common.Epsilon &&
            Math.Abs(utils.GetDimension(vR2) - q) < Common.Epsilon)
        {
            // Right Vertices are on the line

            var indexVR1Left = leftVertices.AddIndex(vR1);
            var indexVR2Left = leftVertices.AddIndex(vR2);

            var indexNormalALeft = leftVerticesNormal!.AddIndex(vnVR);
            var indexNormalBLeft = leftVerticesNormal!.AddIndex(vnVL1);
            var indexNormalCLeft = leftVerticesNormal!.AddIndex(vnVL2);
            leftFaces.Add(new Face(indexVLLeft, indexVR1Left, indexVR2Left, indexNormalALeft, indexNormalBLeft, indexNormalCLeft));
            //leftFaces.Add(new Face(indexVLLeft, indexVR1Left, indexVR2Left, 0, 0, 0));
            return;
        }

        var indexVR1Right = rightVertices.AddIndex(vR1);
        var indexVR2Right = rightVertices.AddIndex(vR2);

        // a on the left, b and c on the right

        // Prima intersezione
        var t1 = utils.CutEdge(vL, vR1, q);
        var indexT1Left = leftVertices.AddIndex(t1);
        var indexT1Right = rightVertices.AddIndex(t1);

        // Seconda intersezione
        var t2 = utils.CutEdge(vL, vR2, q);
        var indexT2Left = leftVertices.AddIndex(t2);
        var indexT2Right = rightVertices.AddIndex(t2);

        var lindexNormalALeft = leftVerticesNormal!.AddIndex(vnVR);
        var lindexNormalBLeft = leftVerticesNormal!.AddIndex(vnVL1);
        var lindexNormalCLeft = leftVerticesNormal!.AddIndex(vnVL2);

        var lface = new Face(indexVLLeft, indexT1Left, indexT2Left, lindexNormalALeft, lindexNormalBLeft, lindexNormalCLeft);
        //var lface = new Face(indexVLLeft, indexT1Left, indexT2Left, 0, 0, 0);
        leftFaces.Add(lface);

        var lindexNormalARight = rightVerticesNormal!.AddIndex(vnVR);
        var lindexNormalBRight = rightVerticesNormal!.AddIndex(vnVL1);
        var lindexNormalCRight = rightVerticesNormal!.AddIndex(vnVL2);

        var rface1 = new Face(indexT1Right, indexVR1Right, indexVR2Right, lindexNormalARight, lindexNormalBRight, lindexNormalCRight);
        //var rface1 = new Face(indexT1Right, indexVR1Right, indexVR2Right, 0, 0, 0);
        rightFaces.Add(rface1);

        var rface2 = new Face(indexT1Right, indexVR2Right, indexT2Right, lindexNormalARight, lindexNormalBRight, lindexNormalCRight);
        //var rface2 = new Face(indexT1Right, indexVR2Right, indexT2Right, 0, 0, 0);
        rightFaces.Add(rface2);
    }

    private void IntersectRight2D(IVertexUtils utils, double q, int indexVR, int indexVL1, int indexVL2,
         int indexNormalVR, int indexNormalVL1, int indexNormalVL2,
        IDictionary<Vertex3, int> leftVertices, IDictionary<Vertex3, int> rightVertices,
         IDictionary<Vertex3, int> leftVerticesNormal,
        IDictionary<Vertex3, int> rightVerticesNormal,
        ICollection<Face> leftFaces, ICollection<Face> rightFaces)
    {
        var vR = _vertices[indexVR];
        var vL1 = _vertices[indexVL1];
        var vL2 = _vertices[indexVL2];

        var vnVR = _verticesNormal[indexNormalVR];
        var vnVL1 = _verticesNormal[indexNormalVL1];
        var vnVL2 = _verticesNormal[indexNormalVL2];
        var indexVRRight = rightVertices.AddIndex(vR);

        if (Math.Abs(utils.GetDimension(vL1) - q) < Common.Epsilon &&
            Math.Abs(utils.GetDimension(vL2) - q) < Common.Epsilon)
        {
            // Left Vertices are on the line

            var indexVL1Right = rightVertices.AddIndex(vL1);
            var indexVL2Right = rightVertices.AddIndex(vL2);

            var indexNormalARight = rightVerticesNormal!.AddIndex(vnVR);
            var indexNormalBRight = rightVerticesNormal!.AddIndex(vnVL1);
            var indexNormalCRight = rightVerticesNormal!.AddIndex(vnVL2);

            rightFaces.Add(new Face(indexVRRight, indexVL1Right, indexVL2Right, indexNormalARight, indexNormalBRight, indexNormalCRight));
            //rightFaces.Add(new Face(indexVRRight, indexVL1Right, indexVL2Right, 0, 0, 0));

            return;
        }

        var indexVL1Left = leftVertices.AddIndex(vL1);
        var indexVL2Left = leftVertices.AddIndex(vL2);

        // a on the right, b and c on the left

        // Prima intersezione
        var t1 = utils.CutEdge(vR, vL1, q);
        var indexT1Left = leftVertices.AddIndex(t1);
        var indexT1Right = rightVertices.AddIndex(t1);

        // Seconda intersezione
        var t2 = utils.CutEdge(vR, vL2, q);
        var indexT2Left = leftVertices.AddIndex(t2);
        var indexT2Right = rightVertices.AddIndex(t2);

        var lindexNormalARight = rightVerticesNormal!.AddIndex(vnVR);
        var lindexNormalBRight = rightVerticesNormal!.AddIndex(vnVL1);
        var lindexNormalCRight = rightVerticesNormal!.AddIndex(vnVL2);

        var rface = new Face(indexVRRight, indexT1Right, indexT2Right, lindexNormalARight, lindexNormalBRight, lindexNormalCRight);
        //var rface = new Face(indexVRRight, indexT1Right, indexT2Right, 0, 0, 0);
        rightFaces.Add(rface);

        var lindexNormalALeft = leftVerticesNormal!.AddIndex(vnVR);
        var lindexNormalBLeft = leftVerticesNormal!.AddIndex(vnVL1);
        var lindexNormalCLeft = leftVerticesNormal!.AddIndex(vnVL2);

        var lface1 = new Face(indexT2Left, indexVL1Left, indexVL2Left, lindexNormalALeft, lindexNormalBLeft, lindexNormalCLeft);
        //var lface1 = new Face(indexT2Left, indexVL1Left, indexVL2Left, 0, 0, 0);
        leftFaces.Add(lface1);

        var lface2 = new Face(indexT2Left, indexT1Left, indexVL1Left, lindexNormalALeft, lindexNormalBLeft, lindexNormalCLeft);
        //var lface2 = new Face(indexT2Left, indexT1Left, indexVL1Left, 0, 0, 0);
        leftFaces.Add(lface2);
    }

    #region Utils

    public Box3 Bounds
    {
        get
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var minZ = double.MaxValue;

            var maxX = double.MinValue;
            var maxY = double.MinValue;
            var maxZ = double.MinValue;

            for (var index = 0; index < _vertices.Count; index++)
            {
                var v = _vertices[index];
                minX = minX < v.X ? minX : v.X;
                minY = minY < v.Y ? minY : v.Y;
                minZ = minZ < v.Z ? minZ : v.Z;

                if (v.X > maxX)
                {
                    maxX = v.X;
                }
                if (v.Y > maxY)
                {
                    maxY = v.Y;
                }
                if (v.Z > maxZ)
                {
                    maxZ = v.Z;
                }

            }

            return new Box3(minX, minY, minZ, maxX, maxY, maxZ);
        }
    }

    public Vertex3 GetVertexBaricenter()
    {
        var x = 0.0;
        var y = 0.0;
        var z = 0.0;

        for (var index = 0; index < _vertices.Count; index++)
        {
            var v = _vertices[index];
            x += v.X;
            y += v.Y;
            z += v.Z;
        }

        x /= _vertices.Count;
        y /= _vertices.Count;
        z /= _vertices.Count;

        return new Vertex3(x, y, z);
    }

    public void WriteObj(string path, bool removeUnused = false)
    {

        if (removeUnused) RemoveUnusedVertices();

        using var writer = new FormattingStreamWriter(path, CultureInfo.InvariantCulture);

        writer.Write("o ");
        writer.WriteLine(string.IsNullOrWhiteSpace(Name) ? DefaultName : Name);

        for (var index = 0; index < _vertices.Count; index++)
        {
            var vertex = _vertices[index];
            writer.Write("v ");
            writer.Write(vertex.X);
            writer.Write(" ");
            writer.Write(vertex.Y);
            writer.Write(" ");
            writer.WriteLine(vertex.Z);
        }
        for (var index = 0; index < _verticesNormal.Count; index++)
        {
            var verticeNormal = _verticesNormal[index];
            writer.Write("vn ");
            writer.Write(verticeNormal.X);
            writer.Write(" ");
            writer.Write(verticeNormal.Y);
            writer.Write(" ");
            writer.WriteLine(verticeNormal.Z);
        }
        for (var index = 0; index < _faces.Count; index++)
        {
            var face = _faces[index];
            writer.WriteLine(face.ToObj());
        }
    }

    private void RemoveUnusedVertices()
    {

        var newVertexes = new Dictionary<Vertex3, int>(_vertices.Count);

        for (var f = 0; f < _faces.Count; f++)
        {
            var face = _faces[f];

            var vA = _vertices[face.IndexA];
            var vB = _vertices[face.IndexB];
            var vC = _vertices[face.IndexC];

            if (!newVertexes.TryGetValue(vA, out var newVA))
                newVA = newVertexes.AddIndex(vA);

            face.IndexA = newVA;

            if (!newVertexes.TryGetValue(vB, out var newVB))
                newVB = newVertexes.AddIndex(vB);

            face.IndexB = newVB;

            if (!newVertexes.TryGetValue(vC, out var newVC))
                newVC = newVertexes.AddIndex(vC);

            face.IndexC = newVC;

        }

        _vertices = newVertexes.Keys.ToList();

    }

    public int FacesCount => _faces.Count;
    public int VertexCount => _vertices.Count;

    #endregion


}