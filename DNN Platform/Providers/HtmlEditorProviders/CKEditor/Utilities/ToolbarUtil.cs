using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using DNNConnect.CKEditorProvider.Constants;
using DNNConnect.CKEditorProvider.Objects;
using DotNetNuke.Common;

namespace DNNConnect.CKEditorProvider.Utilities
{

    /// <summary>
    /// Toolbar Helper Class
    /// </summary>
    public class ToolbarUtil
    {
        #region Public Methods

        /// <summary>
        /// Loads the tool bar buttons.
        /// </summary>
        /// <param name="homeDirPath">The home folder path.</param>
        /// <returns>Returns the Toolbar Button List</returns>
        public static List<ToolbarButton> LoadToolBarButtons(string homeDirPath)
        {
            List<ToolbarButton> buttons;

            var serializer = new XmlSerializer(typeof(List<ToolbarButton>));

            using (
                TextReader textReader =
                    new StreamReader(
                        new FileStream(
                            Path.Combine(homeDirPath, SettingConstants.ToolbarButtonXmlFileName),
                            FileMode.Open,
                            FileAccess.Read,
                            FileShare.Read)))
            {
                buttons = (List<ToolbarButton>)serializer.Deserialize(textReader);

                textReader.Close();
            }

            foreach (var button in buttons.Where(button => button.ToolbarName.Equals("oEmbed")))
            {
                button.ToolbarName = "oembed";
            }

            return buttons;
        }

        /// <summary>
        /// Converts the string to toolbar set.
        /// </summary>
        /// <param name="inputString">The input string.</param>
        /// <returns>Returns the ToolbarSet</returns>
        public static ToolbarSet ConvertStringToToolbarSet(string inputString)
        {
            inputString = inputString.Replace(" ", string.Empty).Replace("\"", "'");

            var toolbarSet = new ToolbarSet();

            // Import old toolbar set older then CKEditor 3.6
            if (inputString.StartsWith("["))
            {
                var groupId = 1;

                var matchOld = Regex.Match(inputString, @"\[(?<group>[^\]]*)\]");

                while (matchOld.Success)
                {
                    var group =
                        matchOld.Groups["group"].Value.Replace("'", string.Empty).Replace(" ", string.Empty).Split(',');

                    var toolBarGroup = new ToolbarGroup
                                           {
                                               items = new List<string>(),
                                               name = string.Format("Group{0}", groupId)
                                           };

                    foreach (string s in group)
                    {
                        toolBarGroup.items.Add(s);
                    }

                    toolbarSet.ToolbarGroups.Add(toolBarGroup);

                    groupId++;

                    matchOld = matchOld.NextMatch();
                }
            }

            var match = Regex.Match(inputString, @"\{name:'(?<groupName>(.+?))',items:\[(?<group>[^\]]*)\]\}");

            while (match.Success)
            {
                var group = match.Groups["group"].Value.Replace("'", string.Empty).Replace(" ", string.Empty).Split(',');

                var toolBarGroup = new ToolbarGroup { name = match.Groups["groupName"].Value };

                foreach (var button in group)
                {
                    toolBarGroup.items.Add(button);
                }

                toolbarSet.ToolbarGroups.Add(toolBarGroup);

                match = match.NextMatch();
            }

            return toolbarSet;
        }

