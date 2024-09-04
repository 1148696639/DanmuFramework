using System.Collections.Generic;
using UnityEngine;

public class AverageFpsCounter : MonoBehaviour
{
    public int frameRange = 60; // 要计算的帧数范围
    private List<float> fpsBuffer; // 存储每一帧的FPS
    private float averageFps;

    void Start()
    {
        fpsBuffer = new List<float>(frameRange);
    }

    void Update()
    {
        if (fpsBuffer.Count >= frameRange)
        {
            fpsBuffer.RemoveAt(0); // 如果达到帧数范围，移除最早的帧率数据
        }

        float currentFps = 1.0f / Time.deltaTime;
        fpsBuffer.Add(currentFps);

        averageFps = CalculateAverageFps();
    }

    private float CalculateAverageFps()
    {
        float sum = 0;
        foreach (var fps in fpsBuffer)
        {
            sum += fps;
        }
        return sum / fpsBuffer.Count;
    }

    void OnGUI()
    {
        GUILayout.Label("Average FPS: " + averageFps.ToString("F2"));
    }
}