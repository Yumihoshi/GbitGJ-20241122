using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GCFramework.Runtime.ABPackage;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GCFramework.Editor.Excel
{
    public class ExcelConvertTool : EditorWindow
    {
        /// <summary>
        /// 仅有一个窗口
        /// </summary>
        private static ExcelConvertTool _ins;

        /// <summary>
        /// Excel列表
        /// </summary>
        private static List<string> _excelPathList;

        /// <summary>
        /// 项目根路径
        /// </summary>
        private static string _pathRoot;

        /// <summary>
        /// 滚动窗口初始位置
        /// </summary>
        private static Vector2 _scrollPos;

        /// <summary>
        /// 输出格式索引
        /// </summary>
        private static int _indexOfFormat;

        /// <summary>
        /// 输出格式选项
        /// </summary>
        private static readonly string[] FormatOption = new[] { "Json" };

        /// <summary>
        /// 输出编码索引
        /// </summary>
        private static int _indexOfEncoding;

        /// <summary>
        /// 编码选项
        /// </summary>
        private static readonly string[] EncodingOption = new[] { "UTF-8" };

        /// <summary>
        /// 保持源文件
        /// </summary>
        private static bool _keepSource = true;

        [MenuItem("Tools/ExcelConvertTool")]
        public static void ShowExcelConvertTool()
        {
            InitTool();
            SelectExcelFiles();
            _ins.Show();
        }

        private void OnGUI()
        {
            DrawOptions();
            DrawExport();
        }

        /// <summary>
        /// 绘制配置界面
        /// </summary>
        private void DrawOptions()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择格式类型: ", GUILayout.Width(85));
            _indexOfFormat = EditorGUILayout.Popup(_indexOfFormat, FormatOption, GUILayout.Width(125));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("选择编码类型: ", GUILayout.Width(85));
            _indexOfEncoding = EditorGUILayout.Popup(_indexOfEncoding, EncodingOption, GUILayout.Width(125));
            GUILayout.EndHorizontal();

            _keepSource = GUILayout.Toggle(_keepSource, "保留Excel源文件");
        }

        /// <summary>
        /// 绘制配置界面输出详细
        /// </summary>
        private void DrawExport()
        {
            if (_excelPathList == null) return;
            if (_excelPathList.Count < 1)
                EditorGUILayout.LabelField("目前没有Excel文件被选中！");
            else
            {
                EditorGUILayout.LabelField("下列Excel文件将会被转换为 " + FormatOption[_indexOfFormat] + "格式！");
                GUILayout.BeginVertical();
                _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true, GUILayout.Height(150));
                foreach (var excel in _excelPathList)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Toggle(true, excel);
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();

                if (GUILayout.Button("点击转换"))
                    Convert();
            }
        }

        private static void Convert()
        {
            if(_excelPathList is { Count: <= 0 }) return; 
            List<string> excelPaths = _excelPathList;
            foreach (var excelPath in excelPaths)
            {
                string finalPath = _pathRoot + "/" + excelPath;
                Encoding encoding = _indexOfEncoding switch
                {
                    0 => Encoding.UTF8,
                    1 => Encoding.GetEncoding("gb2312"),
                    _ => null
                };
                if (encoding == null)
                    throw new ArgumentNullException($"Encoding为null，无法转换Json！");

                int startIndex = finalPath.LastIndexOf("/", StringComparison.Ordinal);
                string fileName = finalPath.Substring(startIndex, finalPath.Length - startIndex);
                if (!Directory.Exists(AssetPath.GetFilePath(AssetPath.ConfigFileRootPath)))
                    AssetPath.CreateAssetDirectory();
                // 放入打包的文件夹中
                // string jsonPath = AssetPath.GetFilePath(AssetPath.ConfigFileRootPath) + "/" +
                //                   fileName + ".json";
                // 得到文件夹路径
                string jsonFilePath = AssetPath.GetFilePath(AssetPath.ConfigFileRootPath) + "/";
                // 选择格式
                switch (_indexOfFormat)
                {
                    case 0:
                        ExcelUtility.ConvertToJson(finalPath, jsonFilePath, encoding);
                        break;
                    // TODO:: 扩展其他格式
                }

                if (!_keepSource)
                {
                    FileUtil.DeleteFileOrDirectory(finalPath);
                }
                
                Debug.Log($"转换Json的路径: {jsonFilePath}");
                AssetDatabase.Refresh();
            }
        }

        private static void SelectExcelFiles()
        {
            if (_excelPathList == null) _excelPathList = new();
            _excelPathList.Clear();
            // 获取选中文件
            Object[] selections = Selection.objects;

            if (selections.Length == 0) return;
            foreach (var obj in selections)
            {
                string objPath = AssetDatabase.GetAssetPath(obj);
                if (objPath.EndsWith(".xlsx"))
                {
                    _excelPathList.Add(objPath);
                }
            }
        }

        private static void InitTool()
        {
            _ins = EditorWindow.GetWindow<ExcelConvertTool>();
            _scrollPos = new Vector2(_ins.position.x, _ins.position.y + 75);
        }

        private void OnSelectionChange()
        {
            Show();
            // 初始化  
            _pathRoot = Application.dataPath;
            // 处理路径
            _pathRoot = _pathRoot.Substring(0, _pathRoot.LastIndexOf("/"));
            SelectExcelFiles();
            Repaint();
        }
    }
}