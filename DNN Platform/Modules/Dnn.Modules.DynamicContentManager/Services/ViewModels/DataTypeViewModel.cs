#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using Dnn.DynamicContent;
using Newtonsoft.Json;

namespace Dnn.Modules.DynamicContentManager.Services.ViewModels
{
    /// <summary>
    /// DataTypeViewModel represents a Data Type object within the DataType Web Service API
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DataTypeViewModel
    {
        /// <summary>
        /// Constructs a DatatTypeViewModel from a DataType object
        /// </summary>
        /// <param name="dataType">The DataType object</param>
        public DataTypeViewModel(DataType dataType)
        {
            BaseType = dataType.UnderlyingDataType;
            DataTypeId = dataType.DataTypeId;
            IsSystem = dataType.IsSystem;
            Name = dataType.Name;
        }

        /// <summary>
        /// The base Data Type of the Data Type
        /// </summary>
        [JsonProperty("baseType")]
        public UnderlyingDataType BaseType { get; set; }

        /// <summary>
        /// The Id of the Data Type
        /// </summary>
        [JsonProperty("dataTypeId")]
        public int DataTypeId { get; set; }

        /// <summary>
        /// The name of the Data Type
        /// </summary>
        [JsonProperty("isSystem")]
        public bool IsSystem { get; set; }

        /// <summary>
        /// The name of the Data Type
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
