using System;
using FsCheck.Xunit;
using Xunit;

namespace Lette.Functional.CSharp.Tests
{
    public class MListTests
    {
        public class EqualityTests
        {
            [Fact]
            public void Empty_lists_are_equal()
            {
                Assert.Equal(MList<int>.Empty, MList<int>.Empty);
            }

            [Fact]
            public void Empty_list_is_not_equal_to_non_empty_list()
            {
                Assert.NotEqual(MList<int>.Empty, MList<int>.List(1, MList<int>.Empty));
                Assert.NotEqual(MList<int>.List(1, MList<int>.Empty), MList<int>.Empty);
            }

            [Fact]
            public void Lists_with_different_lengths_are_not_equal()
            {
                Assert.NotEqual(
                    MList<int>.List(1, MList<int>.Empty),
                    MList<int>.List(1, MList<int>.List(1, MList<int>.Empty)));
                Assert.NotEqual(
                    MList<int>.List(1, MList<int>.List(1, MList<int>.Empty)),
                    MList<int>.List(1, MList<int>.Empty));
            }

            [Fact]
            public void Lists_with_same_elements_in_same_order_are_equal()
            {
                Assert.Equal(
                    MList<int>.List(1, MList<int>.List(2, MList<int>.Empty)),
                    MList<int>.List(1, MList<int>.List(2, MList<int>.Empty)));
            }

            [Fact]
            public void Lists_with_swapped_elements_are_not_equal()
            {
                Assert.NotEqual(
                    MList<string>.List("a", MList<string>.List("b", MList<string>.Empty)),
                    MList<string>.List("b", MList<string>.List("a", MList<string>.Empty)));
            }
        }

        public class UtilityTests
        {
            [Fact]
            public void Length_of_empty_list_is_zero()
            {
                Assert.Equal(0, MList<int>.Empty.MLength());
            }

            [Fact]
            public void Length_of_three_element_list_is_three()
            {
                Assert.Equal(3, MList<int>.List(2, MList<int>.List(2, MList<int>.List(2, MList<int>.Empty))).MLength());
            }

            [Fact]
            public void Reverse_of_empty_list_is_empty_list()
            {
                Assert.Equal(
                    MList<string>.Empty,
                    MList<string>.Empty.Reverse());
            }

            [Fact]
            public void Reverse_of_list_is_list_with_elements_in_reverse_order()
            {
                var list = MList<string>.List(
                    "w", MList<string>.List(
                        "x", MList<string>.List(
                            "y", MList<string>.List(
                                "z", MList<string>.Empty))));

                var reversedList = MList<string>.List(
                    "z", MList<string>.List(
                        "y", MList<string>.List(
                            "x", MList<string>.List(
                                "w", MList<string>.Empty))));

                Assert.Equal(reversedList, list.Reverse());
            }
        }

        public class FunctorTests
        {
            [Fact]
            public void Empty_list_is_mapped_to_empty_list()
            {
                Func<int, string> f = null;

                var result = f.FMMap()(MList<int>.Empty);

                Assert.Equal(MList<string>.Empty, result);
            }

            [Fact]
            public void Non_empty_list_is_mapped()
            {
                Func<string, int> f = s => s.Length;

                var list = MList<string>.List(
                    "a", MList<string>.List(
                        "aa", MList<string>.List(
                            "aaa", MList<string>.Empty)));

                var expected = MList<int>.List(
                    f("a"), MList<int>.List(
                        f("aa"), MList<int>.List(
                            f("aaa"), MList<int>.Empty)));

                var mappedF = f.FMMap();

                Assert.Equal(expected, mappedF(list));
            }
        }

        public class FunctorLaws
        {
            // TODO
        }

        public class ApplicativeTests
        {
            [Property]
            public void Pure_returns_a_single_element_list(byte b)
            {
                Assert.Equal(b.MPure(), MList<byte>.List(b, MList<byte>.Empty));
            }

            [Fact]
            public void Apply_with_empty_lists_returns_empty_list()
            {
                var fs = MList<Func<int, long>>.Empty;
                var xs = MList<int>.Empty;

                Assert.Equal(fs.MApply(xs), MList<long>.Empty);
            }

            [Fact]
            public void Apply_with_empty_list_of_functions_returns_empty_list()
            {
                var fs = MList<Func<int, long>>.Empty;
                var xs = MList<int>.List(1, MList<int>.Empty);

                Assert.Equal(fs.MApply(xs), MList<long>.Empty);
            }

            [Fact]
            public void Apply_with_empty_list_of_values_returns_empty_list()
            {
                var fs = MList<Func<int, long>>.List(i => (long)i, MList<Func<int, long>>.Empty);
                var xs = MList<int>.Empty;

                Assert.Equal(fs.MApply(xs), MList<long>.Empty);
            }

            [Property]
            public bool Apply_with_list_of_one_function_and_list_of_one_value_returns_list_with_one_result(int a)
            {
                Func<int, string> f = i => i.ToString();
                var fs = MList<Func<int, string>>.List(f, MList<Func<int, string>>.Empty);
                var xs = MList<int>.List(a, MList<int>.Empty);

                return fs.MApply(xs).Equals(MList<string>.List(f(a), MList<string>.Empty));
            }

            [Fact]
            public void Apply_with_functions_and_values_returns_the_cartesian_product()
            {
                Func<int, int> f = i => i + 3;
                Func<int, int> g = i => i * 5;

                var fs = MList<Func<int, int>>.List(
                    f, MList<Func<int, int>>.List(
                        g, MList<Func<int, int>>.Empty));

                var xs = MList<int>.List(
                    2, MList<int>.List(
                        3, MList<int>.List(
                            4, MList<int>.Empty)));

                var expected = MList<int>.List(
                    f(2), MList<int>.List(
                        f(3), MList<int>.List(
                            f(4), MList<int>.List(
                                g(2), MList<int>.List(
                                    g(3), MList<int>.List(
                                        g(4), MList<int>.Empty))))));

                var actual = fs.MApply(xs);

                Assert.Equal(expected, actual);
            }
        }

        public class ApplicativeLaws
        {
            // TODO
        }

        public class MonadTests
        {
            // TODO
        }

        public class MonadLaws
        {
            // TODO
        }

        public class KleisliTests
        {
            // TODO
        }
    }
}