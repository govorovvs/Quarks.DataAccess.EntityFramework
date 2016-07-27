using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Quarks.DataAccess.EntityFramework.ContextManagement;
using Quarks.DomainModel.Impl;

namespace Quarks.DataAccess.EntityFramework.Tests
{
	[TestFixture]
	public class EfUnitOfWorkTests
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
		public void GetCurrent_Creates_EfUnitOfWork_If_No_Current_UnitOfWork()
		{
			EfUnitOfWork<DbContext> currentEfUnitOfWork = EfUnitOfWork.GetCurrent(_mockContextManager.Object);

			Assert.That(currentEfUnitOfWork, Is.Not.Null);
			Assert.That(currentEfUnitOfWork.ContextManager, Is.SameAs(_mockContextManager.Object));
		}

		[Test]
		public void GetCurrent_Creates_EfUnitOfWork_If_Current_UnitOfWork()
		{
			using (UnitOfWork currentUnitOfWork = new UnitOfWork())
			{
				EfUnitOfWork<DbContext> currentEfUnitOfWork = EfUnitOfWork.GetCurrent(_mockContextManager.Object);

				Assert.That(currentEfUnitOfWork, Is.Not.Null);
				Assert.That(currentEfUnitOfWork.ContextManager, Is.SameAs(_mockContextManager.Object));

				currentUnitOfWork.Dispose();
			}
		}

		[Test]
		public void GetCurrent_Enlists_EfUnitOfWork_To_Current_UnitOfWork()
		{
			using (UnitOfWork currentUnitOfWork = new UnitOfWork())
			{
				EfUnitOfWork<DbContext> efUnitOfWork = EfUnitOfWork.GetCurrent(_mockContextManager.Object);

				string key = _mockContextManager.Object.GetHashCode().ToString();
				Assert.That(currentUnitOfWork.DependentUnitOfWorks[key], Is.SameAs(efUnitOfWork));

				currentUnitOfWork.Dispose();
			}
		}

		[Test]
		public void GetCurrent_Returns_Previously_Created_EfUnitOfWork()
		{
			using (UnitOfWork currentUnitOfWork = new UnitOfWork())
			{
				EfUnitOfWork<DbContext> previouslyCreatedEfUnitOfWork = EfUnitOfWork.GetCurrent(_mockContextManager.Object);

				EfUnitOfWork<DbContext> efUnitOfWork = EfUnitOfWork.GetCurrent(_mockContextManager.Object);

				Assert.That(efUnitOfWork, Is.SameAs(previouslyCreatedEfUnitOfWork));

				currentUnitOfWork.Dispose();
			}
		}
	}
}