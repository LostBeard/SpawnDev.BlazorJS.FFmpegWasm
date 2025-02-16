using System.Text.Json.Serialization;
using File = SpawnDev.BlazorJS.JSObjects.File;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    /// <summary>
    /// Options used when mounting an FFmpeg worker file system
    /// </summary>
    public class FSMountWorkerFSOptions : FSMountOptions
    {
        /// <summary>
        /// Files to mount
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<File>? Files { get; set; }
        /// <summary>
        /// Blobs to mount
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<WorkerFSBlobEntry>? Blobs { get; set; }
    }
}