        /// <summary>
        /// Converts the toolbar set to string.
        /// </summary>
        /// <param name="toolbarSet">The toolbar set.</param>
        /// <param name="convertRowBreak">if set to <c>true</c> [convert row break].</param>
        /// <returns>
        /// Returns the Toolbar set as string
        /// </returns>
        public static string ConvertToolbarSetToString(ToolbarSet toolbarSet, bool convertRowBreak = false)
        {
            var stringBuilder = new StringBuilder();

            for (int i = 0; i < toolbarSet.ToolbarGroups.Count; i++)
            {
                var t = toolbarSet.ToolbarGroups[i];

                if (t.name.Equals("rowBreak") && convertRowBreak)
                {
                    stringBuilder.Append(i.Equals(@toolbarSet.ToolbarGroups.Count - 1) ? "'/'" : "'/',");
                }
                else
                {
                    stringBuilder.Append("{");

                    stringBuilder.AppendFormat("name:'{0}',items:", t.name);

                    stringBuilder.Append("[");

                    var buttons = t.items;

                    foreach (var button in buttons)
                    {
                        stringBuilder.AppendFormat("'{0}',", button);
                    }

                    stringBuilder.Remove(stringBuilder.Length - 1, 1);

                    stringBuilder.Append("]");

                    stringBuilder.Append(i.Equals(@toolbarSet.ToolbarGroups.Count - 1) ? "}" : "},");
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Create Default Toolbar Button xml File
        /// </summary>
        /// <param name="savePath">The save path.</param>
        public static void CreateDefaultToolbarButtonXml(string savePath)
        {
            List<ToolbarButton> toolbarButtons = new List<ToolbarButton>
                                                     {
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Source",
                                                                 ToolbarIcon =
                                                                     "Source.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Save",
                                                                 ToolbarIcon =
                                                                     "Save.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "NewPage",
                                                                 ToolbarIcon =
                                                                     "NewPage.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Preview",
                                                                 ToolbarIcon =
                                                                     "Preview.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Print",
                                                                 ToolbarIcon =
                                                                     "Print.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "Templates",
                                                                 ToolbarIcon =
                                                                     "Templates.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Cut",
                                                                 ToolbarIcon = "Cut.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Copy",
                                                                 ToolbarIcon =
                                                                     "Copy.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Paste",
                                                                 ToolbarIcon =
                                                                     "Paste.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "PasteText",
                                                                 ToolbarIcon =
                                                                     "PasteText.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "PasteFromWord",
                                                                 ToolbarIcon =
                                                                     "PasteFromWord.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Undo",
                                                                 ToolbarIcon =
                                                                     "Undo.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Redo",
                                                                 ToolbarIcon =
                                                                     "Redo.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Find",
                                                                 ToolbarIcon =
                                                                     "Find.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Replace",
                                                                 ToolbarIcon =
                                                                     "Replace.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "SelectAll",
                                                                 ToolbarIcon =
                                                                     "SelectAll.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Scayt",
                                                                 ToolbarIcon =
                                                                     "SpellChecker.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Bold",
                                                                 ToolbarIcon =
                                                                     "Bold.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Italic",
                                                                 ToolbarIcon =
                                                                     "Italic.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "Underline",
                                                                 ToolbarIcon =
                                                                     "Underline.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Strike",
                                                                 ToolbarIcon =
                                                                     "Strike.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "Subscript",
                                                                 ToolbarIcon =
                                                                     "Subscript.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "Superscript",
                                                                 ToolbarIcon =
                                                                     "Superscript.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "RemoveFormat",
                                                                 ToolbarIcon =
                                                                     "RemoveFormat.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "NumberedList",
                                                                 ToolbarIcon =
                                                                     "NumberedList.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "BulletedList",
                                                                 ToolbarIcon =
                                                                     "BulletedList.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Outdent",
                                                                 ToolbarIcon =
                                                                     "Outdent.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Indent",
                                                                 ToolbarIcon =
                                                                     "Indent.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "Blockquote",
                                                                 ToolbarIcon =
                                                                     "Blockquote.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "CreateDiv",
                                                                 ToolbarIcon =
                                                                     "CreateDiv.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "JustifyLeft",
                                                                 ToolbarIcon =
                                                                     "JustifyLeft.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "JustifyCenter",
                                                                 ToolbarIcon =
                                                                     "JustifyCenter.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "JustifyRight",
                                                                 ToolbarIcon =
                                                                     "JustifyRight.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "JustifyBlock",
                                                                 ToolbarIcon =
                                                                     "JustifyBlock.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "BidiLtr",
                                                                 ToolbarIcon =
                                                                     "BidiLtr.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "BidiRtl",
                                                                 ToolbarIcon =
                                                                     "BidiRtl.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Link",
                                                                 ToolbarIcon =
                                                                     "Link.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Unlink",
                                                                 ToolbarIcon =
                                                                     "Unlink.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Anchor",
                                                                 ToolbarIcon =
                                                                     "Anchor.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Image",
                                                                 ToolbarIcon =
                                                                     "Image.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Flash",
                                                                 ToolbarIcon =
                                                                     "Flash.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Table",
                                                                 ToolbarIcon =
                                                                     "Table.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "HorizontalRule",
                                                                 ToolbarIcon =
                                                                     "HorizontalRule.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Smiley",
                                                                 ToolbarIcon =
                                                                     "Smiley.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "SpecialChar",
                                                                 ToolbarIcon =
                                                                     "SpecialChar.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "PageBreak",
                                                                 ToolbarIcon =
                                                                     "PageBreak.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Styles",
                                                                 ToolbarIcon =
                                                                     "Styles.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Format",
                                                                 ToolbarIcon =
                                                                     "Format.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Font",
                                                                 ToolbarIcon =
                                                                     "Font.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "FontSize",
                                                                 ToolbarIcon =
                                                                     "FontSize.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "TextColor",
                                                                 ToolbarIcon =
                                                                     "TextColor.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "BGColor",
                                                                 ToolbarIcon =
                                                                     "BGColor.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "Maximize",
                                                                 ToolbarIcon =
                                                                     "Maximize.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "ShowBlocks",
                                                                 ToolbarIcon =
                                                                     "ShowBlocks.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "About",
                                                                 ToolbarIcon =
                                                                     "About.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Iframe",
                                                                 ToolbarIcon =
                                                                     "Iframe.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "Mathjax",
                                                                 ToolbarIcon =
                                                                     "mathjax.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "oembed",
                                                                 ToolbarIcon =
                                                                     "oEmbed.png"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName =
                                                                     "syntaxhighlight",
                                                                 ToolbarIcon =
                                                                     "syntaxhighlight.gif"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "qrcodes",
                                                                 ToolbarIcon =
                                                                     "qrcodes.jpg"
                                                             },
                                                         new ToolbarButton
                                                             {
                                                                 ToolbarName = "-",
                                                                 ToolbarIcon =
                                                                     "separator.png"
                                                             }
                                                     };

            // Save the file
            var serializer = new XmlSerializer(typeof(List<ToolbarButton>));

            using (var textWriter = new StreamWriter(Path.Combine(savePath, SettingConstants.ToolbarButtonXmlFileName)))
            {
                serializer.Serialize(textWriter, toolbarButtons);

                textWriter.Close();
            }
        }

        /// <summary>
        /// Gets the default toolbar.
        /// </summary>
        /// <param name="toolbarName">Name of the toolbar.</param>
        /// <returns>Gets the Default Toolbar Based on the toolbarName</returns>
        public static ToolbarSet GetDefaultToolbar(string toolbarName)
        {
            switch (toolbarName)
            {
                case "Basic":
                    {
                        var toolbarSetBasic = new ToolbarSet("Basic", 10);

                        // Basic Toolbar
                        var toolBarGroup = new ToolbarGroup
                                               {
                                                   items =
                                                       new List<string>
                                                           {
                                                               "Bold",
                                                               "Italic",
                                                               "-",
                                                               "NumberedList",
                                                               "BulletedList",
                                                               "-",
                                                               "Link",
                                                               "Unlink",
                                                               "Image",
                                                               "Mathjax",
                                                               "oembed",
                                                               "-",
                                                               "About"
                                                           },
                                                   name = "basicset"
                                               };

                        toolbarSetBasic.ToolbarGroups.Add(toolBarGroup);

                        return toolbarSetBasic;
                    }
                case "Standard":
                    {
                        var toolbarSetStandard = new ToolbarSet("Standard", 15);

                        // Standard Toolbar
                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items =
                                        new List<string>
                                            {
                                                "Cut",
                                                "Copy",
                                                "Paste",
                                                "PasteText",
                                                "PasteFromWord",
                                                "-",
                                                "Undo",
                                                "Redo"
                                            },
                                    name = "clipboard"
                                });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "Link", "Unlink", "Anchor" }, name = "link" });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items =
                                        new List<string> { "Image", "Mathjax", "oembed", "HorizontalRule" },
                                    name = "insert"
                                });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "Maximize" }, name = "tools" });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "Source" }, name = "document" });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "/" }, name = "rowBreak" });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items = new List<string> { "Bold", "Italic", "Strike", "RemoveFormat" },
                                    name = "basicstyles"
                                });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items =
                                        new List<string>
                                            {
                                                "NumberedList",
                                                "BulletedList",
                                                "-",
                                                "Outdent",
                                                "Indent",
                                                "Blockquote"
                                            },
                                    name = "paragraph"
                                });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "Styles" }, name = "styles" });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "Format" }, name = "format" });

                        toolbarSetStandard.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "About" }, name = "about" });

                        return toolbarSetStandard;
                    }
                case "Full":
                    {
                        var toolbarSetFull = new ToolbarSet("Full", 20);

                        // Full Toolbar
                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items =
                                        new List<string>
                                            {
                                                "Source",
                                                "-",
                                                "Preview",
                                                "Print",
                                                "-",
                                                "Templates"
                                            },
                                    name = "document"
                                });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items =
                                        new List<string>
                                            {
                                                "Cut",
                                                "Copy",
                                                "Paste",
                                                "PasteText",
                                                "PasteFromWord",
                                                "-",
                                                "Undo",
                                                "Redo"
                                            },
                                    name = "clipboard"
                                });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items =
                                        new List<string>
                                            {
                                                "Find",
                                                "Replace",
                                                "-",
                                                "SelectAll"
                                            },
                                    name = "editing"
                                });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items = new List<string> { "Maximize", "ShowBlocks", "-", "About" },
                                    name = "tools"
                                });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "/" }, name = "rowBreak" });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items =
                                        new List<string>
                                            {
                                                "NumberedList",
                                                "BulletedList",
                                                "-",
                                                "Outdent",
                                                "Indent",
                                                "Blockquote",
                                                "CreateDiv",
                                                "-",
                                                "JustifyLeft",
                                                "JustifyCenter",
                                                "JustifyRight",
                                                "JustifyBlock",
                                                "-",
                                                "BidiLtr",
                                                "BidiRtl"
                                            },
                                    name = "paragraph"
                                });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "Link", "Unlink", "Anchor" }, name = "links" });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items =
                                        new List<string>
                                            {
                                                "Image",
                                                "Mathjax",
                                                "oembed",
                                                "syntaxhighlight",
                                                "Table",
                                                "HorizontalRule",
                                                "Smiley",
                                                "SpecialChar",
                                                "PageBreak",
                                                "Iframe"
                                            },
                                    name = "insert"
                                });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "/" }, name = "rowBreak" });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items =
                                        new List<string>
                                            {
                                                "Bold",
                                                "Italic",
                                                "Underline",
                                                "Strike",
                                                "Subscript",
                                                "Superscript",
                                                "-",
                                                "RemoveFormat"
                                            },
                                    name = "basicstyles"
                                });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup
                                {
                                    items = new List<string> { "Styles", "Format", "Font", "FontSize" },
                                    name = "styles"
                                });

                        toolbarSetFull.ToolbarGroups.Add(
                            new ToolbarGroup { items = new List<string> { "TextColor", "BGColor" }, name = "colors" });


                        return toolbarSetFull;
                    }
            }

            return null;
        }

        /// <summary>
        /// Create Default ToolbarSet xml File
        /// </summary>
        /// <param name="savePath">
        /// The save path.
        /// </param>
        public static void CreateDefaultToolbarSetXml(string savePath)
        {
            var listToolbarsSets = new List<ToolbarSet>
                                       {
                                           GetDefaultToolbar("Basic"),
                                           GetDefaultToolbar("Standard"),
                                           GetDefaultToolbar("Full")
                                       };

            var serializer = new XmlSerializer(typeof(List<ToolbarSet>));

            using (var textWriter = new StreamWriter(Path.Combine(savePath, SettingConstants.ToolbarSetXmlFileName)))
            {
                serializer.Serialize(textWriter, listToolbarsSets);

                textWriter.Close();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Find the Toolbar Set with the x Value
        /// </summary>
        /// <param name="toolbars">
        /// Toolbar List to Search in
        /// </param>
        /// <param name="maxPriority">
        /// The Value of the Priority
        /// </param>
        /// <returns>
        /// The Toolbar Set List
        /// </returns>
        internal static ToolbarSet FindHighestToolbar(List<ToolbarSet> toolbars, int maxPriority)
        {
            return toolbars.Find(toolbarSel => toolbarSel.Priority.Equals(maxPriority));
        }

        /// <summary>
        /// Get Toolbar Sets from the Serialized Xml File
        /// </summary>
        /// <param name="homeDirPath">
        /// Current Portal Home directory
        /// </param>
        /// <param name="alternateConfigSubFolder">
        /// The alternate config sub folder.
        /// </param>
        /// <returns>
        /// The Toolbar Set List
        /// </returns>
        internal static List<ToolbarSet> GetToolbars(string homeDirPath, string alternateConfigSubFolder)
        {
            if (!string.IsNullOrEmpty(alternateConfigSubFolder))
            {
                var alternatePath = Path.Combine(homeDirPath, alternateConfigSubFolder);

                if (!Directory.Exists(alternatePath))
                {
                    Directory.CreateDirectory(alternatePath);
                }

                homeDirPath = alternatePath;
            }

            bool createDefault = false;

            // Import old ToolbarXmlFileName first if exist
            if (File.Exists(Path.Combine(homeDirPath, SettingConstants.ToolbarXmlFileName)))
            {
                ImportOldToolbarXml(homeDirPath);
            }

            if (!File.Exists(Path.Combine(homeDirPath, SettingConstants.ToolbarSetXmlFileName)))
            {
                if (!File.Exists(Path.Combine(Globals.HostMapPath, SettingConstants.ToolbarSetXmlFileName)))
                {
                    CreateDefaultToolbarSetXml(Globals.HostMapPath);
                    createDefault = true;
                }

                File.Copy(
                    Path.Combine(Globals.HostMapPath, SettingConstants.ToolbarSetXmlFileName),
                    Path.Combine(homeDirPath, SettingConstants.ToolbarSetXmlFileName));
            }

            // also create default CKToolbarButtons.xml if needed
            if (!File.Exists(Path.Combine(homeDirPath, SettingConstants.ToolbarButtonXmlFileName)))
            {
                if (!File.Exists(Path.Combine(Globals.HostMapPath, SettingConstants.ToolbarButtonXmlFileName)))
                {
                    CreateDefaultToolbarButtonXml(Globals.HostMapPath);
                }

                File.Copy(
                    Path.Combine(Globals.HostMapPath, SettingConstants.ToolbarButtonXmlFileName),
                    Path.Combine(homeDirPath, SettingConstants.ToolbarButtonXmlFileName));
            }

            var serializer = new XmlSerializer(typeof(List<ToolbarSet>));

            var toolBarSets = new List<ToolbarSet>();

            try
            {
                using (
                    var textReader =
                        new StreamReader(
                            new FileStream(
                                Path.Combine(homeDirPath, SettingConstants.ToolbarSetXmlFileName),
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.Read)))
                {
                    toolBarSets = (List<ToolbarSet>)serializer.Deserialize(textReader);

                    textReader.Close();
                }
            }
            catch (Exception)
            {
                if (!createDefault)
                {
                    // Delete Wrong Xml
                    if (File.Exists(Path.Combine(homeDirPath, SettingConstants.ToolbarSetXmlFileName)))
                    {
                        File.Delete(Path.Combine(homeDirPath, SettingConstants.ToolbarSetXmlFileName));
                    }

                    File.Copy(
                        Path.Combine(Globals.HostMapPath, SettingConstants.ToolbarSetXmlFileName),
                        Path.Combine(homeDirPath, SettingConstants.ToolbarSetXmlFileName));

                    using (
                        TextReader textReader =
                            new StreamReader(
                                new FileStream(
                                    Path.Combine(homeDirPath, SettingConstants.ToolbarSetXmlFileName),
                                    FileMode.Open,
                                    FileAccess.Read,
                                    FileShare.Read)))
                    {
                        toolBarSets = (List<ToolbarSet>)serializer.Deserialize(textReader);

                        textReader.Close();
                    }
                }
            }

            foreach (var @group in toolBarSets.SelectMany(set => set.ToolbarGroups))
            {
                for (int index = 0; index < @group.items.Count; index++)
                {
                    if (@group.items[index].Equals("oEmbed"))
                    {
                        @group.items[index] = "oembed";
                    }
                }
            }

            return toolBarSets;
        }

        /// <summary>
        /// Save Toolbar Set list as serialized Xml File
        /// </summary>
        /// <param name="toolBarSets">The Toolbar Set list</param>
        /// <param name="homeDirPath">The home directory path.</param>
        internal static void SaveToolbarSets(List<ToolbarSet> toolBarSets, string homeDirPath)
        {
            var serializer = new XmlSerializer(typeof(List<ToolbarSet>));

            using (
                var textWriter =
                    new StreamWriter(
                        new FileStream(
                            Path.Combine(homeDirPath, SettingConstants.ToolbarSetXmlFileName),
                            FileMode.Truncate,
                            FileAccess.Write,
                            FileShare.Write)))
            {
                serializer.Serialize(textWriter, toolBarSets);

                textWriter.Close();
            }
        }

        /// <summary>
        /// Imports the old toolbar XML.
        /// </summary>
        /// <param name="homeDirPath">The home folder path.</param>
        internal static void ImportOldToolbarXml(string homeDirPath)
        {
            // Delete old xml file in Host Path
            if (File.Exists(Path.Combine(Globals.HostMapPath, SettingConstants.ToolbarXmlFileName)))
            {
                File.Delete(Path.Combine(Globals.HostMapPath, SettingConstants.ToolbarXmlFileName));
            }

            // Import old xml file
            var oldXmlPath = Path.Combine(homeDirPath, SettingConstants.ToolbarXmlFileName);

            var oldSerializer = new XmlSerializer(typeof(List<Toolbar>));

            List<Toolbar> toolBars;

            using (
                var textReader =
                    new StreamReader(new FileStream(oldXmlPath, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                toolBars = (List<Toolbar>)oldSerializer.Deserialize(textReader);

                textReader.Close();
            }

            var listToolbarsSets = new List<ToolbarSet>();

            // Migrate toolbars
            foreach (var toolbar in toolBars)
            {
                ToolbarSet toolbarSet;

                switch (toolbar.sToolbarName)
                {
                    case "Basic":
                        toolbarSet = GetDefaultToolbar("Basic");
                        break;
                    case "Standard":
                        toolbarSet = GetDefaultToolbar("Standard");
                        break;
                    case "Full":
                        toolbarSet = GetDefaultToolbar("Full");
                        break;
                    default:
                        {
                            toolbar.sToolbarSet = toolbar.sToolbarSet.Replace("oEmbed", "oembed");
                            toolbarSet = ConvertStringToToolbarSet(toolbar.sToolbarSet);
                        }

                        break;
                }

                toolbarSet.Name = toolbar.sToolbarName;
                toolbarSet.Priority = toolbar.iPriority;

                listToolbarsSets.Add(toolbarSet);
            }

            // Save update new xml file
            var newSerializer = new XmlSerializer(typeof(List<ToolbarSet>));

            using (var textWriter = new StreamWriter(Path.Combine(homeDirPath, SettingConstants.ToolbarSetXmlFileName)))
            {
                newSerializer.Serialize(textWriter, listToolbarsSets);

                textWriter.Close();
            }

            // Delete old xml file
            File.Delete(Path.Combine(homeDirPath, SettingConstants.ToolbarXmlFileName));
        }

        #endregion
    }
}