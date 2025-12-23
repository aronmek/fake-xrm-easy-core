using System.IO;

namespace FakeXrmEasy.Snapshots
{
    /// <summary>
    /// Helper class for building snapshot file paths
    /// </summary>
    public static class SnapshotPathBuilder
    {
        /// <summary>
        /// Gets the default snapshot path based on test class and method names
        /// </summary>
        /// <param name="baseDirectory">The base directory for snapshots</param>
        /// <param name="testName">The test method name (typically from CallerMemberName)</param>
        /// <param name="testFilePath">The test file path (typically from CallerFilePath)</param>
        /// <param name="lineNumber">The line number (typically from CallerLineNumber)</param>
        /// <returns>The full path to the snapshot file</returns>
        public static string GetDefaultPath(string baseDirectory, string testName, string testFilePath, int lineNumber)
        {
            var className = GetClassNameFromFilePath(testFilePath);
            var fileName = $"{testName}_{lineNumber}.json";
            return Path.Combine(baseDirectory, className, fileName);
        }

        /// <summary>
        /// Extracts the class name (file name without extension) from a file path
        /// </summary>
        /// <param name="filePath">The full file path</param>
        /// <returns>The file name without extension</returns>
        private static string GetClassNameFromFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return "Unknown";
            }

            var fileName = Path.GetFileNameWithoutExtension(filePath);
            return fileName;
        }
    }
}
