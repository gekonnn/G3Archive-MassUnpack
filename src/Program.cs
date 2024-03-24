using System;
using System.IO;
using G3Archive;

namespace G3ArchiveMassUnpack
{
    class Program
    {
        static void Main(string[] args)
        {
            string targetArchive = null;
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify archive files directory");
                return;
            }

            if (args.Length > 1)
            {
                targetArchive = args[1];
            }

            ParsedOptions.Overwrite = true;
            Logger.Quiet = true;

            DirectoryInfo directory = new DirectoryInfo(args[0].TrimEnd('"').TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (!directory.Exists)
            {
                Console.WriteLine("Specified directory does not exist");
                return;
            }

            FileInfo[] files = directory.GetFiles();
            List<FileInfo> sortedFiles = SortFiles(files);
            
            foreach (FileInfo file in sortedFiles)
            {
                if (targetArchive != null && Path.GetFileNameWithoutExtension(file.Name) != targetArchive)
                {
                    continue;
                }
                try
                {
                    G3Pak_Archive Archive = new G3Pak_Archive();
                    Archive.ReadArchive(file);

                    Console.WriteLine($"Extracting {file.Name}...");
                    Archive.Extract(Path.Combine(Directory.GetCurrentDirectory(), "Extracted", Path.GetFileNameWithoutExtension(file.Name))).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }
            }

            Console.WriteLine("Extraction complete.");
        }

        static List<FileInfo> SortFiles(FileInfo[] files)
        {
            string[] extensions = { ".p", ".c", ".m", ".n" };
            string[] generations = Enumerable.Range(0, 100).Select(i => i.ToString("D2")).ToArray();

            List<FileInfo> sortedFiles = files.OrderBy(file =>
            {
                string extension = file.Extension.ToLower();
                int extensionIndex = Array.IndexOf(extensions, extension.Substring(0, 2)) + 1;

                string generation = extension.Substring(2);
                int generationIndex = Array.IndexOf(generations, generation) + 1; 

                return (extensionIndex * generations.Length) + generationIndex;
            }).ToList();

            return sortedFiles;
        }
    }
}