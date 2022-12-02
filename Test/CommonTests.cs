using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common;
using Xunit;

namespace Test
{
    public class CommonTests
    {
        [Fact]
        void Test_WithIn()
        {
            Assert.False(3.WithIn(-2, -3));
            Assert.False(3.WithIn(-4, 2));
            Assert.False(3.WithIn(4, 6));
            Assert.True(3.WithIn(3, 8));
            Assert.True(3.WithIn(8, 3));
            Assert.True(3.WithIn(2, 8));
            Assert.True(3.WithIn(8, 2));
        }
        [Fact]
        void Test_Intersects()
        {
            Assert.False((1, 3).Intersects((4, 8)));
            Assert.True((1, 4).Intersects((4, 8)));
            Assert.True((1, 5).Intersects((4, 8)));
            Assert.True((4, 8).Intersects((4, 8)));
            Assert.True((5, 7).Intersects((4, 8)));
            Assert.True((3, 9).Intersects((4, 8)));
            Assert.False((9, 11).Intersects((4, 8)));
        }
        [Fact]
        void Test_Intersection()
        {
            Assert.Equal(actual: (1, 4).Intersection((4, 8)), expected: (4, 4));
            Assert.Equal(actual: (1, 6).Intersection((4, 8)), expected: (4, 6));
            Assert.Equal(actual: (5, 9).Intersection((4, 8)), expected: (5, 8));
            Assert.Equal(actual: (8, 11).Intersection((4, 8)), expected: (8, 8));
            Assert.Equal(actual: (5, 7).Intersection((4, 8)), expected: (5, 7));
            Assert.Equal(actual: (1, 11).Intersection((4, 8)), expected: (4, 8));
            Assert.Equal(actual: (9, 11).Intersection((4, 8)), expected: null);
            Assert.Equal(actual: (1, 2).Intersection((4, 8)), expected: null);
        }

        [Fact]
        void TestRange()
        {
            var l1 = EnumerableExtensions.Range(1, 3).ToList();
            var l2 = EnumerableExtensions.Range(-1, -3).ToList();
            var l3 = EnumerableExtensions.Range(3, -3).ToList();
            var l4 = EnumerableExtensions.Range(-3, 3).ToList();
            var l5 = EnumerableExtensions.Range(0, 0).ToList();
        }
    }
}
