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
        Debug.Log("开始性能测试");
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
        Debug.Log($"测试结束 - 平均帧率: {_frameCount / _timeElapsed:F2} FPS");
        Debug.Log($"低于60帧的次数: {_below60FpsCount}");
        Debug.Log($"参考系数: {referenceCoefficient*100:F3}%");
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