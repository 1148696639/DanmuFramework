using UnityEngine;
namespace DMFramework
{
    public class GameMgrPack : MonoBehaviour
    {
        private void Start()
        {

            DontDestroyOnLoad(gameObject);
        }
    }
}