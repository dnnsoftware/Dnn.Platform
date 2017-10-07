#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Installer;
using DotNetNuke.Tests.Utilities;

using NUnit.Framework;
using Assembly = System.Reflection.Assembly;

namespace DotNetNuke.Tests.Integration.Services.Installer
{
    [TestFixture]
    public class XmlMergeTests : DnnUnitTest
    {
        private readonly Assembly _assembly = typeof (XmlMergeTests).Assembly;
        private const bool OutputXml = true;

        /// <summary>
        /// Merges the Merge and Target files based on the name of the calling method.
        /// </summary>
        /// <remarks>xml files must be embedded resources in the MergeFiles folder named {method}Merge.xml and {method}Target.xml</remarks>
        /// <returns>XmlDocument with the result of the merge operation</returns>
        private XmlDocument ExecuteMerge()
        {
            return ExecuteMerge(null);
        }

        /// <summary>
        /// As ExecuteMerge but allows the merge file prefix to be specified
        /// </summary>
        private XmlDocument ExecuteMerge(string mergeName)
        {
            string testMethodName = GetTestMethodName();

            XmlMerge merge = GetXmlMerge(mergeName ?? testMethodName);
            XmlDocument targetDoc = LoadTargetDoc(testMethodName);

            merge.UpdateConfig(targetDoc);

            WriteToDebug(targetDoc);

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
            } while (name == "ExecuteMerge");

            return name;
        }

        private XmlDocument LoadTargetDoc(string testMethodName)
        {
            using (Stream targetStream =
                _assembly.GetManifestResourceStream(string.Format("DotNetNuke.Tests.Integration.Services.Installer.MergeFiles.{0}Target.xml",
                                                                  testMethodName)))
            {
                Debug.Assert(targetStream != null,
                             string.Format("Unable to location embedded resource for {0}Target.xml", testMethodName));
                var targetDoc = new XmlDocument();
                targetDoc.Load(targetStream);
                return targetDoc;
            }
        }

