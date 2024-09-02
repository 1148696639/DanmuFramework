using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LogUploader
{
    private readonly string _localLogName;
    private readonly string _upLogFileName;
    private readonly Dictionary<string, string> _upLogHeaders;
    private readonly object _fileLock = new();
    private readonly StringBuilder _logBuffer = new();
    private readonly object _logBufferLock = new();
    private readonly string _logFilePath;
    private readonly string _logFolder=Application.persistentDataPath;
    private readonly string _serverURL;
    private DateTime _lastWriteTime;

    public LogUploader(string serverURL, string upLogFileName, string localLogName,
        Dictionary<string, string> upLogHeaders)
    {
        this._serverURL = serverURL;
        _upLogFileName = upLogFileName;
        _upLogHeaders = upLogHeaders;
        DeleteOldLogs();
        _logFilePath = $"{_logFolder}/{localLogName}";
        Application.logMessageReceived += OnLogCallBack;
        _lastWriteTime = DateTime.Now;
    }

    /// <summary>
    ///     实时监测日志并写入文件
    /// </summary>
    public void Update()
    {
        var timeSinceLastWrite = DateTime.Now - _lastWriteTime;
        if (timeSinceLastWrite.TotalSeconds > 10)
        {
            WriteLogToFile();
            _lastWriteTime = DateTime.Now;
        }
    }

    /// <summary>
    ///     产生日志的回调
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="stackTrace"></param>
    /// <param name="type"></param>
    private void OnLogCallBack(string condition, string stackTrace, LogType type)
    {
        lock (_logBufferLock)
        {
            _logBuffer.AppendLine(condition);
            if (type == LogType.Error || type == LogType.Exception) _logBuffer.AppendLine(stackTrace);
        }
    }

    /// <summary>
    ///     写入日志到文件
    /// </summary>
    private async void WriteLogToFile()
    {
        lock (_logBufferLock)
        {
            if (_logBuffer.Length == 0)
                return;
        }

        if (!Directory.Exists(_logFolder)) Directory.CreateDirectory(_logFolder);

        await Task.Run(() =>
        {
            lock (_fileLock)
            {
                Debug.Log("正在将日志写入文件...");
                string logContent;
                lock (_logBufferLock)
                {
                    logContent = _logBuffer.ToString();
                }

                try
                {
                    using (var fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write,
                               FileShare.ReadWrite))
                    {
                        using (var writer = new StreamWriter(fileStream))
                        {
                            writer.Write(logContent);
                        }
                    }

                    lock (_logBufferLock)
                    {
                        _logBuffer.Clear();
                    }

                    Debug.Log("日志成功写入文件。");
                }
                catch (Exception ex)
                {
                    Debug.LogError("写入日志文件时发生错误：" + ex.Message);
                }
            }
        });
    }


    /// <summary>
    ///     删除旧日志
    /// </summary>
    private void DeleteOldLogs()
    {
        var logFiles = Directory.GetFiles(_logFolder, "*.mlog");

        if (logFiles.Length > 3)
        {
            List<(string filePath, DateTime creationTime)> fileInfoList = new();

            foreach (var filePath in logFiles)
            {
                var fileInfo = new FileInfo(filePath);
                fileInfoList.Add((filePath, fileInfo.CreationTime));
            }

            fileInfoList.Sort((a, b) => DateTime.Compare(b.creationTime, a.creationTime));

            for (var i = 3; i < fileInfoList.Count; i++) File.Delete(fileInfoList[i].filePath);
        }
    }

    /// <summary>
    ///     上传最新本地日志
    /// </summary>
    public void UploadNewestLog()
    {
        var logFiles = Directory.GetFiles(_logFolder, "*.mlog");

        var newestLog = "";
        var newestTime = DateTime.MinValue;
        foreach (var filePath in logFiles)
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.CreationTime > newestTime)
            {
                newestTime = fileInfo.CreationTime;
                newestLog = filePath;
            }
        }

        if (!string.IsNullOrEmpty(newestLog)) UploadLog(newestLog);
    }


    /// <summary>
    ///     压缩和上传系统日志
    /// </summary>
    public void CompressionAndUploadLog()
    {
        var logPath = Application.persistentDataPath + "/Player.log";

        if (File.Exists(logPath))
        {
            var tempFilePath = Application.persistentDataPath + "/Temp_Player_Log_Copy.log";
            File.Copy(logPath, tempFilePath, true);
            UploadLog(tempFilePath);
            File.Delete(tempFilePath);
        }
        else
        {
            Debug.LogWarning("Player.log文件找不到!");
        }
    }

    /// <summary>
    ///     上传日志
    /// </summary>
    /// <param name="logFilePath"></param>
    private void UploadLog(string logFilePath)
    {
        var compressedFilePath = CompressLogFile(logFilePath);
        var logBytes = File.ReadAllBytes(compressedFilePath);
        var form = new WWWForm();
        var fileName = $"{_upLogFileName}_{DateTime.Now:yyyy-MM-ddTHH-mm-ss}.zip";
        form.AddBinaryData("file", logBytes, fileName);
        using (var request = UnityWebRequest.Post(_serverURL, form))
        {
            foreach (var header in _upLogHeaders) request.SetRequestHeader(header.Key, header.Value);
            var asyncOperation = request.SendWebRequest();

            while (!asyncOperation.isDone)
            {
            }

            if (request.result == UnityWebRequest.Result.Success)
                Debug.Log("日志上传成功！");
            else
                Debug.LogError("日志上传失败: " + request.error);
        }

        File.Delete(compressedFilePath);
    }

    /// <summary>
    ///     压缩日志文件
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private string CompressLogFile(string filePath)
    {
        var compressedFilePath = filePath + ".zip";

        using (var fs = new FileStream(compressedFilePath, FileMode.Create))
        {
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry(Path.GetFileName(filePath));

                using (var entryStream = entry.Open())
                {
                    using (var fileStream = File.OpenRead(filePath))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }
            }
        }

        return compressedFilePath;
    }
}