﻿using System.Diagnostics;
using Massive.ECS;
using UnityEngine;

namespace Massive.Samples.Shooter
{
	public class ShooterSimulation : MonoBehaviour
	{
		[SerializeField] private int _simulationsPerFrame = 120;
		[SerializeField] private int _entitiesCapacity = 3000;
		[SerializeField] private int _charactersAmount = 10;

		[Header("Entities")] [SerializeField] private EntityRoot<CharacterState> _characterPrefab;
		[SerializeField] private EntityRoot<BulletState> _bulletPrefab;

		private Registry _registry;
		private WorldUpdater[] _worldUpdaters;

		private EntitySynchronisation<CharacterState> _characterSynchronisation;
		private EntitySynchronisation<BulletState> _bulletSynchronisation;

		private void Awake()
		{
			_registry = new Registry(_simulationsPerFrame + 1, _entitiesCapacity);

			_worldUpdaters = FindObjectsOfType<WorldUpdater>();
			foreach (var worldUpdaters in _worldUpdaters)
			{
				worldUpdaters.Init(_registry);
			}

			_characterSynchronisation = new EntitySynchronisation<CharacterState>(new EntityFactory<CharacterState>(_characterPrefab));
			_bulletSynchronisation = new EntitySynchronisation<BulletState>(new EntityFactory<BulletState>(_bulletPrefab));

			for (int i = 0; i < _charactersAmount; i++)
			{
				int character = _registry.CreateEntity(new CharacterState()
				{
					Transform = new EntityTransform()
					{
						Position = Vector3.right * (i - _charactersAmount / 2f) * 1.5f,
						Rotation = Quaternion.AngleAxis(180f * (i - _charactersAmount / 2f) / _charactersAmount, Vector3.forward)
					}
				});
				
				_registry.Add(character, new WeaponState());
			}
		}

		private int _currentFrame;

		private float _elapsedTime;

		private void Update()
		{
			Stopwatch stopwatch = Stopwatch.StartNew();

			if (_registry.CanRollbackFrames >= 0)
			{
				_currentFrame -= _registry.CanRollbackFrames;
				_registry.Rollback(_registry.CanRollbackFrames);
			}

			_elapsedTime += Time.deltaTime;

			float deltaTime = 1f / 60f;
			
			int targetFrame = Mathf.RoundToInt(_elapsedTime * 60);

			while (_currentFrame < targetFrame)
			{
				foreach (var worldUpdater in _worldUpdaters)
				{
					worldUpdater.UpdateWorld(deltaTime);
				}

				_registry.SaveFrame();
				_currentFrame++;
			}

			_characterSynchronisation.Synchronize(_registry.Components<CharacterState>());
			_bulletSynchronisation.Synchronize(_registry.Components<BulletState>());

			_debugTime = stopwatch.ElapsedMilliseconds;
		}

		private long _debugTime;

		private void OnGUI()
		{
			GUILayout.TextField($"{_debugTime}ms Simulation", new GUIStyle() { fontSize = 70, normal = new GUIStyleState() { textColor = Color.white } });
			GUILayout.TextField($"{_registry.CanRollbackFrames} Resimulations", new GUIStyle() { fontSize = 50, normal = new GUIStyleState() { textColor = Color.white } });
			GUILayout.TextField($"{_registry.Components<CharacterState>().AliveCount} Characters", new GUIStyle() { fontSize = 50, normal = new GUIStyleState() { textColor = Color.white } });
			GUILayout.TextField($"{_registry.Components<BulletState>().AliveCount} Bullets", new GUIStyle() { fontSize = 50, normal = new GUIStyleState() { textColor = Color.white } });
		}
	}
}