﻿using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Simple.Data.Ado;

namespace Simple.Data.SqlTest
{
    [TestFixture]
    public class TransactionTests
    {
        [SetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void TestCommit()
        {
            var db = DatabaseHelper.Open();

            using (var tx = db.BeginTransaction())
            {
                try
                {
                    var order = tx.Orders.Insert(CustomerId: 1, OrderDate: DateTime.Today);
                    tx.OrderItems.Insert(OrderId: order.OrderId, ItemId: 1, Quantity: 3);
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
            ClassicAssert.AreEqual(2, db.Orders.All().ToList().Count);
            ClassicAssert.AreEqual(2, db.OrderItems.All().ToList().Count);
        }

        [Test]
        public void TestRollback()
        {
            var db = DatabaseHelper.Open();

            using (var tx = db.BeginTransaction())
            {
                var order = tx.Orders.Insert(CustomerId: 1, OrderDate: DateTime.Today);
                tx.OrderItems.Insert(OrderId: order.OrderId, ItemId: 1, Quantity: 3);
                tx.Rollback();
            }
            ClassicAssert.AreEqual(1, db.Orders.All().ToList().Count);
            ClassicAssert.AreEqual(1, db.OrderItems.All().ToList().Count);
        }

        [Test]
        public void TestWithOptionsTransaction()
        {
            var dbWithOptions = DatabaseHelper.Open().WithOptions(new AdoOptions(commandTimeout: 60000));
            using (var tx = dbWithOptions.BeginTransaction())
            {
                tx.Orders.Insert(CustomerId: 1, OrderDate: DateTime.Today);
                tx.Rollback();
            }

            Assert.Pass();
        }

        [Test]
        public void TestRollbackOnProcedure()
        {
            var db = DatabaseHelper.Open();

            Customer customer;
            using (var tx = db.BeginTransaction())
            {
                customer = tx.CreateCustomer().FirstOrDefault();
                tx.Rollback();
            }
            customer = db.Customers.FindByCustomerId(customer.CustomerId);
            ClassicAssert.IsNull(customer);
        }

        [Test]
        public void QueryInsideTransaction()
        {
            var db = DatabaseHelper.Open();

            using (var tx = db.BeginTransaction())
            {
                tx.Users.Insert(Name: "Arthur", Age: 42, Password: "Ladida");
                User u2 = tx.Users.FindByName("Arthur");
                ClassicAssert.IsNotNull(u2);
                ClassicAssert.AreEqual("Arthur", u2.Name);
            }
        }
    }
}
