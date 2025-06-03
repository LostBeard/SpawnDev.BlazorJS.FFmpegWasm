using SpawnDev.BlazorJS.JSObjects;
using System.Text.Json.Serialization;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    /// <summary>
    /// FFMessage options
    /// </summary>
    public class FFMessageOptions
    {
        /// <summary>
        /// Abort signal
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public AbortSignal? Signal { get; set; }
    }
}
