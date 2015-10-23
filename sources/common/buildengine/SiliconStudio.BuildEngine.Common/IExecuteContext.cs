﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Threading;

using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.BuildEngine
{
    public interface IPrepareContext
    {
        Logger Logger { get; }
        ObjectId ComputeInputHash(UrlType type, string filePath);
    }

    public interface IExecuteContext : IPrepareContext
    {
        CancellationTokenSource CancellationTokenSource { get; }
        ObjectDatabase ResultMap { get; }
        Dictionary<string, string> Variables { get; }

        void ScheduleBuildStep(BuildStep step);

        IEnumerable<IDictionary<ObjectUrl, OutputObject>> GetOutputObjectsGroups();

        CommandBuildStep IsCommandCurrentlyRunning(ObjectId commandHash);
        void NotifyCommandBuildStepStarted(CommandBuildStep commandBuildStep, ObjectId commandHash);
        void NotifyCommandBuildStepFinished(CommandBuildStep commandBuildStep, ObjectId commandHash);
    }
}