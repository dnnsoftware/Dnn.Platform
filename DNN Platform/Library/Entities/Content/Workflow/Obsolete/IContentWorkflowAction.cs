// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow;

// ReSharper enable CheckNamespace
using DotNetNuke.Internal.SourceGenerators;

[DnnDeprecated(7, 4, 0, "Use IWorkflowEngine", RemovalVersion = 10)]
public partial interface IContentWorkflowAction
{
    string GetAction(string[] parameters);
}
