using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.XR.CoreUtils.Collections;
using System.Collections.Generic;

namespace Unity.XR.CoreUtils.Editor.Tests
{
    class ReadOnlyStructSpanTests
    {
        [Test]
        public void Constructor_ThrowsOnNullInput()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var readOnlyStructSpan = new ReadOnlyStructSpan<int>(null);
            });
        }

        [Test]
        public void Constructor_HandlesEmptyList()
        {
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(new List<int>());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var slice = readOnlyStructSpan.Slice(0, 1);
            });

            using var enumerator = readOnlyStructSpan.GetEnumerator();
            Assert.AreEqual(false, enumerator.MoveNext());

            Assert.AreEqual(0, readOnlyStructSpan.Count);
        }

        [Test]
        public void GetEnumerator_IteratesFullList()
        {
            var list = new List<int> { 1, 2, 3 };
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

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
            var list = new List<int> { 1, 2, 3 };
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list, 1, 1);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void GetEnumerator_IteratesSliceMethod()
        {
            var list = new List<int> { 1, 2, 3 };
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list);
            var slice = readOnlyStructSpan.Slice(1, 1);
            using var enumerator = slice.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void EnumeratorCurrent_ThrowsIfNeverMoveNext()
        {
            var list = new List<int> { 1, 2, 3 };
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = enumerator.Current;
            });
        }

        [Test]
        public void EnumeratorCurrent_ThrowsWhenAccessAfterEnd()
        {
            var list = new List<int> { 1, 2, 3 };
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

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
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list, 1, 2);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

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
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list, 1, 2);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

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
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void Index_PositionsInSlice()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list, 1, 2);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = readOnlyStructSpan[-1];
            });

            Assert.AreEqual(2, readOnlyStructSpan[0]);
            Assert.AreEqual(3, readOnlyStructSpan[1]);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = readOnlyStructSpan[2];
            });
        }

        [Test]
        public void GetEnumerator_DoesntInterfereWithOtherEnumerators()
        {
            var list = new List<int> { 1, 2, 3 };
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list);
            using var enumerator1 = readOnlyStructSpan.GetEnumerator();

            enumerator1.MoveNext();
            enumerator1.MoveNext();

            using var enumerator2 = readOnlyStructSpan.GetEnumerator();
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
        public void Slice_ThrowsOutsideSliceRange()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list, 2, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var anotherReadOnlyStructSpan = readOnlyStructSpan.Slice(1, 3);
            });
        }

        [Test]
        public void Slice_ReturnsNewInstance()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list);

            var slice = readOnlyStructSpan.Slice(1, 3);
            Assert.AreNotEqual(readOnlyStructSpan.Count, slice.Count);

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
                var readOnlyStructSpan = new ReadOnlyStructSpan<int>(list, -1, 2);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var anotherReadOnlyStructSpan = new ReadOnlyStructSpan<int>(list, 0, 4);
            });
        }

        [Test]
        public void NativeArrayConstructor_EnumeratesValid()
        {
            var nativeArray = new NativeArray<int>(3, Allocator.Temp);
            nativeArray[0] = 1;
            nativeArray[1] = 2;
            nativeArray[2] = 3;

            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);

            Assert.False(enumerator.MoveNext());

            nativeArray.Dispose();
        }

        [Test]
        public void NativeArraySliceConstructor_EnumeratesValid()
        {
            var nativeArray = new NativeArray<int>(5, Allocator.Temp);
            nativeArray[0] = 1;
            nativeArray[1] = 2;
            nativeArray[2] = 3;
            nativeArray[3] = 4;
            nativeArray[4] = 5;

            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray, 1, 2);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);

            Assert.False(enumerator.MoveNext());

            nativeArray.Dispose();
        }

        [Test]
        public void Slice_SlicesNativeArray()
        {
            var nativeArray = new NativeArray<int>(5, Allocator.Temp);
            nativeArray[0] = 1;
            nativeArray[1] = 2;
            nativeArray[2] = 3;
            nativeArray[3] = 4;
            nativeArray[4] = 5;

            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray);
            var slice = readOnlyStructSpan.Slice(1, 3);

            Assert.AreEqual(3, slice.Count);
            Assert.AreEqual(2, slice[0]);
            Assert.AreEqual(3, slice[1]);
            Assert.AreEqual(4, slice[2]);
        }

        [Test]
        public void Slices_ThrowsWhenOutOfNativeArrayRange()
        {
            var nativeArray = new NativeArray<int>(5, Allocator.Temp);
            nativeArray[0] = 1;
            nativeArray[1] = 2;
            nativeArray[2] = 3;
            nativeArray[3] = 4;
            nativeArray[4] = 5;

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray, -1, 2);
            });

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray, 3, 3);
            });

            nativeArray.Dispose();
        }

        [Test]
        public void Index_WorksForNativeArray()
        {
            var nativeArray = new NativeArray<int>(5, Allocator.Temp);
            nativeArray[0] = 1;
            nativeArray[1] = 2;
            nativeArray[2] = 3;
            nativeArray[3] = 4;
            nativeArray[4] = 5;

            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray, 1, 3);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = readOnlyStructSpan[-1];
            });

            Assert.AreEqual(2, readOnlyStructSpan[0]);
            Assert.AreEqual(3, readOnlyStructSpan[1]);
            Assert.AreEqual(4, readOnlyStructSpan[2]);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = readOnlyStructSpan[3];
            });

            nativeArray.Dispose();
        }

        [Test]
        public void EmptyNativeArray_Enumerates()
        {
            var nativeArray = new NativeArray<int>(0, Allocator.Temp);
            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray);

            Assert.AreEqual(0, readOnlyStructSpan.Count);

            using var enumerator = readOnlyStructSpan.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void Reset_ResetsWithNativeArray()
        {
            var nativeArray = new NativeArray<int>(3, Allocator.Temp);
            nativeArray[0] = 1;
            nativeArray[1] = 2;
            nativeArray[2] = 3;

            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray);
            using var enumerator = readOnlyStructSpan.GetEnumerator();

            enumerator.MoveNext();
            enumerator.MoveNext();
            enumerator.Reset();

            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);

            nativeArray.Dispose();
        }

        [Test]
        public void Slice_SlicesNativeArraySlice()
        {
            var nativeArray = new NativeArray<int>(10, Allocator.Temp);
            for (int i = 0; i < 10; i++)
                nativeArray[i] = i;

            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray, 2, 6);
            var slice = readOnlyStructSpan.Slice(1, 4);

            Assert.AreEqual(4, slice.Count);
            Assert.AreEqual(3, slice[0]);
            Assert.AreEqual(4, slice[1]);
            Assert.AreEqual(5, slice[2]);
            Assert.AreEqual(6, slice[3]);
        }

        [Test]
        public void ToString_HandlesNativeArray()
        {
            var nativeArray = new NativeArray<int>(3, Allocator.Temp);
            nativeArray[0] = 1;
            nativeArray[1] = 2;
            nativeArray[2] = 3;

            var readOnlyStructSpan = new ReadOnlyStructSpan<int>(nativeArray);
            var result = readOnlyStructSpan.ToString();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("1"));
            Assert.IsTrue(result.Contains("2"));
            Assert.IsTrue(result.Contains("3"));
        }

        [Test]
        public void Empty_ReturnsEmpty()
        {
            var emptySpan = ReadOnlyStructSpan<int>.Empty();

            Assert.AreEqual(0, emptySpan.Count);

            using var enumerator = emptySpan.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void Equals_TrueForSameListAndRange()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span1 = new ReadOnlyStructSpan<int>(list, 1, 3);
            var span2 = new ReadOnlyStructSpan<int>(list, 1, 3);

            Assert.True(span1.Equals(span2));
            Assert.True(span1 == span2);
            Assert.False(span1 != span2);
        }

        [Test]
        public void Equals_FalseForDifferentRanges()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span1 = new ReadOnlyStructSpan<int>(list, 1, 3);
            var span2 = new ReadOnlyStructSpan<int>(list, 1, 2);

            Assert.False(span1.Equals(span2));
            Assert.False(span1 == span2);
            Assert.True(span1 != span2);
        }

        [Test]
        public void Equals_FalseForDifferentLists()
        {
            var list1 = new List<int> { 1, 2, 3 };
            var list2 = new List<int> { 1, 2, 3 };
            var span1 = new ReadOnlyStructSpan<int>(list1);
            var span2 = new ReadOnlyStructSpan<int>(list2);

            Assert.False(span1.Equals(span2));
            Assert.False(span1 == span2);
            Assert.True(span1 != span2);
        }

        [Test]
        public void EqualsObject_TrueForEquivalentObject()
        {
            var list = new List<int> { 1, 2, 3 };
            var span1 = new ReadOnlyStructSpan<int>(list);
            var span2 = new ReadOnlyStructSpan<int>(list);
            object span2Object = span2;

            Assert.True(span1.Equals(span2Object));
        }

        [Test]
        public void EqualsObject_FalseForDifferentObjects()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyStructSpan<int>(list);
            object other = new object();

            Assert.False(span.Equals(other));
        }

        [Test]
        public void GetHashCode_IsDeterministic()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span1 = new ReadOnlyStructSpan<int>(list, 1, 3);
            var span2 = new ReadOnlyStructSpan<int>(list, 1, 3);

            Assert.AreEqual(span1.GetHashCode(), span2.GetHashCode());
        }

        [Test]
        public void GetHashCode_IsDifferentForDifferentSpans()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span1 = new ReadOnlyStructSpan<int>(list, 1, 3);
            var span2 = new ReadOnlyStructSpan<int>(list, 2, 3);

            Assert.AreNotEqual(span1.GetHashCode(), span2.GetHashCode());
        }

        [Test]
        public void IEnumerable_Iterates()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyStructSpan<int>(list);
            IEnumerable<int> enumerable = span;

            using var enumerator = enumerable.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
        }

        [Test]
        public void GenericIEnumerable_Iterates()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyStructSpan<int>(list);
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
            var span = new ReadOnlyStructSpan<int>(list);
            var slice = span.Slice(1, 0);

            Assert.AreEqual(0, slice.Count);

            using var enumerator = slice.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void Slice_HandlesZeroLengthAtCollectionEnd()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyStructSpan<int>(list);
            var slice = span.Slice(3, 0);

            Assert.AreEqual(0, slice.Count);

            using var enumerator = slice.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void GetEnumerator_MultipleConcurrentWithReset()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };
            var span = new ReadOnlyStructSpan<int>(list);

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
            var span = new ReadOnlyStructSpan<int>(list, 1, 0);

            Assert.AreEqual(0, span.Count);

            using var enumerator = span.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }

        [Test]
        public void Index_ZeroLengthSlice()
        {
            var list = new List<int> { 1, 2, 3 };
            var span = new ReadOnlyStructSpan<int>(list, 1, 0);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var temp = span[0];
            });
        }

        [Test]
        public void Equals_HandlesNativeArrays()
        {
            var nativeArray = new NativeArray<int>(5, Allocator.Temp);
            for (int i = 0; i < 5; i++)
                nativeArray[i] = i + 1;

            var span1 = new ReadOnlyStructSpan<int>(nativeArray, 1, 3);
            var span2 = new ReadOnlyStructSpan<int>(nativeArray, 1, 3);

            Assert.True(span1.Equals(span2));
            Assert.True(span1 == span2);
            Assert.False(span1 != span2);
        }

        [Test]
        public void Slice_HandlesNativeArrayAndZeroLength()
        {
            var nativeArray = new NativeArray<int>(3, Allocator.Temp);
            nativeArray[0] = 1;
            nativeArray[1] = 2;
            nativeArray[2] = 3;

            var span = new ReadOnlyStructSpan<int>(nativeArray);
            var slice = span.Slice(1, 0);

            Assert.AreEqual(0, slice.Count);

            using var enumerator = slice.GetEnumerator();
            Assert.False(enumerator.MoveNext());
        }
    }
}
