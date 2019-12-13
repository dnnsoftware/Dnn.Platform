using NUnit.Framework;

namespace DotNetNuke.Tests.Data
{
    public class DataAssert
    {
        public static void IsFieldValueEqual<T>(T expectedValue, string databaseName, string tableName, string field, string primaryKeyField, int id)
        {
            var value = DataUtil.GetFieldValue<T>(databaseName, tableName, field, primaryKeyField, id.ToString());

            Assert.AreEqual(expectedValue, value);
        }

        public static void RecordWithIdPresent(string databaseName, string tableName, string primaryKeyField, int id)
        {
            var count = DataUtil.GetRecordCount(databaseName, tableName, primaryKeyField, id.ToString());
            Assert.IsTrue(count == 1);
        }

        public static void RecordWithIdNotPresent(string databaseName, string tableName, string primaryKeyField, int id)
        {
            var count = DataUtil.GetRecordCount(databaseName, tableName, primaryKeyField, id.ToString());
            Assert.IsTrue(count == 0);
        }
    }
}
