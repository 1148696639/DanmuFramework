using System.Threading.Tasks;
using UnityEngine;

namespace Utility
{
    public class InstantiateHelper
    {
        public static async Task<GameObject> InstantiateAsync(GameObject original, Transform parent = null)
        {
            var task= Object.InstantiateAsync(original, parent);
            while (!task.isDone)
            {
                await Task.Yield();
            }

            return task.Result[0];
        }
    }
}