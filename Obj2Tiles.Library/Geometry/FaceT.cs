namespace Obj2Tiles.Library.Geometry;

public class FaceT : Face
{

    public int TextureIndexA;
    public int TextureIndexB;
    public int TextureIndexC;

    public int MaterialIndex;

    public override string ToString()
    {
        return $"{IndexA} {IndexB} {IndexC} | {TextureIndexA} {TextureIndexB} {TextureIndexC} | {MaterialIndex}";
    }

    public FaceT(int indexA, int indexB, int indexC, int textureIndexA, int textureIndexB,
        int textureIndexC, int materialIndex, int normalA, int normalB, int normalC) : base(indexA, indexB, indexC, normalA, normalB, normalC)
    {

        TextureIndexA = textureIndexA;
        TextureIndexB = textureIndexB;
        TextureIndexC = textureIndexC;

        MaterialIndex = materialIndex;
    }

    public override string ToObj()
    {
        if (HasNormal())
        {
            return $"f {IndexA + 1}/{TextureIndexA + 1}/{NormalA + 1} {IndexB + 1}/{TextureIndexB + 1}/{NormalB + 1} {IndexC + 1}/{TextureIndexC + 1}/{NormalC + 1}";
        }
        return $"f {IndexA + 1}/{TextureIndexA + 1} {IndexB + 1}/{TextureIndexB + 1} {IndexC + 1}/{TextureIndexC + 1}";
    }
}