# Quarks.DataAccess.EntityFramework

[![Version](https://img.shields.io/nuget/v/Quarks.DataAccess.EntityFramework.svg)](https://www.nuget.org/packages/Quarks.DataAccess.EntityFramework)

## Example

Here is an example that describes how to use HNibernate with Quarks.Transactions.

```csharp
public class User : IEntity, IAggregate
{
	public int Id { get; set; }
	public string Name { get; set; }
}

public class UserManagementDbContext()
{
	public UserManagementDbContext(string connectionString) : base(connectionString)
	{
	}

	public DbSet<User> Users { get; set; }
}

public class UserManagementDbContextManager(string connectionString) : IEfContextManager<UserManagementDbContext>
{
	private readonly string _connectionString = connectionString;

	public UserManagementDbContext CreateContext() => new UserManagementDbContext(_connectionString);
	
	public int GetHashCode() => _connectionString.GetHashCode();
}

public class EfUserRepository : EfRepository<UserManagementDbContext>, IUserRepository
{
	public NhUserRepository(UserManagementDbContextManager manager) : base(manager) { }

	public User FindById(int id) => Context.Users.SingleOrDefault(x => x.Id == id);

	public void Modify(User user) => Context.Users.Update(user);
}

public class RenameUserCommandHandler : ICommandHandler<RenameUserCommand>
{
	private readonly IUserRepository _userRepository;

	public async Task HandleAsync(RenameUserCommand command, CancellationToken ct)
	{
		using(ITransaction transaction = Transaction.BeginTransaction())
		{
			User user = _userRepository.FindById(command.Id);
			user.Name = command.Name;
			_userRepository.Modify(user);
			await transaction.CommitAsync(ct);
		}
	}
}
```

## How it works

*EfRepository* internally uses *EfTransaction* and gets it from the current *Quarks.Transaction*.

```csharp
public abstract class EfRepository<TDbContext>(IEfContextManager<TDbContext> contextManager)
{
	private readonly IEfContextManager<TDbContext> _contextManager = contextManager;

	protected TDbContext Context => Transaction.Context;
	
	private EfTransaction Transaction => EfTransaction.GetCurrent(_contextManager);
}

internal class EfTransaction<TDbContext>(TDbContext context) : IDependentTransaction
{
	public TDbContext Context { get; } = context;
}

internal static class EfTransaction
{
	public static EfTransaction<TDbContext> GetCurrent<TDbContext>(IEfContextManager<TDbContext> contextManager)
	{
		int key = contextManager.GetHashCode().ToString();
		IDependentTransaction current;
		if (!Transaction.Current.DependentTransactions.TryGetValue(key, out current))
		{
			current = new contextManager(contextManager.CreateContext());
			Transaction.Current.Enlist(key, current);
		}

		return (EfTransaction<TDbContext>)current;
	}
}
```