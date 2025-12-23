using FakeXrmEasy.Snapshots;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace FakeXrmEasy.Extensions
{
    /// <summary>
    /// Extension methods for snapshot functionality on XrmFakedContext
    /// </summary>
    public static class IXrmFakedContextSnapshotExtensions
    {
        private static readonly ISnapshotSerializer _defaultSerializer = new JsonSnapshotSerializer();

        /// <summary>
        /// Saves the current context state to a snapshot file
        /// </summary>
        /// <param name="context">The context to save</param>
        /// <param name="baseDirectory">The base directory for snapshots</param>
        /// <param name="snapshotName">Optional explicit snapshot file name. If null, uses CallerMemberName pattern</param>
        /// <param name="testName">The test method name (automatically populated via CallerMemberName)</param>
        /// <param name="testFilePath">The test file path (automatically populated via CallerFilePath)</param>
        /// <param name="lineNumber">The calling line number (automatically populated via CallerLineNumber)</param>
        public static void SaveSnapshot(
            this XrmFakedContext context,
            string baseDirectory,
            string snapshotName = null,
            [CallerMemberName] string testName = null,
            [CallerFilePath] string testFilePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            SaveSnapshot(context, baseDirectory, _defaultSerializer, snapshotName, testName, testFilePath, lineNumber);
        }

        /// <summary>
        /// Saves the current context state to a snapshot file with a custom serializer
        /// </summary>
        /// <param name="context">The context to save</param>
        /// <param name="baseDirectory">The base directory for snapshots</param>
        /// <param name="serializer">The serializer to use</param>
        /// <param name="snapshotName">Optional explicit snapshot file name. If null, uses CallerMemberName pattern</param>
        /// <param name="testName">The test method name (automatically populated via CallerMemberName)</param>
        /// <param name="testFilePath">The test file path (automatically populated via CallerFilePath)</param>
        /// <param name="lineNumber">The calling line number (automatically populated via CallerLineNumber)</param>
        public static void SaveSnapshot(
            this XrmFakedContext context,
            string baseDirectory,
            ISnapshotSerializer serializer,
            string snapshotName = null,
            [CallerMemberName] string testName = null,
            [CallerFilePath] string testFilePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            var snapshotPath = GetSnapshotPath(baseDirectory, snapshotName, testName, testFilePath, lineNumber);
            var entities = context.GetAllEntities();
            
            // Convert entities to snapshots
            var snapshotData = new SnapshotData
            {
                Entities = EntitySnapshotConverter.ToSnapshots(entities)
            };

            // Ensure directory exists
            var directory = Path.GetDirectoryName(snapshotPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = serializer.Serialize(snapshotData);
            File.WriteAllText(snapshotPath, json);
        }

        /// <summary>
        /// Initializes the context from a snapshot file
        /// </summary>
        /// <param name="context">The context to initialize</param>
        /// <param name="baseDirectory">The base directory for snapshots</param>
        /// <param name="snapshotName">Optional explicit snapshot file name. If null, uses CallerMemberName pattern</param>
        /// <param name="testName">The test method name (automatically populated via CallerMemberName)</param>
        /// <param name="testFilePath">The test file path (automatically populated via CallerFilePath)</param>
        /// <param name="lineNumber">The calling line number (automatically populated via CallerLineNumber)</param>
        public static void InitializeFromSnapshot(
            this XrmFakedContext context,
            string baseDirectory,
            string snapshotName = null,
            [CallerMemberName] string testName = null,
            [CallerFilePath] string testFilePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            InitializeFromSnapshot(context, baseDirectory, _defaultSerializer, snapshotName, testName, testFilePath, lineNumber);
        }

        /// <summary>
        /// Initializes the context from a snapshot file with a custom serializer
        /// </summary>
        /// <param name="context">The context to initialize</param>
        /// <param name="baseDirectory">The base directory for snapshots</param>
        /// <param name="serializer">The serializer to use</param>
        /// <param name="snapshotName">Optional explicit snapshot file name. If null, uses CallerMemberName pattern</param>
        /// <param name="testName">The test method name (automatically populated via CallerMemberName)</param>
        /// <param name="testFilePath">The test file path (automatically populated via CallerFilePath)</param>
        /// <param name="lineNumber">The calling line number (automatically populated via CallerLineNumber)</param>
        public static void InitializeFromSnapshot(
            this XrmFakedContext context,
            string baseDirectory,
            ISnapshotSerializer serializer,
            string snapshotName = null,
            [CallerMemberName] string testName = null,
            [CallerFilePath] string testFilePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            var snapshotPath = GetSnapshotPath(baseDirectory, snapshotName, testName, testFilePath, lineNumber);
            
            if (!File.Exists(snapshotPath))
            {
                throw new FileNotFoundException($"Snapshot file not found: {snapshotPath}", snapshotPath);
            }

            var json = File.ReadAllText(snapshotPath);
            var snapshotData = serializer.Deserialize(json);
            
            // Convert snapshots back to entities
            var entities = EntitySnapshotConverter.FromSnapshots(snapshotData.Entities);
            context.Initialize(entities);
        }

        /// <summary>
        /// Initializes the context from a snapshot file if it exists, otherwise executes the setup callback and saves the snapshot
        /// </summary>
        /// <param name="context">The context to initialize</param>
        /// <param name="baseDirectory">The base directory for snapshots</param>
        /// <param name="setupCallback">Callback to execute if snapshot doesn't exist. Receives the context as parameter.</param>
        /// <param name="snapshotName">Optional explicit snapshot file name. If null, uses CallerMemberName pattern</param>
        /// <param name="testName">The test method name (automatically populated via CallerMemberName)</param>
        /// <param name="testFilePath">The test file path (automatically populated via CallerFilePath)</param>
        /// <param name="lineNumber">The calling line number (automatically populated via CallerLineNumber)</param>
        public static void InitializeFromSnapshotOrCreate(
            this XrmFakedContext context,
            string baseDirectory,
            Action<XrmFakedContext> setupCallback,
            string snapshotName = null,
            [CallerMemberName] string testName = null,
            [CallerFilePath] string testFilePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            InitializeFromSnapshotOrCreate(context, baseDirectory, _defaultSerializer, setupCallback, snapshotName, testName, testFilePath, lineNumber);
        }

        /// <summary>
        /// Initializes the context from a snapshot file if it exists, otherwise executes the setup callback and saves the snapshot
        /// </summary>
        /// <param name="context">The context to initialize</param>
        /// <param name="baseDirectory">The base directory for snapshots</param>
        /// <param name="serializer">The serializer to use</param>
        /// <param name="setupCallback">Callback to execute if snapshot doesn't exist. Receives the context as parameter.</param>
        /// <param name="snapshotName">Optional explicit snapshot file name. If null, uses CallerMemberName pattern</param>
        /// <param name="testName">The test method name (automatically populated via CallerMemberName)</param>
        /// <param name="testFilePath">The test file path (automatically populated via CallerFilePath)</param>
        /// <param name="lineNumber">The calling line number (automatically populated via CallerLineNumber)</param>
        public static void InitializeFromSnapshotOrCreate(
            this XrmFakedContext context,
            string baseDirectory,
            ISnapshotSerializer serializer,
            Action<XrmFakedContext> setupCallback,
            string snapshotName = null,
            [CallerMemberName] string testName = null,
            [CallerFilePath] string testFilePath = null,
            [CallerLineNumber] int lineNumber = 0)
        {
            var snapshotPath = GetSnapshotPath(baseDirectory, snapshotName, testName, testFilePath, lineNumber);

            if (File.Exists(snapshotPath))
            {
                // Load from snapshot
                var json = File.ReadAllText(snapshotPath);
                var snapshotData = serializer.Deserialize(json);
                
                // Convert snapshots back to entities
                var entities = EntitySnapshotConverter.FromSnapshots(snapshotData.Entities);
                context.Initialize(entities);
            }
            else
            {
                // Execute callback to populate context
                setupCallback?.Invoke(context);

                // Save snapshot for next time
                var entities = context.GetAllEntities();
                
                // Convert entities to snapshots
                var snapshotData = new SnapshotData
                {
                    Entities = EntitySnapshotConverter.ToSnapshots(entities)
                };

                // Ensure directory exists
                var directory = Path.GetDirectoryName(snapshotPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = serializer.Serialize(snapshotData);
                File.WriteAllText(snapshotPath, json);
            }
        }

        /// <summary>
        /// Gets the snapshot file path
        /// </summary>
        private static string GetSnapshotPath(
            string baseDirectory,
            string snapshotName,
            string testName,
            string testFilePath,
            int lineNumber)
        {
            if (!string.IsNullOrEmpty(snapshotName))
            {
                // Use explicit snapshot name
                return Path.Combine(baseDirectory, snapshotName);
            }
            else
            {
                // Generate from caller attributes
                return SnapshotPathBuilder.GetDefaultPath(baseDirectory, testName, testFilePath, lineNumber);
            }
        }
    }
}
