// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.CodeDom;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.UI;
using System.Web.UI.Design;

#endregion

namespace DotNetNuke.Services.Localization
{
    [ExpressionPrefix("dnnLoc"), ExpressionEditor("DotNetNuke.Services.Localization.LocalizationExpressionBuilderEditor")]
    public class LocalizationExpressionBuilder : ExpressionBuilder
    {
        public override bool SupportsEvaluate
        {
            get
            {
                return true;
            }
        }

        public override CodeExpression GetCodeExpression(BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            var inputParams = new CodeExpression[]
                                  {
                                      new CodePrimitiveExpression(entry.Expression.Trim()), 
                                      new CodeTypeOfExpression(entry.DeclaringType), 
                                      new CodePrimitiveExpression(entry.PropertyInfo.Name),
                                      new CodePrimitiveExpression(context.VirtualPath)
                                  };

            return new CodeMethodInvokeExpression(new CodeTypeReferenceExpression(GetType()), "GetLocalizedResource", inputParams);
        }

        public static object GetLocalizedResource(string key, Type targetType, string propertyName, string virtualPath)
        {
            if (HttpContext.Current == null)
            {
                return null;
            }

            string localResourceFile = "";
            if (!string.IsNullOrEmpty(virtualPath))
            {
                string filename = Path.GetFileName(virtualPath);
                if (filename != null)
                {
                    localResourceFile = virtualPath.Replace(filename, Localization.LocalResourceDirectory + "/" + filename);
                }
            }
            string value = Localization.GetString(key, localResourceFile);

            if (value == null)
            {
                throw new InvalidOperationException(string.Format("Localized Value '{0}' not found.", key));
            }

            // check if value can be converted to property type
            if (targetType != null)
            {
                PropertyDescriptor propDesc = TypeDescriptor.GetProperties(targetType)[propertyName];
                if (propDesc != null && propDesc.PropertyType != value.GetType())
                {
                    // Type mismatch - make sure that the value can be converted to the Web control property type 
                    if (propDesc.Converter != null)
                    {
                        if (propDesc.Converter.CanConvertFrom(value.GetType()) == false)
                        {
                            throw new InvalidOperationException(string.Format("Localized value '{0}' cannot be converted to type {1}.", key, propDesc.PropertyType));
                        }
                        return propDesc.Converter.ConvertFrom(value);
                    }
                }
            }

            // If we reach here, no type mismatch - return the value 
            return value;
        }

        public override object EvaluateExpression(object target, BoundPropertyEntry entry, object parsedData, ExpressionBuilderContext context)
        {
            string key = entry.Expression.Trim();
            Type targetType = entry.DeclaringType;
            string propertyName = entry.PropertyInfo.Name;
            string virtualPath = context.VirtualPath;

            return GetLocalizedResource(key, targetType, propertyName, virtualPath);
        }
    }

    public class LocalizationExpressionBuilderEditor : ExpressionEditor
    {
        public override object EvaluateExpression(string expression, object parseTimeData, Type propertyType, IServiceProvider serviceProvider)
        {
            return string.Concat("[dnnLoc:", expression, "]");
        }
    }
}
