![Massive ECS](https://github.com/user-attachments/assets/7b608876-c28d-48db-9fcb-85e920aefa32)

`Massive` is a lightwight and easy-to-use library for game programming and more.  
Designed for use in games with deterministic prediction-rollback netcode.  
Based on sparse sets. Inspired by [EnTT](https://github.com/skypjack/entt).

Does not reference Unity Engine, so it could be used in a regular C# project.

> [!NOTE]
> Some APIs are subject to change, but overall the architecture is stable.

## Installation

Make sure you have standalone [Git](https://git-scm.com/downloads) installed first. Reboot after installation.  
In Unity, open "Window" -> "Package Manager".  
Click the "+" sign at the top left corner -> "Add package from git URL..."  
Paste this: `https://github.com/nilpunch/massive-ecs.git#v18.0.0-alpha`  
See minimum required Unity version in the `package.json` file.

## Overview

This is **a library**, not a framework. Thus, it does not try to take control of the user codebase or the main game loop.

### Entity Component System ([wiki](https://github.com/nilpunch/massive-ecs/wiki/Entity-Component-System))

- Fast and simple.
- No code generation.
- No archetypes or bitsets.
- Fully managed, no unsafe code.
- Supports components of any type.
- No allocations and minimal memory consumption.
- No deferred command execution — all changes take effect immediately.
- Generic API for in-place filtering, with a non-generic version to improve IL2CPP build time and size.
- Lightweight [views](https://github.com/nilpunch/massive-ecs/wiki/Entity-Component-System#what-is-allowed-during-iterations) for safe and flexible iteration over entities and components.
- Fully stable storage on demand:
  - Use the `IStable` marker interface for components.
  - Or enable full stability for the entire registry.
- IL2CPP friendly, tested with high stripping level on PC, Android, and WebGL.

### Rollbacks ([wiki](https://github.com/nilpunch/massive-ecs/wiki/Rollbacks))

- Fully optional and non-intrusive, integrates seamlessly with the existing ECS core.
- Minimalistic API: `SaveFrame()` and `Rollback(frames)`
- Supports components with managed data (e.g., arrays, strings, etc.).
- Performance reference (PC, CPU i7-11700KF, RAM 2666 MHz):  
  - 1000 entities, each with 150 components, can be saved 24 times in 6 ms.  
    The 150 components include 50 components of 64 bytes, 50 components of 4 bytes, and 50 tags.
  - Need more entities or reduced overhead? Adjust components or saving amount.  
    For example, 10000 entities with 15 components each can be saved 12 times in 2.3 ms.

### Addons

- Full-state serialization and deserialization ([package](https://github.com/nilpunch/massive-serialization)).
- Networking with input buffers, commands prediction, and resimulation loop ([package](https://github.com/nilpunch/massive-netcode)).
- Unity integration ([package](https://github.com/nilpunch/massive-unity-integration)).

Consider this list a work in progress as well as the project.

## Code Examples

```cs
struct Player { }
struct Position { public float X; public float Y; }
class Velocity { public float Magnitude; } // Classes work just fine
delegate void ShootingMethod(); // So are the delegates
interface IDontEvenAsk { }

class Program
{
	static void Main()
	{
		var registry = new Registry();

		// Create empty entity
		var enemy = registry.Create();

		// Or with a component
		var player = registry.Create(new Player());

		// Assign components
		registry.Assign(player, new Velocity() { Magnitude = 10f });
		registry.Assign(enemy, new Velocity());
		registry.Assign<Position>(enemy); // Assigns component without initialization

		// Get full entity identifier from player ID.
		// Handy when uniqueness is required, for example, when storing entities for later
		Entity playerEntity = registry.GetEntity(player);

		var deltaTime = 1f / 60f;

		// Iterate using lightweight views
		var view = registry.View();

		// Views will select only those entities that contain all the necessary components
		view.ForEach((int entityId, ref Position position, ref Velocity velocity) =>
		{
			position.Y += velocity.Magnitude * deltaTime;

			if (position.Y > 5f)
			{
				// Create and destroy any amount of entities during iteration
				registry.Destroy(entityId);
			}

			// NOTE:
			// After destroying any entities, cached refs to components
			// may become invalid for the current interation cycle.
			// If this behavior does not suit you, use IStable components
		});

		// Pass extra arguments to avoid boxing
		view.ForEachExtra((registry, deltaTime),
			(ref Position position, ref Velocity velocity,
				(Registry Registry, float DeltaTime) args) =>
			{
				// ...
			});

		// Filter entities right in place.
		// You don't have to cache anything
		registry.View()
			.Filter<Include<Player>, Exclude<Velocity>>()
			.ForEach((ref Position position) =>
			{
				// ...
			});

		// Iterate using foreach
		foreach (var entityId in registry.View().Include<Player, Position>())
		{
			ref var position = ref registry.Get<Position>(entityId);
			// ...
		}

		// Iterate manually over data sets
		var velocities = registry.DataSet<Velocity>();
		for (int i = 0; i < velocities.Count; ++i)
		{
			ref var velocity = ref velocities.Data[i];
			// ...
		}

		// Chain any amount of components in filters
		var filter = registry.Filter<
			Include<int, string, bool, Include<short, byte, uint, Include<ushort>>>,
			Exclude<long, char, float, Exclude<double>>>();

		// Reuse filter variable to reduce code duplication
		// in case of multiple iterations
		registry.View().Filter(filter).ForEach((ref int n, ref bool b) => { });
		registry.View().Filter(filter).ForEach((ref string str) => { });
	}
}
```
