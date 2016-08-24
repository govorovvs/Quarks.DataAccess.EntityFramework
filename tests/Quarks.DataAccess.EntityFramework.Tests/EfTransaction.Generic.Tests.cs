using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Quarks.DataAccess.EntityFramework.ContextManagement;
using Quarks.Transactions;

namespace Quarks.DataAccess.EntityFramework.Tests
{
	[TestFixture]
	public class EfTransactionGenericTests
	{
		private Mock<DbContext> _mockDbContext;
		private Mock<IEfContextManager<DbContext>> _mockContextManager;
		private CancellationToken _cancellationToken;

		[SetUp]
		public void SetUp()
		{
			_cancellationToken = new CancellationTokenSource().Token;
			_mockDbContext = new Mock<DbContext>();
			_mockContextManager = new Mock<IEfContextManager<DbContext>>();

			_mockContextManager
				.Setup(x => x.CreateContext())
				.Returns(_mockDbContext.Object);
		}

		[Test]
		public void Can_Be_Constructed_With_ContextManager()
		{
			var transaction = new EfTransaction<DbContext>(_mockContextManager.Object);

			Assert.That(transaction.ContextManager, Is.EqualTo(_mockContextManager.Object));
		}

		[Test]
		public void Is_Instance_Of_IDependentTransaction()
		{
			var transaction =  CreateTransaction();

			Assert.That(transaction, Is.InstanceOf<IDependentTransaction>());
		}

		[Test]
		public void Context_Test()
		{
			var transaction = CreateTransaction();

			Assert.That(transaction.Context, Is.SameAs(_mockDbContext.Object));
		}

		[Test]
		public void Dispose_Disposes_Context()
		{
			_mockDbContext.Setup(x => x.Dispose());
			var transaction = CreateTransaction();

			transaction.Dispose();

			_mockDbContext.VerifyAll();
		}

		[Test]
		public void Dispose_Throws_An_Exception_If_It_Was_Previously_Disposed()
		{
			var transaction = CreateTransaction();

			transaction.Dispose();

			Assert.Throws<ObjectDisposedException>(() => transaction.Dispose());
		}

		[Test]
		public async Task Commit_Saves_Context()
		{
			_mockDbContext
				.Setup(x => x.SaveChangesAsync(_cancellationToken))
				.ReturnsAsync(10);
			var transaction = CreateTransaction();

			await transaction.CommitAsync(_cancellationToken);

			_mockDbContext.VerifyAll();
		}

		[Test]
		public void Commit_Throws_An_Exception_If_It_Was_Previously_Disposed()
		{
			var transaction = CreateTransaction();

			transaction.Dispose();

			Assert.ThrowsAsync<ObjectDisposedException>(() => transaction.CommitAsync(_cancellationToken));
		}

		private EfTransaction<DbContext> CreateTransaction()
		{
			var transaction = new EfTransaction<DbContext>(_mockContextManager.Object);

			Assert.That(transaction.Context, Is.Not.Null);

			return transaction;
		}
	}
}