using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Quarks.Transactions;

namespace Quarks.DataAccess.EntityFramework.Tests
{
	[TestFixture]
	public class EfTransactionTests
	{
		private Mock<DbContext> _mockDbContext;
		private Mock<IDbContextFactory<DbContext>> _mockContextFactory;

	    private const string Key = "key";

		[SetUp]
		public void SetUp()
		{
			_mockDbContext = new Mock<DbContext>();
			_mockContextFactory = new Mock<IDbContextFactory<DbContext>>();

		    _mockContextFactory
		        .Setup(x => x.GetKey())
		        .Returns(Key);
			_mockContextFactory
				.Setup(x => x.Create())
				.Returns(_mockDbContext.Object);
		}

		[Test]
		public void GetCurrent_Creates_EfTransaction_If_No_Current_Transaction()
		{
			var currentEfTransaction = EfTransaction.GetCurrent(_mockContextFactory.Object);

			Assert.That(currentEfTransaction, Is.Not.Null);
		    Assert.That(currentEfTransaction.Context, Is.EqualTo(_mockDbContext.Object));
		}

		[Test]
		public void GetCurrent_Creates_EfTransaction_If_Current_Transaction()
		{
			using (ITransaction currentTransaction = Transaction.BeginTransaction())
			{
                var currentEfTransaction = EfTransaction.GetCurrent(_mockContextFactory.Object);

				Assert.That(currentEfTransaction, Is.Not.Null);
                Assert.That(currentEfTransaction.Context, Is.EqualTo(_mockDbContext.Object));

                currentTransaction.Dispose();
			}
		}

		[Test]
		public void GetCurrent_Enlists_EfTransaction_To_Current_Transaction()
		{
			using (ITransaction currentTransaction = Transaction.BeginTransaction())
			{
                var efTransaction = EfTransaction.GetCurrent(_mockContextFactory.Object);

				string key = _mockContextFactory.Object.GetKey();
				Assert.That(Transaction.Current.DependentTransactions[key], Is.SameAs(efTransaction));

				currentTransaction.Dispose();
			}
		}

		[Test]
		public void GetCurrent_Returns_Previously_Created_EfTransaction()
		{
			using (ITransaction currentTransaction = Transaction.BeginTransaction())
			{
				var previouslyCreatedEfTransaction = EfTransaction.GetCurrent(_mockContextFactory.Object);

				var efTransaction = EfTransaction.GetCurrent(_mockContextFactory.Object);

				Assert.That(efTransaction, Is.SameAs(previouslyCreatedEfTransaction));

				currentTransaction.Dispose();
			}
		}
	}
}