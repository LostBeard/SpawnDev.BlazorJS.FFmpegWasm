using SpawnDev.BlazorJS.JsonConverters;
using System.Text.Json.Serialization;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    /// <summary>
    /// ffmpeg.wasm file system types<br/>
    /// https://ffmpegwasm.netlify.app/docs/api/ffmpeg/enums/fffstype/
    /// </summary>
    [JsonConverter(typeof(EnumStringConverterFactory))]
    public enum FFFSType
    {
        /// <summary>
        /// IDBFS
        /// </summary>
        [JsonPropertyName("IDBFS")]
        IDBFS,
        /// <summary>
        /// MEMFS
        /// </summary>
        [JsonPropertyName("MEMFS")]
        MEMFS,
        /// <summary>
        /// NODEFS
        /// </summary>
        [JsonPropertyName("NODEFS")]
        NODEFS,
        /// <summary>
        /// NODERAWFS
        /// </summary>
        [JsonPropertyName("NODERAWFS")]
        NODERAWFS,
        /// <summary>
        /// PROXYFS
        /// </summary>
        [JsonPropertyName("PROXYFS")]
        PROXYFS,
        /// <summary>
        /// WORKERFS
        /// </summary>
        [JsonPropertyName("WORKERFS")]
        WORKERFS,
    }
}
