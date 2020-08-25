// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Integration.Services.Installer
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Tests.Utilities;
    using NUnit.Framework;

    [TestFixture]
    public class XmlMergeTests : DnnUnitTest
    {
        private const bool OutputXml = true;
        private readonly Assembly _assembly = typeof(XmlMergeTests).Assembly;

        [SetUp]
        public void SetUp()
        {
            AppDomain.CurrentDomain.SetData("APPBASE", this.WebsitePhysicalAppPath);

            LoggerSource.SetTestableInstance(new TestLogSource());
        }

        // ReSharper disable PossibleNullReferenceException
        [Test]
        public void SimpleUpdate()
        {
            XmlDocument targetDoc = this.ExecuteMerge();

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void SimpleUpdateInLocation()
        {
            XmlDocument targetDoc = this.ExecuteMerge("SimpleUpdate");

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void SimpleUpdateInLocationWithDistractingLocations()
        {
            XmlDocument targetDoc = this.ExecuteMerge("SimpleUpdate");

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateWithTargetPath()
        {
            XmlDocument targetDoc = this.ExecuteMerge();

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateInLocationWithTargetPath()
        {
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateWithDistractingLocationAndTargetPath()
        {
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateInLocationWithDistractingLocationAndTargetPath()
        {
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateInFirstLocationWithDistractingLocationAndTargetPath()
        {
            XmlDocument targetDoc = this.ExecuteMerge("UpdateWithTargetPath");

            // children are in correct location
            // first location/updateme has updated node
            XmlNode root = targetDoc.SelectSingleNode("/configuration/location[1]");
            XmlNodeList nodes = root.SelectNodes("updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // second location/updateme still empty
            root = targetDoc.SelectSingleNode("/configuration/location[2]");
            nodes = root.SelectNodes("updateme/children/child");
            Assert.AreEqual(0, nodes.Count);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);

            // two instances of location/updateme exist
            nodes = targetDoc.SelectNodes("//configuration/location/updateme");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void SimpleAdd()
        {
            XmlDocument targetDoc = this.ExecuteMerge();

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void AddWithLocation()
        {
            XmlDocument targetDoc = this.ExecuteMerge("SimpleAdd");

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // second location/updateme still empty
            var root = targetDoc.SelectSingleNode("/configuration/location[2]");
            nodes = root.SelectNodes("updateme/children/child");
            Assert.AreEqual(0, nodes.Count);

            // children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);

            // 1 instance of location/updateme exist
            nodes = targetDoc.SelectNodes("//configuration/location/updateme");
            Assert.AreEqual(1, nodes.Count);
        }

        [Test]
        public void SimpleInsertBefore()
        {
            XmlDocument targetDoc = this.ExecuteMerge();

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.AreEqual("node2", node.NextSibling.Name);
        }

        [Test]
        public void InsertBeforeInLocation()
        {
            XmlDocument targetDoc = this.ExecuteMerge("SimpleInsertBefore");

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual("node2", node.NextSibling.Name);
        }

        [Test]
        public void SimpleInsertAfter()
        {
            XmlDocument targetDoc = this.ExecuteMerge();

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.AreEqual("node1", node.PreviousSibling.Name);
        }

        [Test]
        public void InsertAfterInLocation()
        {
            XmlDocument targetDoc = this.ExecuteMerge("SimpleInsertAfter");

            // children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            // inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual("node2", node.NextSibling.Name);
        }

        [Test]
        public void SimpleRemove()
        {
            XmlDocument targetDoc = this.ExecuteMerge();

            // node is gone
            var nodes = targetDoc.SelectNodes("//removeme");
            Assert.AreEqual(0, nodes.Count);

            // other nodes still present
            nodes = targetDoc.SelectNodes("/configuration/distraction");
            Assert.AreEqual(1, nodes.Count);
        }

        [Test]
        public void RemoveFromLocation()
        {
            XmlDocument targetDoc = this.ExecuteMerge("SimpleRemove");

            // node is gone
            var nodes = targetDoc.SelectNodes("//removeme");
            Assert.AreEqual(0, nodes.Count);

            // other nodes still present
            nodes = targetDoc.SelectNodes("/configuration/distraction");
            Assert.AreEqual(1, nodes.Count);
        }

        [Test]
        public void SimpleRemoveAttribute()
        {
            var targetDoc = this.ExecuteMerge();

            var node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.AreEqual(0, node.Attributes.Count);
        }

        [Test]
        public void RemoveAttributeFromLocation()
        {
            var targetDoc = this.ExecuteMerge("SimpleRemoveAttribute");

            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual(0, node.Attributes.Count);
        }

        [Test]
        public void SimpleInsertAttribute()
        {
            var targetDoc = this.ExecuteMerge();

            var node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.AreEqual(2, node.Attributes.Count);
            Assert.AreEqual("fee", node.Attributes["attrib2"].Value);
        }

        [Test]
        public void InsertAttributeInLocation()
        {
            var targetDoc = this.ExecuteMerge("SimpleInsertAttribute");

            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual(2, node.Attributes.Count);
            Assert.AreEqual("fee", node.Attributes["attrib2"].Value);
        }

        [Test]
        public void UpdateAttributeInLocation()
        {
            var targetDoc = this.ExecuteMerge("SimpleInsertAttribute");

            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual(2, node.Attributes.Count);
            Assert.AreEqual("fee", node.Attributes["attrib2"].Value);
        }

        [Test]
        public void SimpleUpdateWithKey()
        {
            var targetDoc = this.ExecuteMerge();

            // a key was added
            var nodes = targetDoc.SelectNodes("/configuration/updateme/add");
            Assert.AreEqual(1, nodes.Count);

            // test attribute is set
            var node = nodes[0];
            Assert.AreEqual("foo", node.Attributes["test"].Value);
        }

        [Test]
        public void UpdateWithKeyInLocation()
        {
            var targetDoc = this.ExecuteMerge("SimpleUpdateWithKey");

            // a key was added
            var nodes = targetDoc.SelectNodes("/configuration/location/updateme/add");
            Assert.AreEqual(1, nodes.Count);

            // test attribute is set
            var node = nodes[0];
            Assert.AreEqual("foo", node.Attributes["test"].Value);
        }

        [Test]
        public void NoChangeOnOverwrite()
        {
            XmlMerge merge = this.GetXmlMerge(nameof(this.NoChangeOnOverwrite));
            XmlDocument targetDoc = this.LoadTargetDoc(nameof(this.NoChangeOnOverwrite));

            merge.UpdateConfig(targetDoc);

            this.WriteToDebug(targetDoc);

            var nodes = targetDoc.SelectNodes("/configuration/appSettings/add");
            Assert.AreEqual(3, nodes.Count);

            Assert.False(merge.ConfigUpdateChangedNodes);
        }

        [Test]
        public void ShouldChangeOnOverwrite()
        {
            XmlMerge merge = this.GetXmlMerge(nameof(this.ShouldChangeOnOverwrite));
            XmlDocument targetDoc = this.LoadTargetDoc(nameof(this.ShouldChangeOnOverwrite));

            merge.UpdateConfig(targetDoc);

            this.WriteToDebug(targetDoc);

            var nodes = targetDoc.SelectNodes("/configuration/appSettings/add");
            Assert.AreEqual(3, nodes.Count);

            Assert.True(merge.ConfigUpdateChangedNodes);
        }

        [Test]
        public void ShouldPreserveEmptyNamespaceOnSave()
        {
            var targetDoc = this.ExecuteMerge();

            var ns = new XmlNamespaceManager(targetDoc.NameTable);
            ns.AddNamespace("ab", "urn:schemas-microsoft-com:asm.v1");

            // removed the existing node, since it matched targetpath attribute
            var nodesWithNamespace = targetDoc.SelectNodes("/configuration/runtime/ab:assemblyBinding/ab:dependentAssembly", ns);
            Assert.AreEqual(0, nodesWithNamespace.Count);

            // added a new node with xmlns=""
            var nodesWithoutNamespace = targetDoc.SelectNodes("/configuration/runtime/ab:assemblyBinding/dependentAssembly", ns);
            Assert.AreEqual(1, nodesWithoutNamespace.Count);

            // non-namespaced node has newVersion from merge
            var dependentAssembly = nodesWithoutNamespace[0];
            var bindingRedirect = dependentAssembly.SelectSingleNode("bindingRedirect", ns);
            Assert.AreEqual("4.1.0.0", bindingRedirect.Attributes["newVersion"].Value);
        }

        /// <summary>
        /// Merges the Merge and Target files based on the name of the calling method.
        /// </summary>
        /// <remarks>xml files must be embedded resources in the MergeFiles folder named {method}Merge.xml and {method}Target.xml.</remarks>
        /// <returns>XmlDocument with the result of the merge operation.</returns>
        private XmlDocument ExecuteMerge()
        {
            return this.ExecuteMerge(null);
        }

        /// <summary>
        /// As ExecuteMerge but allows the merge file prefix to be specified.
        /// </summary>
        private XmlDocument ExecuteMerge(string mergeName)
        {
            string testMethodName = this.GetTestMethodName();

            XmlMerge merge = this.GetXmlMerge(mergeName ?? testMethodName);
            XmlDocument targetDoc = this.LoadTargetDoc(testMethodName);

            merge.UpdateConfig(targetDoc);

            this.WriteToDebug(targetDoc);

            return targetDoc;
        }

        private string GetTestMethodName()
        {
            var st = new StackTrace(2);

            string name;
            int i = 0;
            do
            {
                name = st.GetFrame(i).GetMethod().Name;
                i++;
            }
            while (name == "ExecuteMerge");

            return name;
        }

        private XmlDocument LoadTargetDoc(string testMethodName)
        {
            using (Stream targetStream =
                this._assembly.GetManifestResourceStream(string.Format(
                    "DotNetNuke.Tests.Integration.Services.Installer.MergeFiles.{0}Target.xml",
                    testMethodName)))
            {
                Debug.Assert(
                    targetStream != null,
                    string.Format("Unable to location embedded resource for {0}Target.xml", testMethodName));
                var targetDoc = new XmlDocument { XmlResolver = null };
                targetDoc.Load(targetStream);
                return targetDoc;
            }
        }

        private XmlMerge GetXmlMerge(string fileName)
        {
            using (Stream mergeStream =
                this._assembly.GetManifestResourceStream(string.Format(
                    "DotNetNuke.Tests.Integration.Services.Installer.MergeFiles.{0}Merge.xml",
                    fileName)))
            {
                Debug.Assert(
                    mergeStream != null,
                    string.Format("Unable to location embedded resource for {0}Merge.xml", fileName));
                var merge = new XmlMerge(mergeStream, "version", "sender");
                return merge;
            }
        }

        private void WriteToDebug(XmlDocument targetDoc)
        {
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (OutputXml)

            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                using (var writer = new StreamWriter(new MemoryStream()))
                {
                    targetDoc.Save(writer);
                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                    using (var sr = new StreamReader(writer.BaseStream))
                    {
                        Debug.WriteLine("{0}", sr.ReadToEnd());
                    }
                }
            }
        }

        // ReSharper restore PossibleNullReferenceException
    }

    internal class TestLogger : ILog
    {
        public bool IsDebugEnabled
        {
            get { return false; }
        }

        public bool IsErrorEnabled
        {
            get { return false; }
        }

        public bool IsFatalEnabled
        {
            get { return false; }
        }

        public bool IsInfoEnabled
        {
            get { return false; }
        }

        public bool IsTraceEnabled
        {
            get { return false; }
        }

        public bool IsWarnEnabled
        {
            get { return false; }
        }

        public void Debug(object message, Exception exception)
        {
        }

        public void Debug(object message)
        {
        }

        public void DebugFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void DebugFormat(string format, params object[] args)
        {
        }

        public void Error(object message, Exception exception)
        {
        }

        public void Error(object message)
        {
        }

        public void ErrorFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void ErrorFormat(string format, params object[] args)
        {
        }

        public void Fatal(object message, Exception exception)
        {
        }

        public void Fatal(object message)
        {
        }

        public void FatalFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void FatalFormat(string format, params object[] args)
        {
        }

        public void Info(object message, Exception exception)
        {
        }

        public void Info(object message)
        {
        }

        public void InfoFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void InfoFormat(string format, params object[] args)
        {
        }

        public void Trace(object message, Exception exception)
        {
        }

        public void Trace(object message)
        {
        }

        public void TraceFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void TraceFormat(string format, params object[] args)
        {
        }

        public void Warn(object message, Exception exception)
        {
        }

        public void Warn(object message)
        {
        }

        public void WarnFormat(IFormatProvider provider, string format, params object[] args)
        {
        }

        public void WarnFormat(string format, params object[] args)
        {
        }
    }
}
