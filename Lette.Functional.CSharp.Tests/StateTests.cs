using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using static Lette.Functional.CSharp.Functional;

namespace Lette.Functional.CSharp.Tests
{
    public class StateTests
    {
        [Fact]
        public void Piping_a_tuple_through_a_chain_of_functions()
        {
            (int, string) Inc(int x, string state) => (x + 1, state + "+1");
            (int, string) Dec(int x, string state) => (x - 1, state + "-1");
            (int, string) Dbl(int x, string state) => (x * 2, state + "*2");

            var result =
                (3, "Ops:")
                .ForwardPipe(Inc)
                .ForwardPipe(Dbl)
                .ForwardPipe(Dec);

            Assert.Equal((7, "Ops:+1*2-1"), result);
        }

        [Fact]
        public void Threading_late_bound_state_through_a_chain_of_functions()
        {
            StateFn<int, string> Inc(int x) => state => (x + 1, state + "+1");
            StateFn<int, string> Dec(int x) => state => (x - 1, state + "-1");
            StateFn<int, string> Dbl(int x) => state => (x * 2, state + "*2");

            var result =
                42.ToState<int, string>()
                    .Bind(Dbl)
                    .Bind(Dbl)
                    .Bind(Dec)
                    .Bind(Dbl)
                    .Bind(Dbl)
                    .Bind(Dbl)
                    .Bind(Inc);

            Assert.Equal((1337, "Ops:*2*2-1*2*2*2+1"), result("Ops:")); // 😀
        }
    }
}
