using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Quarks.DataAccess.EntityFramework.ContextManagement;
using Quarks.Transactions;

namespace Quarks.DataAccess.EntityFramework.Tests
{
	[TestFixture]
	public class EfTransactionTests
	{
		private Mock<DbContext> _mockDbContext;
		private Mock<IEfContextManager<DbContext>> _mockContextManager;

		[SetUp]
		public void SetUp()
		{
			_mockDbContext = new Mock<DbContext>();
			_mockContextManager = new Mock<IEfContextManager<DbContext>>();

			_mockContextManager
				.Setup(x => x.CreateContext())
				.Returns(_mockDbContext.Object);
		}

		[Test]
		public void GetCurrent_Creates_EfTransaction_If_No_Current_Transaction()
		{
			EfTransaction<DbContext> currentEfTransaction = EfTransaction.GetCurrent(_mockContextManager.Object);

			Assert.That(currentEfTransaction, Is.Not.Null);
			Assert.That(currentEfTransaction.ContextManager, Is.SameAs(_mockContextManager.Object));
		}

		[Test]
		public void GetCurrent_Creates_EfTransaction_If_Current_Transaction()
		{
			using (ITransaction currentTransaction = Transaction.BeginTransaction())
			{
				EfTransaction<DbContext> currentEfTransaction = EfTransaction.GetCurrent(_mockContextManager.Object);

				Assert.That(currentEfTransaction, Is.Not.Null);
				Assert.That(currentEfTransaction.ContextManager, Is.SameAs(_mockContextManager.Object));

				currentTransaction.Dispose();
			}
		}

		[Test]
		public void GetCurrent_Enlists_EfTransaction_To_Current_Transaction()
		{
			using (ITransaction currentTransaction = Transaction.BeginTransaction())
			{
				EfTransaction<DbContext> efTransaction = EfTransaction.GetCurrent(_mockContextManager.Object);

				string key = _mockContextManager.Object.GetHashCode().ToString();
				Assert.That(Transaction.Current.DependentTransactions[key], Is.SameAs(efTransaction));

				currentTransaction.Dispose();
			}
		}

		[Test]
		public void GetCurrent_Returns_Previously_Created_EfTransaction()
		{
			using (ITransaction currentTransaction = Transaction.BeginTransaction())
			{
				EfTransaction<DbContext> previouslyCreatedEfTransaction = EfTransaction.GetCurrent(_mockContextManager.Object);

				EfTransaction<DbContext> efTransaction = EfTransaction.GetCurrent(_mockContextManager.Object);

				Assert.That(efTransaction, Is.SameAs(previouslyCreatedEfTransaction));

				currentTransaction.Dispose();
			}
		}
	}
}