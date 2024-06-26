﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.SqlTest
{
    using NUnit.Framework.Legacy;
    using System.Collections;

    [TestFixture]
    public class QueryTest
    {
        [OneTimeSetUp]
        public void Setup()
        {
            DatabaseHelper.Reset();
        }

        [Test]
        public void CountWithNoCriteriaShouldSelectThree()
        {
            var db = DatabaseHelper.Open();
            var count = db.Users.GetCount();
            Assert.That(count, Is.EqualTo(3));

        }

        [Test]
        public void CountWithCriteriaShouldSelectTwo()
        {
            var db = DatabaseHelper.Open();
            int count = db.Users.GetCount(db.Users.Age > 30);
            ClassicAssert.AreEqual(2, count);
        }

        [Test]
        public void CountByShouldSelectOne()
        {
            var db = DatabaseHelper.Open();
            ClassicAssert.AreEqual(1, db.Users.GetCountByName("Bob"));
        }

        [Test]
        public void ExistsWithNoCriteriaShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            ClassicAssert.AreEqual(true, db.Users.Exists());
        }

        [Test]
        public void ExistsWithValidCriteriaShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            ClassicAssert.AreEqual(true, db.Users.Exists(db.Users.Age > 30));
        }

        [Test]
        public void ExistsWithInvalidCriteriaShouldReturnFalse()
        {
            var db = DatabaseHelper.Open();
            ClassicAssert.AreEqual(false, db.Users.Exists(db.Users.Age == -1));
        }

        [Test]
        public void ExistsByValidValueShouldReturnTrue()
        {
            var db = DatabaseHelper.Open();
            ClassicAssert.AreEqual(true, db.Users.ExistsByName("Bob"));
        }

        [Test]
        public void ExistsByInvalidValueShouldReturnFalse()
        {
            var db = DatabaseHelper.Open();
            ClassicAssert.AreEqual(false, db.Users.ExistsByName("Peter Kay"));
        }

        [Test]
        public void ColumnAliasShouldChangeDynamicPropertyName()
        {
            var db = DatabaseHelper.Open();
            var actual = db.Users.QueryById(1).Select(db.Users.Name.As("Alias")).First();
            ClassicAssert.AreEqual("Bob", actual.Alias);
        }

        [Test]
        public void MissingColumnShouldHaveColumnNotFoundMessage()
        {
            var db = DatabaseHelper.Open();
            var actual = db.Users.QueryById(1).Select(db.Users.Name).First();
            Assert.Throws<UnresolvableObjectException>(() => Console.WriteLine(actual.Bobbins), "Column not found.");
        }

        [Test]
        public void ShouldSelectFromOneToTen()
        {
            var db = DatabaseHelper.Open();
            var query = db.PagingTest.QueryById(1.to(100)).Take(10);
            int index = 1;
            foreach (var row in query)
            {
                ClassicAssert.AreEqual(index, row.Id);
                index++;
            }
        }

        [Test]
        public void ShouldSelectFromElevenToTwenty()
        {
            var db = DatabaseHelper.Open();
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
            var db = DatabaseHelper.Open();
            var query = db.PagingTest.QueryById(1.to(100)).OrderByDescending(db.PagingTest.Id).Skip(0).Take(10);
            int index = 100;
            foreach (var row in query)
            {
                ClassicAssert.AreEqual(index, row.Id);
                index--;
            }
        }

        [Test]
        public void WithTotalCountShouldGiveCount()
        {
            Promise<int> count;
            var db = DatabaseHelper.Open();
            var list = db.PagingTest.QueryById(1.to(50))
                .Take(10)
                .WithTotalCount(out count)
                .ToList();

            ClassicAssert.AreEqual(10, list.Count);
            ClassicAssert.IsTrue(count.HasValue);
            ClassicAssert.AreEqual(50, count.Value);
        }

        [Test]
        public void WithTotalCountWithExplicitSelectShouldGiveCount()
        {
            Promise<int> count;
            var db = DatabaseHelper.Open();
            List<dynamic> list = db.PagingTest.QueryById(1.to(50))
                .Select(db.PagingTest.Id)
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            ClassicAssert.IsTrue(count.HasValue);
            ClassicAssert.AreEqual(50, count.Value);
            ClassicAssert.AreEqual(10, list.Count);
            foreach (var dictionary in list.Cast<IDictionary<string, object>>())
            {
                ClassicAssert.AreEqual(1, dictionary.Count);
            }
        }

        [Test]
        public void WithTotalCountWithExplicitSelectAndOrderByShouldGiveCount()
        {
            Promise<int> count;
            var db = DatabaseHelper.Open();
            List<dynamic> list = db.PagingTest.QueryById(1.to(50))
                .Select(db.PagingTest.Id)
                .OrderByDescending(db.PagingTest.Id)
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            ClassicAssert.IsTrue(count.HasValue);
            ClassicAssert.AreEqual(50, count.Value);
            ClassicAssert.AreEqual(10, list.Count);
            foreach (var dictionary in list.Cast<IDictionary<string, object>>())
            {
                ClassicAssert.AreEqual(1, dictionary.Count);
            }
        }

        [Test]
        // ReSharper disable InconsistentNaming
        public void WithTotalCountShouldGiveCount_ObsoleteFutureVersion()
        // ReSharper restore InconsistentNaming
        {
            Future<int> count;
            var db = DatabaseHelper.Open();
            var list = db.PagingTest.QueryById(1.to(50))
                .WithTotalCount(out count)
                .Take(10)
                .ToList();

            ClassicAssert.AreEqual(10, list.Count);
            ClassicAssert.IsTrue(count.HasValue);
            ClassicAssert.AreEqual(50, count.Value);
        }

        [Test]
        public void ShouldDirectlyQueryDetailTable()
        {
            var db = DatabaseHelper.Open();
            var order = db.Customers.QueryByNameAndAddress("Test", "100 Road").Orders.FirstOrDefault();
            ClassicAssert.IsNotNull(order);
            ClassicAssert.AreEqual(1, order.OrderId);
        }

        [Test]
        public void ShouldReturnNullWhenNoRowFound()
        {
            var db = DatabaseHelper.Open();
            string name = db.Customers
                        .Query()
                        .Select(db.Customers.Name)
                        .Where(db.Customers.CustomerId == 0) // There is no CustomerId 0
                        .OrderByName()
                        .Take(1) // Should return only one record no matter what
                        .ToScalarOrDefault<string>();
            ClassicAssert.IsNull(name);
        }

        [Test]
        public void ToScalarListShouldReturnStringList()
        {
            var db = DatabaseHelper.Open();
            List<string> name = db.Customers
                        .Query()
                        .Select(db.Customers.Name)
                        .OrderByName()
                        .ToScalarList<string>();
            ClassicAssert.IsNotNull(name);
            ClassicAssert.AreNotEqual(0, name.Count);
        }

        [Test]
        public void ToScalarArrayShouldReturnStringArray()
        {
            var db = DatabaseHelper.Open();
            string[] name = db.Customers
                        .Query()
                        .Select(db.Customers.Name)
                        .OrderByName()
                        .ToScalarArray<string>();
            ClassicAssert.IsNotNull(name);
            ClassicAssert.AreNotEqual(0, name.Length);
        }

        [Test]
        public void HavingWithMinDateShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row =
                db.GroupTestMaster.Query().Having(db.GroupTestMaster.GroupTestDetail.Date.Min() >=
                                                  new DateTime(2000, 1, 1))
                                                  .FirstOrDefault();
            ClassicAssert.IsNotNull(row);
            ClassicAssert.AreEqual("Two", row.Name);
        }
        [Test]
        public void HavingWithMaxDateShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row =
                db.GroupTestMaster.Query().Having(db.GroupTestMaster.GroupTestDetail.Date.Max() <
                                                  new DateTime(2009, 1, 1))
                                                  .FirstOrDefault();
            ClassicAssert.IsNotNull(row);
            ClassicAssert.AreEqual("One", row.Name);
        }

        [Test]
        public void HavingWithCountShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.GroupTestMaster.Query()
                .Having(db.GroupTestMaster.GroupTestDetail.Id.Count() == 2)
                .FirstOrDefault();
            ClassicAssert.IsNotNull(row);
            ClassicAssert.AreEqual("Two", row.Name);
        }

        [Test]
        public void HavingWithAverageShouldReturnCorrectRow()
        {
            var db = DatabaseHelper.Open();
            var row = db.GroupTestMaster.Query()
                .Having(db.GroupTestMaster.GroupTestDetail.Number.Average() == 2)
                .FirstOrDefault();
            ClassicAssert.IsNotNull(row);
            ClassicAssert.AreEqual("One", row.Name);
        }

        [Test]
        public void ToScalarOrDefault()
        {
            var db = DatabaseHelper.Open();
            int max = db.Users.FindAllByName("ZXCVBNM").Select(db.Users.Age.Max()).ToScalarOrDefault<int>();
            ClassicAssert.AreEqual(0, max);
        }


        [Test]
        public void WithClauseShouldPreselectDetailTableAsCollection()
        {
            var db = DatabaseHelper.Open();
            var result = db.Customers.FindAllByCustomerId(1).WithOrders().FirstOrDefault() as IDictionary<string, object>;
            ClassicAssert.IsNotNull(result);
            ClassicAssert.Contains("Orders", (ICollection)result.Keys);
            var orders = result["Orders"] as IList<IDictionary<string, object>>;
            ClassicAssert.IsNotNull(orders);
            ClassicAssert.AreEqual(1, orders.Count);
        }

        [Test]
        public void WithClauseShouldPreselectDetailTablesAsCollections()
        {
            var db = DatabaseHelper.Open();
            var result = db.Customers.FindAllByCustomerId(1).WithOrders().WithNotes().FirstOrDefault() as IDictionary<string, object>;
            ClassicAssert.IsNotNull(result);
            ClassicAssert.Contains("Orders", (ICollection)result.Keys);
            var orders = result["Orders"] as IList<IDictionary<string, object>>;
            ClassicAssert.IsNotNull(orders);
            ClassicAssert.AreEqual(1, orders.Count);
            ClassicAssert.Contains("Notes", (ICollection)result.Keys);
            var notes = result["Notes"] as IList<IDictionary<string, object>>;
            ClassicAssert.IsNotNull(notes);
            ClassicAssert.AreEqual(2, notes.Count);
        }

        [Test]
        public void FindAllWithClauseWithJoinCriteriaShouldPreselectDetailTableAsCollection()
        {
            var db = DatabaseHelper.Open();
            var result = db.Customers.FindAllByCustomerId(1).With(db.Customers.Orders.OrderItems).FirstOrDefault() as IDictionary<string, object>;
            ClassicAssert.IsNotNull(result);
            ClassicAssert.Contains("OrderItems", (ICollection)result.Keys);
            var orderItems = result["OrderItems"] as IList<IDictionary<string, object>>;
            ClassicAssert.IsNotNull(orderItems);
            ClassicAssert.AreEqual(1, orderItems.Count);
        }

        [Test]
        [Ignore("Ignore a test")]
        public void FindAllWithClauseWithNestedDetailTable()
        {
            var db = DatabaseHelper.Open();
            var result = db.Customers.FindAllByCustomerId(1).With(db.Customers.Orders).With(db.Customers.Orders.OrderItems).FirstOrDefault() as IDictionary<string, object>;
            ClassicAssert.IsNotNull(result);
            ClassicAssert.Contains("Orders", result.Keys.ToArray());
            var orders = result["Orders"] as IList<IDictionary<string, object>>;
            ClassicAssert.IsNotNull(orders);
            ClassicAssert.AreEqual(1, orders.Count);
            var order = orders[0];
            ClassicAssert.Contains("OrderItems", order.Keys.ToArray());
        }

        [Test]
        public void GetWithClauseWithJoinCriteriaShouldPreselectDetailTableAsCollection()
        {
            var db = DatabaseHelper.Open();
            var result = db.Customers.With(db.Customers.Orders.OrderItems).Get(1) as IDictionary<string, object>;
            ClassicAssert.IsNotNull(result);
            ClassicAssert.Contains("OrderItems", (ICollection)result.Keys);
            var orderItems = result["OrderItems"] as IList<IDictionary<string, object>>;
            ClassicAssert.IsNotNull(orderItems);
            ClassicAssert.AreEqual(1, orderItems.Count);
        }

        [Test]
        public void WithClauseWithTwoStepShouldPreselectManyToManyTableAsCollection()
        {
            var db = DatabaseHelper.Open();
            var result = db.Customers.FindAll(db.Customers.Order.OrderId == 1).WithOrders().FirstOrDefault() as IDictionary<string, object>;
            ClassicAssert.IsNotNull(result);
            ClassicAssert.Contains("Orders", (ICollection)result.Keys);
            var orders = result["Orders"] as IList<IDictionary<string, object>>;
            ClassicAssert.IsNotNull(orders);
            ClassicAssert.AreEqual(1, orders.Count);
        }

        [Test]
        public void WithClauseShouldPreselectMasterTableAsDictionary()
        {
            var db = DatabaseHelper.Open();
            var result = db.Orders.FindAllByOrderId(1).WithCustomer().FirstOrDefault() as IDictionary<string, object>;
            ClassicAssert.IsNotNull(result);
            ClassicAssert.Contains("Customer", (ICollection)result.Keys);
            var customer = result["Customer"] as IDictionary<string, object>;
            ClassicAssert.IsNotNull(customer);
        }

        [Test]
        public void WithClauseShouldCastToStaticTypeWithComplexProperty()
        {
            var db = DatabaseHelper.Open();
            Order actual = db.Orders.FindAllByOrderId(1).WithCustomer().FirstOrDefault();
            ClassicAssert.IsNotNull(actual);
            ClassicAssert.IsNotNull(actual.Customer);
            ClassicAssert.AreEqual("Test", actual.Customer.Name);
            ClassicAssert.AreEqual("100 Road", actual.Customer.Address);
        }

        [Test]
        public void WithClauseShouldCastToStaticTypeWithCollection()
        {
            var db = DatabaseHelper.Open();
            Customer actual = db.Customers.FindAllByCustomerId(1).WithOrders().FirstOrDefault();
            ClassicAssert.IsNotNull(actual);
            ClassicAssert.AreEqual(1, actual.Orders.Single().OrderId);
            ClassicAssert.AreEqual(new DateTime(2010, 10, 10), actual.Orders.Single().OrderDate);
        }

        [Test]
        public void WithClauseShouldCastToStaticTypeWithEmptyCollection()
        {
            var db = DatabaseHelper.Open();
            var newCustomer = db.Customers.Insert(Name: "No Orders");
            Customer actual = db.Customers.FindAllByCustomerId(newCustomer.CustomerId).WithOrders().FirstOrDefault();
            ClassicAssert.IsNotNull(actual);
            ClassicAssert.IsNotNull(actual.Orders);
            ClassicAssert.AreEqual(0, actual.Orders.Count);
        }

        [Test]
        public void WithClauseContainingAliasShouldReturnResults()
        {
            var db = DatabaseHelper.Open();
            var actual = db.Customers
                           .With(db.Customers.Orders.As("Orders_1"))
                           .With(db.Customers.Orders.As("Orders_2"))
                           .FirstOrDefault();
            ClassicAssert.IsNotNull(actual);
            ClassicAssert.AreEqual(1, actual.Orders_1.Single().OrderId);
            ClassicAssert.AreEqual(1, actual.Orders_2.Single().OrderId);
            ClassicAssert.AreEqual(new DateTime(2010, 10, 10), actual.Orders_1.Single().OrderDate);
            ClassicAssert.AreEqual(new DateTime(2010, 10, 10), actual.Orders_2.Single().OrderDate);
        }

        [Test]
        public void SelfJoinShouldNotThrowException()
        {
            var db = DatabaseHelper.Open();

            var q = db.Employees.Query().LeftJoin(db.Employees.As("Manager"), Id: db.Employees.ManagerId);
            q = q.Select(db.Employees.Name, q.Manager.Name.As("Manager"));
            List<dynamic> employees = q.ToList();

            ClassicAssert.AreEqual(3, employees.Count); // The top man is missing

            var kingsSubordinates = employees.Where(e => e.Manager == "Alice").ToList();

            ClassicAssert.AreEqual(1, kingsSubordinates.Count);
        }

        [Test]
        public void OrderByOnJoinedColumnShouldUseJoinedColumn()
        {
            SimpleDataTraceSources.TraceSource.Switch.Level = SourceLevels.All;
            var traceListener = new TestTraceListener();
            SimpleDataTraceSources.TraceSource.Listeners.Add(traceListener);
            Trace.Listeners.Add(traceListener);
            var db = DatabaseHelper.Open();

            var q = db.Employees.Query().LeftJoin(db.Employees.As("Manager"), Id: db.Employees.ManagerId);
            q = q.Select(db.Employees.Name, q.Manager.Name.As("Manager"));
            List<dynamic> employees = q.OrderBy(q.Manager.Name).ToList();
            SimpleDataTraceSources.TraceSource.Listeners.Remove(traceListener);
            ClassicAssert.Greater(traceListener.Output.IndexOf("order by [manager].[name]", StringComparison.OrdinalIgnoreCase), 0);
        }

        [Test]
        public void CanFetchMoreThanOneHundredRows()
        {
            var db = DatabaseHelper.Open();

            db.Customers.Insert(Enumerable.Range(0, 200).Select(n => new Customer { Name = "Customer " + n }));

            List<dynamic> customers = db.Customers.All().ToList();

            ClassicAssert.GreaterOrEqual(customers.Count, 200);
        }

        [Test]
        public void QueryWithForUpdateFalseShouldReturnCorrectResult()
        {
            var db = DatabaseHelper.Open();
            var actual = db.Users.QueryById(1).Select(db.Users.Name).ForUpdate(false).First();
            ClassicAssert.AreEqual("Bob", actual.Name);
        }

        [Test]
        public void QueryWithForUpdateTrueShouldReturnCorrectResult()
        {
            var db = DatabaseHelper.Open();
            var actual = db.Users.QueryById(1).Select(db.Users.Name).ForUpdate(true).First();
            ClassicAssert.AreEqual("Bob", actual.Name);
        }
    }
}
