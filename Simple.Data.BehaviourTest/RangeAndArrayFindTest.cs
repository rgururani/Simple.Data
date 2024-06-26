﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Simple.Data.Ado;
using Simple.Data.Mocking.Ado;

namespace Simple.Data.IntegrationTest
{
    [TestFixture]
    public class RangeAndArrayFindTest
    {
        static Database CreateDatabase(MockDatabase mockDatabase)
        {
            var mockSchemaProvider = new MockSchemaProvider();
            mockSchemaProvider.SetTables(new[] { "dbo", "Users", "BASE TABLE" });
            mockSchemaProvider.SetColumns(new[] { "dbo", "Users", "Id" },
                                          new[] { "dbo", "Users", "Name" },
                                          new[] { "dbo", "Users", "Password" },
                                          new[] { "dbo", "Users", "Age" },
                                          new[] { "dbo", "Users", "JoinDate" });
            mockSchemaProvider.SetPrimaryKeys(new object[] { "dbo", "Users", "Id", 0 });
            return new Database(new AdoAdapter(new MockConnectionProvider(new MockDbConnection(mockDatabase), mockSchemaProvider)));
        }

        [Test]
        public void TestFindByWithIntRange()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.FindAllById(1.to(10)).ToList();
            ClassicAssert.AreEqual("select [dbo].[users].[id],[dbo].[users].[name],[dbo].[users].[password],[dbo].[users].[age],[dbo].[users].[joindate] from [dbo].[users] where [dbo].[users].[id] between @p1_start and @p1_end".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            ClassicAssert.AreEqual(1, mockDatabase.Parameters[0]);
            ClassicAssert.AreEqual(10, mockDatabase.Parameters[1]);
        }

        [Test]
        public void TestFindByWithIntArray()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.FindAllById(new[] { 1, 2, 3 }).ToList();
            ClassicAssert.AreEqual("select [dbo].[users].[id],[dbo].[users].[name],[dbo].[users].[password],[dbo].[users].[age],[dbo].[users].[joindate] from [dbo].[users] where [dbo].[users].[id] in (@p1_0,@p1_1,@p1_2)".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            ClassicAssert.AreEqual(1, mockDatabase.Parameters[0]);
            ClassicAssert.AreEqual(2, mockDatabase.Parameters[1]);
            ClassicAssert.AreEqual(3, mockDatabase.Parameters[2]);
        }

        [Test]
        public void TestQueryWithNotEqualIntArray()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.Query().Where(database.Users.Id != new[] { 1, 2, 3 }).ToList();
            ClassicAssert.AreEqual("select [dbo].[users].[id],[dbo].[users].[name],[dbo].[users].[password],[dbo].[users].[age],[dbo].[users].[joindate] from [dbo].[users] where [dbo].[users].[id] not in (@p1_0,@p1_1,@p1_2)".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            ClassicAssert.AreEqual(1, mockDatabase.Parameters[0]);
            ClassicAssert.AreEqual(2, mockDatabase.Parameters[1]);
            ClassicAssert.AreEqual(3, mockDatabase.Parameters[2]);
        }

        [Test]
        public void TestFindByWithDateRange()
        {
            var mockDatabase = new MockDatabase();
            dynamic database = CreateDatabase(mockDatabase);
            database.Users.FindAllByJoinDate("2011-01-01".to("2011-01-31")).ToList();
            ClassicAssert.AreEqual("select [dbo].[users].[id],[dbo].[users].[name],[dbo].[users].[password],[dbo].[users].[age],[dbo].[users].[joindate] from [dbo].[users] where [dbo].[users].[joindate] between @p1_start and @p1_end".ToLowerInvariant(), mockDatabase.Sql.ToLowerInvariant());
            ClassicAssert.AreEqual(new DateTime(2011, 1, 1), mockDatabase.Parameters[0]);
            ClassicAssert.AreEqual(new DateTime(2011, 1, 31), mockDatabase.Parameters[1]);
        }
    }
}
