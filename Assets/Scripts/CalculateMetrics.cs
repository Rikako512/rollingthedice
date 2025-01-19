using UnityEngine;
using Python.Runtime;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

public class CalculateMetrics : MonoBehaviour
{
    public static bool IsInitialized { get; private set; } = false;
    public static Dictionary<string, List<int[]>> top10variables;

    void Start()
    {
        if (!IsInitialized)
        {
            InitializeData();
        }
    }

    void InitializeData()
    {
        PythonEngine.Initialize();

        try
        {
            using (Py.GIL())
            {
                string scriptPath = Path.Combine(Application.dataPath, "StreamingAssets", "myproject", "scagnostics_analyzer.py");

                using (PyModule sys = (PyModule)Py.Import("sys"))
                {
                    sys.Get("path").InvokeMethod("append", new PyString(Path.GetDirectoryName(scriptPath)));
                }

                using (PyModule scagnosticsAnalyzer = (PyModule)Py.Import("scagnostics_analyzer"))
                {
                    if (scagnosticsAnalyzer == null)
                    {
                        Debug.LogError("Failed to import scagnostics_analyzer.py");
                        return;
                    }

                    string jsonData = JsonConvert.SerializeObject(CSVData.pointList);

                    using (PyObject resultPy = scagnosticsAnalyzer.InvokeMethod("initialize_data", new PyObject[] { jsonData.ToPython() }))
                    {
                        string result = resultPy.As<string>();
                        Debug.Log("Analysis Result: " + result);
                        top10variables = JsonConvert.DeserializeObject<Dictionary<string, List<int[]>>>(result);
                        IsInitialized = true;
                    }
                }
            }
            Debug.Log("----------- CalculateMetrics 計算完了-----------");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in InitializeData: {e.Message}");
        }
        finally
        {
            PythonEngine.Shutdown();
        }
    }
}
