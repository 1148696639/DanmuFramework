using UnityEngine;

public class PerformanceTester : MonoBehaviour
{
    private int _frameCount;
    private float _timeElapsed;
    private int _below60FpsCount;
    private float _testDuration;
    private bool _isTesting;

    // 启动性能测试
    public void StartTest(float duration)
    {
        Debug.Log($"<color=green>开始性能测试...测试时间为：{duration}</color>");
        _testDuration = duration;
        _frameCount = 0;
        _timeElapsed = 0f;
        _below60FpsCount = 0;
        _isTesting = true;
    }

    // 停止性能测试
    private void StopTest()
    {
        _isTesting = false;
        float referenceCoefficient = (float)_below60FpsCount / _frameCount;
        Debug.Log($"<color=green>测试结束 - 平均帧率: {_frameCount / _timeElapsed:F2} FPS</color>");
        Debug.Log($"<color=green>低于60帧的次数: {_below60FpsCount}</color>");
        Debug.Log($"<color=green>参考系数: {referenceCoefficient*100:F3}%</color>");
    }

    // 每帧更新
    private void Update()
    {
        if (!_isTesting) return;

        _frameCount++;
        _timeElapsed += Time.unscaledDeltaTime;

        if (Time.unscaledDeltaTime > 1f / 60f)
        {
            _below60FpsCount++;
        }

        if (_timeElapsed >= _testDuration)
        {
            StopTest();
        }
    }
}