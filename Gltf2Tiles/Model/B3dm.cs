using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gltf2Tiles.Model
{
    public class B3dm
    {
        public B3dm()
        {
            B3dmHeader = new B3dmHeader();
            FeatureTableJson = string.Empty;
            BatchTableJson = string.Empty;
            FeatureTableJson = "{\"BATCH_LENGTH\":0}  ";
            FeatureTableBinary = Array.Empty<byte>();
            BatchTableBinary = Array.Empty<byte>();
        }

        public B3dm(byte[] glb) : this()
        {
            GlbData = glb;
        }

        public B3dmHeader B3dmHeader { get; set; }
        public string FeatureTableJson { get; set; }
        public byte[] FeatureTableBinary { get; set; }
        public string BatchTableJson { get; set; }
        public byte[] BatchTableBinary { get; set; }
        public byte[] GlbData { get; set; }

        public byte[] ToBytes()
        {
            var header_length = 28;

            var featureTableJson = BufferPadding.AddPadding(FeatureTableJson, header_length);
            var batchTableJson = BufferPadding.AddPadding(BatchTableJson);
            var featureTableBinary = BufferPadding.AddPadding(FeatureTableBinary);
            var batchTableBinary = BufferPadding.AddPadding(BatchTableBinary);

            B3dmHeader.ByteLength = GlbData.Length + header_length + featureTableJson.Length + Encoding.UTF8.GetByteCount(batchTableJson) + batchTableBinary.Length + FeatureTableBinary.Length;

            B3dmHeader.FeatureTableJsonByteLength = featureTableJson.Length;
            B3dmHeader.BatchTableJsonByteLength = Encoding.UTF8.GetByteCount(batchTableJson);
            B3dmHeader.FeatureTableBinaryByteLength = featureTableBinary.Length;
            B3dmHeader.BatchTableBinaryByteLength = batchTableBinary.Length;

            var memoryStream = new MemoryStream();
            var binaryWriter = new BinaryWriter(memoryStream);
            binaryWriter.Write(B3dmHeader.AsBinary());
            binaryWriter.Write(Encoding.UTF8.GetBytes(featureTableJson));
            if (featureTableBinary != null)
            {
                binaryWriter.Write(featureTableBinary);
            }
            binaryWriter.Write(Encoding.UTF8.GetBytes(batchTableJson));
            if (batchTableBinary != null)
            {
                binaryWriter.Write(batchTableBinary);
            }
            binaryWriter.Write(GlbData);
            binaryWriter.Flush();
            binaryWriter.Close();
            return memoryStream.ToArray();
        }
    }
    public static class BufferPadding
    {
        private static int boundary = 8;
        public static byte[] AddPadding(byte[] bytes, int offset = 0)
        {
            var remainder = (offset + bytes.Length) % boundary;
            var padding = (remainder == 0) ? 0 : boundary - remainder;
            var whitespace = new string(' ', padding);
            var paddingBytes = Encoding.UTF8.GetBytes(whitespace);
            var res = bytes.Concat(paddingBytes);
            return res.ToArray();
        }
        public static string AddPadding(string input, int offset = 0)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var paddedBytes = BufferPadding.AddPadding(bytes, offset);
            var result = Encoding.UTF8.GetString(paddedBytes);
            return result;
        }
    }
    public class B3dmHeader
    {
        public string Magic { get; set; }
        public int Version { get; set; }
        public int ByteLength { get; set; }
        public int FeatureTableJsonByteLength { get; set; }
        public int FeatureTableBinaryByteLength { get; set; }
        public int BatchTableJsonByteLength { get; set; }
        public int BatchTableBinaryByteLength { get; set; }

        public B3dmHeader()
        {
            Magic = "b3dm";
            Version = 1;
            FeatureTableJsonByteLength = 0;
            FeatureTableBinaryByteLength = 0;
            BatchTableJsonByteLength = 0;
            BatchTableBinaryByteLength = 0;
        }

        public B3dmHeader(BinaryReader reader)
        {
            Magic = Encoding.UTF8.GetString(reader.ReadBytes(4));
            Version = (int)reader.ReadUInt32();
            ByteLength = (int)reader.ReadUInt32();

            FeatureTableJsonByteLength = (int)reader.ReadUInt32();
            FeatureTableBinaryByteLength = (int)reader.ReadUInt32();
            BatchTableJsonByteLength = (int)reader.ReadUInt32();
            BatchTableBinaryByteLength = (int)reader.ReadUInt32();
        }

        public int Length
        {
            get
            {
                return 28 + FeatureTableJsonByteLength + FeatureTableBinaryByteLength + BatchTableJsonByteLength + BatchTableBinaryByteLength;
            }
        }

        public byte[] AsBinary()
        {
            var magicBytes = Encoding.UTF8.GetBytes(Magic);
            var versionBytes = BitConverter.GetBytes(Version);
            var byteLengthBytes = BitConverter.GetBytes(ByteLength);
            var featureTableJsonByteLengthBytes = BitConverter.GetBytes(FeatureTableJsonByteLength);
            var featureTableBinaryByteLengthBytes = BitConverter.GetBytes(FeatureTableBinaryByteLength);
            var batchTableJsonByteLength = BitConverter.GetBytes(BatchTableJsonByteLength);
            var batchTableBinaryByteLength = BitConverter.GetBytes(BatchTableBinaryByteLength);

            return magicBytes.
                Concat(versionBytes).
                Concat(byteLengthBytes).
                Concat(featureTableJsonByteLengthBytes).
                Concat(featureTableBinaryByteLengthBytes).
                Concat(batchTableJsonByteLength).
                Concat(batchTableBinaryByteLength).
                ToArray();
        }

        public List<string> Validate()
        {
            var res = new List<string>();

            var headerByteLength = AsBinary().Count();
            var featureTableJsonByteOffset = headerByteLength;
            var featureTableBinaryByteOffset = featureTableJsonByteOffset + FeatureTableJsonByteLength;
            var batchTableJsonByteOffset = featureTableBinaryByteOffset + FeatureTableBinaryByteLength;
            var batchTableBinaryByteOffset = batchTableJsonByteOffset + BatchTableJsonByteLength;
            var glbByteOffset = batchTableBinaryByteOffset + BatchTableBinaryByteLength;

            if (featureTableBinaryByteOffset % 8 > 0)
            {
                res.Add("Feature table binary must be aligned to an 8-byte boundary.");
            }
            if (batchTableBinaryByteOffset % 8 > 0)
            {
                res.Add("Batch table binary must be aligned to an 8-byte boundary.");
            }
            if (glbByteOffset % 8 > 0)
            {
                res.Add("Glb must be aligned to an 8-byte boundary.");
            }

            return res;
        }

    }
}
