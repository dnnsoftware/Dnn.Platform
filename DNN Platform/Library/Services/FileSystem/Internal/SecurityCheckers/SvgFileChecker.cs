// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem.Internal.SecurityCheckers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

/// <summary>An <see cref="IFileSecurityChecker"/> for SVG files.</summary>
public class SvgFileChecker : IFileSecurityChecker
{
    private static readonly XNamespace XlinkNamespace = "http://www.w3.org/1999/xlink";

    /// <seealso href="https://github.com/cure53/DOMPurify/blob/55970a919f65c24f2d5a18f07ab8b36f50a9bf2b/src/regexp.ts#L9-L11" />
    private static readonly Regex IsAllowedUriRegex = new Regex(@"^(?:(?:(?:f|ht)tps?|mailto|tel|callto|sms|cid|xmpp|matrix):|[^a-z]|[a-z+.\-]+(?:[^a-z+.\-:]|$))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <seealso href="https://github.com/cure53/DOMPurify/blob/55970a919f65c24f2d5a18f07ab8b36f50a9bf2b/src/regexp.ts#L9-L11" />
    private static readonly Regex AttributeWhitespaceRegex = new Regex(@"[\u0000-\u0020\u00A0\u1680\u180E\u2000-\u2029\u205F\u3000]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

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

    /// <seealso href="https://github.com/cure53/DOMPurify/blob/55970a919f65c24f2d5a18f07ab8b36f50a9bf2b/src/purify.ts#L402-L409" />
    private static readonly HashSet<string> AllowedDataUriElements = new HashSet<string>(
        [
            "audio",
            "video",
            "img",
            "source",
            "image",
            "track",
        ],
        StringComparer.OrdinalIgnoreCase);

    /// <seealso href="https://github.com/cure53/DOMPurify/blob/55970a919f65c24f2d5a18f07ab8b36f50a9bf2b/src/purify.ts#L1273" />
    private static readonly Dictionary<string, HashSet<string>> AllowedDataUriAttributes = new Dictionary<string, HashSet<string>>
        {
            { XNamespace.None.NamespaceName, new HashSet<string>(["src", "href",], StringComparer.OrdinalIgnoreCase) },
            { XlinkNamespace.NamespaceName, new HashSet<string>(["href",], StringComparer.OrdinalIgnoreCase) },
        };

    /// <seealso href="https://github.com/cure53/DOMPurify/blob/55970a919f65c24f2d5a18f07ab8b36f50a9bf2b/src/attrs.ts#L123-L312" />
    private static readonly Dictionary<string, HashSet<string>> AllowedSvgAttributes = new Dictionary<string, HashSet<string>>
        {
            { XNamespace.Xmlns.NamespaceName, new HashSet<string>(["xlink",], StringComparer.OrdinalIgnoreCase) },
            { XNamespace.Xml.NamespaceName, new HashSet<string>(["space", "id",], StringComparer.OrdinalIgnoreCase) },
            { XlinkNamespace.NamespaceName, new HashSet<string>(["href", "title",], StringComparer.OrdinalIgnoreCase) },
            {
                XNamespace.None.NamespaceName, new HashSet<string>(
                    [
                        "accent-height",
                        "accumulate",
                        "additive",
                        "alignment-baseline",
                        "amplitude",
                        "ascent",
                        "attributename",
                        "attributetype",
                        "azimuth",
                        "basefrequency",
                        "baseline-shift",
                        "baseProfile",
                        "begin",
                        "bias",
                        "by",
                        "class",
                        "clip",
                        "clippathunits",
                        "clip-path",
                        "clip-rule",
                        "color",
                        "color-interpolation",
                        "color-interpolation-filters",
                        "color-profile",
                        "color-rendering",
                        "cx",
                        "cy",
                        "d",
                        "dx",
                        "dy",
                        "diffuseconstant",
                        "direction",
                        "display",
                        "divisor",
                        "dur",
                        "edgemode",
                        "elevation",
                        "end",
                        "exponent",
                        "fill",
                        "fill-opacity",
                        "fill-rule",
                        "filter",
                        "filterunits",
                        "flood-color",
                        "flood-opacity",
                        "font-family",
                        "font-size",
                        "font-size-adjust",
                        "font-stretch",
                        "font-style",
                        "font-variant",
                        "font-weight",
                        "fx",
                        "fy",
                        "g1",
                        "g2",
                        "glyph-name",
                        "glyphref",
                        "gradientunits",
                        "gradienttransform",
                        "height",
                        "href",
                        "id",
                        "image-rendering",
                        "in",
                        "in2",
                        "intercept",
                        "k",
                        "k1",
                        "k2",
                        "k3",
                        "k4",
                        "kerning",
                        "keypoints",
                        "keysplines",
                        "keytimes",
                        "lang",
                        "lengthadjust",
                        "letter-spacing",
                        "kernelmatrix",
                        "kernelunitlength",
                        "lighting-color",
                        "local",
                        "marker-end",
                        "marker-mid",
                        "marker-start",
                        "markerheight",
                        "markerunits",
                        "markerwidth",
                        "maskcontentunits",
                        "maskunits",
                        "max",
                        "mask",
                        "mask-type",
                        "media",
                        "method",
                        "mode",
                        "min",
                        "name",
                        "numoctaves",
                        "offset",
                        "operator",
                        "opacity",
                        "order",
                        "orient",
                        "orientation",
                        "origin",
                        "overflow",
                        "paint-order",
                        "path",
                        "pathlength",
                        "patterncontentunits",
                        "patterntransform",
                        "patternunits",
                        "points",
                        "preservealpha",
                        "preserveaspectratio",
                        "primitiveunits",
                        "r",
                        "rx",
                        "ry",
                        "radius",
                        "refx",
                        "refy",
                        "repeatcount",
                        "repeatdur",
                        "restart",
                        "result",
                        "rotate",
                        "scale",
                        "seed",
                        "shape-rendering",
                        "slope",
                        "specularconstant",
                        "specularexponent",
                        "spreadmethod",
                        "startoffset",
                        "stddeviation",
                        "stitchtiles",
                        "stop-color",
                        "stop-opacity",
                        "stroke-dasharray",
                        "stroke-dashoffset",
                        "stroke-linecap",
                        "stroke-linejoin",
                        "stroke-miterlimit",
                        "stroke-opacity",
                        "stroke",
                        "stroke-width",
                        "style",
                        "surfacescale",
                        "systemlanguage",
                        "tabindex",
                        "tablevalues",
                        "targetx",
                        "targety",
                        "transform",
                        "transform-origin",
                        "text-anchor",
                        "text-decoration",
                        "text-rendering",
                        "textlength",
                        "type",
                        "u1",
                        "u2",
                        "unicode",
                        "values",
                        "viewbox",
                        "visibility",
                        "version",
                        "vert-adv-y",
                        "vert-origin-x",
                        "vert-origin-y",
                        "width",
                        "word-spacing",
                        "wrap",
                        "writing-mode",
                        "xchannelselector",
                        "ychannelselector",
                        "x",
                        "x1",
                        "x2",
                        "xmlns",
                        "y",
                        "y1",
                        "y2",
                        "z",
                        "zoomandpan",
                    ],
                    StringComparer.OrdinalIgnoreCase)
            },
        };

    /// <seealso href="https://github.com/cure53/DOMPurify/blob/55970a919f65c24f2d5a18f07ab8b36f50a9bf2b/src/purify.ts#L413-L428" />
    private static readonly HashSet<string> UriSafeAttributes = new HashSet<string>(
        [
            "alt",
            "class",
            "for",
            "id",
            "label",
            "name",
            "pattern",
            "placeholder",
            "role",
            "summary",
            "title",
            "value",
            "style",
            "xmlns",
        ],
        StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
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

        bool IsValidSvgAttribute(XAttribute a)
        {
            if (!AllowedSvgAttributes.TryGetValue(a.Name.NamespaceName, out var attributeNames) || !attributeNames.Contains(a.Name.LocalName))
            {
                return false;
            }

            if (UriSafeAttributes.Contains(a.Name.LocalName))
            {
                return true;
            }

            if (IsAllowedUriRegex.IsMatch(TrimAttributeValue(a.Value)))
            {
                return true;
            }

            if (AllowedDataUriAttributes.TryGetValue(a.Name.NamespaceName, out var dataUriAttributeNames) &&
                dataUriAttributeNames.Contains(a.Name.LocalName)
                && !e.Name.LocalName.Equals("script", StringComparison.OrdinalIgnoreCase)
                && a.Value.StartsWith("data:", StringComparison.Ordinal)
                && AllowedDataUriElements.Contains(e.Name.LocalName))
            {
                return true;
            }

            return false;
        }
    }

    private static string TrimAttributeValue(string value)
    {
        return AttributeWhitespaceRegex.Replace(value, string.Empty);
    }
}