        private XmlMerge GetXmlMerge(string fileName)
        {
            using (Stream mergeStream =
                _assembly.GetManifestResourceStream(string.Format("DotNetNuke.Tests.Integration.Services.Installer.MergeFiles.{0}Merge.xml",
                                                                  fileName)))
            {
                Debug.Assert(mergeStream != null,
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

		[SetUp]
		public void SetUp()
		{
			AppDomain.CurrentDomain.SetData("APPBASE", WebsitePhysicalAppPath);

			LoggerSource.SetTestableInstance(new TestLogSource());
		}

// ReSharper disable PossibleNullReferenceException
        [Test]
        public void SimpleUpdate()
        {
            XmlDocument targetDoc = ExecuteMerge();

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void SimpleUpdateInLocation()
        {
            XmlDocument targetDoc = ExecuteMerge("SimpleUpdate");

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void SimpleUpdateInLocationWithDistractingLocations()
        {
            XmlDocument targetDoc = ExecuteMerge("SimpleUpdate");

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateWithTargetPath()
        {
            XmlDocument targetDoc = ExecuteMerge();

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateInLocationWithTargetPath()
        {
            XmlDocument targetDoc = ExecuteMerge("UpdateWithTargetPath");

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateWithDistractingLocationAndTargetPath()
        {
            XmlDocument targetDoc = ExecuteMerge("UpdateWithTargetPath");

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateInLocationWithDistractingLocationAndTargetPath()
        {
            XmlDocument targetDoc = ExecuteMerge("UpdateWithTargetPath");

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void UpdateInFirstLocationWithDistractingLocationAndTargetPath()
        {
            XmlDocument targetDoc = ExecuteMerge("UpdateWithTargetPath");

            //children are in correct location
            //first location/updateme has updated node
            XmlNode root = targetDoc.SelectSingleNode("/configuration/location[1]");
            XmlNodeList nodes = root.SelectNodes("updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //second location/updateme still empty
            root = targetDoc.SelectSingleNode("/configuration/location[2]");
            nodes = root.SelectNodes("updateme/children/child");
            Assert.AreEqual(0, nodes.Count);

            //children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);

            //two instances of location/updateme exist
            nodes = targetDoc.SelectNodes("//configuration/location/updateme");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void SimpleAdd()
        {
            XmlDocument targetDoc = ExecuteMerge();

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);
        }

        [Test]
        public void AddWithLocation()
        {
            XmlDocument targetDoc = ExecuteMerge("SimpleAdd");

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //second location/updateme still empty
            var root = targetDoc.SelectSingleNode("/configuration/location[2]");
            nodes = root.SelectNodes("updateme/children/child");
            Assert.AreEqual(0, nodes.Count);

            //children only inserted once
            nodes = targetDoc.SelectNodes("//child");
            Assert.AreEqual(2, nodes.Count);

            //1 instance of location/updateme exist
            nodes = targetDoc.SelectNodes("//configuration/location/updateme");
            Assert.AreEqual(1, nodes.Count);
        }

        [Test]
        public void SimpleInsertBefore()
        {
            XmlDocument targetDoc = ExecuteMerge();

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.AreEqual("node2", node.NextSibling.Name);
        }

        [Test]
        public void InsertBeforeInLocation()
        {
            XmlDocument targetDoc = ExecuteMerge("SimpleInsertBefore");

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual("node2", node.NextSibling.Name);
        }

        [Test]
        public void SimpleInsertAfter()
        {
            XmlDocument targetDoc = ExecuteMerge();

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.AreEqual("node1", node.PreviousSibling.Name);
        }

        [Test]
        public void InsertAfterInLocation()
        {
            XmlDocument targetDoc = ExecuteMerge("SimpleInsertAfter");

            //children are in correct location
            XmlNodeList nodes = targetDoc.SelectNodes("/configuration/location/updateme/children/child");
            Assert.AreEqual(2, nodes.Count);

            //inserted before node2
            XmlNode node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual("node2", node.NextSibling.Name);
        }

        [Test]
        public void SimpleRemove()
        {
            XmlDocument targetDoc = ExecuteMerge();

            //node is gone
            var nodes = targetDoc.SelectNodes("//removeme");
            Assert.AreEqual(0, nodes.Count);

            //other nodes still present
            nodes = targetDoc.SelectNodes("/configuration/distraction");
            Assert.AreEqual(1, nodes.Count);
        }

        [Test]
        public void RemoveFromLocation()
        {
            XmlDocument targetDoc = ExecuteMerge("SimpleRemove");

            //node is gone
            var nodes = targetDoc.SelectNodes("//removeme");
            Assert.AreEqual(0, nodes.Count);

            //other nodes still present
            nodes = targetDoc.SelectNodes("/configuration/distraction");
            Assert.AreEqual(1, nodes.Count);
        }

        [Test]
        public void SimpleRemoveAttribute()
        {
            var targetDoc = ExecuteMerge();

            var node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.AreEqual(0, node.Attributes.Count);
        }

        [Test]
        public void RemoveAttributeFromLocation()
        {
            var targetDoc = ExecuteMerge("SimpleRemoveAttribute");

            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual(0, node.Attributes.Count);
        }

        [Test]
        public void SimpleInsertAttribute()
        {
            var targetDoc = ExecuteMerge();

            var node = targetDoc.SelectSingleNode("/configuration/updateme");
            Assert.AreEqual(2, node.Attributes.Count);
            Assert.AreEqual("fee", node.Attributes["attrib2"].Value);
        }

        [Test]
        public void InsertAttributeInLocation()
        {
            var targetDoc = ExecuteMerge("SimpleInsertAttribute");

            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual(2, node.Attributes.Count);
            Assert.AreEqual("fee", node.Attributes["attrib2"].Value);
        }

        [Test]
        public void UpdateAttributeInLocation()
        {
            var targetDoc = ExecuteMerge("SimpleInsertAttribute");

            var node = targetDoc.SelectSingleNode("/configuration/location/updateme");
            Assert.AreEqual(2, node.Attributes.Count);
            Assert.AreEqual("fee", node.Attributes["attrib2"].Value);
        }

        [Test]
        public void SimpleUpdateWithKey()
        {
            var targetDoc = ExecuteMerge();

            //a key was added
            var nodes = targetDoc.SelectNodes("/configuration/updateme/add");
            Assert.AreEqual(1, nodes.Count);
            
            //test attribute is set
            var node = nodes[0];
            Assert.AreEqual("foo", node.Attributes["test"].Value);

        }

        [Test]
        public void UpdateWithKeyInLocation()
        {
            var targetDoc = ExecuteMerge("SimpleUpdateWithKey");

            //a key was added
            var nodes = targetDoc.SelectNodes("/configuration/location/updateme/add");
            Assert.AreEqual(1, nodes.Count);

            //test attribute is set
            var node = nodes[0];
            Assert.AreEqual("foo", node.Attributes["test"].Value);

        }

        [TestCase("Install", "Install", true)]
        [TestCase("Install", "Upgrade", false)]
        [TestCase("Install", "None", false)]
        [TestCase("Upgrade", "Install", false)]
        [TestCase("Upgrade", "Upgrade", true)]
        [TestCase("Upgrade", "None", false)]
        [TestCase("None", "Install", false)]
        [TestCase("None", "Upgrade", false)]
        [TestCase("None", "None", true)]
        [TestCase("!Install", "Install", false)]
        [TestCase("!Install", "Upgrade", true)]
        [TestCase("!Install", "None", true)]
        [TestCase("!Upgrade", "Install", true)]
        [TestCase("!Upgrade", "Upgrade", false)]
        [TestCase("!Upgrade", "None", true)]
        [TestCase("!None", "Install", true)]
        [TestCase("!None", "Upgrade", true)]
        [TestCase("!None", "None", false)]
        [TestCase("Whatever", "Install", true)]
        public void ProcessNodeWithCondition(string condition, string status, bool shouldMerge)
        {
            Globals.SetStatus((Globals.UpgradeStatus)Enum.Parse(typeof(Globals.UpgradeStatus), status, true));

            var fileName = $"ProcessNodeWithCondition_{condition.Replace("!", "Not")}_";
            var targetDoc = ExecuteMerge(fileName);

            var node = targetDoc.SelectSingleNode("/configuration/updateme");
            var expectedChild = shouldMerge ? 1 : 0;
            Assert.AreEqual(expectedChild, node.ChildNodes.Count);
        }

        // ReSharper restore PossibleNullReferenceException
    }

	class TestLogSource : ILoggerSource
	{

		public ILog GetLogger(string name)
		{
			return new TestLogger();
		}

		public ILog GetLogger(Type type)
		{
			return new TestLogger();
		}
	}

	class TestLogger : ILog
	{

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