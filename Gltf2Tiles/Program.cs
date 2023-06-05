using CommandLine;
using Gltf2Tiles.Stages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Obj2Tiles.Library.Geometry;
using Obj2Tiles.Stages.Model;
using System.Diagnostics;
using System.Xml;

namespace Gltf2Tiles
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var oResult = await Parser.Default.ParseArguments<Options>(args).WithParsedAsync(Run);

            if (oResult.Tag == ParserResultType.NotParsed)
            {
                Console.WriteLine("Usage: gltf2tiles [options]");
            }
        }
        private static async Task Run(Options opts)
        {
            Console.WriteLine();
            Console.WriteLine(" *** Gltf to Tiles ***");
            Console.WriteLine();

            if (!CheckOptions(opts)) return;

            opts.Output = Path.GetFullPath(opts.Output);
            opts.Input = Path.GetFullPath(opts.Input);

            Directory.CreateDirectory(opts.Output);

            var pipelineId = Guid.NewGuid().ToString();
            var sw = new Stopwatch();
            var swg = Stopwatch.StartNew();

            Func<string, string> createTempFolder = opts.UseSystemTempFolder
                ? s => CreateTempFolder(s, Path.GetTempPath())
                : s => CreateTempFolder(s, Path.Combine(opts.Output, ".temp"));

            string? destFolderDecimation = null;
            string? destFolderSplit = null;

            try
            {
                //var boundsMapper;

                Console.WriteLine(" ?> Splitting stage done in {0}", sw.Elapsed);
                string boundsMapperString = File.ReadAllText(Path.Combine(opts.Input, "boundsMapper.json"));
                var boundsMapperRaw = JsonConvert.DeserializeObject<JArray>(boundsMapperString);
                if (boundsMapperRaw != null)
                {
                    var boundsMapper = new List<Dictionary<string, Box3>>();

                    foreach (JObject o in boundsMapperRaw.Children<JObject>())
                    {
                        //make new dictionary
                        var box3Arr = new Dictionary<string, Box3>();

                        foreach (JProperty p in o.Properties())
                        {
                            string name = p.Name;
                            var value = p.Value;
                            if (value != null)
                            {
                                var min = JsonConvert.DeserializeObject<Vertex3>(value["Min"].ToString());
                                var max = JsonConvert.DeserializeObject<Vertex3>(value["Max"].ToString());
                                var box3 = new Box3(min, max);
                                box3Arr.Add(key: name, value: box3);
                            }
                        }

                        //add dictionary to list
                        boundsMapper.Add(box3Arr);
                    }
                    var gpsCoords = opts.Latitude != null && opts.Longitude != null
                        ? new GpsCoords(opts.Latitude.Value, opts.Longitude.Value, opts.Altitude)
                        : null;

                    Console.WriteLine();
                    Console.WriteLine($" => Tiling stage {(gpsCoords != null ? $"with GPS coords {gpsCoords}" : "")}");

                    sw.Restart();
                    StagesFacade.Tile(opts.Input, opts.Output, opts.LODs, boundsMapper.ToArray(), gpsCoords);

                    Console.WriteLine(" ?> Tiling stage done in {0}", sw.Elapsed);
                }
                Console.WriteLine(" ?> Missing bound box data");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" !> Exception: {0}", ex.Message);
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine(" => Pipeline completed in {0}", swg.Elapsed);

                var tmpFolder = Path.Combine(opts.Output, ".temp");

                if (opts.KeepIntermediateFiles)
                {
                    Console.WriteLine(
                        $" ?> Skipping cleanup, intermediate files are in '{tmpFolder}' with pipeline id '{pipelineId}'");

                    Console.WriteLine(" ?> You should delete this folder manually, it is only for debugging purposes");
                }
                else
                {

                    Console.WriteLine(" => Cleaning up");

                    if (destFolderDecimation != null && destFolderDecimation != opts.Output)
                        Directory.Delete(destFolderDecimation, true);

                    if (destFolderSplit != null && destFolderSplit != opts.Output)
                        Directory.Delete(destFolderSplit, true);

                    if (Directory.Exists(tmpFolder))
                        Directory.Delete(tmpFolder, true);

                    Console.WriteLine(" ?> Cleaning up ok");
                }
            }
        }

        private static bool CheckOptions(Options opts)
        {

            if (string.IsNullOrWhiteSpace(opts.Input))
            {
                Console.WriteLine(" !> Input file is required");
                return false;
            }

            //if (!File.Exists(opts.Input))
            //{
            //    Console.WriteLine(" !> Input file does not exist");
            //    return false;
            //}

            if (string.IsNullOrWhiteSpace(opts.Output))
            {
                Console.WriteLine(" !> Output folder is required");
                return false;
            }

            //if (opts.LODs < 1)
            //{
            //    Console.WriteLine(" !> LODs must be at least 1");
            //    return false;
            //}
            return true;
        }


        private static string CreateTempFolder(string folderName, string baseFolder)
        {
            var tempFolder = Path.Combine(baseFolder, folderName);
            Directory.CreateDirectory(tempFolder);
            return tempFolder;
        }
    }
}