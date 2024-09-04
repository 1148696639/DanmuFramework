using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public class WebRequestUtils
{
    // 自动生成并挂载 UnityMainThreadDispatcher 脚本的静态方法
    private static UnityMainThreadDispatcher _autoGenerateDispatcher;

    private readonly int _timeOut;

    /// <summary>
    ///     web请求工具类
    /// </summary>
    /// <param name="timeOut"></param>
    public WebRequestUtils(int timeOut = 3)
    {
        if (_autoGenerateDispatcher == null)
        {
            var go = new GameObject("UnityMainThreadDispatcher");
            _autoGenerateDispatcher = go.AddComponent<UnityMainThreadDispatcher>();
            Object.DontDestroyOnLoad(go);
        }

        _timeOut = timeOut;
    }

    /// <summary>
    ///     Get请求
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    public void GetRequestAsync(string path, Action<string> callback)
    {
        var webRequest = UnityWebRequest.Get(path);
        SendRequestAsync(webRequest, callback);
    }

    public void GetRequest(string path, Action<string> callback)
    {
        var webRequest = UnityWebRequest.Get(path);
        webRequest.SendWebRequest();
        while (!webRequest.isDone)
        {
        }

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            var responseText = webRequest.downloadHandler.text;
            callback.Invoke(responseText);
        }
        else
        {
            Debug.LogError($"{webRequest.method} Request Failed: {webRequest.error}");
        }

        webRequest.Dispose(); // 释放资源
    }

    /// <summary>
    ///     Post请求
    /// </summary>
    /// <param name="path"></param>
    /// <param name="jsonData"></param>
    /// <param name="callback"></param>
    public void PostRequestAsync(string path, string jsonData, Action<string> callback)
    {
        var webRequest = new UnityWebRequest(path, UnityWebRequest.kHttpVerbPOST);
        if (jsonData != null)
        {
            var jsonToSend = new UTF8Encoding().GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
        }

        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.timeout = _timeOut;
        SendRequestAsync(webRequest, callback);
    }

    public void Post(string path)
    {
        DebugCtrl.Log("POST请求: " + path);
        var webRequest = new UnityWebRequest(path, UnityWebRequest.kHttpVerbPOST);
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SendWebRequest();
    }

    private void SendRequestAsync(UnityWebRequest webRequest, Action<string> callback)
    {
        var operation = webRequest.SendWebRequest();
        operation.completed += asyncOp =>
        {
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var responseText = webRequest.downloadHandler.text;
                callback.Invoke(responseText);
                webRequest.Dispose(); // 释放资源
            }
            else
            {
                var error = $"{webRequest.method} Request Failed: {webRequest.error}";
                Debug.LogError(error);
            }
        };
    }

    /// <summary>
    ///     通用的post请求
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="url"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<string> PostRequest<T>(string url, T data)
    {
        using var www = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        DownloadHandler downloadHandler = new DownloadHandlerBuffer();
        www.downloadHandler = downloadHandler;
        www.SetRequestHeader("Content-Type", "application/json;charset=utf-8");
        www.SetRequestHeader("version", Application.version);
        var jsonData = JsonUtility.ToJson(data);
        var bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        var request = www.SendWebRequest();
        while (!request.isDone) await Task.Delay(10);
        if (www.result == UnityWebRequest.Result.Success)
            return www.downloadHandler.text;
        throw new Exception($"Request failed with error: {www.error}");
    }
}