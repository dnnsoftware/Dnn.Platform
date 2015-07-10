using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientDependency.Core.CompositeFiles;

namespace ClientDependency.Core
{
    /// <summary>
    /// Defines the file writers for file extensions or for explicit file paths
    /// </summary>
    public class FileWriters
    {

        private static readonly ConcurrentDictionary<string, IFileWriter> ExtensionWriters = new ConcurrentDictionary<string, IFileWriter>();
        private static readonly ConcurrentDictionary<string, IFileWriter> PathWriters = new ConcurrentDictionary<string, IFileWriter>();
        private static readonly IFileWriter DefaultFileWriter = new DefaultFileWriter();

        /// <summary>
        /// Returns the default writer
        /// </summary>
        /// <returns></returns>
        public static IFileWriter GetDefault()
        {
            return DefaultFileWriter;
        }

        /// <summary>
        /// returns all extensions that have been registered
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<string> GetRegisteredExtensions()
        {
            return ExtensionWriters.Select(x => x.Key.ToUpper());
        }

        /// <summary>
        /// This will add or update a writer for a specific file extension
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <param name="?"></param>
        /// <param name="writer"></param>
        public static void AddWriterForExtension(string fileExtension, IFileWriter writer)
        {
            if (fileExtension == null) throw new ArgumentNullException("fileExtension");
            if (writer == null) throw new ArgumentNullException("writer");

            if (!fileExtension.StartsWith("."))
            {
                throw new FormatException("A file extension must begin with a '.'");
            }
            ExtensionWriters.AddOrUpdate(fileExtension.ToUpper(), s => writer, (s, fileWriter) => writer);
        }

        /// <summary>
        /// Returns the writer for the file extension, if none is found then the default writer will be returned
        /// </summary>
        /// <param name="fileExtension"></param>
        /// <returns></returns>
        public static IFileWriter GetWriterForExtension(string fileExtension)
        {
            if (fileExtension == null) throw new ArgumentNullException("fileExtension");
            
            IFileWriter writer;
            return ExtensionWriters.TryGetValue(fileExtension.ToUpper(), out writer) 
                ? writer 
                : DefaultFileWriter;
        }

        /// <summary>
        /// This will add or update a writer for a specific file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="writer"></param>
        public static void AddWriterForFile(string filePath, IFileWriter writer)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            if (writer == null) throw new ArgumentNullException("writer");

            if (!filePath.StartsWith("/"))
            {
                throw new FormatException("A file path must begin with a '/'");
            }
            PathWriters.AddOrUpdate(filePath.ToUpper(), s => writer, (s, fileWriter) => writer);
        }

        /// <summary>
        /// Returns the writer for the file path, if none is found then the default writer will be returned
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static IFileWriter GetWriterForFile(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            
            IFileWriter writer;
            return PathWriters.TryGetValue(filePath.ToUpper(), out writer)
                ? writer
                : DefaultFileWriter;
        }
    }
}
