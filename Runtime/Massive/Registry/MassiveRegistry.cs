using System;
using System.Runtime.CompilerServices;

namespace Massive.ECS
{
	public class MassiveRegistry : Registry, IMassive
	{
		private readonly MassiveIdentifiers _massiveEntities;

		public MassiveRegistry(int framesCapacity = Constants.FramesCapacity, int dataCapacity = Constants.DataCapacity)
			: base(new MassiveSetFactory(framesCapacity, dataCapacity))
		{
			// Fetch instance from base
			_massiveEntities = (MassiveIdentifiers)Entities;
		}

		public int CanRollbackFrames => _massiveEntities.CanRollbackFrames;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void SaveFrame()
		{
			_massiveEntities.SaveFrame();

			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			foreach (IMassive massive in AllSets)
			{
				massive.SaveFrame();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Rollback(int frames)
		{
			_massiveEntities.Rollback(frames);

			// ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
			foreach (IMassive massive in AllSets)
			{
				massive.Rollback(Math.Min(frames, massive.CanRollbackFrames));
			}
		}
	}
}