﻿using Microsoft.JSInterop;
using SpawnDev.BlazorJS.JSObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawnDev.BlazorJS.FFmpegWasm
{
    public static class FFmpegWasmContent
    {
        public const string FFmpegJSURL = "./_content/SpawnDev.BlazorJS.FFmpegWasm/ffmpeg.js";
        public const string FFmpeg814JSURL = "./_content/SpawnDev.BlazorJS.FFmpegWasm/814.ffmpeg.js";
    }
    public class FFmpeg : JSObject
    {
        public FFmpeg(IJSInProcessObjectReference _ref) : base(_ref) { }
        public FFmpeg() : base(JS.New("FFmpegWASM.FFmpeg")) { }
        public Task<bool> Load(FFMessageLoadConfig config) => JSRef.CallAsync<bool>("load", config);
        public Task<bool> Load() => JSRef.CallAsync<bool>("load");
        public void On(string eventNam, Callback callback) => JSRef.CallVoid("on", eventNam , callback);
        public void Off(string eventNam, Callback callback) => JSRef.CallVoid("off", eventNam, callback);

        public JSEventCallback<FFmpegLogEvent> OnLog { get => new JSEventCallback<FFmpegLogEvent>(o => On("log", o), o => Off("log", o)); set { /** set MUST BE HERE TO ENABLE += -= operands **/ } }
        public JSEventCallback<FFmpegProgressEvent> OnProgress { get => new JSEventCallback<FFmpegProgressEvent>(o => On("progress", o), o => Off("progress", o)); set { /** set MUST BE HERE TO ENABLE += -= operands **/ } }

        /// <summary>
        /// Terminate all ongoing API calls and terminate web worker. FFmpeg.load() must be called again before calling any other APIs
        /// </summary>
        public void Terminate() => JSRef.CallVoid("terminate");

        /// <summary>
        /// Execute ffmpeg command.
        /// </summary>
        /// <param name="args">ffmpeg command line args</param>
        /// <param name="timeout">milliseconds to wait before stopping the command execution. Default Value -1</param>
        /// <returns></returns>
        public Task<int> Exec(IEnumerable<string> args, long timeout) => JSRef.CallAsync<int>("exec", args, timeout);
        /// <summary>
        /// Execute ffmpeg command.
        /// </summary>
        /// <param name="args">ffmpeg command line args</param>
        /// <returns></returns>
        public Task<int> Exec(IEnumerable<string> args) => JSRef.CallAsync<int>("exec", args);

        #region FileSystemMethods
        //
        /// <summary>
        /// Create a directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<bool> CreateDir(string path) => JSRef.CallAsync<bool>("createDir", path);
        /// <summary>
        /// Delete an empty directory.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<bool> DeleteDir(string path) => JSRef.CallAsync<bool>("deleteDir", path);
        /// <summary>
        /// List directory contents.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<FSNode[]> ListDir(string path) => JSRef.CallAsync<FSNode[]>("listDir", path);
        /// <summary>
        /// Rename a file or directory.
        /// </summary>
        /// <param name="oldPath"></param>
        /// <param name="newPath"></param>
        /// <returns></returns>
        public Task<bool> Rename(string oldPath, string newPath) => JSRef.CallAsync<bool>("rename", oldPath, newPath);
        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Task<bool> DeleteFile(string path) => JSRef.CallAsync<bool>("deleteFile", path);
        // Read
        public Task<string> ReadFileUTF8(string path) => JSRef.CallAsync<string>("readFile", path, "utf8");
        public Task<Uint8Array> ReadFileUInt8Array(string path) => JSRef.CallAsync<Uint8Array>("readFile", path, "binary");
        public Task<byte[]> ReadFileBytes(string path) => JSRef.CallAsync<byte[]>("readFile", path, "binary");
        // Write
        public Task<bool> WriteFile(string path, string data) => JSRef.CallAsync<bool>("writeFile", path, data);
        public Task<bool> WriteFile(string path, Uint8Array data) => JSRef.CallAsync<bool>("writeFile", path, data);
        public Task<bool> WriteFile(string path, byte[] data) => JSRef.CallAsync<bool>("writeFile", path, data);
        #endregion
    }
}