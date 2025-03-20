﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Mvc
{
  using System;
  using System.Collections.Generic;
  using System.Web;

  public class MvcClientAPI
  {
    public static Dictionary<string, string> GetClientVariableList()
    {
      var dic = HttpContext.Current.Items["CAPIVariableList"] as Dictionary<string, string>;
      if (dic == null)
      {
        dic = new Dictionary<string, string>();
        HttpContext.Current.Items["CAPIVariableList"] = dic;
      }

      return dic;
    }

    public static Dictionary<string, string> GetClientStartupScriptList()
    {
      var dic = HttpContext.Current.Items["CAPIStartupScriptList"] as Dictionary<string, string>;
      if (dic == null)
      {
        dic = new Dictionary<string, string>();
        HttpContext.Current.Items["CAPIStartupScriptList"] = dic;
      }

      return dic;
    }

    public static void RegisterClientVariable(string key, string value, bool overwrite)
    {
      GetClientVariableList().Add(key, value);
    }

    public static void RegisterEmbeddedResource(string fileName, Type assemblyType)
    {
      // RegisterClientVariable(FileName + ".resx", ThePage.ClientScript.GetWebResourceUrl(AssemblyType, FileName), true);
    }

    public static void RegisterStartupScript(string key, string value)
    {
      if (!GetClientStartupScriptList().ContainsKey(key))
      {
        GetClientStartupScriptList().Add(key, value);
      }
    }

    public static void RegisterScript(string key, string value)
    {
      if (!GetClientStartupScriptList().ContainsKey(key))
      {
        GetClientStartupScriptList().Add(key, value);
      }
    }
  }
}
