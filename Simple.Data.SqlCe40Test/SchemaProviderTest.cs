namespace Simple.Data.SqlCe40Test
{
    using System;
    using System.Linq;
    using NUnit.Framework;
    using NUnit.Framework.Legacy;
    using Simple.Data.SqlCe40;

    [TestFixture]
    public class SchemaProviderTest
    {
        [Test]
        public void NullConnectionProviderCausesException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqlCe40SchemaProvider(null));
        }

        [Test]
        public void ProceduresIsEmpty()
        {
            ClassicAssert.AreEqual(0, new SqlCe40SchemaProvider(new SqlCe40ConnectionProvider()).GetStoredProcedures().Count());
        }
    }
}