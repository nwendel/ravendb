﻿using Raven.Abstractions.Json.Linq;

namespace Raven.Database.Bundles.Replication.Impl
{
	using System.Linq;

	using Abstractions.Data;
	using Imports.Newtonsoft.Json.Linq;
	using Raven.Json.Linq;

	internal static class Historian
	{
		public static bool IsDirectChildOfCurrent(RavenJObject incomingMetadata, RavenJObject existingMetadata)
		{
			var version = new RavenJObject
			{
				{ Constants.RavenReplicationSource, existingMetadata[Constants.RavenReplicationSource] },
				{ Constants.RavenReplicationVersion, existingMetadata[Constants.RavenReplicationVersion] },
			};

			var history = incomingMetadata[Constants.RavenReplicationHistory];
			if (history == null || history.Type == JTokenType.Null) // no history, not a parent
				return false;

			if (history.Type != JTokenType.Array)
				return false;

			return history.Values().Contains(version, RavenJTokenEqualityComparer.Default);
		}
	}
}
