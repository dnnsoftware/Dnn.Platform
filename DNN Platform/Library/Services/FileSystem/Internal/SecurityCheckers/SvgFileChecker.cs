// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal.SecurityCheckers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

/// <summary>An <see cref="IFileSecurityChecker"/> for SVG files.</summary>
public class SvgFileChecker : IFileSecurityChecker
{
    /// <seealso href="https://github.com/cure53/DOMPurify/blob/e43d3f354861f273852d16f35359f529199dc104/src/tags.ts#L125-L202" />
    private static readonly HashSet<string> AllowedSvgElements = new HashSet<string>(
        [
            "svg",
            "a",
            "altglyph",
            "altglyphdef",
            "altglyphitem",
            "animatecolor",
            "animatemotion",
            "animatetransform",
            "circle",
            "clippath",
            "defs",
            "desc",
            "ellipse",
            "enterkeyhint",
            "exportparts",
            "filter",
            "font",
            "g",
            "glyph",
            "glyphref",
            "hkern",
            "image",
            "inputmode",
            "line",
            "lineargradient",
            "marker",
            "mask",
            "metadata",
            "mpath",
            "part",
            "path",
            "pattern",
            "polygon",
            "polyline",
            "radialgradient",
            "rect",
            "slot",
            "stop",
            "style",
            "switch",
            "symbol",
            "text",
            "textpath",
            "title",
            "tref",
            "tspan",
            "view",
            "vkern",
            "feBlend",
            "feColorMatrix",
            "feComponentTransfer",
            "feComposite",
            "feConvolveMatrix",
            "feDiffuseLighting",
            "feDisplacementMap",
            "feDistantLight",
            "feDropShadow",
            "feFlood",
            "feFuncA",
            "feFuncB",
            "feFuncG",
            "feFuncR",
            "feGaussianBlur",
            "feImage",
            "feMerge",
            "feMergeNode",
            "feMorphology",
            "feOffset",
            "fePointLight",
            "feSpecularLighting",
            "feSpotLight",
            "feTile",
            "feTurbulence",
        ],
        StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public bool Validate(Stream fileContent)
    {
        try
        {
            return XDocument.Load(fileContent)
                .Descendants()
                .All(IsValidSvgElement);
        }
        catch (Exception)
        {
            // when an exception occurs, just return false as not validated, no need log the error.
            return false;
        }
    }

    private static bool IsValidSvgElement(XElement e)
    {
        return AllowedSvgElements.Contains(e.Name.LocalName)
            && e.Attributes().All(IsValidSvgAttribute);

        static bool IsValidSvgAttribute(XAttribute a)
        {
            return !a.Name.LocalName.StartsWith("on", StringComparison.OrdinalIgnoreCase);
        }
    }
}
