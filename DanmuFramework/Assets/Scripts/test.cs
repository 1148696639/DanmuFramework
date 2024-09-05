using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using QFramework;
using UnityEngine;

public class test : AbstractController
{
    // Start is called before the first frame update
    private TencentCosUtility _tencentCosUtility;

    private void Start()
    {

        //开启dotween动画，先向上移动3个单位，用时2秒，然后向下移动3个单位，用时2秒，循环播放
        transform.DOMove(transform.up * 3, 2).SetLoops(-1, LoopType.Yoyo);

    }

    private async void Update()
    {

        if (Input.GetKeyDown(KeyCode.A))
        {
            // GetComponent<PerformanceTester>().StartTest(60);

            // this.GetSystem<IServerCommunicationSystem>().GetRequestAsync("/java/open/sts-token", (isSuccess, data) =>
            // {
            //     if (isSuccess)
            //     {
            //         Debug.Log(data);
            //         _tencentCosUtility = new TencentCosUtility(data);
            //         // Task.Run(TencentCosTestAsync);
            //         TencentCosTest();
            //     }
            //     else
            //     {
            //         DebugCtrl.Log("请求sts-token失败");
            //     }
            // });



        }



    }

    private async void TencentCosTest()
    {
        isStart = true;
        while (isStart)
        {
            var res= await _tencentCosUtility.GetBulletResult("res-1312097821", "测试", true);
            res.listBucket.commonPrefixesList.ForEach(item =>
            {
               Debug.Log(item.prefix);
            });
            _tencentCosUtility.PutObject("res-1312097821","测试/log5",Application.persistentDataPath+"/Player.log");
            _tencentCosUtility.GetFilesInFolder("res-1312097821","测试/",Application.dataPath, e =>
            {
                if (e)
                {
                    Debug.Log("下载成功,地址为"+Application.dataPath);
                }
            });

            await Task.Delay(5000);
        }
    }


    private bool isStart = false;
    private void OnDestroy()
    {
        //结束所有的异步任务
        isStart = false;

    }
}