using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class DownloadHelper : MonoBehaviour
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

    /// <summary>
    /// 异步下载头像
    /// </summary>
    /// <param name="avatarURL">头像地址</param>
    /// <param name="onComplete">完成回调</param>
    /// <param name="maxRetryCount">最大的尝试次数</param>
    /// <param name="retryInterval">重试间隔</param>
    /// <returns></returns>
    public static async Task<Sprite> DownloadAvatarAsync(string avatarURL, int maxRetryCount = 3, float retryInterval = 1f)
    {
        Texture2D texture = null;
        int attempts = 0;

        while (attempts < maxRetryCount)
        {
            attempts++;
            await Task.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(0f, 1f)));

            using (var www = UnityWebRequestTexture.GetTexture(avatarURL))
            {
                var asyncOperation = www.SendWebRequest();

                while (!asyncOperation.isDone)
                {
                    await Task.Yield(); // 等待直到请求完成
                }

                if (www.result == UnityWebRequest.Result.Success)
                {
                    texture = DownloadHandlerTexture.GetContent(www);
                    break; // 下载成功，跳出循环
                }
                else
                {
                    Debug.LogWarning($"Avatar download failed: {www.error}");
                    if (attempts < maxRetryCount)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(retryInterval)); // 等待重试
                    }
                }
            }
        }

        // 如果 texture 为 null，返回 null，否则创建并返回 Sprite
        return texture != null ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f) : null;
    }
}