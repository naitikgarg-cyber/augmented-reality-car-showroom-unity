using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Unity.XR.CoreUtils.Tests
{
    [TestFixture]
    class HashSetListTests
    {
        class EqualityComparerHook<T> : IEqualityComparer<T> where T : class
        {
            public bool equalsInvoked { get; private set; }

            public bool getHashCodeInvoked { get; private set; }

            public bool Equals(T x, T y)
            {
                equalsInvoked = true;
                return EqualityComparer<T>.Default.Equals(x, y);
            }

            public int GetHashCode(T obj)
            {
                getHashCodeInvoked = true;
                return EqualityComparer<T>.Default.GetHashCode(obj);
            }
        }

        [Test]
        [Description("Invoke all of the constructors to ensure each of them can compile with its optional parameters and constructor chaining code.")]
        public void TestConstructors()
        {
            Assert.That(new HashSetList<GameObject>(), Is.Not.Null);
            Assert.That(new HashSetList<GameObject>(null), Is.Not.Null);
            Assert.That(new HashSetList<GameObject>(new EqualityComparerHook<GameObject>()), Is.Not.Null);
            Assert.That(new HashSetList<GameObject>(4), Is.Not.Null);
            Assert.That(new HashSetList<GameObject>(4, null), Is.Not.Null);
            Assert.That(new HashSetList<GameObject>(4, new EqualityComparerHook<GameObject>()), Is.Not.Null);
        }

        [Test]
        [Description("Pass a custom equality comparer to ensure it is used in the underlying HashSet field.")]
        public void TestCustomComparer()
        {
            var comparer = new EqualityComparerHook<GameObject>();

            Assert.That(comparer.equalsInvoked, Is.False);
            Assert.That(comparer.getHashCodeInvoked, Is.False);

            var hashSetList = new HashSetList<GameObject>(comparer);
            var gameObjectA = new GameObject("A");
            var gameObjectB = new GameObject("B");

            Assert.That(hashSetList.Add(gameObjectA), Is.True);
            Assert.That(hashSetList.Contains(gameObjectA), Is.True);
            Assert.That(hashSetList.Contains(gameObjectB), Is.False);

            Assert.That(comparer.equalsInvoked, Is.True);
            Assert.That(comparer.getHashCodeInvoked, Is.True);

            Object.Destroy(gameObjectA);
            Object.Destroy(gameObjectB);
        }

        [Test]
        [Description("Count returns expected value when using Add/Remove/Clear.")]
        public void TestCount()
        {
            // Starts empty
            var hashSetList = new HashSetList<int>();
            Assert.That(hashSetList.Count, Is.EqualTo(0));

            // Adding increments Count
            Assert.That(hashSetList.Add(10), Is.True);
            Assert.That(hashSetList.Count, Is.EqualTo(1));

            Assert.That(hashSetList.Add(15), Is.True);
            Assert.That(hashSetList.Count, Is.EqualTo(2));

            // Adding duplicate does not increment Count
            Assert.That(hashSetList.Add(15), Is.False);
            Assert.That(hashSetList.Count, Is.EqualTo(2));

            // Removing decrements Count
            Assert.That(hashSetList.Remove(10), Is.True);
            Assert.That(hashSetList.Count, Is.EqualTo(1));

            // Clearing empties the collection
            hashSetList.Clear();
            Assert.That(hashSetList.Count, Is.EqualTo(0));
        }

        [Test]
        [Description("Use a collection initializer and verify internal consistency.")]
        public void TestCollectionInitializer()
        {
            var hashSetList = new HashSetList<int> { 10, 20, 30 };

            Assert.That(hashSetList.Count, Is.EqualTo(3));
            Assert.That(hashSetList.AsList(), Is.EqualTo(new[] { 10, 20, 30 }));
            Assert.That(hashSetList.Contains(10), Is.True);
            Assert.That(hashSetList.Contains(20), Is.True);
            Assert.That(hashSetList.Contains(30), Is.True);
        }
    }
}
