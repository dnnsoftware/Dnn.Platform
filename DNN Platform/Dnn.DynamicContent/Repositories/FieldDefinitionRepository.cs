// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Data;
using DotNetNuke.Common;
using DotNetNuke.Data;

namespace Dnn.DynamicContent.Repositories
{
    internal class FieldDefinitionRepository : ControllerBase<FieldDefinition, IFieldDefinitionRepository, FieldDefinitionRepository>, IFieldDefinitionRepository
    {
        protected override Func<IFieldDefinitionRepository> GetFactory()
        {
            return () => new FieldDefinitionRepository();
        }

        public FieldDefinitionRepository() : this(DotNetNuke.Data.DataContext.Instance()) { }

        public FieldDefinitionRepository(IDataContext dataContext) : base(dataContext) { }

        public new void Delete(FieldDefinition field)
        {
            Requires.NotNull(field);
         
            using (DataContext)
            {
                DataContext.BeginTransaction();

                var fieldRepository = DataContext.GetRepository<FieldDefinition>();
                fieldRepository.Delete(field);

                const string sql = @"UPDATE {objectQualifier}ContentTypes_FieldDefinitions
                                    SET [Order] = [Order] - 1
                                    WHERE [Order] > @1
                                    AND ContentTypeID = @0";
                DataContext.Execute(CommandType.Text, sql, field.ContentTypeId, field.Order);
                
                DataContext.Commit();
            }
        }

        public void Move(int contentTypeId, int sourceIndex, int targetIndex)
        {
            //Next update all the intermediate fields
            const string sql = @"IF @1 > @2 -- Move other items down order
                            BEGIN
                                UPDATE {objectQualifier}ContentTypes_FieldDefinitions
                                    SET [Order] = [Order] + 1
                                        WHERE [Order] < @1 AND[Order] >= @2
                                        AND ContentTypeID = @0
                            END
                        ELSE --Move other items up order
                            BEGIN
                                UPDATE {objectQualifier}ContentTypes_FieldDefinitions
                                    SET [Order] = [Order] - 1
                                        WHERE [Order] > @1 AND[Order] <= @2
                                        AND ContentTypeId = @0
                            END";

            using (DataContext)
            {
                DataContext.Execute(CommandType.Text, sql, contentTypeId, sourceIndex, targetIndex);
            }
        }
    }
}
