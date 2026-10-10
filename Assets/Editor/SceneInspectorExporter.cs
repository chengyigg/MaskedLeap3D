using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

public class SceneInspectorExporter
{
    [MenuItem("Tools/导出场景信息到文本")]
    public static void ExportSceneInfo()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("========== 场景导出报告 ==========");
        sb.AppendLine($"导出时间: {System.DateTime.Now}");
        sb.AppendLine($"场景名称: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        sb.AppendLine();

        // 遍历所有根物体
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in rootObjects)
        {
            DumpGameObject(root, sb, 0);
        }

        // 写入文件
        string path = Application.dataPath + "/SceneExport.txt";
        File.WriteAllText(path, sb.ToString());
        AssetDatabase.Refresh();

        Debug.Log($"【导出成功】场景信息已写入: {path}");
        EditorUtility.DisplayDialog("导出成功", $"已导出到:\n{path}\n\n你可以用记事本打开并复制内容。", "OK");
    }

    static void DumpGameObject(GameObject go, StringBuilder sb, int depth)
    {
        string indent = new string(' ', depth * 2);
        sb.AppendLine($"{indent}■ {go.name}  [Tag: {go.tag}]  [Active: {go.activeSelf}]");
        sb.AppendLine($"{indent}  Position: {go.transform.position}  Scale: {go.transform.localScale}");

        // 遍历所有组件
        Component[] components = go.GetComponents<Component>();
        foreach (var comp in components)
        {
            if (comp == null)
            {
                sb.AppendLine($"{indent}  ⚠ [Missing Script]");
                continue;
            }

            sb.AppendLine($"{indent}  ├─ {comp.GetType().Name}");

            // 对于 MonoBehaviour，用反射读取 public 字段和 [SerializeField] 私有字段
            if (comp is MonoBehaviour mb)
            {
                DumpSerializedFields(mb, sb, indent + "     ");
            }
        }
        sb.AppendLine();

        // 递归子物体
        foreach (Transform child in go.transform)
        {
            DumpGameObject(child.gameObject, sb, depth + 1);
        }
    }

    static void DumpSerializedFields(MonoBehaviour mb, StringBuilder sb, string indent)
    {
        var type = mb.GetType();
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var field in fields)
        {
            // 只显示 public 字段或者带 [SerializeField] 的字段
            bool isSerialized = field.IsPublic || field.GetCustomAttribute<SerializeField>() != null;
            if (!isSerialized) continue;
            // 跳过一些 Unity 内部字段
            if (field.Name == "m_Script") continue;

            object value = field.GetValue(mb);
            string valueStr = FormatValue(value);
            sb.AppendLine($"{indent}• {field.Name} ({field.FieldType.Name}) = {valueStr}");
        }
    }

    static string FormatValue(object value)
    {
        if (value == null) return "null  ❗（未赋值）";

        // 关键引用类型：检查是否是未赋值的 Object 引用
        if (value is UnityEngine.Object unityObj)
        {
            if (unityObj == null) return "null  ❗（未赋值）";
            return $"{unityObj.name}";
        }

        if (value is Vector3 v) return $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
        if (value is Vector2 v2) return $"({v2.x:F2}, {v2.y:F2})";
        if (value is Color c) return $"RGBA({c.r:F2}, {c.g:F2}, {c.b:F2}, {c.a:F2})";

        return value.ToString();
    }
}