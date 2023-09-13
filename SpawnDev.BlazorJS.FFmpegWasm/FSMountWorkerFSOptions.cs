using System.Text.Json.Serialization;
using File = SpawnDev.BlazorJS.JSObjects.File;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    public class FSMountWorkerFSOptions : FSMountOptions
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<File>? Files { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<WorkerFSBlobEntry>? Blobs { get; set; }
    }
}
