using System.Threading.Tasks;
using DG.Tweening;
using QFramework;
using UnityEngine;
namespace DMFramework
{
    public class test : AbstractController
    {
        private bool isStart;
        // Start is called before the first frame update

        private void Start()
        {
            //开启dotween动画，先向上移动3个单位，用时2秒，然后向下移动3个单位，用时2秒，循环播放
            transform.DOMove(transform.up * 3, 2).SetLoops(-1, LoopType.Yoyo);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
                // 测试游戏帧数
                GetComponent<PerformanceTester>().StartTest(60);
            if (Input.GetKeyDown(KeyCode.B))
                // 测试腾讯云COS
                TencentCosTest();
        }

        private void OnDestroy()
        {
            //结束所有的异步任务
            isStart = false;
        }


        private async void TencentCosTest()
        {
            isStart = true;
            var tencentCosUtility = this.GetSystem<ITencentServerSystem>();

            while (isStart)
            {
                var res = await tencentCosUtility.GetBulletResult("res-1312097821", "测试", true);
                res.listBucket.commonPrefixesList.ForEach(item => { Debug.Log(item.prefix); });
                tencentCosUtility.PutObject("res-1312097821", "测试/log5", Application.persistentDataPath + "/Player.log");
                tencentCosUtility.GetFilesInFolder("res-1312097821", "测试/", Application.dataPath, e =>
                {
                    if (e) Debug.Log("下载成功,地址为" + Application.dataPath);
                });

                await Task.Delay(5000);
            }
        }
    }
}