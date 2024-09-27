using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using COSXML.CosException;
using COSXML.Model.Bucket;
using COSXML.Model.Object;
using COSXML.Transfer;
using Newtonsoft.Json;
using QFramework;
using UnityEngine;

/// <summary>
///   腾讯云COS工具类
/// </summary>
public class TencentCosUtility : DefaultSessionQCloudCredentialProvider, IController
{
    private CosXml _cosXml;

    /// <summary>
    ///     过期时间
    /// </summary>
    private DateTime _expiredTime;

    public TencentCosUtility(string data) : base(
        SerializeData(data).secretId,
        SerializeData(data).secretKey,
        SerializeData(data).expiredTime,
        SerializeData(data).token)
    {
        InitializeCosService(SerializeData(data));
    }

    public IArchitecture GetArchitecture()
    {
        return GameArchitecture.Interface;
    }


    /// <summary>
    ///     下载目标文件
    /// </summary>
    /// <param name="bucket">桶名</param>
    /// <param name="targetPath">目标路径</param>
    /// <param name="localPath">本地路径</param>
    /// <param name="onFinish">完成时触发</param>
    public void GetFilesInFolder(string bucket, string targetPath, string localPath, Action<bool> onFinish)
    {
        Task.Run(() => { CheckAndExecute(GetGetFilesInFolderAsync(bucket, targetPath, localPath, onFinish)); });
    }

    private async Task GetGetFilesInFolderAsync(string bucket, string targetPath, string localPath,
        Action<bool> onFinish)
    {
        var taskList = new List<Task>();
        try
        {
            // 获取该文件夹中的文件列表
            var result = await GetBulletResultAsync(bucket, targetPath);
            var files = result.listBucket.contentsList;
            foreach (var file in files)
                if (file.key != targetPath)
                {
                    var filePath = file.key;
                    var downloadTask = new COSXMLDownloadTask(bucket, filePath, localPath,
                        filePath.Substring(targetPath.Length));
                    var transferConfig = new TransferConfig
                    {
                        ByNewFunc = true
                    };
                    var transferManager = new TransferManager(_cosXml, transferConfig);
                    taskList.Add(Task.Run(() => transferManager.DownloadAsync(downloadTask)));
                }

            await Task.WhenAll(taskList);
            onFinish.Invoke(true);
        }
        catch (Exception ex)
        {
            Debug.LogError("下载失败" + ex.Message);
            onFinish.Invoke(false);
        }
    }

    /// <summary>
    ///     异步刷新Token
    /// </summary>
    private async Task RefreshAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        this.GetSystem<IServerCommunicationSystem>().GetRequestAsync("/java/open/sts-token", (isSuccess, data) =>
        {
            if (isSuccess)
            {
                var res = SerializeData(data);
                SetQCloudCredential(res.secretId, res.secretKey,
                    $"{res.startTime - 10};{res.expiredTime}", res.token);
                tcs.SetResult(true);
            }
            else
            {
                DebugCtrl.Log("请求sts-token失败");
                tcs.SetResult(false);
            }
        });

        await tcs.Task;
    }


    /// <summary>
    ///     每次执行方法时检查Token是否过期，如果过期则刷新后再执行
    /// </summary>
    /// <param name="onRefreshFinish"></param>
    private async void CheckAndExecute(Task onRefreshFinish)
    {
        if (CheckTokenIsExpired())
            await RefreshAsync();
        await onRefreshFinish;
    }


    /// <summary>
    ///     检查Token是否过期
    /// </summary>
    /// <returns></returns>
    private bool CheckTokenIsExpired()
    {
        if (DateTime.Now >= _expiredTime)
        {
            UnityMainThreadDispatcher.RunOnMainThread(() => { Debug.Log("Token已过期，重新获取"); });
            return true;
        }

        return false;
    }

    /// <summary>
    ///   获取桶中的信息
    /// </summary>
    /// <param name="bucket"></param>
    /// <param name="targetPath"></param>
    /// <param name="isOnlySon"></param>
    /// <returns></returns>
    public async Task<GetBucketResult> GetBulletResult(string bucket, string targetPath, bool isOnlySon = false)
    {
        if (CheckTokenIsExpired())
            await RefreshAsync();
        return await GetBulletResultAsync(bucket, targetPath, isOnlySon);
    }

    private async Task<GetBucketResult> GetBulletResultAsync(string bucket, string targetPath, bool isOnlySon = false)
    {
        // 使用 TaskCompletionSource 来处理异步操作
        var tcs = new TaskCompletionSource<GetBucketResult>();
        var request = new GetBucketRequest(bucket);
        request.SetPrefix(targetPath);
        if (isOnlySon)
            request.SetDelimiter("/");
        var result = await Task.Run(() =>
        {
            try
            {
                return _cosXml.GetBucket(request);
            }
            catch (Exception e)
            {
                DebugCtrl.LogError(e);
                throw;
            }
        });
        tcs.SetResult(result); // 设置结果
        return await tcs.Task; // 等待并返回任务结果
    }

    /// <summary>
    ///     简单上传对象，不超过5GB
    /// </summary>
    /// <param name="bucket">存储桶名</param>
    /// <param name="cosPath">对象键</param>
    /// <param name="localPath">本地文件绝对路径</param>
    public void PutObject(string bucket, string cosPath, string localPath)
    {
        CheckAndExecute(PutObjectAsync(bucket, cosPath, localPath));
    }

    private async Task PutObjectAsync(string bucket, string cosPath, string localPath)
    {
        await Task.Run(() =>
        {
            try
            {
                var request = new PutObjectRequest(bucket, cosPath, localPath);
                _cosXml.PutObject(request);
                DebugCtrl.Log("上传成功");
            }
            catch (CosClientException clientEx)
            {
                DebugCtrl.LogError("CosClientException: " + clientEx);
            }
            catch (CosServerException serverEx)
            {
                DebugCtrl.LogError("CosServerException: " + serverEx.GetInfo());
            }
        });
    }


    /// <summary>
    ///     初始化COS服务
    /// </summary>
    private void InitializeCosService(StsTokenData data)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dateTime = epoch.AddSeconds(data.expiredTime);
        var localDateTime = dateTime.ToLocalTime();
        Debug.Log("过期时间为：" + localDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
        _expiredTime = localDateTime;
        var region = "ap-shanghai";
        var config = new CosXmlConfig.Builder()
            .IsHttps(true)
            .SetRegion(region)
            .SetDebugLog(true)
            .Build();

        _cosXml = new CosXmlServer(config, this);
    }


    private static StsTokenData SerializeData(string data)
    {
        try
        {
            var stsTokenResponse = JsonConvert.DeserializeObject<StsTokenData>(data);
            return stsTokenResponse;
        }
        catch (Exception e)
        {
            DebugCtrl.LogError("解析STS Token失败: " + e.Message);
            throw;
        }
    }


    public class StsTokenData
    {
        public string secretId;
        public string secretKey;
        public int expiredTime;
        public int startTime;
        public string token;
    }
}