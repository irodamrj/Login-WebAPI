using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LoginProject.Models;

public partial class UserContext : DbContext
{
	public UserContext()
	{
	}

	public UserContext(DbContextOptions<UserContext> options)
		: base(options)
	{
	}

	public virtual DbSet<User> Users { get; set; }


	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(e => e.Email);

			entity.ToTable("User");
		});

		OnModelCreatingPartial(modelBuilder);
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
