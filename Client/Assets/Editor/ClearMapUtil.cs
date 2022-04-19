using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ClearMapUtil
{
    /// <summary>
    /// 清空地图的根节点
    /// </summary>
    /// <param name="mapRoot">GameObject 根节点</param>
    public static void clearMapRoot(GameObject mapRoot)
    {
        int count = mapRoot.transform.childCount;
        GameObject item;
        for (int i = 0; i < count; i++)
        {
            item = mapRoot.transform.GetChild(0).gameObject;
            GameObject.DestroyImmediate(item);
        }
    }
}
