using System;
using System.Threading.Tasks;
using COSXML.Model.Bucket;
using QFramework;
using UnityEngine;

public class TencentServerSystem : AbstractSystem, ITencentServerSystem
{
    private TencentCosUtility m_TencentCosUtility { get; set; }

    protected override void OnInit()
    {
        this.RegisterEvent<GamePrepareEvent>(OnGamePrepare);
    }

    private void OnGamePrepare(GamePrepareEvent obj)
    {
        Init();
    }

    private async void Init()
    {
        m_TencentCosUtility = await TencentCosGetAsync();
        IsInit.Value = true;
        DebugCtrl.Log("初始化腾讯云系统成功");
    }

    private async Task<TencentCosUtility> TencentCosGetAsync()
    {
        var tcs = new TaskCompletionSource<TencentCosUtility>();

        this.GetSystem<IServerCommunicationSystem>().GetRequestAsync("/api/sts-token", (isSuccess, data) =>
        {
            if (isSuccess)
            {
                var tencentCosUtility = new TencentCosUtility(data);
                tcs.SetResult(tencentCosUtility);
            }
            else
            {
                DebugCtrl.Log("请求sts-token失败");
            }
        });

        return await tcs.Task;
    }

    public BindableProperty<bool> IsInit { get; } = new();

    public void GetFilesInFolder(string bucket, string targetPath, string localPath, Action<bool> onFinish)
    {
        m_TencentCosUtility.GetFilesInFolder(bucket, targetPath, localPath, onFinish);
    }

    public async Task<GetBucketResult> GetBulletResult(string bucket, string targetPath, bool isOnlySon = false)
    {
        return await m_TencentCosUtility.GetBulletResult(bucket, targetPath, isOnlySon);
    }

    public void PutObject(string bucket, string cosPath, string localPath)
    {
        m_TencentCosUtility.PutObject(bucket, cosPath, localPath);
    }
}

public interface ITencentServerSystem : ISystem
{
    BindableProperty<bool> IsInit { get; }
    void GetFilesInFolder(string bucket, string targetPath, string localPath, Action<bool> onFinish);
    Task<GetBucketResult> GetBulletResult(string bucket, string targetPath, bool isOnlySon = false);
    void PutObject(string bucket, string cosPath, string localPath);
}