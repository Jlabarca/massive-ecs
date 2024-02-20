﻿using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Massive.Samples.ECS
{
	/// <summary>
	/// Cross-platform component information.
	/// </summary>
	public static class ComponentMeta<T> where T : struct
	{
		public static int Id { get; }
		public static int SizeInBytes { get; }
		public static bool HasAnyFields { get; }

		static ComponentMeta()
		{
			var type = typeof(T);
			var typeFullName = type.FullName;
			var id = typeFullName.GetHashCode();

			if (ComponentIds.UsedIds.Contains(id))
			{
				Debug.LogError($"Id collision happened with type: {typeFullName}");
				Application.Quit();
				return;
			}

			ComponentIds.UsedIds.Add(id);
			
			Id = id;
			SizeInBytes = Marshal.SizeOf<T>();
			HasAnyFields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length > 0;
		}
	}
}