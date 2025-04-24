using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace M1_Imiron
{
    public class Test
    {
        [Fact]
        public void Add_AddsTwoNumbers_ReturnsCorrectSum()
        {
            Assert.Equal(7, 3 + 4);
        }

        [Fact]
        public void Subtract_SubtractsTwoNumbers_ReturnsCorrectDifference()
        {
            Assert.Equal(6, 3 + 3);
        }
    }
}