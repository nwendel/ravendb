﻿using System.Threading.Tasks;
using Raven.Client.Connection;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Xunit;

namespace Raven.Tests.Bundles.Replication.Async
{
	public class FailoverBetweenTwoMultiTenantDatabases : ReplicationBase
	{
		[Fact]
		public async Task CanReplicateBetweenTwoMultiTenantDatabases()
		{
			var store1 = CreateStore();
			var store2 = CreateStore();

			store1.DatabaseCommands.Admin.EnsureDatabaseExists("FailoverTest");
			store2.DatabaseCommands.Admin.EnsureDatabaseExists("FailoverTest");

			SetupReplication(store1.DatabaseCommands.ForDatabase("FailoverTest"),
			                 store2.Url + "/databases/FailoverTest");

			using (var store = new DocumentStore
			                   	{
			                   		DefaultDatabase = "FailoverTest",
			                   		Url = store1.Url,
			                   		Conventions =
			                   			{
			                   				FailoverBehavior = FailoverBehavior.AllowReadsFromSecondariesAndWritesToSecondaries
			                   			}
			                   	})
			{
				store.Initialize();
				var replicationInformerForDatabase = store.GetReplicationInformerForDatabase(null);
				var databaseCommands = (ServerClient) store.DatabaseCommands;
				await replicationInformerForDatabase.UpdateReplicationInformationIfNeeded(databaseCommands);

				var replicationDestinations = replicationInformerForDatabase.ReplicationDestinationsUrls;
				
				Assert.NotEmpty(replicationDestinations);

				using (var session = store.OpenAsyncSession())
				{
					await session.StoreAsync(new Item {});
					await session.SaveChangesAsync();
				}

				var sanityCheck = store.DatabaseCommands.Head("items/1");
				Assert.NotNull(sanityCheck);

				WaitForDocument(store2.DatabaseCommands.ForDatabase("FailoverTest"), "items/1");
			}
		}

		[Fact]
		public async Task CanFailoverReplicationBetweenTwoMultiTenantDatabases()
		{
			var store1 = CreateStore();
			var store2 = CreateStore();

			store1.DatabaseCommands.Admin.EnsureDatabaseExists("FailoverTest");
			store2.DatabaseCommands.Admin.EnsureDatabaseExists("FailoverTest");

			SetupReplication(store1.DatabaseCommands.ForDatabase("FailoverTest"),
			                 store2.Url + "/databases/FailoverTest");

			using (var store = new DocumentStore
			                   	{
			                   		DefaultDatabase = "FailoverTest",
			                   		Url = store1.Url,
			                   		Conventions =
			                   			{
			                   				FailoverBehavior = FailoverBehavior.AllowReadsFromSecondariesAndWritesToSecondaries
			                   			}
			                   	})
			{
				store.Initialize();
				var replicationInformerForDatabase = store.GetReplicationInformerForDatabase(null);
				await replicationInformerForDatabase.UpdateReplicationInformationIfNeeded((ServerClient) store.DatabaseCommands);

				using (var session = store.OpenAsyncSession())
				{
					await session.StoreAsync(new Item {});
					await session.SaveChangesAsync();
				}


				WaitForDocument(store2.DatabaseCommands.ForDatabase("FailoverTest"), "items/1");

				servers[0].Dispose();

				using (var session = store.OpenAsyncSession())
				{
					var load = await session.LoadAsync<Item>("items/1");
					Assert.NotNull(load);
				}
			}
		}

		[Fact]
		public async Task CanFailoverReplicationBetweenTwoMultiTenantDatabases_WithExplicitUrl()
		{
			var store1 = CreateStore();
			var store2 = CreateStore();

			store1.DatabaseCommands.Admin.EnsureDatabaseExists("FailoverTest");
			store2.DatabaseCommands.Admin.EnsureDatabaseExists("FailoverTest");

			SetupReplication(store1.DatabaseCommands.ForDatabase("FailoverTest"),
			                 store2.Url + "/databases/FailoverTest");

			using (var store = new DocumentStore
								{
									DefaultDatabase = "FailoverTest",
									Url = store1.Url + "/databases/FailoverTest",
									Conventions =
										{
											FailoverBehavior = FailoverBehavior.AllowReadsFromSecondariesAndWritesToSecondaries
										}
								})
			{
				store.Initialize();
				var replicationInformerForDatabase = store.GetReplicationInformerForDatabase("FailoverTest");
				await replicationInformerForDatabase.UpdateReplicationInformationIfNeeded((ServerClient) store.DatabaseCommands);

				Assert.NotEmpty(replicationInformerForDatabase.ReplicationDestinations);

				using (var session = store.OpenAsyncSession())
				{
					await session.StoreAsync(new Item { });
					await session.SaveChangesAsync();
				}

				WaitForDocument(store2.DatabaseCommands.ForDatabase("FailoverTest"), "items/1");

				servers[0].Dispose();

				using (var session = store.OpenAsyncSession())
				{
					var load = await session.LoadAsync<Item>("items/1");
					Assert.NotNull(load);
				}
			}
		}

		public class Item
		{
		}
	}
}