﻿using System;
using NUnit.Framework;

namespace Massive.Tests
{
	[TestFixture]
	public class GroupTest
	{
		public enum TestGroupType
		{
			FullOwningGroup,
			PartialOwningGroup,
			NonOwningGroup,
		}

		[TestCase(TestGroupType.FullOwningGroup)]
		[TestCase(TestGroupType.PartialOwningGroup)]
		[TestCase(TestGroupType.NonOwningGroup)]
		public void Group_Functionality(TestGroupType testGroupType)
		{
			var registry = new Registry();

			var group = SetUpGroupWithIntAndChar(registry, testGroupType);

			Assert.AreEqual(0, group.Length);

			var entity1 = registry.Create('1');
			var entity2 = registry.Create('2');

			registry.Add(entity2, 42);

			Assert.AreEqual(1, group.Length);

			registry.Add<int>(entity1);

			Assert.AreEqual(2, group.Length);

			registry.Remove<int>(entity1);

			Assert.AreEqual(1, group.Length);

			foreach (var id in group.Ids)
			{
				Assert.AreEqual(registry.Get<int>(id), 42);
				Assert.AreEqual(registry.Get<char>(id), '2');
			}

			Assert.AreEqual(group.Ids[0], entity2);

			registry.Remove<char>(entity1);
			registry.Remove<char>(entity2);

			Assert.AreEqual(0, group.Length);
		}

		[TestCase(TestGroupType.FullOwningGroup)]
		[TestCase(TestGroupType.NonOwningGroup)]
		public void Group_Exclude(TestGroupType testGroupType)
		{
			var registry = new Registry();

			var entity1 = registry.Create(1);

			var entity2 = registry.Create(2);
			registry.Add<char>(entity2);

			var group = SetUpGroupWithIntAndWithoutChar(registry, testGroupType);

			var entity3 = registry.Create(3);

			var entity4 = registry.Create(4);
			registry.Add<char>(entity4);

			foreach (var entity in group.Ids)
			{
				Assert.True(entity == entity1 || entity == entity3);

				if (entity == entity1)
				{
					Assert.AreEqual(1, registry.Get<int>(entity1));
				}
				else if (entity == entity3)
				{
					Assert.AreEqual(3, registry.Get<int>(entity3));
				}
			}

			registry.Add<char>(entity1);
			registry.Add<char>(entity3);

			Assert.AreEqual(0, group.Length);

			registry.Remove<char>(entity2);
			registry.Remove<char>(entity4);

			foreach (var entity in group.Ids)
			{
				Assert.True(entity == entity2 || entity == entity4);

				if (entity == entity2)
				{
					Assert.AreEqual(2, registry.Get<int>(entity2));
				}
				else if (entity == entity4)
				{
					Assert.AreEqual(4, registry.Get<int>(entity4));
				}
			}
		}

		[TestCase(TestGroupType.FullOwningGroup)]
		[TestCase(TestGroupType.NonOwningGroup)]
		public void Group_StableLateInitialization(TestGroupType testGroupType)
		{
			var registry = new Registry();

			for (int i = 0; i < 30; i++)
			{
				var entity = registry.Create();

				if (i % 2 != 0)
				{
					registry.Add<int>(entity);
				}

				if (i % 3 != 0)
				{
					registry.Add<char>(entity);
				}
			}

			var group = SetUpGroupWithIntAndWithoutChar(registry, testGroupType);

			Assert.AreEqual(5, group.Length);
		}

		[Test]
		public void Group_ShouldAllowMultilayerNesting()
		{
			var registry = new Registry();

			var entity1 = registry.Create(1);
			registry.Add<float>(entity1);

			var entity2 = registry.Create(2);
			registry.Add<char>(entity2);
			registry.Add<float>(entity2);

			var owningGroup3 = registry.Groups.EnsureGroup(new[] { registry.Any<int>(), registry.Any<char>() },
				new[] { registry.Any<float>(), registry.Any<double>() });
			var owningGroup = registry.Groups.EnsureGroup(new[] { registry.Any<int>() }, new[] { registry.Any<float>() });
			var owningGroup2 = registry.Groups.EnsureGroup(new[] { registry.Any<int>(), registry.Any<char>() }, new[] { registry.Any<float>() });

			var entity3 = registry.Create(3);
			registry.Add<float>(entity3);

			var entity4 = registry.Create(4);
			registry.Add<char>(entity4);
			registry.Add<float>(entity4);

			Assert.AreEqual(4, owningGroup.Length);
			Assert.AreEqual(2, owningGroup2.Length);
			Assert.AreEqual(0, owningGroup3.Length);

			registry.Add<char>(entity1);
			registry.Add<char>(entity3);

			Assert.AreEqual(4, owningGroup.Length);
			Assert.AreEqual(4, owningGroup2.Length);
			Assert.AreEqual(0, owningGroup3.Length);

			registry.Add<double>(entity1);
			registry.Add<double>(entity3);

			Assert.AreEqual(4, owningGroup.Length);
			Assert.AreEqual(4, owningGroup2.Length);
			Assert.AreEqual(2, owningGroup3.Length);

			registry.Remove<char>(entity1);
			registry.Remove<char>(entity3);

			Assert.AreEqual(4, owningGroup.Length);
			Assert.AreEqual(2, owningGroup2.Length);
			Assert.AreEqual(0, owningGroup3.Length);

			registry.Remove<float>(entity2);
			registry.Remove<float>(entity4);

			Assert.AreEqual(2, owningGroup.Length);
			Assert.AreEqual(0, owningGroup2.Length);
			Assert.AreEqual(0, owningGroup3.Length);
		}

		private IGroup SetUpGroupWithIntAndChar(IRegistry registry, TestGroupType testGroupType)
		{
			switch (testGroupType)
			{
				case TestGroupType.FullOwningGroup:
					return registry.Groups.EnsureGroup(owned: new ISet[]
					{
						registry.Components<int>(),
						registry.Components<char>()
					});
				case TestGroupType.PartialOwningGroup:
					return registry.Groups.EnsureGroup(
						owned: new ISet[]
						{
							registry.Components<int>()
						},
						include: new IReadOnlySet[]
						{
							registry.Components<char>()
						}
					);
				case TestGroupType.NonOwningGroup:
					return registry.Groups.EnsureGroup(include: new IReadOnlySet[]
					{
						registry.Components<int>(),
						registry.Components<char>()
					});
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private IGroup SetUpGroupWithIntAndWithoutChar(IRegistry registry, TestGroupType testGroupType)
		{
			switch (testGroupType)
			{
				case TestGroupType.FullOwningGroup:
				case TestGroupType.PartialOwningGroup:
					return registry.Groups.EnsureGroup(
						owned: new ISet[]
						{
							registry.Components<int>(),
						},
						exclude: new IReadOnlySet[]
						{
							registry.Components<char>()
						});
				case TestGroupType.NonOwningGroup:
					return registry.Groups.EnsureGroup(
						include: new IReadOnlySet[]
						{
							registry.Components<int>(),
						},
						exclude: new IReadOnlySet[]
						{
							registry.Components<char>()
						});
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}