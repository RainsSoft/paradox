﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Reflection;
using System.Text;

using NUnit.Framework;
using SiliconStudio.BuildEngine.Tests.Commands;
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.IO;

namespace SiliconStudio.BuildEngine.Tests
{
    public static class Utils
    {
        private static bool loggerHandled;

        private const string FileSourceFolder = "source";

        public static string BuildPath => Path.Combine(PlatformFolders.ApplicationBinaryDirectory, Assembly.GetEntryAssembly() == null? TestContext.CurrentContext.Test.Name: "data/"+Assembly.GetEntryAssembly().GetName().Name);

        private static StringBuilder logCollecter;

        public static Logger CleanContext()
        {
            // delete previous build data
            if(Directory.Exists(BuildPath))
                Directory.Delete(BuildPath, true);

            // Create database directory
            ((FileSystemProvider)VirtualFileSystem.ApplicationData).ChangeBasePath(BuildPath);
            VirtualFileSystem.CreateDirectory(VirtualFileSystem.ApplicationDatabasePath);

            // Delete source folder if exists
            if (Directory.Exists(FileSourceFolder))
                Directory.Delete(FileSourceFolder, true);

            IndexFileCommand.ObjectDatabase = null;

            TestCommand.ResetCounter();
            if (!loggerHandled)
            {
                GlobalLogger.GlobalMessageLogged += new ConsoleLogListener();
                loggerHandled = true;
            }

            return GlobalLogger.GetLogger("UnitTest");
        }

        public static Builder CreateBuilder(bool createIndexFile)
        {
            var logger = new LoggerResult();
            logger.ActivateLog(LogMessageType.Debug);
            var indexName = createIndexFile ? VirtualFileSystem.ApplicationDatabaseIndexName : null;
            var builder = new Builder(logger, BuildPath, "Windows", indexName) { BuilderName = "TestBuilder", SlaveBuilderPath = @"SiliconStudio.BuildEngine.exe" };
            return builder;
        }

        public static void GenerateSourceFile(string filename, string content, bool overwrite = false)
        {
            string filepath = GetSourcePath(filename);

            if (!Directory.Exists(FileSourceFolder))
                Directory.CreateDirectory(FileSourceFolder);

            if (!overwrite && File.Exists(filepath))
                throw new IOException("File already exists");

            File.WriteAllText(filepath, content);
        }

        public static string GetSourcePath(string filename)
        {
            // TODO: return a path in the temporary folder
            return Path.Combine(FileSourceFolder, filename);
        }
    }
}
