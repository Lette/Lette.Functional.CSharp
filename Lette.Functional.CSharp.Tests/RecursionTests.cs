using System.Numerics;
using Xunit;

namespace Lette.Functional.CSharp.Tests
{
    public class RecursionTests
    {
        [Fact]
        public void Returns_final_result()
        {
            RecursionResult<int> F() => RecursionResult<int>.Final(1);

            Assert.Equal(1, TailRecursion.Run(F));
        }

        [Fact]
        public void Returns_final_result_from_next_iteration()
        {
            RecursionResult<int> F() => RecursionResult<int>.Next(() => RecursionResult<int>.Final(2));

            Assert.Equal(2, TailRecursion.Run(F));
        }

        [Fact]
        public void Returns_final_result_after_recursion()
        {
            RecursionResult<int> Factorial(int k)
            {
                RecursionResult<int> FactorialAcc(int m, int acc)
                {
                    if (m == 1)
                    {
                        return RecursionResult<int>.Final(acc);
                    }

                    return RecursionResult<int>.Next(() => FactorialAcc(m - 1, m * acc));
                }

                return FactorialAcc(k, 1);
            }

            Assert.Equal(5 * 4 * 3 * 2 * 1, TailRecursion.Run(() => Factorial(5)));
        }

        [Fact]
        public void Deep_recursion_does_not_blow_the_stack()
        {
            RecursionResult<BigInteger> Factorial(int k)
            {
                RecursionResult<BigInteger> FactorialAcc(int m, BigInteger acc)
                {
                    if (m == 1)
                        return RecursionResult<BigInteger>.Final(acc);

                    return RecursionResult<BigInteger>.Next(() => FactorialAcc(m - 1, m * acc));
                }

                return FactorialAcc(k, 1);
            }

            var ex = Record.Exception(() => TailRecursion.Run(() => Factorial(20000)));

            Assert.Null(ex);
        }

        // [Fact(Skip = "Use for finding machine specific limit of recursion.")]
        // public void Recursion_depth_test()
        // {
        //     BigInteger F(int k, BigInteger acc)
        //     {
        //         if (k == 1)
        //             return acc;
        //
        //         return F(k - 1, k * acc);
        //     }
        //
        //     F(18000, 1);
        // }
    }
}
