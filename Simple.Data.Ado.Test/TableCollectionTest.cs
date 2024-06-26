﻿using Simple.Data.Ado.Schema;
using NUnit.Framework;
using System;
using NUnit.Framework.Legacy;

namespace Simple.Data.UnitTest
{


    /// <summary>
    ///This is a test class for TableCollection and is intended
    ///to contain all TableCollection Unit Tests
    ///</summary>
    [TestFixture()]
    public class TableCollectionTest
    {
        /// <summary>
        ///A test for Find
        ///</summary>
        [Test()]
        public void FindExactMatchTest()
        {
            Helper("Users", "Users");
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [Test()]
        public void FindCaseInsensitiveMatchTest()
        {
            Helper("USERS", "Users");
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [Test()]
        public void FindPluralMatchTest()
        {
            Helper("Users", "User");
        }

        /// <summary>
        ///A test for Find
        ///</summary>
        [Test()]
        public void FindSingularMatchTest()
        {
            Helper("User", "Users");
        }

        [Test]
        public void FindSnakeFromCamel()
        {
            Helper("USER_PROFILE", "UserProfile");
        }

        [Test]
        public void FindSnakeFromPluralCamel()
        {
            Helper("USER_PROFILE", "UserProfiles");
        }

        [Test]
        public void FindCamelFromSnake()
        {
            Helper("UserProfile", "USER_PROFILE");
        }

        [Test]
        public void FindCamelFromPluralSnake()
        {
            Helper("UserProfiles", "USER_PROFILE");
        }

        private static void Helper(string actualName, string searchName)
        {
            var target = new TableCollection { new Table(actualName, "", TableType.Table) };
            var actual = target.Find(searchName);
            ClassicAssert.IsNotNull(actual);
            ClassicAssert.AreEqual(actualName, actual.ActualName);
        }
    }
}
