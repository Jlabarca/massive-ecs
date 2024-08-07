using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Massive.Serialization;

namespace Massive.Samples.Serialization
{
	class SerializationSample
	{
		private static readonly string ApplicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
		private static readonly string PathToSaveFile = Path.Combine(ApplicationPath!, "save.txt");

		static void Main()
		{
			var registry = new Registry();

			PopulateEntitiesAndComponents(registry);

			var registrySerializer = new RegistrySerializer();

			// Add each unmanaged component in this way
			registrySerializer.AddComponent<Player>();
			registrySerializer.AddComponent<Enemy>();
			registrySerializer.AddComponent<Health>();

			// Add each managed component in this way.
			// By default, it uses BinaryFormatter and requires Serializable attribute on the component.
			// But it can be customized
			registrySerializer.AddCustomComponent<Inventory>();

			// Save registry to the file
			using (FileStream stream = new FileStream(PathToSaveFile, FileMode.Create, FileAccess.Write))
			{
				registrySerializer.Serialize(registry, stream);
			}

			// Load registry from the file.
			// It is important to use a serializer with the same configuration as for saving
			var savedRegistry = new Registry();
			using (FileStream stream = new FileStream(PathToSaveFile, FileMode.Open, FileAccess.Read))
			{
				registrySerializer.Deserialize(savedRegistry, stream);
			}

			// Done, use your registry as you wish
		}

		private static void PopulateEntitiesAndComponents(Registry registry)
		{
			for (int i = 0; i < 10; ++i)
			{
				var playerEntity = registry.Create<Player>();
				registry.Assign(playerEntity, new Health() { Value = 5 + i });
				registry.Assign(playerEntity, new Inventory()
				{
					Items = new List<int>()
					{
						i, 2, 3
					}
				});
			}

			for (int i = 0; i < 5; ++i)
			{
				var enemyEntity = registry.Create<Enemy>();
				registry.Assign(enemyEntity, new Health() { Value = 1 + i });
			}
		}
	}
}
