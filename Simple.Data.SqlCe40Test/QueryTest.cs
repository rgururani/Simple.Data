﻿using System.IO;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Simple.Data.SqlCe40Test
{
    [TestFixture]
    public class QueryTest
    {
        private static readonly string DatabasePath = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase.Substring(8)),
            "TestDatabase.sdf");

        [Test]
        public void ShouldSelectFromOneToTen()
        {
            var db = Database.Opener.OpenFile(DatabasePath);
            var query = db.PagingTest.QueryById(1.to(100)).Take(10);
            int index = 1;
            foreach (var row in query)
            {
                ClassicAssert.AreEqual(index, row.Id);
                index++;
            }
        }

        [Test]
        public void ShouldSelectFromOneToTenWithCount()
        {
            var db = Database.Opener.OpenFile(DatabasePath);
            Promise<int> count;
            var query = db.PagingTest.QueryById(1.to(100)).WithTotalCount(out count).Take(10);
            int index = 1;
            foreach (var row in query)
            {
                ClassicAssert.AreEqual(index, row.Id);
                index++;
            }
            ClassicAssert.IsTrue(count.HasValue);
            ClassicAssert.AreEqual(100, count.Value);
        }

        [Test]
        public void ShouldSelectFromElevenToTwenty()
        {
            var db = Database.Opener.OpenFile(DatabasePath);
            var query = db.PagingTest.QueryById(1.to(100)).Skip(10).Take(10);
            int index = 11;
            foreach (var row in query)
            {
                ClassicAssert.AreEqual(index, row.Id);
                index++;
            }
        }

        [Test]
        public void ShouldSelectFromOneHundredToNinetyOne()
        {
            var db = Database.Opener.OpenFile(DatabasePath);
            var query = db.PagingTest.QueryById(1.to(100)).OrderByDescending(db.PagingTest.Id).Skip(0).Take(10);
            int index = 100;
            foreach (var row in query)
            {
                ClassicAssert.AreEqual(index, row.Id);
                index--;
            }
        }
    }
}
