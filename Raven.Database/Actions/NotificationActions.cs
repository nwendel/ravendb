﻿// -----------------------------------------------------------------------
//  <copyright file="NotificationActions.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
using System;

using Raven.Abstractions.Data;
using Raven.Abstractions.Logging;
using Raven.Database.Data;
using Raven.Database.Impl;
using Raven.Database.Util;
using Raven.Json.Linq;

namespace Raven.Database.Actions
{
    public class NotificationActions : ActionsBase
    {
        public NotificationActions(DocumentDatabase database, SizeLimitedConcurrentDictionary<string, TouchedDocumentInfo> recentTouches, IUuidGenerator uuidGenerator, ILog log)
            : base(database, recentTouches, uuidGenerator, log)
        {
        }

        public event Action<DocumentDatabase, DocumentChangeNotification, RavenJObject> OnDocumentChange;
		public event Action<DocumentDatabase, IndexChangeNotification> OnIndexChange;
	    public event Action<DocumentDatabase, TransformerChangeNotification> OnTransformerChange;

	    public void RaiseNotifications(DocumentChangeNotification obj, RavenJObject metadata)
        {
            Database.TransportState.Send(obj);
            var onDocumentChange = OnDocumentChange;
            if (onDocumentChange != null)
                onDocumentChange(Database, obj, metadata);
        }

        public void RaiseNotifications(IndexChangeNotification obj)
        {
            Database.TransportState.Send(obj);
			var onIndexChange = OnIndexChange;
			if (onIndexChange != null)
				onIndexChange(Database, obj);
		}

        public void RaiseNotifications(TransformerChangeNotification obj)
        {
            Database.TransportState.Send(obj);
			var handler = OnTransformerChange;
			if (handler != null) handler(Database, obj);
        }

        public void RaiseNotifications(ReplicationConflictNotification obj)
        {
            Database.TransportState.Send(obj);
        }

        public void RaiseNotifications(BulkInsertChangeNotification obj)
        {
            Database.TransportState.Send(obj);
        }

		public void RaiseNotifications(DataSubscriptionChangeNotification obj)
		{
			Database.TransportState.Send(obj);
		}
    }
}