using System.Collections;
using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DMFramework
{
    public class SceneTransitionManager : AbstractController
    {
        [Header("UI Elements")] public Image loadingProgressBar;

        [Header("最小过渡时间")] public float minTransitionTime = 2.0f; // 最小过渡时间

        public GameObject loadingCanvas;

        private void Awake()
        {
            this.RegisterEvent<SceneChangeEvent>(OnSceneChanged);
        }

        private void OnSceneChanged(SceneChangeEvent obj)
        {
            LoadScene(obj.SceneName);
        }

        /// <summary>
        ///     异步加载场景并播放过渡动画
        /// </summary>
        /// <param name="sceneName">目标场景名称</param>
        private void LoadScene(string sceneName)
        {
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            loadingCanvas.SetActive(true);
            // 开始加载场景并记录时间
            var startTime = Time.time;
            var operation = SceneManager.LoadSceneAsync(sceneName);
            if (operation == null)
            {
                DebugCtrl.LogError("场景加载失败：" + sceneName);
                yield break;
            }

            operation.allowSceneActivation = false; // 加载完也暂时不激活场景

            var targetProgress = 0f;

            // 更新进度条，确保加载过程与时间同步
            while (!operation.isDone)
            {
                targetProgress = Mathf.Clamp01(operation.progress / 0.9f); // 0.9是异步加载最大值

                // 计算当前加载持续的时间
                var elapsedTime = Time.time - startTime;

                // 计算期望的进度，根据最小过渡时间进行插值
                var expectedProgress = Mathf.Clamp01(elapsedTime / minTransitionTime);

                // 实际进度是异步加载的进度和时间进度之间的最小值
                var displayProgress = Mathf.Min(targetProgress, expectedProgress);

                // 更新进度条
                loadingProgressBar.fillAmount = displayProgress;

                if (targetProgress >= 1f && elapsedTime >= minTransitionTime)
                {
                    // 当加载和过渡时间都完成时，激活场景
                    operation.allowSceneActivation = true;
                    break;
                }

                yield return null;
            }
            loadingCanvas.SetActive(false);
        }
    }
}