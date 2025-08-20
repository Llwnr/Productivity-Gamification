using Gamification.Infrastructure.Interfaces;
using Python.Runtime;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace Gamification.Infrastructure.Services;

public class ContentAnalysisFilter : IContentAnalysisFilter, IDisposable{
    
    private readonly dynamic _classifier;
    private readonly IConfiguration _config;
    
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr LoadLibrary(string libname);

    public ContentAnalysisFilter(IConfiguration config){
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
        
            Console.WriteLine("Loading ML model into memory...");
            _classifier = classifierModule.Classifier(modelPath);
            Console.WriteLine("ML model loaded. ContentAnalysisFilter is ready.");
        }
        

        // Release the GIL, allowing other .NET threads to run.
        // For a server, you do not call EndAllowThreads here.
        // This "unlocks" the application.
        PythonEngine.BeginAllowThreads();
    }
    
    public bool IsAnalysisRequired(string content){
        if (string.IsNullOrEmpty(content)) return true;
        if (RunInference(content) == 0){
            Console.WriteLine("No need to perform analysis. Content is clearly unproductive");
            return false;
        }
        else{
            Console.WriteLine("Content may be productive. Perform analysis");
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
                Console.WriteLine($"--- Python Exception during inference: {ex.Message} ---");
                // Decide on a safe default. Returning 1 (analyze) is often safer than 0(don't analyze).
                return 1;
            }
        }
    }

    public void Dispose(){
        if (!PythonEngine.IsInitialized) return;
        using (Py.GIL()){
            Console.WriteLine("Disposing Python resources...");

            // 1. Explicitly dispose the classifier object.
            try{
                if (_classifier is IDisposable disposableClassifier){
                    disposableClassifier.Dispose();
                    Console.WriteLine("Classifier object disposed.");
                }
            }
            catch (Exception ex){
                Console.WriteLine($"Error disposing classifier object: {ex.Message}");
            }

            // 2. (Optional but Recommended) Trigger Python's garbage collector.
            try{
                dynamic gc = Py.Import("gc");
                gc.collect();
                Console.WriteLine("Python garbage collection triggered.");
            }
            catch (PythonException ex){
                Console.WriteLine($"Error during Python GC: {ex.Message}");
            }

            // 3. Finally, shut down the Python engine.
            Console.WriteLine("Shutting down Python Engine");
            //Don't need to run PythonEngine.Shutdown() as on application exit, it will automatically be handled.
            // PythonEngine.Shutdown();
        }
    }
}