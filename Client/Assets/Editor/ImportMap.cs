using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ImportMap : EditorWindow
{

    static string m_sWritePath = "/AssetsPackage/Datas/Map/"; //写入的路径
    static string m_sprefix = "map"; //地图的前缀
    static string m_sLevelNum = "1"; //地图的下标

    [MenuItem("MapManager/ImportMap")]
    private static void ShowWindow()
    {
        var window = GetWindow<ImportMap>();
        window.titleContent = new GUIContent("ImportMap");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("选择地图根节点");
        if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<MapEditorRoot>() != null)
        {
            GUILayout.Label(Selection.activeGameObject.name);
        }
        else
        {
            GUILayout.Label("没有选中的地图节点，无法生成");
        }

        GUILayout.Label("\n导入路径");
        m_sWritePath = GUILayout.TextField(m_sWritePath);
        GUILayout.Label("\n地图文件前缀");
        m_sprefix = GUILayout.TextField(m_sprefix);
        GUILayout.Label("设置关卡数:(1~N)");
        m_sLevelNum = GUILayout.TextField(m_sLevelNum);

        string fileName = m_sWritePath + m_sprefix + m_sLevelNum + ".csv";
        GUILayout.Label(fileName + "\n");

        if (GUILayout.Button("导入地图数据"))
        {

            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<MapEditorRoot>() != null)
            {
                Debug.Log("导入地图数据生成关卡..." + fileName);
                ImportMapUtil.ImportMapDataToMapRoot(Selection.activeGameObject, fileName);
                Debug.Log("导入地图数据成功!!");
            }
            else
            {
                Debug.Log("没有选择节点，无法导入地图数据");
            }
        }
    }

    void OnSelectionChange()
    {
        this.Repaint();
    }
}
