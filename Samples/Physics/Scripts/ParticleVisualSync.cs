﻿using MassiveData.Samples.Shooter;
using UnityEngine;

namespace MassiveData.Samples.Physics
{
	public class ParticleVisualSync : VisualSync<SphereCollider>
	{
		public override void SyncState(ref SphereCollider state)
		{
			base.SyncState(ref state);

			transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		}

		protected override void TransformFromState(in SphereCollider state, out EntityTransform transform)
		{
			transform = new EntityTransform() { Position = state.WorldPosition, Rotation = Quaternion.identity };
		}
	}
}