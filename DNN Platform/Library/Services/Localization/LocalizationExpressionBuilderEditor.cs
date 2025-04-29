// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Localization;

using System;
using System.Web.UI.Design;

public class LocalizationExpressionBuilderEditor : ExpressionEditor
{
    /// <inheritdoc/>
    public override object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider)
    {
        return string.Concat("[dnnLoc:", expression, "]");
    }
}
