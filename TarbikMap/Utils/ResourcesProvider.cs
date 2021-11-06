namespace TarbikMap.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    internal static class ResourcesProvider
    {
        private const string ResourcesDirectoryName = "TarbikMap.Resources";

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA3003", Justification = "We check the file name")]
        public static Stream GetStream(string container, string subcontainer, string fileName)
        {
            if (fileName.Contains("/", StringComparison.Ordinal) || fileName.Contains("\\", StringComparison.Ordinal) || fileName.Contains("~", StringComparison.Ordinal) || fileName.Contains(":", StringComparison.Ordinal))
            {
                throw new ArgumentException("Invalid file name");
            }

            return File.OpenRead(Path.Combine(GetResourcesRootPath(), container, subcontainer, fileName));
        }

        public static IList<string> GetFileNames(string container, string subcontainer)
        {
            return Directory.GetFiles(Path.Combine(GetResourcesRootPath(), container, subcontainer))
                .Select(p => Path.GetFileName(p)!)
                .ToList();
        }

        private static string GetResourcesRootPath()
        {
            {
                string productionPath = ResourcesDirectoryName;
                if (Directory.Exists(productionPath))
                {
                    return productionPath;
                }
            }

            {
                string devPath = Path.Combine("..", ResourcesDirectoryName);
                if (Directory.Exists(devPath))
                {
                    return devPath;
                }
            }

            {
                string localTestsPath = Path.Combine("..", "..", "..", "..", ResourcesDirectoryName);
                if (Directory.Exists(localTestsPath))
                {
                    return localTestsPath;
                }
            }

            throw new DirectoryNotFoundException("Resources root path not found from: " + Directory.GetCurrentDirectory());
        }
    }
}