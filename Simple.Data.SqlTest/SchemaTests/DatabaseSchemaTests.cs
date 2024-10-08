﻿using System;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.TestHelper;

namespace Simple.Data.SqlTest.SchemaTests
{
    [TestFixture]
    public class DatabaseSchemaTests : DatabaseSchemaTestsBase
    {

        protected override Database GetDatabase()
        {
            return Database.OpenConnection(DatabaseHelper.ConnectionString);
        }

        [Test]
        public void TestTables()
        {
            ClassicAssert.AreEqual(1, Schema.Tables.Count(t => t.ActualName == "Users"));
        }

        [Test]
        public void TestColumns()
        {
            var table = Schema.FindTable("Users");
            ClassicAssert.AreEqual(1, table.Columns.Count(c => c.ActualName == "Id"));
        }

        [Test]
        public void TestPrimaryKey()
        {
            var table = Schema.FindTable("Customers");
            ClassicAssert.AreEqual(1, table.PrimaryKey.Length);
            ClassicAssert.AreEqual("CustomerId", table.PrimaryKey[0]);
        }

        [Test]
        public void TestForeignKey()
        {
            var table = Schema.FindTable("Orders");
            var fkey = table.ForeignKeys.Single();
            ClassicAssert.AreEqual("CustomerId", fkey.Columns[0]);
            ClassicAssert.AreEqual("Customers", fkey.MasterTable.Name);
            ClassicAssert.AreEqual("CustomerId", fkey.UniqueColumns[0]);
        }

        [Test]
        public void TestIdentityIsTrueWhenItShouldBe()
        {
            var column = Schema.FindTable("Customers").FindColumn("CustomerId");
            ClassicAssert.IsTrue(column.IsIdentity);
        }

        [Test]
        public void TestIdentityIsFalseWhenItShouldBe()
        {
            var column = Schema.FindTable("Customers").FindColumn("Name");
            ClassicAssert.IsFalse(column.IsIdentity);
        }

        [Test]
        public void TestNewTableIsAddedToSchemaAfterReset()
        {
            dynamic db = GetDatabase();
            db.Users.FindById(1); // Forces population of schema...

            using (var cn = new SqlConnection(DatabaseHelper.ConnectionString))
            using (var cmd = cn.CreateCommand())
            {
                cn.Open();

                cmd.CommandText = @"IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RuntimeTable]') AND type in (N'U'))
DROP TABLE [dbo].[RuntimeTable]";
                cmd.ExecuteNonQuery();
                cmd.CommandText = @"CREATE TABLE [dbo].[RuntimeTable] ([Id] int,
	CONSTRAINT [PK_RuntimeTable] PRIMARY KEY CLUSTERED ( [Id] ASC))";
                cmd.ExecuteNonQuery();
            }

            db.GetAdapter().Reset();
            db.RuntimeTable.Insert(Id: 1);
            var row = db.RuntimeTable.FindById(1);
            ClassicAssert.AreEqual(1, row.Id);
        }
    }
}
