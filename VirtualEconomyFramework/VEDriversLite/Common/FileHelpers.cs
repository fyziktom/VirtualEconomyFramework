using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite
{
    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }
    public static class FileHelpers
    {
        public static void ClearEmptyDirs(string DataFolder)
        {
            var directories = Directory.GetDirectories(DataFolder);

            foreach (string directory in directories)
            {
                if (Directory.GetFileSystemEntries(directory).Length == 0)
                {
                    Directory.Delete(directory);
                }
            }
        }

        public static bool CopyFile(string sourceFilePath, string destinatinFilePath)
        {
            try
            {
                File.Copy(sourceFilePath, destinatinFilePath);
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot copy file: {Path.GetFileName(sourceFilePath)} to the folder {Path.GetDirectoryName(destinatinFilePath)} exception: {ex}");
                return false;
            }
        }

        public static string GetDateTimeString()
        {
            string date = DateTime.Today.ToShortDateString().Replace('.', '_').Replace('/', '_');
            string time = DateTime.Now.ToLongTimeString().Replace(':', '_');

            return date + "-" + time;
        }

        public static string GetTimeString()
        {
            string time = DateTime.Now.ToLongTimeString().Replace(':', '_');
            return time;
        }

        public static string GetDateString()
        {
            string date = DateTime.Today.ToShortDateString().Replace('.', '_').Replace('/', '_');
            return date;
        }

        public static string CheckOrCreateTheFolder(string _outputFolderPath, string suffix = "", bool withdatetime = false)
        {
            string OutputDirectory = string.Empty;

            try
            {
                if (!Directory.Exists(_outputFolderPath))
                {
                    Directory.CreateDirectory(_outputFolderPath);
                }

                if (withdatetime)
                {
                    OutputDirectory = System.IO.Path.Combine(_outputFolderPath, $"{GetDateTimeString()}-{suffix}");
                }
                else
                {
                    OutputDirectory = System.IO.Path.Combine(_outputFolderPath, $"{suffix}");
                }

                Directory.CreateDirectory(OutputDirectory);

                return OutputDirectory;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot create output folder! {ex}");
                return string.Empty;
            }
        }

        public static void AppendLineToTextFile(string line, string outputPath)
        {
            File.AppendAllText(outputPath, line + Environment.NewLine);
        }

        public static void WriteTextToFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }

        public static string ReadTextFromFile(string path)
        {
            if (File.Exists(path))
                return File.ReadAllText(path);
            else
                return string.Empty;
        }

        public static bool IsFileExists(string path)
        {
            if (File.Exists(path))
                return true;
            else
                return false;
        }
    }
}
