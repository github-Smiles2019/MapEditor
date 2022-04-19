using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExportMap : EditorWindow
{

    public static string m_sLevelNum = "1";  //关卡尾数
    public static string m_sPrefix = "map";  //关卡前缀
    public static string m_sWritePath = "/AssetsPackage/Datas/Map/";  //关卡文件保存地址

    [MenuItem("MapManager/ExportMap")]
    private static void ShowWindow()
    {
        var window = GetWindow<ExportMap>();
        window.titleContent = new GUIContent("ExportMap");
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
        GUILayout.Label("\n输出路径");
        m_sWritePath = GUILayout.TextField(m_sWritePath);

        GUILayout.Label("地图文件前缀"); // level1, level2, level3 map1, map2, map3
        m_sPrefix = GUILayout.TextField(m_sPrefix);

        GUILayout.Label("关卡数字(1~N)"); // level1, level2, level3 map1, map2, map3
        m_sLevelNum = GUILayout.TextField(m_sLevelNum);

        string fileName = m_sWritePath + m_sPrefix + m_sLevelNum + ".csv";
        GUILayout.Label(fileName + "\n");

        if (GUILayout.Button("生成地图文件"))
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<MapEditorRoot>() != null)
            {
                Debug.Log("正在生成地图文件....");
                ExportMapUtil.ExportMapToFile(Selection.activeGameObject, fileName);
                Debug.Log("生成地图文件成功!!");
            }

        }
        if (GUILayout.Button("清理地图"))
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<MapEditorRoot>() != null)
            {
                Debug.Log("正在清理地图....");
                ClearMapUtil.clearMapRoot(Selection.activeGameObject);
                EditorApplication.SaveScene(EditorApplication.currentScene);  // 使用代码来保存我们的场景
                Debug.Log("清理地图成功!!");
            }
        }
    }

    void OnSelectionChange()
    {
        this.Repaint();
    }
}

