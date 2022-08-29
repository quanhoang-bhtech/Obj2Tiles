namespace Obj2Tiles.Library.Geometry;

public class Face
{

    public int IndexA;
    public int IndexB;
    public int IndexC;
    public int NormalA;
    public int NormalB;
    public int NormalC;
    public override string ToString()
    {
        return $"{IndexA} {IndexB} {IndexC} | {NormalA} {NormalB} {NormalC} |";
    }
    public bool HasNormal()
    {
        return NormalA != 0 && NormalB != 0 && NormalC != 0;
    }
    public Face(int indexA, int indexB, int indexC, int normalA, int normalB = 0, int normalC = 0)
    {
        IndexA = indexA;
        IndexB = indexB;
        IndexC = indexC;
        NormalA = normalA;
        NormalB = normalB;
        NormalC = normalC;
    }

    public virtual string ToObj()
    {
        if (HasNormal())
        {
            return $"f {IndexA + 1}//{NormalA + 1} {IndexB + 1}//{NormalB + 1} {IndexC + 1}//{NormalC + 1}";
        }
        return $"f {IndexA + 1} {IndexB + 1} {IndexC + 1}";
    }
}