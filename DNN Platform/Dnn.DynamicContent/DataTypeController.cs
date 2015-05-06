// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Dnn.DynamicContent.Exceptions;
using DotNetNuke.Common;
using DotNetNuke.Data;
using DotNetNuke.Entities.Content;

namespace Dnn.DynamicContent
{
    public class DataTypeController : ControllerBase<DataType, IDataTypeController, DataTypeController>, IDataTypeController
    {
        internal const string FindWhereDataTypeSql = "WHERE DataTypeId = @0";
        internal const string DataTypeCacheKey = "ContentTypes_DataTypes";

        protected override Func<IDataTypeController> GetFactory()
        {
            return () => new DataTypeController();
        }

        public DataTypeController() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public DataTypeController(IDataContext dataContext) : base(dataContext) { }

        /// <summary>
        /// Adds a new data type for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="dataType">The data type to add.</param>
        /// <returns>data type id.</returns>
        /// <exception cref="System.ArgumentNullException">data type is null.</exception>
        /// <exception cref="System.ArgumentException">dataType.Name is empty.</exception>
        public int AddDataType(DataType dataType)
        {
            //Argument Contract
            Requires.PropertyNotNullOrEmpty(dataType, "Name");

            Add(dataType);

            return dataType.DataTypeId;
        }

        /// <summary>
        /// Deletes the data type for use with Structured(Dynamic) Content Types.
        /// </summary>
        /// <param name="dataType">The data type to delete.</param>
        /// <exception cref="System.ArgumentNullException">data type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">data type id is less than 0.</exception>
        public void DeleteDataType(DataType dataType)
        {
            //Argument Contract
            Requires.NotNull(dataType);
            Requires.PropertyNotNull(dataType, "DataTypeId");
            Requires.PropertyNotNegative(dataType, "DataTypeId");

            using (DataContext)
            {
                var fieldDefinitionRepository = DataContext.GetRepository<FieldDefinition>();

                var definitions = fieldDefinitionRepository.Find(FindWhereDataTypeSql, dataType.DataTypeId);

                if (!definitions.Any())
                {
                    var dataTypeRepository = DataContext.GetRepository<DataType>();

                    dataTypeRepository.Delete(dataType);
                }
                else
                {
                    throw new DataTypeInUseException(dataType);
                }
            }
        }

        /// <summary>
        /// Gets the data types.
        /// </summary>
        /// <returns>data type collection.</returns>
        public IQueryable<DataType> GetDataTypes()
        {
            return Get().AsQueryable();
        }

        /// <summary>
        /// Updates the data type.
        /// </summary>
        /// <param name="dataType">The data type.</param>
        /// <param name="overrideWarning">An optional flag that allows developers to update DataTypes even if they are in use</param>
        /// <exception cref="System.ArgumentNullException">data type is null.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">data type id is less than 0.</exception>
        /// <exception cref="System.ArgumentException">dataType.Name is empty.</exception>
        public void UpdateDataType(DataType dataType, bool overrideWarning = false)
        {
            //Argument Contract
            Requires.NotNull(dataType);
            Requires.PropertyNotNull(dataType, "DataTypeId");
            Requires.PropertyNotNegative(dataType, "DataTypeId");
            Requires.PropertyNotNullOrEmpty(dataType, "Name");

            using (DataContext)
            {
                var canUpdate = true;

                if (!overrideWarning)
                {
                    var fieldDefinitionRepository = DataContext.GetRepository<FieldDefinition>();

                    var definitions = fieldDefinitionRepository.Find(FindWhereDataTypeSql, dataType.DataTypeId);

                    // ReSharper disable once LoopCanBeConvertedToQuery
                    foreach (var definition in definitions)
                    {
                        var contentItems = ContentController.Instance.GetContentItemsByContentType(definition.ContentTypeId);

                        if (contentItems.Any())
                        {
                            canUpdate = false;
                            break;
                        }
                    }
                }

                if (canUpdate)
                {
                    var rep = DataContext.GetRepository<DataType>();

                    rep.Update(dataType);
                }
                else
                {
                    throw new DataTypeInUseException(dataType);
                }
            }
        }
    }
}
