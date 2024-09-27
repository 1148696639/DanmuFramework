using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class UnityEditorExtensions:EditorWindow
{
#if UNITY_EDITOR

    #region 切换物体的激活状态

    [MenuItem("MyTool/DeleteAllObj %_q", true)]
    private static bool DeleteValidate()
    {
        if (Selection.objects.Length > 0)
            return true;
        return false;
    }


    [MenuItem("MyTool/DeleteAllObj %_q", false)]
    private static void MyToolDelete()
    {
        //Selection.objects 返回场景或者Project中选择的多个对象
        foreach (GameObject item in Selection.objects)
            //记录删除操作，允许撤销
            if (item.activeSelf)
                item.SetActive(false);
            else
                item.SetActive(true);
    }

    #endregion
}

#endif