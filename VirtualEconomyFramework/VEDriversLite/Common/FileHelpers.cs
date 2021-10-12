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
        /// <summary>
        /// Remove all the empty folders in the main folder
        /// </summary>
        /// <param name="DataFolder">Main folder address</param>
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

        /// <summary>
        /// Copy file from one to another destionation
        /// </summary>
        /// <param name="sourceFilePath">Input file destionation</param>
        /// <param name="destinatinFilePath">Final file destination</param>
        /// <returns></returns>
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
        /// <summary>
        /// Check if the folder exists in the inputed location. If not exists, it will create it
        /// The new folder name can contains the automatic datetime stamp and optional suffix
        /// </summary>
        /// <param name="_outputFolderPath">The path to check</param>
        /// <param name="suffix">Sufix for the new folder</param>
        /// <param name="withdatetime">If you want to add datetime stamp automatically</param>
        /// <returns></returns>
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
        /// <summary>
        /// Add line to the end of some file.
        /// If the file does not exists it will create the new file.
        /// </summary>
        /// <param name="line">New line which should be added to the file</param>
        /// <param name="outputPath">Output path of the file</param>
        public static void AppendLineToTextFile(string line, string outputPath)
        {
            File.AppendAllText(outputPath, line + Environment.NewLine);
        }
        /// <summary>
        /// Write whole text into the file.
        /// If the file does not exists it will create the new file.
        /// </summary>
        /// <param name="path">Output path of the file</param>
        /// <param name="content">content text</param>
        public static void WriteTextToFile(string path, string content)
        {
            File.WriteAllText(path, content);
        }
        /// <summary>
        /// Read all the text from the file
        /// </summary>
        /// <param name="path">Path of the input file</param>
        /// <returns></returns>
        public static string ReadTextFromFile(string path)
        {
            if (File.Exists(path))
                return File.ReadAllText(path);
            else
                return string.Empty;
        }
        /// <summary>
        /// Check if the file exists
        /// </summary>
        /// <param name="path">Path of the file</param>
        /// <returns>true if the file exists</returns>
        public static bool IsFileExists(string path)
        {
            if (File.Exists(path))
                return true;
            else
                return false;
        }
    }
}
