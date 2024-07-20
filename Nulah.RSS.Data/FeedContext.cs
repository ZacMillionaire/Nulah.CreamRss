using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Nulah.RSS.Data.Entities;

namespace Nulah.RSS.Data;

public class FeedContext : DbContext
{
	internal DbSet<Feed> Feeds { get; set; }
	private readonly TimeProvider _timeProvider = TimeProvider.System;

	public FeedContext()
	{
	}

	public FeedContext(DbContextOptions<FeedContext> options) : base(options)
	{
	}

	/// <summary>
	/// Used for testing so times can be controlled
	/// </summary>
	/// <param name="timeProvider"></param>
	/// <param name="options"></param>
	public FeedContext(TimeProvider timeProvider, DbContextOptions<FeedContext> options) : base(options)
	{
		_timeProvider = timeProvider;
	}

	public override int SaveChanges()
	{
		SetCreatedUpdatedForSavingEntities();
		return base.SaveChanges();
	}

	public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
	{
		SetCreatedUpdatedForSavingEntities();
		return base.SaveChangesAsync(cancellationToken);
	}

	public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
	{
		SetCreatedUpdatedForSavingEntities();
		return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
	}

	private void SetCreatedUpdatedForSavingEntities()
	{
		var entries = ChangeTracker
			.Entries()
			.Where(e => e.Entity is BaseEntity
			            && e.State is EntityState.Added or EntityState.Modified
			);

		// Entities added/modified in the same batch should have identical timestamps
		var nowUtc = _timeProvider.GetUtcNow();

		foreach (var entityEntry in entries)
		{
			((BaseEntity)entityEntry.Entity).UpdatedUtc = nowUtc;

			if (entityEntry.State == EntityState.Added)
			{
				((BaseEntity)entityEntry.Entity).CreatedUtc = nowUtc;
			}
		}
	}

	/// <summary>
	/// Method called when building migrations from command line to create a database in a default location.
	/// <para>
	/// Running the application handles this on its own based on configuration
	/// </para>
	/// </summary>
	/// <param name="options"></param>
	protected override void OnConfiguring(DbContextOptionsBuilder options)
	{
		// If we're called from the cli, configure the data source to be somewhere just so we can build migrations.
		// Any proper context creation should be calling the option builder constructor with its own
		// data source location so this should always be true.
		if (!options.IsConfigured)
		{
			options.UseSqlite($"Data Source=./cli.db");
		}
	}
}