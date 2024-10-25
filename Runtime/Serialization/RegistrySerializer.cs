using System;
using System.Collections.Generic;
using System.IO;

namespace Massive.Serialization
{
	public class RegistrySerializer : IRegistrySerializer
	{
		private readonly Dictionary<Type, IDataSerializer> _customSerializers = new Dictionary<Type, IDataSerializer>();

		public void AddCustomSerializer(Type type, IDataSerializer dataSerializer)
		{
			_customSerializers[type] = dataSerializer;
		}

		public void Serialize(Registry registry, Stream stream)
		{
			// Entities
			SerializationHelpers.WriteEntities(registry.Entities, stream);

			// Sets
			SerializationHelpers.WriteInt(registry.SetRegistry.All.Length, stream);
			foreach (var sparseSet in registry.SetRegistry.All)
			{
				var setType = registry.SetRegistry.TypeOf(sparseSet);

				SerializationHelpers.WriteType(setType, stream);
				SerializationHelpers.WriteSparseSet(sparseSet, stream);

				if (sparseSet is not IDataSet dataSet)
				{
					continue;
				}

				if (_customSerializers.TryGetValue(setType, out var customSerializer))
				{
					customSerializer.Write(dataSet.Data, sparseSet.Count, stream);
					continue;
				}

				if (dataSet.Data.ElementType.IsUnmanaged())
				{
					SerializationHelpers.WriteUnmanagedPagedArray(dataSet.Data, sparseSet.Count, stream);
				}
				else
				{
					SerializationHelpers.WriteManagedPagedArray(dataSet.Data, sparseSet.Count, stream);
				}
			}

			// Groups
			List<IGroup> syncedGroups = new List<IGroup>();
			foreach (var group in registry.GroupRegistry.All)
			{
				if (group.IsSynced)
				{
					syncedGroups.Add(group);
				}
			}
			SerializationHelpers.WriteInt(syncedGroups.Count, stream);
			foreach (var group in syncedGroups)
			{
				var (includeSelector, excludeSelector, ownSelector) = registry.GroupRegistry.GetSelectorsOfGroup(group);
				SerializationHelpers.WriteType(includeSelector, stream);
				SerializationHelpers.WriteType(excludeSelector, stream);
				SerializationHelpers.WriteType(ownSelector, stream);

				if (group is not IOwningGroup)
				{
					SerializationHelpers.WriteSparseSet(group.MainSet, stream);
				}
			}
		}

		public void Deserialize(Registry registry, Stream stream)
		{
			// Entities
			SerializationHelpers.ReadEntities(registry.Entities, stream);

			// Sets
			var setCount = SerializationHelpers.ReadInt(stream);
			for (var i = 0; i < setCount; i++)
			{
				var setType = SerializationHelpers.ReadType(stream);

				var sparseSet = registry.SetRegistry.Get(setType);

				SerializationHelpers.ReadSparseSet(sparseSet, stream);

				if (sparseSet is not IDataSet dataSet)
				{
					continue;
				}

				if (_customSerializers.TryGetValue(setType, out var customSerializer))
				{
					customSerializer.Read(dataSet.Data, sparseSet.Count, stream);
					continue;
				}

				if (dataSet.Data.ElementType.IsUnmanaged())
				{
					SerializationHelpers.ReadUnmanagedPagedArray(dataSet.Data, sparseSet.Count, stream);
				}
				else
				{
					SerializationHelpers.ReadManagedPagedArray(dataSet.Data, sparseSet.Count, stream);
				}
			}

			// Groups
			int groupCount = SerializationHelpers.ReadInt(stream);
			for (int i = 0; i < groupCount; i++)
			{
				var includeSelector = SerializationHelpers.ReadType(stream);
				var excludeSelector = SerializationHelpers.ReadType(stream);
				var ownSelector = SerializationHelpers.ReadType(stream);

				var group = registry.GroupRegistry.Get(includeSelector, excludeSelector, ownSelector);

				if (group is not IOwningGroup)
				{
					SerializationHelpers.ReadSparseSet(group.MainSet, stream);
				}
			}
		}
	}
}
