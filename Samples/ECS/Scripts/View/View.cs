﻿using System.Runtime.CompilerServices;

namespace Massive.Samples.ECS
{
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.NullChecks, false)]
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.ArrayBoundsChecks, false)]
	[Unity.IL2CPP.CompilerServices.Il2CppSetOption(Unity.IL2CPP.CompilerServices.Option.DivideByZeroChecks, false)]
	public class View
	{
		private readonly MassiveSparseSet _tags;
		private readonly Filter _filter;

		public View(MassiveSparseSet tags, Filter filter = null)
		{
			_tags = tags;
			_filter = filter;
		}

		public void ForEach(EntityAction action)
		{
			if (_filter is null)
			{
				ForEachRaw(action);
			}
			else
			{
				ForEachFiltered(action);
			}
		}

		private void ForEachRaw(EntityAction action)
		{
			var ids = _tags.AliveIds;
			for (int dense = ids.Length - 1; dense >= 0; dense--)
			{
				action.Invoke(ids[dense]);
			}
		}

		private void ForEachFiltered(EntityAction action)
		{
			var ids = _tags.AliveIds;
			for (int dense = ids.Length - 1; dense >= 0; dense--)
			{
				int id = ids[dense];
				if (_filter.IsOkay(id))
				{
					action.Invoke(id);
				}
			}
		}
	}
}