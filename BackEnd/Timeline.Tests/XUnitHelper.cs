using System;
using System.Collections.Generic;
using System.Linq;

namespace Timeline.Tests
{
    public static class XUnitHelper
    {
        public static IEnumerable<object?[]> ComposeTestData(params IEnumerable<object?[]>[] testDatas)
        {
            return ComposeTestData(new ArraySegment<IEnumerable<object?[]>>(testDatas));
        }

        public static IEnumerable<object?[]> ComposeTestData(ArraySegment<IEnumerable<object?[]>> testDatas)
        {
            if (testDatas.Count == 0)
                throw new ArgumentException("Test data list can't be empty.", nameof(testDatas));

            if (testDatas.Count == 1)
            {
                foreach (var d in testDatas[0])
                    yield return d;
            }
            else
            {
                foreach (var head in testDatas[0])
                    foreach (var rest in ComposeTestData(testDatas.Slice(1)))
                        yield return head.Concat(rest).ToArray();
            }
        }

        public static IEnumerable<object?[]> AppendTestData(this IEnumerable<object?[]> origin, params IEnumerable<object?>[] toAppend)
        {
            IEnumerable<object?[]> result = origin;

            foreach (var oneToAppend in toAppend)
            {
                result = ComposeTestData(result, oneToAppend.Select(testData => new object?[] { testData }));
            }

            return result;
        }
    }
}
