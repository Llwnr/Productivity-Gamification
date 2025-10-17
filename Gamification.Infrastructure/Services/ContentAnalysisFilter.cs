using Gamification.Infrastructure.Interfaces;
using Python.Runtime;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Gamification.Infrastructure.Services;

public class ContentAnalysisFilter : IContentAnalysisFilter, IDisposable{
    
    private readonly dynamic _classifier;
    private readonly IConfiguration _config;
    private readonly ILogger<ContentAnalysisFilter> _logger;
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string libname);

    public ContentAnalysisFilter(IConfiguration config, ILogger<ContentAnalysisFilter> logger){
        _logger = logger;
        return;
        _config = config;
        // Set configuration before initializing
        Runtime.PythonDLL = Path.Join(_config.GetValue<string>("PythonPath:Home"), "Python311.dll");
        PythonEngine.PythonHome = _config.GetValue<string>("PythonPath:Home");
        
        PythonEngine.Initialize();

        // The GIL is held by this thread after Initialize().
        // We do all our initial setup now.
        using (Py.GIL()){
            var scriptDirectory = _config.GetValue<string>("PythonPath:Script");
            dynamic sys = Py.Import("sys");
        
            sys.path.append(scriptDirectory);

            dynamic classifierModule = Py.Import("classifier");
            var modelPath = Path.Join(_config.GetValue<string>("PythonPath:Script"), "models");
        
            _logger.LogInformation("Loading ML model into memory...");
            _classifier = classifierModule.Classifier(modelPath);
            _logger.LogInformation("ML model loaded. ContentAnalysisFilter is ready.");
        }
        

        // Release the GIL, allowing other .NET threads to run.
        // For a server, you do not call EndAllowThreads here.
        // This "unlocks" the application.
        PythonEngine.BeginAllowThreads();
    }
    
    public bool IsAnalysisRequired(string content){
        if (string.IsNullOrEmpty(content)) return true;
        if (RunInference(content) == 0){
            _logger.LogInformation("No need to perform analysis. Content is clearly unproductive");
            return false;
        }
        else{
            _logger.LogInformation("Content may be productive. Perform analysis");
            return true;
        }
    }

    //Will run the pre-trained BERT model that returns 0 meaning don't need to analyze, 1 meaning analyze the content
    int RunInference(string content){
        return 1;
        using (Py.GIL()){
            try{
                var result = _classifier.predict(content);
                return result.As<int>();
            }
            catch (PythonException ex){
                _logger.LogError("--- Python Exception during inference: {ErrorMessage} ---", ex.Message);
                // Decide on a safe default. Returning 1 (analyze) is often safer than 0(don't analyze).
                return 1;
            }
        }
    }

    public void Dispose(){
        if (!PythonEngine.IsInitialized) return;
        using (Py.GIL()){
            _logger.LogInformation("Disposing Python resources...");

            // 1. Explicitly dispose the classifier object.
            try{
                if (_classifier is IDisposable disposableClassifier){
                    disposableClassifier.Dispose();
                    _logger.LogInformation("Classifier object disposed.");
                }
            }
            catch (Exception ex){
                _logger.LogError("Error disposing classifier object: {ErrorMessage}", ex.Message);
            }

            // 2. (Optional but Recommended) Trigger Python's garbage collector.
            try{
                dynamic gc = Py.Import("gc");
                gc.collect();
                _logger.LogInformation("Python garbage collection triggered.");
            }
            catch (PythonException ex){
                _logger.LogError("Error during Python GC: {ErrorMessage}", ex.Message);
            }

            // 3. Finally, shut down the Python engine.
            _logger.LogInformation("Shutting down Python Engine");
            //Don't need to run PythonEngine.Shutdown() as on application exit, it will automatically be handled.
            // PythonEngine.Shutdown();
        }
    }
}