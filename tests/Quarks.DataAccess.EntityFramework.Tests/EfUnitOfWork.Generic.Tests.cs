using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Quarks.DataAccess.EntityFramework.ContextManagement;
using Quarks.DomainModel.Impl;

namespace Quarks.DataAccess.EntityFramework.Tests
{
	[TestFixture]
	public class EfUnitOfWorkGenericTests
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
			var unitOfWork = new EfUnitOfWork<DbContext>(_mockContextManager.Object);

			Assert.That(unitOfWork.ContextManager, Is.EqualTo(_mockContextManager.Object));
		}

		[Test]
		public void Is_Instance_Of_IDependentUnitOfWork()
		{
			var unitOfWork =  CreateUnitOfWork();

			Assert.That(unitOfWork, Is.InstanceOf<IDependentUnitOfWork>());
		}

		[Test]
		public void Context_Test()
		{
			var unitOfWork = CreateUnitOfWork();

			Assert.That(unitOfWork.Context, Is.SameAs(_mockDbContext.Object));
		}

		[Test]
		public void Dispose_Disposes_Context()
		{
			_mockDbContext.Setup(x => x.Dispose());
			var unitOfWork = CreateUnitOfWork();

			unitOfWork.Dispose();

			_mockDbContext.VerifyAll();
		}

		[Test]
		public void Dispose_Throws_An_Exception_If_It_Was_Previously_Disposed()
		{
			var unitOfWork = CreateUnitOfWork();

			unitOfWork.Dispose();

			Assert.Throws<ObjectDisposedException>(() => unitOfWork.Dispose());
		}

		[Test]
		public async Task Commit_Saves_Context()
		{
			_mockDbContext
				.Setup(x => x.SaveChangesAsync(_cancellationToken))
				.ReturnsAsync(10);
			var unitOfWork = CreateUnitOfWork();

			await unitOfWork.CommitAsync(_cancellationToken);

			_mockDbContext.VerifyAll();
		}

		[Test]
		public void Commit_Throws_An_Exception_If_It_Was_Previously_Disposed()
		{
			var unitOfWork = CreateUnitOfWork();

			unitOfWork.Dispose();

			Assert.ThrowsAsync<ObjectDisposedException>(() => unitOfWork.CommitAsync(_cancellationToken));
		}

		private EfUnitOfWork<DbContext> CreateUnitOfWork()
		{
			var unitOfWork = new EfUnitOfWork<DbContext>(_mockContextManager.Object);

			Assert.That(unitOfWork.Context, Is.Not.Null);

			return unitOfWork;
		}
	}
}