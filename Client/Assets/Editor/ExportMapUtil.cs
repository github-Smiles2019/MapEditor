using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExportMapUtil
{

    /// <summary>
    /// 将三维坐标转换成字符串
    /// </summary>
    /// <param name="data">Vector3的数据</param>
    /// <returns>"string"</returns>
    private static string _getNumberToFixed2(Vector3 data)
    {
        string str = String.Format("{0:F},{1:F},{2:F}", data.x, data.y, data.z);
        return str;
    }

    public static void ExportMapToFile(GameObject mapRoot, string fileName)
    {
        // 打开一个文件
        StreamWriter sw = new StreamWriter(Application.dataPath + fileName);

        // 写入文件数据
        // 文件头
        sw.WriteLine("编号,对应资源编号,位置,缩放,角度");
        sw.WriteLine("number,string,string,string,string");
        sw.WriteLine("ID,name,position,scale,eul");

        GameObject mapItem;
        string name, pos, scale, euler, lineData;
        // 遍历mapNode下面的孩子
        for (int i = 0; i < mapRoot.transform.childCount; i++)
        {
            mapItem = mapRoot.transform.GetChild(i).gameObject;
            name = mapItem.name;
            pos = _getNumberToFixed2(mapItem.transform.localPosition);
            scale = _getNumberToFixed2(mapItem.transform.localScale);
            euler = _getNumberToFixed2(mapItem.transform.localEulerAngles);
            lineData = String.Format("{0},{1},\"{2}\",\"{3}\",\"{4}\"", i + 1, name, pos, scale, euler);
            sw.WriteLine(lineData);
        }

        // 关闭一个文件
        sw.Flush();
        sw.Close();
        // 刷新我们的AssetDatabase让你能快速的识别;
        AssetDatabase.Refresh();
    }
}
