using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck.Xunit;
using Xunit;

namespace Lette.Functional.CSharp.Tests
{
    public class ListTests
    {
        public class EqualityTests
        {
            [Fact]
            public void Empty_lists_are_equal()
            {
                Assert.Equal(new List<byte>(), new List<byte>());
            }

            [Fact]
            public void Non_empty_lists_with_equal_elements_are_equal()
            {
                Assert.Equal(new List<int> { 1, 2, 3 }, new List<int> { 1, 2, 3 });
                Assert.True((new List<int> { 1, 2, 3 }).SequenceEqual(new List<int> { 1, 2, 3 }));
                Assert.True((new List<int> { 1, 2, 3 }).SequenceEqual(new List<int> { 1, 2, 3 }));
            }

            [Fact]
            public void Lists_with_different_number_of_elements_are_unequal()
            {
                Assert.NotEqual(new List<string> { "a", "b" }, new List<string> { "a", "b", "c" });
            }
        }

        public class FunctorTests
        {
            [Fact]
            public void Empty_list_is_mapped()
            {
                Func<int, string> map = i => i.ToString();

                var elevatedMap = ListExtensions.FMap(map);

                Assert.Equal(new List<string>(), elevatedMap(new List<int>()));
            }

            [Fact]
            public void Elements_are_mapped()
            {
                Func<int, string> map = i => i.ToString();

                var elevatedMap = ListExtensions.FMap(map);

                Assert.Equal(
                    new List<string> { map(1), map(2), map(3) },
                    elevatedMap(new List<int> { 1, 2, 3 }));
            }
        }

        public class FunctorLaws
        {
            public class FirstLaw
            {
                // Assert that:
                // fmap id = id

                private static readonly Func<List<int>, List<int>> ElevatedId = ListExtensions.FMap(((Func<int, int>)Functional.Id));

                [Fact]
                public void Empty_list()
                {
                    var left = ElevatedId(new List<int>());
                    var right = Functional.Id(new List<int>());


                    Assert.Equal(left, right);
                }

                [Property]
                public bool Non_empty_list(int a, int b)
                {
                    var left = ElevatedId(new List<int> { 1, 2 });
                    var right = Functional.Id(new List<int> { 1, 2 });

                    return left.SequenceEqual(right);
                }
            }

            public class SecondLaw
            {
                // Assert that:
                // fmap(g.h) = (fmap g) . (fmap h)

                public static readonly Func<string, int> Length = s => s.Length;
                public static readonly Func<int, double> DivBy4 = i => i / 4.0;

                public static readonly Func<List<string>, List<double>> ComposedThenElevated =
                    ListExtensions.FMap(Length.Compose(DivBy4));

                public static readonly Func<List<string>, List<double>> ElevatedThenComposed =
                    ListExtensions.FMap(Length).Compose(ListExtensions.FMap(DivBy4));

                [Fact]
                public void Empty_list()
                {
                    var emptyList = new List<string>();

                    var left = ComposedThenElevated(emptyList);
                    var right = ElevatedThenComposed(emptyList);

                    Assert.Equal(left, right);
                }

                [Fact]
                public void Non_empty_list()
                {
                    var list = new List<string> { "a", "aa" };
                    var list2 = new List<string> { "a", "aa" };

                    var left = ComposedThenElevated(list);
                    var right = ElevatedThenComposed(list2);

                    Assert.Equal(left, right);
                }
            }
        }

        public class ApplicativeTests
        {
            [Property]
            public bool Pure_returns_a_single_element_list(int a)
            {
                return new List<int> { a }.SequenceEqual(ListExtensions.Pure(a));
            }

            public static readonly Func<int, int> IntDivBy2 = x => x / 2;
            public static readonly List<Func<int, int>> NonEmptyList = new List<Func<int, int>> { IntDivBy2 };
            public static readonly List<Func<int, int>> EmptyList = new List<Func<int, int>>();

            [Property]
            public bool Invoking_applied_elevated_function_with_a_list_returns_list_of_function_result(int i)
            {
                Func<List<int>, List<int>> appliedElevatedFunc = NonEmptyList.Apply();

                return appliedElevatedFunc(new List<int> { i }).SequenceEqual(new List<int> { IntDivBy2(i) });
            }

            [Fact]
            public void Invoking_applied_functions_with_empty_list_returns_empty_list()
            {
                Func<List<int>, List<int>> appliedElevatedFunc = NonEmptyList.Apply();

                Assert.Equal(new List<int>(), appliedElevatedFunc(new List<int>()));
            }

            [Property]
            public bool Invoking_applied_empty_list_of_functions_with_a_non_empty_list_returns_empty_list(int i)
            {
                Func<List<int>, List<int>> appliedElevatedEmpty = EmptyList.Apply();

                return appliedElevatedEmpty(new List<int> {i}).SequenceEqual(new List<int>());
            }

            [Fact]
            public void Invoking_applied_empty_list_of_functions_with_empty_list_returns_empty_list()
            {
                Func<List<int>, List<int>> appliedElevatedEmpty = EmptyList.Apply();

                Assert.Equal(new List<int>(), appliedElevatedEmpty(new List<int>()));
            }

            [Fact]
            public void Applies_multiple_functions_to_multiple_values()
            {
                Func<int, int> IntDivBy4 = IntDivBy2.Compose(IntDivBy2);

                var functions = new List<Func<int, int>> { IntDivBy2, IntDivBy4 };
                var args = new List<int> { 4, 8, 15 };

                Assert.Equal(
                    new List<int>
                    {
                        IntDivBy2(4), IntDivBy2(8), IntDivBy2(15),
                        IntDivBy4(4), IntDivBy4(8), IntDivBy4(15)
                    },
                    functions.Apply()(args));
            }
        }
    }
}