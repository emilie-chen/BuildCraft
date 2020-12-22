using System;
using System.Runtime.CompilerServices;
using BuildCraft.Base.GlWrappers;
using BuildCraft.Base.Std;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BuildCraftBaseUnitTest
{
    [TestClass]
    public unsafe class UnitTestNativeArray
    {
        private const int COUNT = 100_000;
        
        [TestMethod]
        public void TestNativeArrayIndexerToRef()
        {
            int count = COUNT;
            using NativeArray<ValueTuple<int, float>> arr = new NativeArray<(int, float)>(count);
            ValueTuple<int, float>[] arrManaged = new ValueTuple<int, float>[count];
            for (int i = 0; i < count; i++)
            {
                arrManaged[i] = (i, (float) i);
                arr[i] = (i, (float) i);
            }

            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(arr.At(i) == arrManaged[i]);
            }
        }
        
        [TestMethod]
        public void TestNativeArrayRefToIndexer()
        {
            int count = COUNT;
            using NativeArray<ValueTuple<int, float>> arr = new NativeArray<(int, float)>(count);
            ValueTuple<int, float>[] arrManaged = new ValueTuple<int, float>[count];
            for (int i = 0; i < count; i++)
            {
                arrManaged[i] = (i, (float) i);
                arr.At(i) = (i, (float) i);
            }

            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(arr[i] == arrManaged[i]);
            }
        }
        
        [TestMethod]
        public void TestNativeArrayRef()
        {
            int count = COUNT;
            using NativeArray<ValueTuple<int, float>> arr = new NativeArray<(int, float)>(count);
            ValueTuple<int, float>[] arrManaged = new ValueTuple<int, float>[count];
            for (int i = 0; i < count; i++)
            {
                arrManaged[i] = (i, (float) i);
                arr.At(i) = (i, (float) i);
            }

            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(arr.At(i) == arrManaged[i]);
            }
        }
        
        [TestMethod]
        public void TestNativeArrayIndexer()
        {
            int count = COUNT;
            using NativeArray<ValueTuple<int, float>> arr = new NativeArray<(int, float)>(count);
            ValueTuple<int, float>[] arrManaged = new ValueTuple<int, float>[count];
            for (int i = 0; i < count; i++)
            {
                arrManaged[i] = (i, (float) i);
                arr[i] = (i, (float) i);
            }

            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(arr[i] == arrManaged[i]);
            }
        }

        [TestMethod]
        public void TestNativeArrayRefMix()
        {
            int count = COUNT;
            using NativeArray<ValueTuple<int, float>> arr = new NativeArray<(int, float)>(count);
            ValueTuple<int, float>[] arrManaged = new ValueTuple<int, float>[count];
            for (int i = 0; i < count; i++)
            {
                arrManaged[i] = (i, (float) i);
                ref (int, float) element = ref arr.At(i);
                element = (i, (float) i);
            }

            for (int i = 0; i < count; i++)
            {
                Assert.IsTrue(arr.Data[i] == arrManaged[i]);
            }
        }

        [TestMethod, MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public void TestNativeArrayPerformance()
        {
            int count = COUNT;
            Console.WriteLine("Hi");
            using NativeArray<ValueTuple<int, float>> arr = new NativeArray<(int, float)>(count);
            ValueTuple<int, float>[] arrManaged = new ValueTuple<int, float>[count];
            
            DateTime currentTime = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                arrManaged[i] = (i, (float) i);
            }
            Console.WriteLine(DateTime.Now - currentTime);
            
            ValueTuple<int, float>* head = arr.Data;
            currentTime = DateTime.Now;
            for (int i = 0; i < count; i++)
            {
                head[i] = (i, (float) i);
            }
            Console.WriteLine(DateTime.Now - currentTime);
        }

    }
}