using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class URLHelper : MonoBehaviour
{
    /// <summary>
    /// 下载好头像后执行回调事件
    /// </summary>
    /// <param name="avatarURL">头像地址</param>
    /// <param name="onComplete">完成回调</param>
    /// <param name="maxRetryCount">最大的尝试次数</param>
    /// <param name="retryInterval">重试间隔</param>
    /// <returns></returns>
    public static IEnumerator DownloadAvatar(string avatarURL, Action<Sprite> onComplete, int maxRetryCount = 3,
        float retryInterval = 1f)
    {
        yield return new WaitForSeconds(Random.Range(0f, 1f));
        var www = UnityWebRequestTexture.GetTexture(avatarURL);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success)
        {
            // 将下载图片转换为Sprite
            var texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            onComplete?.Invoke(sprite);
        }
        else
        {
            Debug.LogWarning($"Avatar download failed: {www.error}");
            if (maxRetryCount > 0)
            {
                yield return new WaitForSeconds(retryInterval);
                yield return DownloadAvatar(avatarURL, onComplete, maxRetryCount - 1, retryInterval);
            }
            else
            {
                onComplete?.Invoke(null);
            }
        }
    }
}