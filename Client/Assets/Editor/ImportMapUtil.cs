using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ImportMapUtil
{
    /// <summary>
    /// 把字符串转换成Vector3的数据
    /// </summary>
    /// <param name="dataString">坐标数据</param>
    /// <returns></returns>
    public static Vector3 StringToVec3(string dataString)
    {
        string[] data = dataString.Split(',');
        return new Vector3(float.Parse(data[0]), float.Parse(data[1]), float.Parse(data[2]));
    }

    /// <summary>
    /// 导出地图
    /// </summary>
    /// <param name="mapRoot">地图的根节点</param>
    /// <param name="fileName">文件名</param>
    public static void ImportMapDataToMapRoot(GameObject mapRoot, string fileName)
    {
        // 清理一下场景里面多余的快;
        ClearMapUtil.clearMapRoot(mapRoot);

        // 加载我们的csv的文件到我们内存;
        CsvStreamReader csr = new CsvStreamReader(Application.dataPath + fileName, System.Text.Encoding.UTF8);


        string mapName,pos,scale,euler;
        GameObject itemPrefab,mapItem;
        // 根据我们的物体的函数，一行行的for循环，一行的创建物体;
        for (int rowNum = 4; rowNum < csr.RowCount + 1; rowNum++)
        {
            mapName = csr[rowNum, 2];
            pos = csr[rowNum, 3];
            scale = csr[rowNum, 4];
            euler = csr[rowNum, 5];

            itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetsPackage/Prefab/MapItems/" + mapName + ".prefab");
            mapItem = PrefabUtility.InstantiatePrefab(itemPrefab) as GameObject;
            mapItem.transform.SetParent(mapRoot.transform, false);
            mapItem.name = mapName;
            mapItem.transform.localPosition = StringToVec3(pos);
            mapItem.transform.localScale = StringToVec3(scale);
            mapItem.transform.localEulerAngles = StringToVec3(euler);
        }

        // 创建完成以后,保存到场景
        EditorApplication.SaveScene(EditorApplication.currentScene);
    }
}
