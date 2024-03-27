using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.IL2CPP.CompilerServices;

namespace Massive
{
	[Il2CppSetOption(Option.NullChecks, false)]
	[Il2CppSetOption(Option.ArrayBoundsChecks, false)]
	public static class SetUtils
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool AliveInAll(int id, IReadOnlyList<IReadOnlySet> sets)
		{
			foreach (var set in sets)
			{
				if (!set.IsAlive(id))
				{
					return false;
				}
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool NotAliveInAll(int id, IReadOnlyList<IReadOnlySet> sets)
		{
			foreach (var set in sets)
			{
				if (set.IsAlive(id))
				{
					return false;
				}
			}

			return true;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static IReadOnlySet GetMinimalSet(IReadOnlyList<IReadOnlySet> sets)
		{
			IReadOnlySet minimal = sets[0];

			for (int i = 1; i < sets.Count; i++)
			{
				if (minimal.AliveCount > sets[i].AliveCount)
				{
					minimal = sets[i];
				}
			}

			return minimal;
		}
	}
}