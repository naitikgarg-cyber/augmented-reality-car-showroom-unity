using System;
using NUnit.Framework;
using UnityEngine;
using Unity.XR.CoreUtils.Collections;
using System.Collections.Generic;

namespace Unity.XR.CoreUtils.Editor.Tests
{
    class ReadOnlyListSpanTests
    {
        [Test]
        public void Constructor_ThrowsIfNullList()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new ReadOnlyListSpan<int>(null);
            });
        }

        [Test]
        public void Constructor_CreatesValidEmptyList()
        {
            var readOnlyListSpan = new ReadOnlyListSpan<int>(new List<int>());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var slice = readOnlyListSpan.Slice(0, 1);
            });

            using var enumerator = readOnlyListSpan.GetEnumerator();
            Assert.AreEqual(false, enumerator.MoveNext());

            Assert.AreEqual(0, readOnlyListSpan.Count);
        }

        [Test]
        public void GetEnumerator_IteratesFullList()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void GetEnumerator_IteratesConstructorSlice()
        {
            var list = new List<int>();
            list.Add(1);
            list.Add(2);
            list.Add(3);

            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 1, 1);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void GetEnumerator_IteratesSliceMethod()
        {
            var list = new List<int> { 1, 2, 3 };
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            var slice = readOnlyListSpan.Slice(1, 1);
            using var enumerator = slice.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void EnumeratorCurrent_ThrowsIfNeverMoveNext()
        {
            var list = new List<int> { 1, 2, 3 };
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator.Current;
            });
        }

        [Test]
        public void EnumeratorCurrent_ThrowsWhenAccessAfterEnd()
        {
            var list = new List<int> { 1, 2, 3 };
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.MoveNext();

            Assert.False(enumerator.MoveNext());
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator.Current;
            });
        }

        [Test]
        public void EnumeratorCurrent_ThrowsWhenAccessAfterSliceEnd()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 1, 2);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator.Current;
            });

            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.MoveNext();

            Assert.False(enumerator.MoveNext());
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator.Current;
            });
        }

        [Test]
        public void Reset_Resets()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 1, 2);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.Reset();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);
        }

        [Test]
        public void Enumerator_IteratesEmptyCollection()
        {
            var list = new List<int>();
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator = readOnlyListSpan.GetEnumerator();

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void Index_PositionsInSlice()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 1, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = readOnlyListSpan[-1];
            });

            Assert.AreEqual(2, readOnlyListSpan[0]);
            Assert.AreEqual(3, readOnlyListSpan[1]);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = readOnlyListSpan[2];
            });
        }

        [Test]
        public void GetEnumerator_DoesntInterfereWithOtherEnumerators()
        {
            var list = new List<int> { 1, 2, 3 };
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            using var enumerator1 = readOnlyListSpan.GetEnumerator();

            enumerator1.MoveNext();
            enumerator1.MoveNext();

            using var enumerator2 = readOnlyListSpan.GetEnumerator();
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator2.Current;
            });

            enumerator2.MoveNext();
            Assert.AreNotEqual(enumerator1.Current, enumerator2.Current);

            Assert.AreEqual(1, enumerator2.Current);
            Assert.AreEqual(2, enumerator1.Current);
        }

        [Test]
        public void Slice_ThrowsWhenOutsideSliceRange()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list, 2, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var anotherReadOnlyListSpan = readOnlyListSpan.Slice(1, 3);
            });
        }

        [Test]
        public void Slice_ReturnsNewInstance()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var readOnlyListSpan = new ReadOnlyListSpan<int>(list);
            var slice = readOnlyListSpan.Slice(1, 3);
            Assert.AreNotEqual(readOnlyListSpan.Count, slice.Count);

            var slice2 = slice.Slice(1, 2);
            Assert.AreNotEqual(slice.Count, slice2.Count);

            using var sliceEnumerator = slice.GetEnumerator();
            sliceEnumerator.MoveNext();
            Assert.AreEqual(2, sliceEnumerator.Current);

            using var slice2Enumerator = slice2.GetEnumerator();
            slice2Enumerator.MoveNext();
            Assert.AreEqual(3, slice2Enumerator.Current);
        }

        [Test]
        public void Constructor_ThrowsWhenOutOfBounds()
        {
            var list = new List<int> { 1, 2, 3 };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var readOnlyListSpan = new ReadOnlyListSpan<int>(list, -1, 2);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var anotherReadOnlyListSpan = new ReadOnlyListSpan<int>(list, 0, 4);
            });
        }

        [Test]
        public void ToString_SucceedsWithNullItemsInList()
        {
            var list = new List<object> { 1, null, 2 };
            var readOnlySpan = new ReadOnlyListSpan<object>(list);
            Debug.Log(readOnlySpan.ToString());
            // test passes if no error are logged
        }

        [Test]
        public void Empty_ReturnsEmpty()
        {
            var emptySpan = ReadOnlyListSpan<int>.Empty();

            Assert.AreEqual(0, emptySpan.Count);

            using var enumerator = emptySpan.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void Equals_TrueForSameListAndRange()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span1 = new ReadOnlyListSpan<int>(list, 1, 3);
            var span2 = new ReadOnlyListSpan<int>(list, 1, 3);

            Assert.True(span1.Equals(span2));
            Assert.True(span1 == span2);
            Assert.False(span1 != span2);
        }

        [Test]
        public void Equals_FalseForDifferentRanges()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span1 = new ReadOnlyListSpan<int>(list, 1, 3);
            var span2 = new ReadOnlyListSpan<int>(list, 1, 2);

            Assert.False(span1.Equals(span2));
            Assert.False(span1 == span2);
            Assert.True(span1 != span2);
        }

        [Test]
        public void Equals_FalseForDifferentLists()
        {
            var list1 = new List<int> { 1, 2, 3 };
            var list2 = new List<int> { 1, 2, 3 };
            var span1 = new ReadOnlyListSpan<int>(list1);
            var span2 = new ReadOnlyListSpan<int>(list2);

            Assert.False(span1.Equals(span2));
            Assert.False(span1 == span2);
            Assert.True(span1 != span2);
        }

        [Test]
        public void EqualsObject_TrueForEquivalentObjects()
        {
            var list = new List<int> { 1, 2, 3 };
            var span1 = new ReadOnlyListSpan<int>(list);
            var span2 = new ReadOnlyListSpan<int>(list);
            object span2Object = span2;

            Assert.True(span1.Equals(span2Object));
        }

        [Test]
        public void EqualsObject_FalseForDifferentObjects()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list);
            object other = new object();

            Assert.False(span.Equals(other));
        }

        [Test]
        public void GetHashCode_IsDeterministic()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span1 = new ReadOnlyListSpan<int>(list, 1, 3);
            var span2 = new ReadOnlyListSpan<int>(list, 1, 3);

            Assert.AreEqual(span1.GetHashCode(), span2.GetHashCode());
        }

        [Test]
        public void GetHashCode_DifferentForDifferentInstances()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span1 = new ReadOnlyListSpan<int>(list, 1, 3);
            var span2 = new ReadOnlyListSpan<int>(list, 2, 3);

            Assert.AreNotEqual(span1.GetHashCode(), span2.GetHashCode());
        }

        [Test]
        public void EnumeratorEquals_TrueForEquivalentEnumerators()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list);
            using var enumerator1 = span.GetEnumerator();
            using var enumerator2 = span.GetEnumerator();

            Assert.True(enumerator1.Equals(enumerator2));
        }

        [Test]
        public void EnumeratorEquals_FalseAfterMoveNext()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list);
            using var enumerator1 = span.GetEnumerator();
            using var enumerator2 = span.GetEnumerator();
            enumerator1.MoveNext();

            Assert.False(enumerator1.Equals(enumerator2));
        }

        [Test]
        public void EnumeratorHashCode_EqualForEquivalentEnumerators()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list);
            using var enumerator1 = span.GetEnumerator();
            using var enumerator2 = span.GetEnumerator();

            Assert.AreEqual(enumerator1.GetHashCode(), enumerator2.GetHashCode());
        }

        [Test]
        public void EnumeratorEqualsObject_EqualForEquivalentObject()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list);
            using var enumerator1 = span.GetEnumerator();
            using var enumerator2 = span.GetEnumerator();
            object enumerator2Object = enumerator2;

            Assert.True(enumerator1.Equals(enumerator2Object));
        }

        [Test]
        public void IEnumerable_Iterates()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list);
            IEnumerable<int> enumerable = span;

            using var enumerator = enumerable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
        }

        [Test]
        public void GenericIEnumerable_Iterates()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list);
            System.Collections.IEnumerable enumerable = span;

            var enumerator = enumerable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            (enumerator as IDisposable)?.Dispose();
        }

        [Test]
        public void Slice_HandlesZeroLength()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list);
            var slice = span.Slice(1, 0);

            Assert.AreEqual(0, slice.Count);

            using var enumerator = slice.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void Slices_HandlesZeroLengthAtCollectionEnd()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list);
            var slice = span.Slice(3, 0);

            Assert.AreEqual(0, slice.Count);

            using var enumerator = slice.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void GetEnumerator_MultipleConcurrentWithReset()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span = new ReadOnlyListSpan<int>(list);

            using var enumerator1 = span.GetEnumerator();
            using var enumerator2 = span.GetEnumerator();

            enumerator1.MoveNext();
            enumerator1.MoveNext();

            enumerator2.MoveNext();
            enumerator2.MoveNext();
            enumerator2.MoveNext();

            enumerator1.Reset();

            Assert.True(enumerator1.MoveNext());
            Assert.AreEqual(1, enumerator1.Current);

            Assert.True(enumerator2.MoveNext());
            Assert.AreEqual(4, enumerator2.Current);
        }

        [Test]
        public void Constructor_ZeroLengthSlice()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list, 1, 0);

            Assert.AreEqual(0, span.Count);

            using var enumerator = span.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void Index_ThrowsOnZeroLengthSlice()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyListSpan<int>(list, 1, 0);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = span[0];
            });
        }
    }
}
