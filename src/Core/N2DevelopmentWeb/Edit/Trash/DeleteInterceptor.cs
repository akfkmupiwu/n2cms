using System;
using System.Collections.Generic;
using System.Text;
using Castle.Core;
using N2.Persistence;
using N2.Plugin;

namespace N2.Edit.Trash
{
	/// <summary>
	/// Intercepts delete operations.
	/// </summary>
	public class DeleteInterceptor : IStartable, IAutoStart
	{
		private readonly IPersister persister;
		private readonly ITrashHandler trashHandler;

		public DeleteInterceptor(IPersister persister, ITrashHandler trashHandler)
		{
			this.persister = persister;
			this.trashHandler = trashHandler;
		}

		public void Start()
		{
			persister.ItemDeleting += ItemDeletingEventHandler;
			persister.ItemMoving += ItemMovedEventHandler;
			persister.ItemCopied += ItemCopiedEventHandler;
		}

		public void Stop()
		{
			persister.ItemDeleting -= ItemDeletingEventHandler;
			persister.ItemMoving -= ItemMovedEventHandler;
			persister.ItemCopied -= ItemCopiedEventHandler;
		}

		private void ItemCopiedEventHandler(object sender, DestinationEventArgs e)
		{
			if(LeavingTrash(e))
			{
				trashHandler.RestoreValues(e.AffectedItem);
			}
			else if (trashHandler.IsInTrash(e.Destination))
			{
				trashHandler.ExpireTrashedItem(e.AffectedItem);
			}
		}

		private void ItemMovedEventHandler(object sender, CancellableDestinationEventArgs e)
		{
			if (LeavingTrash(e))
			{
				trashHandler.RestoreValues(e.AffectedItem);
			}
			else if (trashHandler.IsInTrash(e.Destination))
			{
				trashHandler.ExpireTrashedItem(e.AffectedItem);
			}
		}

		private void ItemDeletingEventHandler(object sender, CancellableItemEventArgs e)
		{
			if (trashHandler.CanThrow(e.AffectedItem))
			{
				e.Cancel = true;
				trashHandler.Throw(e.AffectedItem);
			}
		}

		private bool LeavingTrash(DestinationEventArgs e)
		{
			return e.AffectedItem["DeletedDate"] != null && !trashHandler.IsInTrash(e.Destination);
		}
	}
}
