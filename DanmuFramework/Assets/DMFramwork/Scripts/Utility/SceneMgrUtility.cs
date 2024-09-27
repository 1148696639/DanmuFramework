using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMgrUtility : MonoBehaviour
{
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // 开始异步加载场景
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // 显示加载进度
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);


            // 当加载进度到达90%时，准备激活场景
            if (operation.progress >= 0.9f)
            {
                // 加载完成后，激活场景
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
