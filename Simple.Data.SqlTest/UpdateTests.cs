﻿using System.Dynamic;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    using NUnit.Framework.Legacy;
    using System.Collections.Generic;

    [TestFixture]
    public class UpdateTests
    {
        [OneTimeSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestUpdateWithNamedArguments()
        {
            var db = DatabaseHelper.Open();

            db.Users.UpdateById(Id: 1, Name: "Ford", Password: "hoopy", Age: 29);
            var user = db.Users.FindById(1);
            ClassicAssert.IsNotNull(user);
            ClassicAssert.AreEqual("Ford", user.Name);
            ClassicAssert.AreEqual("hoopy", user.Password);
            ClassicAssert.AreEqual(29, user.Age);
        }

        [Test]
        public void TestUpdateWithStaticTypeObject()
        {
            var db = DatabaseHelper.Open();

            var user = new User { Id = 2, Name = "Zaphod", Password = "zarquon", Age = 42 };

            db.Users.Update(user);

            User actual = db.Users.FindById(2);

            ClassicAssert.IsNotNull(user);
            ClassicAssert.AreEqual("Zaphod", actual.Name);
            ClassicAssert.AreEqual("zarquon", actual.Password);
            ClassicAssert.AreEqual(42, actual.Age);
        }

        [Test]
        public void TestUpdateWithDynamicTypeObject()
        {
            var db = DatabaseHelper.Open();

            dynamic user = new ExpandoObject();
            user.Id = 3;
            user.Name = "Marvin";
            user.Password = "diodes";
            user.Age = 42000000;

            db.Users.Update(user);

            var actual = db.Users.FindById(3);

            ClassicAssert.IsNotNull(user);
            ClassicAssert.AreEqual("Marvin", actual.Name);
            ClassicAssert.AreEqual("diodes", actual.Password);
            ClassicAssert.AreEqual(42000000, actual.Age);
        }

        [Test]
        public void TestUpdateWithVarBinaryMaxColumn()
        {
            var db = DatabaseHelper.Open();
            var blob = new Blob
            {
                Id = 1,
                Data = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0 }
            };
            db.Blobs.Insert(blob);

            var newData = blob.Data = new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            db.Blobs.Update(blob);

            blob = db.Blobs.FindById(1);

            ClassicAssert.IsTrue(newData.SequenceEqual(blob.Data));
        }

        [Test]
        public void TestUpdateWithJoinCriteria()
        {
            var db = DatabaseHelper.Open();
            db.Customers.UpdateAll(db.Customers.Orders.OrderId == 1, Name: "Updated");
            var customer = db.Customers.Get(1);
            ClassicAssert.AreEqual("Updated", customer.Name);
        }

        [Test]
        public void TestUpdateAllWithNoMatchingRows()
        {
            var db = DatabaseHelper.Open();
            db.test.SchemaTable.UpdateAll(db.test.SchemaTable.Id == 1138, Description: "Updated");
            var test = db.test.SchemaTable.FindById(1138);
            ClassicAssert.IsNull(test);
        }

        [Test]
        public void TestUpdateWithJoinCriteriaOnCompoundKeyTable()
        {
            var db = DatabaseHelper.Open();
            db.CompoundKeyMaster.UpdateAll(db.CompoundKeyMaster.CompoundKeyDetail.Value == 1, Description: "Updated");
            var record = db.CompoundKeyMaster.Get(1, 1);
            ClassicAssert.AreEqual("Updated", record.Description);
        }

        [Test]
        public void ToListShouldExecuteQuery()
        {
            var db = DatabaseHelper.Open();
            List<Customer> customers = db.Customers.All().ToList<Customer>();
            foreach (var customer in customers)
            {
                customer.Address = "Updated";
            }

            Assert.DoesNotThrow(() =>
                db.Customers.Update(customers));
        }

        [Test]
        public void TestUpdateWithTimestamp()
        {
            var db = DatabaseHelper.Open();
            var row = db.TimestampTest.Insert(Description: "Inserted");
            row.Description = "Updated";
            db.TimestampTest.Update(row);
            row = db.TimestampTest.Get(row.Id);
            ClassicAssert.AreEqual("Updated", row.Description);
        }

        [Test]
        public void TestUpdateByInputIsNotMutated()
        {
            var db = DatabaseHelper.Open();
            var user = new Dictionary<string, object>
                           {
              {"Id", 0},
              {"Age", 1},
              {"Name", "X"},
              {"Password", "P"}
            };

            user["Id"] = db.Users.Insert(user).Id;

            db.Users.UpdateById(user);

            ClassicAssert.AreEqual(4, user.Keys.Count);
        }

        [Test]
        public void TestUpdatingACriteriaColumn()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.Insert(Age: 42, Name: "Z1", Password: "argh");
            db.Users.UpdateAll(db.Users.Name == "Z1", Name: "1Z");
        }

        [Test]
        public void TestUpdateWithOriginalUsingAnonymousObjects()
        {
            var db = DatabaseHelper.Open();
            var user = db.Users.Insert(Age: 54, Name: "YZ1", Password: "argh");
            db.Users.Update(new { Name = "2YZ" }, new { Name = "YZ1" });
            var actual = db.Users.FindById(user.Id);
            ClassicAssert.AreEqual("2YZ", actual.Name);
        }
    }
}