using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TES.Services
{
    public class CoreCompilerService
    {
        private const string ProjContent = "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>netstandard2.0</TargetFramework><RuntimeFrameworkVersion>3.1.202</RuntimeFrameworkVersion></PropertyGroup></Project>";

        public static bool Compile(string fileName, string content, out string generatedDll)
        {

            var directory = "C:\\home\\temporaryfiles"; // cia jei azure. 

            var fullName = Path.Combine(directory, fileName);
            File.WriteAllText(fullName, content);

            var globalJson = Path.Combine(directory, "global.json");
            File.WriteAllText(globalJson, "{\r\n  \"sdk\": {\r\n    \"version\": \"5.0.204\"\r\n  }\r\n}");

            var projFile = Path.ChangeExtension(fullName, ".csproj");
            File.WriteAllText(projFile, ProjContent);

            generatedDll = Path.Combine(directory, $"bin\\Debug\\netstandard2.0\\{Path.GetFileNameWithoutExtension(fullName)}.dll");

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "build --no-dependencies --nologo",
                    WorkingDirectory = directory
                };

                var process = StartProcess(startInfo, outputLine => { });

                if (process == null)
                    return false;

                process.WaitForExit();

                return File.Exists(generatedDll);
            }

            catch (Exception)
            {
                return false;
            }
            finally
            {
                SafeDeleteFile(fullName);
                SafeDeleteFile(globalJson);
                SafeDeleteFile(projFile);
            }
        }

        private static Process StartProcess(ProcessStartInfo processInfo, Action<string> redirectOutput)
        {
            processInfo.RedirectStandardOutput = true;

            //this is required for redirected output:
            processInfo.UseShellExecute = false;

            var process = new Process { StartInfo = processInfo };

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    redirectOutput(e.Data);
            };

            process.Start();

            process.BeginOutputReadLine();

            return process;
        }

        private static void SafeDeleteFile(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch
            {
                // ignored
            }
        }

        const string argFile = "ForTests.txt";
        const string testName = "Task.cs";


        static void Main(string[] args)
        {
            double score = TestScore();
            Console.WriteLine(score + " score");
        }

        static double TestScore()
        {



            string content = File.ReadAllText(testName);
            bool canCompile = Compile(testName, content, out string generatedDll);
            if (!canCompile) return 0;
            Console.WriteLine(generatedDll);

            File.WriteAllText(testName, content); //for testing restores Task.cs delete later

            return RunTests(argFile, generatedDll);
        }

        static object Convert(string ob)
        {
            if (bool.TryParse(ob, out var result1)) return result1;
            if (int.TryParse(ob, out var result2)) return result2;
            if (double.TryParse(ob, out var result)) return result;
            if (float.TryParse(ob, out var result3)) return result3;
            if (char.TryParse(ob, out var result4)) return result4;

            return ob;
        }

        static object ArrayConvert(string part, string type)
        {
            if (type.Contains("bool"))
            {
                var enumerable = part.Split(' ').Select(bool.Parse);
                return enumerable;
            }
            if (type.Contains("int"))
            {
                var enumerable = part.Split(' ').Select(int.Parse);
                return enumerable;
            }
            if (type.Contains("double"))
            {
                var enumerable = part.Split(' ').Select(double.Parse);
                return enumerable;
            }
            if (type.Contains("float"))
            {
                var enumerable = part.Split(' ').Select(float.Parse);
                return enumerable;
            }
            if (type.Contains("char"))
            {
                var enumerable = part.Split(' ').Select(char.Parse);
                return enumerable;
            }
            return part.Split(' ');
        }
        static double RunTests(string argFile, string generatedDll)
        {
            string[] lines = File.ReadAllLines(argFile);
            double goodResults = 0;
            foreach (string line in lines)
            {
                string[] parts;
                parts = line.Split("; ");
                object expected = Convert(parts[parts.Length - 1]);

                object[] obj = new object[parts.Length - 1];

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    bool str = false;
                    if (parts[i].StartsWith('['))
                    {
                        int index = parts[i].IndexOf(']');
                        string tp = parts[i].Substring(1, index);
                        parts[i] = parts[i].Remove(0, index + 1);
                        if (parts[i].StartsWith("\"\""))
                        {
                            parts[i] = parts[i].Remove(0, 2);
                            str = true;
                        }
                        if (!str)
                        {
                            var enumer = parts[i].Split(' ');
                            obj[i] = ArrayConvert(parts[i], tp);
                        }
                        else
                        {
                            var enumerable = parts[i].Split(' ');
                            obj[i] = enumerable;
                        }
                    }
                    else
                    {
                        if (!str) obj[i] = Convert(parts[i]);
                        else obj[i] = parts[i];
                    }
                }
                object result = Execute(generatedDll, "Task", "ElementTest", obj); //Lacks logic
                Console.WriteLine(result + "    Expected: " + expected);
                if (object.Equals(result.ToString(), expected.ToString())) goodResults++;
            }
            return goodResults / lines.Length;
        }

        public static object Execute(string path, string className, string function, object[] args)
        {
            var assembly = Assembly.LoadFile(path);

            var type = assembly.GetTypes().First(t => t.Name == className);

            var testInstance = Activator.CreateInstance(type);

            var methodInfo = type.GetMethods().First(m => m.Name == function);

            return methodInfo.Invoke(testInstance, args);
        }
    }
}
