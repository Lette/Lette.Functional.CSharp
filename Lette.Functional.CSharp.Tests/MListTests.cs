using System;
using System.Collections.Generic;
using System.Linq;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Lette.Functional.CSharp.Functional;

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
                Assert.Equal(0, MList<int>.Empty.Length());
            }

            [Fact]
            public void Length_of_three_element_list_is_three()
            {
                Assert.Equal(3, MList<int>.List(2, MList<int>.List(2, MList<int>.List(2, MList<int>.Empty))).Length());
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
                var list = CreateList("w", "x", "y", "z");
                var reversedList = CreateList("z", "y", "x", "w");

                Assert.Equal(reversedList, list.Reverse());
            }

            [Fact]
            public void Combining_two_empty_lists_is_an_empty_list()
            {
                var result = MList.Combine(MList<int>.Empty, MList<int>.Empty);

                Assert.Equal(MList<int>.Empty, result);
            }

            [Fact]
            public void Combining_a_non_empty_list_and_an_empty_list_is_the_non_empty_list()
            {
                Assert.Equal(CreateList(1, 2), MList.Combine(CreateList(1, 2), MList<int>.Empty));
                Assert.Equal(CreateList(1, 2), MList.Combine(MList<int>.Empty, CreateList(1, 2)));
            }

            [Fact]
            public void Combining_two_lists_returns_one_list_with_all_elements()
            {
                var list1 = CreateList(1, 2, 3);
                var list2 = CreateList(4, 5);

                var result = MList.Combine(list1, list2);

                Assert.Equal(CreateList(1, 2, 3, 4, 5), result);
            }

            [Fact]
            public void Concat_empty_list_of_lists_returns_empty_list()
            {
                var actual = MList.Concat(MList<MList<int>>.Empty);

                Assert.Equal(MList<int>.Empty, actual);
            }

            [Fact]
            public void Concat_returns_concatenated_list()
            {
                var lists = CreateList(
                    CreateList(1, 2),
                    CreateList<int>(),
                    CreateList(3, 4, 5));

                Assert.Equal(
                    CreateList(1, 2, 3, 4, 5),
                    MList.Concat(lists));
            }

            [Fact]
            public void FoldL_on_empty_list_returns_initial_value()
            {
                var result = MList.FoldL((x, y) => null, "initial", MList<string>.Empty);

                Assert.Equal("initial", result);
            }

            [Fact]
            public void FoldL_calls_function_with_arguments_from_left_to_right()
            {
                var list = CreateList(1, 2, 3);
                var result = MList.FoldL((acc, i) => $"{acc} {i}", "digits:", list);

                Assert.Equal("digits: 1 2 3", result);
            }

            [Fact]
            public void FoldR_on_empty_list_returns_initial_value()
            {
                var result = MList.FoldR((x, y) => null, "initial", MList<int>.Empty);

                Assert.Equal("initial", result);
            }

            [Fact]
            public void FoldR_calls_function_with_arguments_from_right_to_left()
            {
                var list = CreateList(1, 2, 3);
                var result = MList.FoldR((i, acc) => $"{acc} {i}", "digits:", list);

                Assert.Equal("digits: 3 2 1", result);
            }
        }

        public class FunctorTests
        {
            [Fact]
            public void Empty_list_is_mapped_to_empty_list()
            {
                var result = MList.FMap<int, string>(null, MList<int>.Empty);

                Assert.Equal(MList<string>.Empty, result);
            }

            [Fact]
            public void Non_empty_list_is_mapped()
            {
                Func<string, int> f = s => s.Length;

                var list = CreateList("a", "aa", "aaa");

                var expected = CreateList(f("a"), f("aa"), f("aaa"));

                var mappedF = MList.FMap(f);

                Assert.Equal(expected, mappedF(list));
            }
        }

        public class FunctorLaws
        {
            public class FirstLaw
            {
                // Assert that:
                // fmap id = id

                private readonly Func<MList<int>, MList<int>> _left = MList.FMap((Func<int, int>)Id);
                private readonly Func<MList<int>, MList<int>> _right = Id;

                [Fact]
                public void Empty_list()
                {
                    Assert.Equal(_left(MList<int>.Empty), _right(MList<int>.Empty));
                }

                [Property]
                public bool Non_empty_list(int i1, int i2)
                {
                    var input = CreateList(i1, i2);

                    return _left(input).Equals(_right(input));
                }
            }

            public class SecondLaw
            {
                // Assert that:
                // fmap (g . h) = (fmap g) . (fmap h)

                public static readonly Func<string, int> Length = s => s.Length;
                public static readonly Func<int, double> DivBy4 = i => i / 4.0;

                public static readonly Func<MList<string>, MList<double>> ComposedThenElevated =
                    MList.FMap(Length.Compose(DivBy4));

                public static readonly Func<MList<string>, MList<double>> ElevatedThenComposed =
                    MList.FMap(Length).Compose(MList.FMap(DivBy4));

                [Fact]
                public void Empty_list()
                {
                    var emptyList = MList<string>.Empty;

                    Assert.Equal(MList<double>.Empty, ComposedThenElevated(emptyList));
                    Assert.Equal(MList<double>.Empty, ElevatedThenComposed(emptyList));
                }

                [Property]
                public bool Non_empty_list(NonNull<string> s1, NonNull<string> s2)
                {
                    var list = CreateList(s1.Item, s2.Item);

                    var expected = CreateList(
                        Length.Compose(DivBy4)(s1.Item),
                        Length.Compose(DivBy4)(s2.Item));

                    return
                        expected.Equals(ComposedThenElevated(list))
                        &&
                        expected.Equals(ElevatedThenComposed(list));
                }
            }
        }

        public class ApplicativeTests
        {
            [Property]
            public void Pure_returns_a_single_element_list(byte b)
            {
                Assert.Equal(MList.Pure(b), MList<byte>.List(b, MList<byte>.Empty));
            }

            [Fact]
            public void Apply_with_empty_lists_returns_empty_list()
            {
                var fs = MList<Func<int, long>>.Empty;
                var xs = MList<int>.Empty;

                Assert.Equal(fs.Apply(xs), MList<long>.Empty);
            }

            [Fact]
            public void Apply_with_empty_list_of_functions_returns_empty_list()
            {
                var fs = MList<Func<int, long>>.Empty;
                var xs = MList<int>.List(1, MList<int>.Empty);

                Assert.Equal(fs.Apply(xs), MList<long>.Empty);
            }

            [Fact]
            public void Apply_with_empty_list_of_values_returns_empty_list()
            {
                var fs = MList<Func<int, long>>.List(i => i, MList<Func<int, long>>.Empty);
                var xs = MList<int>.Empty;

                Assert.Equal(fs.Apply(xs), MList<long>.Empty);
            }

            [Property]
            public bool Apply_with_list_of_one_function_and_list_of_one_value_returns_list_with_one_result(int a)
            {
                Func<int, string> f = i => i.ToString();
                var fs = MList<Func<int, string>>.List(f, MList<Func<int, string>>.Empty);
                var xs = MList<int>.List(a, MList<int>.Empty);

                return fs.Apply(xs).Equals(MList<string>.List(f(a), MList<string>.Empty));
            }

            [Fact]
            public void Apply_with_functions_and_values_returns_the_cartesian_product()
            {
                Func<int, int> f = i => i + 3;
                Func<int, int> g = i => i * 5;

                var fs = CreateList(f, g);
                var xs = CreateList(2, 3, 4);

                var expected = CreateList(
                    f(2), f(3), f(4),
                    g(2), g(3), g(4));

                var actual = fs.Apply(xs);

                Assert.Equal(expected, actual);
            }
        }

        public class ApplicativeLaws
        {
            public class FirstLaw
            {
                // Assert that:
                // pure id <*> v = v
                //   <=>
                // apply (pure id) v = v

                [Fact]
                public void Empty_list()
                {
                    var v = MList<int>.Empty;
                    var pureId = MList.Pure((Func<int, int>)Id);

                    var left = pureId.Apply(v);
                    var right = v;

                    Assert.Equal(left, right);
                }

                [Property]
                public bool Non_empty_list(byte b1, byte b2)
                {
                    var v = MList<byte>.List(b1, MList<byte>.List(b2, MList<byte>.Empty));
                    var pureId = MList.Pure((Func<byte, byte>)Id);

                    var left = pureId.Apply(v);
                    var right = v;

                    return left.Equals(right);
                }
            }

            public class SecondLaw
            {
                // pure f <*> pure x = pure (f x)
                //   <=>
                // apply (pure f) (pure x) = pure (f x)

                [Fact]
                public void Second_law()
                {
                    // pure f <*> pure x = pure (f x)
                    //   <=>
                    // apply (pure f) (pure x) = pure (f x)

                    Func<string, int> f = s => s.Length;
                    var x = "some string";

                    var left = MList.Pure(f).Apply(MList.Pure(x));
                    var right = MList.Pure(f(x));

                    Assert.Equal(left, right);
                }
            }

            public class ThirdLaw
            {
                // u <*> pure y = pure ($ y) <*> u
                // u <*> (pure y) = (pure ($ y)) <*> u
                // apply u (pure y) = apply (pure ($ y)) u
                // apply u (pure y) = apply (pure (\x -> x $ y)) u
                // apply u (pure y) = apply (pure (\x -> ($) x y)) u
                // apply u (pure y) = apply (pure (\x -> x y)) u

                [Property]
                public bool Third_law(int y)
                {
                    var u = CreateList<Func<int, int>>(
                        i => i + 1,
                        i => i * 2);

                    var left = u.Apply(MList.Pure(y));

                    //                           \x -> x y
                    Func<Func<int, int>, int> f = h => h(y);
                    var right = MList.Pure(f).Apply(u);

                    return left.Equals(right);
                }
            }

            public class FourthLaw
            {
                // u <*> (v <*> w) = pure (.) <*> u <*> v <*> w
                // apply u (apply v w) = apply (apply (apply (pure (.)) u)) v) w

                [Property]
                public bool Fourth_law(int i1, int i2, int i3)
                {
                    // u <*> (v <*> w) = pure (.) <*> u <*> v <*> w
                    // apply u (apply v w) = apply (apply (apply (pure (.)) u)) v) w

                    var u = CreateList<Func<int, int>>(x => x * 2, x => x * 3);
                    var v = CreateList<Func<int, int>>(x => x + 3, x => x + 4);
                    var w = CreateList(i1, i2, i3);

                    var pureComposeRight = MList.Pure(ComposeRight<int, int, int>());

                    var left = u.Apply(v.Apply(w));
                    var right = pureComposeRight.Apply(u).Apply(v).Apply(w);

                    return left.Equals(right);
                }
            }
        }

        public class MonadTests
        {
            [Fact]
            public void Join_of_empty_list_of_lists_is_empty_list()
            {
                var emptyListOfLists = MList<MList<int>>.Empty;

                var result = MList.Join(emptyListOfLists);

                Assert.Equal(MList<int>.Empty, result);
            }

            [Fact]
            public void Join_of_single_list_with_a_single_item_is_a_list_with_a_single_item()
            {
                var list = MList<int>.List(1, MList<int>.Empty);
                var listOfLists = MList<MList<int>>.List(list, MList<MList<int>>.Empty);

                var result = MList.Join(listOfLists);

                Assert.Equal(MList<int>.List(1, MList<int>.Empty), result);
            }

            [Fact]
            public void Join_of_single_list_with_multiple_items_is_a_list_with_multiple_items()
            {
                var list = CreateList(1, 2, 3);
                var listOfLists = CreateList(list);

                var result = MList.Join(listOfLists);

                Assert.Equal(CreateList(1, 2, 3), result);
            }

            [Fact]
            public void Join_of_multiple_lists_with_multiple_values_is_a_single_list_of_all_values()
            {
                var list1 = CreateList('a', 'b', 'c');
                var list2 = CreateList('d', 'e', 'f');
                var listOfLists = CreateList(list1, list2);

                var result = MList.Join(listOfLists);

                Assert.Equal(CreateList('a', 'b', 'c', 'd', 'e', 'f'), result);
            }

            [Fact]
            public void Bind_empty_list_returns_empty_list()
            {
                var input = MList<string>.Empty;
                Func<string, MList<int>> f = s => null;

                var actual = input.Bind(f);

                var expected = MList<int>.Empty;

                Assert.Equal(expected, actual);
            }

            [Fact]
            public void Bind_one_element_list_returns_result_of_bound_function()
            {
                var input = MList<string>.List("abc", MList<string>.Empty);
                Func<string, MList<int>> f = s => CreateList(s.Select(c => (int)c));

                var actual = input.Bind(f);

                var expected = CreateList((int)'a', 'b', 'c');

                Assert.Equal(expected, actual);
            }

            [Fact]
            public void Bind_two_element_list_returns_concatenated_results_of_bound_function()
            {
                var input = CreateList("abc", "AB");
                Func<string, MList<int>> f = s => CreateList(s.Select(c => (int)c));

                var actual = input.Bind(f);

                var expected = CreateList((int)'a', 'b', 'c', 'A', 'B');

                Assert.Equal(expected, actual);
            }
        }

        public class MonadLaws
        {
            [Property]
            public bool Left_identity(int a)
            {
                // return a >>= k  = k a
                // pure a >>= k    = k a
                // bind (pure a) k = k a

                Func<int, MList<int>> k = x => CreateList(x + 1, x + 2);

                var left = MList.Pure(a).Bind(k);
                var right = k(a);

                return left.Equals(right);
            }

            [Property]
            public bool Right_identity(byte length, byte b)
            {
                // m >>= return = m
                // bind m pure  = m

                var m = CreateList(Enumerable.Repeat(b, length).ToArray());

                var left = m.Bind(MList.Pure);
                var right = m;

                return left.Equals(right);
            }

            [Property]
            public bool Associativity(byte length, int i)
            {
                // m >>= (\x -> k x >>= h)     = (m >>= k) >>= h
                // bind m (\x -> bind (k x) h) = bind (bind m k) h

                Func<int, MList<int>> k = x => CreateList(x + 2, x + 3);
                Func<int, MList<int>> h = x => CreateList(x * 2, x * 3);
                var m = CreateList(Enumerable.Repeat(i, length).ToArray());

                var left = m.Bind(x => k(x).Bind(h));
                var right = m.Bind(k).Bind(h);

                return left.Equals(right);
            }
        }

        public class KleisliTests
        {
            // TODO
        }

        public static MList<T> CreateList<T>(IEnumerable<T> items)
        {
            return CreateList(items.ToArray());
        }

        public static MList<T> CreateList<T>(params T[] items)
        {
            var acc = MList<T>.Empty;

            foreach (var value in items.Reverse())
            {
                acc = MList<T>.List(value, acc);
            }

            return acc;
        }
    }
}