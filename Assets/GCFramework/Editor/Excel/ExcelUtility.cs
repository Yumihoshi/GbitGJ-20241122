using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NUnit.Framework.Internal.Execution;
using OfficeOpenXml;
using UnityEngine;

namespace GCFramework.Editor.Excel
{
    /// <summary>
    /// Excel工具
    /// </summary>
    public static class ExcelUtility
    {
        //private const string FileSuffix = ".xlsx";

        public static void ConvertToJson(string excelFileName, string jsonFilePath, Encoding encoding)
        {
            // 获取Excel文件信息
            FileInfo fileInfo = new FileInfo(excelFileName);
            
            // 打开Excel
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                if (!package.File.Exists)
                {
                    Debug.LogError($"打开Excel文件失败！ExcelName: {excelFileName}");
                    return;
                }
                if (package.Workbook.Worksheets.Count <= 0 
                    || package.Workbook.Worksheets[1].Dimension == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"Excel表内没有内容！");                    
#endif
                    return;
                }

                // 一整个Excel
                Dictionary<string, List<Dictionary<string, object>>> excelTable = new();

                foreach (var worksheet in package.Workbook.Worksheets) // 取出Excel中的一个Worksheet
                {
                    List<Dictionary<string, object>> table = new();
                    string sheetName = worksheet.Name;
                    int rowCount = worksheet.Dimension.Rows;
                    int colCount = worksheet.Dimension.Columns;
                    for (int row = 3; row <= rowCount; ++row) // 遍历行
                    {
                        Dictionary<string, object> rowData = new();
                        for (int col = 1; col <= colCount; ++col) // 遍历列
                        {
                            string field = worksheet.Cells[1, col].Text;
                            var cellValue = worksheet.Cells[row, col].Value?.ToString();
                            if (int.TryParse(cellValue, out int intValue))
                            {
                                rowData.Add(field, intValue);
                            }
                            else if (float.TryParse(cellValue, out float floatValue))
                            {
                                rowData.Add(field, floatValue);
                            }
                            else if (bool.TryParse(field, out bool boolValue))
                            {
                                rowData.Add(field, boolValue);
                            }
                            else if (double.TryParse(field, out double dleValue))
                            {
                                rowData.Add(field, dleValue);
                            }
                            else if (string.IsNullOrEmpty(cellValue))
                            {
                                rowData.Add(field, "");
                            }
                            else
                            {
                                rowData.Add(field, cellValue);
                            }

                            //rowData.Add(field, cellValue);
                            // 转换数据类型
                            // switch (cellValue)
                            // {
                            //     case int intValue:
                            //         rowData.Add(field, intValue);
                            //         break;
                            //     case float value:
                            //         rowData.Add(field, value);
                            //         break;
                            //     case string strValue:
                            //         rowData.Add(field, strValue);
                            //         break;
                            //     case bool boolValue:
                            //         rowData.Add(field, boolValue);
                            //         break;
                            //     case double dleValue:
                            //         rowData.Add(field, dleValue);
                            //         break;
                            //     default:
                            //         Debug.LogWarning($"在遍历第{row}行，第{col}列时，表格数据转换数据类型时失败！格子数据为: {cellValue}");
                            //         rowData.Add(field, "");
                            //         break;
                            // }
                        }

                        table.Add(rowData);
                    }
                    excelTable.Add(sheetName, table);
                }
                
                // 创建json数据
                foreach (var table in excelTable)
                {
                    string tableJsonData = JsonConvert.SerializeObject(table.Value, Formatting.Indented);
                    using (FileStream fileStream = new FileStream(jsonFilePath + table.Key + ".json", FileMode.Create, FileAccess.Write))
                    {
                        using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
                        {
                            textWriter.Write(tableJsonData);
                        }
                    }
                }
                
                // string jsonData = JsonConvert.SerializeObject(excelTable, Formatting.Indented);
                // using (FileStream fileStream = new FileStream(jsonFilePath, FileMode.Create, FileAccess.Write))
                // {
                //     using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
                //     {
                //         textWriter.Write(jsonData);
                //     }
                // }
                
                // 写入文件
                // using (StreamWriter streamWriter = new StreamWriter(jsonFilePath, true, encoding))
                // {
                //     streamWriter.Write(jsonData);
                // }
            }
        }
    }
}