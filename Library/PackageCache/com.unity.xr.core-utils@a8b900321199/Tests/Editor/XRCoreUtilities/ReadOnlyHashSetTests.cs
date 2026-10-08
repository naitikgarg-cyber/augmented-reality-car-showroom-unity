using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;

namespace Unity.XR.CoreUtils.Editor.Tests
{
    class ReadOnlyHashSetTests
    {
        [Test]
        public void Constructor_ThrowsOnNull()
        {
            Assert.Throws<ArgumentNullException>(() => new ReadOnlyHashSet<int>(null));
        }

        [Test]
        public void Empty_ReturnsSameInstance()
        {
            var empty1 = ReadOnlyHashSet<int>.Empty();
            var empty2 = ReadOnlyHashSet<int>.Empty();
            Assert.AreSame(empty1, empty2);
            Assert.AreEqual(0, empty1.Count);
        }

        [Test]
        public void Count_ReturnsCorrectValue()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);
            Assert.AreEqual(3, readOnlySet.Count);
        }

        [Test]
        public void Count_UpdatesWhenUnderlyingSetChanges()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);
            Assert.AreEqual(3, readOnlySet.Count);

            hashSet.Add(4);
            Assert.AreEqual(4, readOnlySet.Count);

            hashSet.Remove(1);
            Assert.AreEqual(3, readOnlySet.Count);
        }

        [Test]
        public void Contains_ReturnsTrueForExistingElement()
        {
            var hashSet = new HashSet<string> { "a", "b", "c" };
            var readOnlySet = new ReadOnlyHashSet<string>(hashSet);
            Assert.IsTrue(readOnlySet.Contains("a"));
            Assert.IsTrue(readOnlySet.Contains("b"));
            Assert.IsTrue(readOnlySet.Contains("c"));
        }

        [Test]
        public void Contains_ReturnsFalseForNonExistingElement()
        {
            var hashSet = new HashSet<string> { "a", "b", "c" };
            var readOnlySet = new ReadOnlyHashSet<string>(hashSet);
            Assert.IsFalse(readOnlySet.Contains("d"));
            Assert.IsFalse(readOnlySet.Contains("z"));
        }

        [Test]
        public void GetEnumerator_IteratesAllElements()
        {
            var hashSet = new HashSet<int> { 1, 2, 3, 4, 5 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);

            var foundElements = new HashSet<int>();
            foreach (var element in readOnlySet)
            {
                foundElements.Add(element);
            }

            Assert.AreEqual(5, foundElements.Count);
            Assert.IsTrue(foundElements.Contains(1));
            Assert.IsTrue(foundElements.Contains(2));
            Assert.IsTrue(foundElements.Contains(3));
            Assert.IsTrue(foundElements.Contains(4));
            Assert.IsTrue(foundElements.Contains(5));
        }

        [Test]
        public void GetEnumerator_WorksWithEmptySet()
        {
            var readOnlySet = ReadOnlyHashSet<int>.Empty();
            var count = 0;
            foreach (var unused in readOnlySet)
            {
                count++;
            }
            Assert.AreEqual(0, count);
        }

        [Test]
        public void Equals_ReturnsTrueForSameUnderlyingSet()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var readOnlySet1 = new ReadOnlyHashSet<int>(hashSet);
            var readOnlySet2 = new ReadOnlyHashSet<int>(hashSet);

            Assert.IsTrue(readOnlySet1.Equals(readOnlySet2));
            Assert.IsTrue(readOnlySet2.Equals(readOnlySet1));
        }

        [Test]
        public void Equals_ReturnsFalseForDifferentUnderlyingSets()
        {
            var hashSet1 = new HashSet<int> { 1, 2, 3 };
            var hashSet2 = new HashSet<int> { 1, 2, 3 };
            var readOnlySet1 = new ReadOnlyHashSet<int>(hashSet1);
            var readOnlySet2 = new ReadOnlyHashSet<int>(hashSet2);

            Assert.IsFalse(readOnlySet1.Equals(readOnlySet2));
        }

        [Test]
        public void Equals_ReturnsTrueForSameInstance()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);

            Assert.IsTrue(readOnlySet.Equals(readOnlySet));
        }

        [Test]
        public void Equals_ReturnsFalseForNull()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);

            Assert.IsFalse(readOnlySet.Equals(null));
        }

        [Test]
        public void Equals_WorksWithObjectOverload()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var readOnlySet1 = new ReadOnlyHashSet<int>(hashSet);
            var readOnlySet2 = new ReadOnlyHashSet<int>(hashSet);

            Assert.IsTrue(readOnlySet1.Equals((object)readOnlySet2));
            // ReSharper disable once SuspiciousTypeConversion.Global
            Assert.IsFalse(readOnlySet1.Equals((object)"not a set"));
            Assert.IsFalse(readOnlySet1.Equals((object)null));
        }

        [Test]
        public void GetHashCode_SameForSameUnderlyingSet()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var readOnlySet1 = new ReadOnlyHashSet<int>(hashSet);
            var readOnlySet2 = new ReadOnlyHashSet<int>(hashSet);

            Assert.AreEqual(readOnlySet1.GetHashCode(), readOnlySet2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentForDifferentUnderlyingSets()
        {
            var hashSet1 = new HashSet<int> { 1, 2, 3 };
            var hashSet2 = new HashSet<int> { 1, 2, 3 };
            var readOnlySet1 = new ReadOnlyHashSet<int>(hashSet1);
            var readOnlySet2 = new ReadOnlyHashSet<int>(hashSet2);

            Assert.AreNotEqual(readOnlySet1.GetHashCode(), readOnlySet2.GetHashCode());
        }

        [Test]
        public void CopyTo_CopiesAllElementsToArray()
        {
            var hashSet = new HashSet<int> { 1, 2, 3, 4, 5 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);
            var array = new int[5];

            readOnlySet.CopyTo(array);

            var copiedSet = new HashSet<int>(array);
            Assert.AreEqual(5, copiedSet.Count);
            Assert.IsTrue(copiedSet.Contains(1));
            Assert.IsTrue(copiedSet.Contains(2));
            Assert.IsTrue(copiedSet.Contains(3));
            Assert.IsTrue(copiedSet.Contains(4));
            Assert.IsTrue(copiedSet.Contains(5));
        }

        [Test]
        public void CopyTo_WorksWithLargerArray()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);
            var array = new int[10];

            readOnlySet.CopyTo(array);

            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(2, array[1]);
            Assert.AreEqual(3, array[2]);
        }

        [Test]
        public void CopyToNativeArray_CopiesAllElements()
        {
            var hashSet = new HashSet<int> { 10, 20, 30 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);
            var nativeArray = new NativeArray<int>(3, Allocator.Temp);

            readOnlySet.CopyTo(nativeArray);

            var copiedSet = new HashSet<int>();
            foreach (var item in nativeArray)
            {
                copiedSet.Add(item);
            }

            Assert.AreEqual(3, copiedSet.Count);
            Assert.IsTrue(copiedSet.Contains(10));
            Assert.IsTrue(copiedSet.Contains(20));
            Assert.IsTrue(copiedSet.Contains(30));
        }

        [Test]
        public void CopyToList_CopiesAllElements()
        {
            var hashSet = new HashSet<int> { 10, 20, 30 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);
            var list = new List<int>(3);

            readOnlySet.CopyTo(list);

            Assert.AreEqual(3, list.Count);
            Assert.IsTrue(list.Contains(10));
            Assert.IsTrue(list.Contains(20));
            Assert.IsTrue(list.Contains(30));
        }

        [Test]
        public void ToString_SucceedsWithEmptySet()
        {
            var readOnlySet = ReadOnlyHashSet<int>.Empty();
            var result = readOnlySet.ToString();
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("{"));
            Assert.IsTrue(result.Contains("}"));
        }

        [Test]
        public void ToString_SucceedsWithNonEmptySet()
        {
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var readOnlySet = new ReadOnlyHashSet<int>(hashSet);
            var result = readOnlySet.ToString();
            Debug.Log(result);
            Assert.IsNotNull(result);
        }

        [Test]
        public void ToString_SucceedsWithNullItemsInSet()
        {
            var hashSet = new HashSet<object> { 1, null, "test" };
            var readOnlySet = new ReadOnlyHashSet<object>(hashSet);
            var result = readOnlySet.ToString();
            Debug.Log(result);
            Assert.IsNotNull(result);
        }
    }
}
