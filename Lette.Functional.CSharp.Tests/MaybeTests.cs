﻿using System;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using static Lette.Functional.CSharp.Functional;

namespace Lette.Functional.CSharp.Tests
{
    public class MaybeTests
    {
        public class FactoryMethodTests
        {
            [Fact]
            public void Disallows_null_values_of_reference_types()
            {
                Assert.Throws<ArgumentNullException>("value", () => Maybe<string>.Just(null));
            }

            [Fact]
            public void Allows_non_null_values_of_reference_types()
            {
                var ex = Record.Exception(() => Maybe<string>.Just(""));

                Assert.Null(ex);
            }

            [Fact]
            public void Disallows_null_values_of_nullables()
            {
                // EXPERIMENTAL: Is this really expected?

                Assert.Throws<ArgumentNullException>("value", () => Maybe<int?>.Just(null));
            }

            [Fact]
            public void Allows_non_null_values_of_nullables()
            {
                // EXPERIMENTAL: Is this really expected?

                var ex = Record.Exception(() => Maybe<int?>.Just(0));

                Assert.Null(ex);
            }

            [Fact]
            public void Null_reference_is_converted_to_Nothing()
            {
                string nil = null;

                var result = nil.ToMaybe();

                Assert.Equal(Maybe<string>.Nothing, result);
            }

            [Property]
            public void Non_null_reference_is_converted_to_Just(NonNull<string> value)
            {
                var result = value.Item.ToMaybe();

                Assert.Equal(Maybe<string>.Just(value.Item), result);
            }

            [Property]
            public void Nullable_with_value_is_converted_to_Just_of_underlying_type(int value)
            {
                int? nullableValue = value;

                var result = nullableValue.ToMaybe();

                Assert.Equal(Maybe<int>.Just(value), result);
            }

            [Fact]
            public void Nullable_without_value_is_converted_to_Nothing_of_underlying_type()
            {
                int? nil = null;

                var result = nil.ToMaybe();

                Assert.Equal(Maybe<int>.Nothing, result);
            }
        }

        public class EqualityTests
        {
            [Fact]
            public void Nothing_equals_nothing()
            {
                Assert.Equal(Maybe<int>.Nothing, Maybe<int>.Nothing);
            }

            [Fact]
            public void Two_justs_with_same_value_are_equal()
            {
                Assert.Equal(Maybe<int>.Just(1), Maybe<int>.Just(1));
            }

            [Fact]
            public void Nothing_is_not_equal_to_just_anything()
            {
                Assert.NotEqual(Maybe<string>.Nothing, Maybe<string>.Just("anything"));
            }

            [Fact]
            public void Two_justs_with_different_values_are_not_equal()
            {
                Assert.NotEqual(Maybe<bool>.Just(true), Maybe<bool>.Just(false));
            }
        }

        public class PatternMatchingTests
        {
            [Fact]
            public void Invokes_function_for_just()
            {
                var maybe = Maybe<int>.Just(1);

                var result = maybe.Match(x => x + 1, () => -1);

                Assert.Equal(2, result);
            }

            [Fact]
            public void Invokes_function_for_nothing()
            {
                var maybe = Maybe<int>.Nothing;

                var result = maybe.Match(x => x + 1, () => -1);

                Assert.Equal(-1, result);
            }

            [Fact]
            public void Invokes_action_for_just()
            {
                var maybe = Maybe<int>.Just(1);
                var result = 0;

                maybe.Match(x => { result = x + 1; }, () => { result = -1; });

                Assert.Equal(2, result);
            }

            [Fact]
            public void Invokes_action_for_nothing()
            {
                var maybe = Maybe<int>.Nothing;
                var result = 0;

                maybe.Match(x => { result = x + 1; }, () => { result = -1; });

                Assert.Equal(-1, result);
            }
        }

        public class FunctorTests
        {
            public static readonly Func<int, int> AddOne = x => x + 1;
            public static readonly Func<Maybe<int>, Maybe<int>> ElevatedAddOne = Maybe.FMap(AddOne);

            [Fact]
            public void Elevated_function_maps_nothing_to_nothing()
            {
                Assert.Equal(Maybe<int>.Nothing, ElevatedAddOne(Maybe<int>.Nothing));
            }

            [Fact]
            public void Elevated_function_maps_value_to_new_value()
            {
                Assert.Equal(Maybe<int>.Just(2), ElevatedAddOne(Maybe<int>.Just(1)));
            }

            [Property]
            public bool Elevated_constant_function_maps_nothing_to_nothing(int value)
            {
                var constMap = value.FConstMap<string, int>();

                return constMap(Maybe<string>.Nothing).Equals(Maybe<int>.Nothing);
            }

            [Property]
            public bool Elevated_constant_function_maps_just_anything_to_just_constant_value(NonNull<string> s, int value)
            {
                var constMap = value.FConstMap<string, int>();

                return constMap(Maybe<string>.Just(s.Item)).Equals(Maybe<int>.Just(value));
            }
        }

        public class FunctorLaws
        {
            public class FirstLaw
            {
                // Assert that:
                // fmap id = id

                private static readonly Func<Maybe<int>, Maybe<int>> ElevatedId = Maybe.FMap((Func<int,int>)Id);

                [Fact]
                public void Nothing()
                {
                    var nothing = Maybe<int>.Nothing;

                    Assert.Equal(
                        Id(nothing),
                        ElevatedId(nothing));
                }

                [Property]
                public bool Just(int x)
                {
                    var just = Maybe<int>.Just(x);

                    return Id(just).Equals(ElevatedId(just));
                }
            }

            public class SecondLaw
            {
                // Assert that:
                // fmap(g.h) = (fmap g) . (fmap h)

                public static readonly Func<string, int> Length = s => s.Length;
                public static readonly Func<int, double> DivBy4 = i => i / 4.0;

                public static readonly Func<Maybe<string>, Maybe<double>> ComposedThenElevated =
                    Maybe.FMap(Length.Compose(DivBy4));

                public static readonly Func<Maybe<string>, Maybe<double>> ElevatedThenComposed =
                    Maybe.FMap(Length).Compose(Maybe.FMap(DivBy4));

                [Fact]
                public void Nothing()
                {
                    var nothing = Maybe<string>.Nothing;

                    Assert.Equal(Maybe<double>.Nothing, ComposedThenElevated(nothing));
                    Assert.Equal(Maybe<double>.Nothing, ElevatedThenComposed(nothing));
                }

                [Property]
                public bool Just(NonNull<string> s)
                {
                    var just = Maybe<string>.Just(s.Item);

                    return ComposedThenElevated(just).Equals(ElevatedThenComposed(just));
                }
            }
        }

        public class ApplicativeTests
        {
            [Property]
            public bool Pure_returns_just_a_value(int i)
            {
                return Maybe.Pure(i).Equals(Maybe<int>.Just(i));
            }

            public static readonly Func<int, int> IntDivBy2 = x => x / 2;
            public static readonly Maybe<Func<int, int>> ElevatedFunc = Maybe<Func<int, int>>.Just(IntDivBy2);
            public static readonly Maybe<Func<int, int>> NothingFunc = Maybe<Func<int, int>>.Nothing;

            [Property]
            public bool Invoking_applied_elevated_function_with_just_a_value_returns_just_function_result(int i)
            {
                Func<Maybe<int>, Maybe<int>> appliedElevatedFunc = ElevatedFunc.Apply();

                return appliedElevatedFunc(Maybe<int>.Just(i)).Equals(Maybe<int>.Just(IntDivBy2(i)));
            }

            [Fact]
            public void Invoking_applied_elevated_function_with_nothing_returns_nothing()
            {
                Func<Maybe<int>, Maybe<int>> appliedElevatedFunc = ElevatedFunc.Apply();

                Assert.Equal(Maybe<int>.Nothing, appliedElevatedFunc(Maybe<int>.Nothing));
            }

            [Property]
            public bool Invoking_applied_nothing_function_with_just_a_value_returns_nothing(int i)
            {
                Func<Maybe<int>, Maybe<int>> appliedNothingFunc = NothingFunc.Apply();

                return appliedNothingFunc(Maybe<int>.Just(i)).Equals(Maybe<int>.Nothing);
            }

            [Fact]
            public void Invoking_applied_nothing_function_with_nothing_returns_nothing()
            {
                Func<Maybe<int>, Maybe<int>> appliedNothingFunc = NothingFunc.Apply();

                Assert.Equal(Maybe<int>.Nothing, appliedNothingFunc(Maybe<int>.Nothing));
            }
        }

        public class ApplicativeLaws
        {
            [Fact]
            public void First_law_for_nothing()
            {
                // Assert that:
                // pure id <*> v = v
                //   <=>
                // apply (pure id) v = v

                var v = Maybe<int>.Nothing;
                var pureId = Maybe.Pure((Func<int, int>)Id);

                var left = pureId.Apply(v);
                var right = v;

                Assert.Equal(left, right);
            }

            [Property]
            public bool First_law_for_just(byte b)
            {
                var v = Maybe<byte>.Just(b);
                var pureId = Maybe.Pure((Func<byte, byte>)Id);

                var left = pureId.Apply(v);
                var right = v;

                return left.Equals(right);
            }

            [Fact]
            public void Second_law()
            {
                // pure f <*> pure x = pure (f x)
                //   <=>
                // apply (pure f) (pure x) = pure (f x)

                Func<string, int> f = s => s.Length;
                var x = "some string";

                var left = Maybe.Pure(f).Apply(Maybe.Pure(x));
                var right = Maybe.Pure(f(x));

                Assert.Equal(left, right);
            }

            [Property]
            public bool Third_law(int y)
            {
                // u <*> pure y = pure ($ y) <*> u
                // u <*> (pure y) = (pure ($ y)) <*> u
                // apply u (pure y) = apply (pure ($ y)) u
                // apply u (pure y) = apply (pure (\x -> x $ y)) u
                // apply u (pure y) = apply (pure (\x -> ($) x y)) u
                // apply u (pure y) = apply (pure (\x -> x y)) u

                Maybe<Func<int, int>> u = Maybe<Func<int, int>>.Just(i => i + 1);

                var left = u.Apply(Maybe.Pure(y));

                //                           \x -> x y
                Func<Func<int, int>, int> f = h => h(y);
                var right = Maybe.Pure(f).Apply(u);

                return left.Equals(right);
            }

            [Property]
            public bool Fourth_law(int i)
            {
                // u <*> (v <*> w) = pure (.) <*> u <*> v <*> w
                // apply u (apply v w) = apply (apply (apply (pure (.)) u)) v) w

                var u = Maybe<Func<int, int>>.Just(x => x * 2);
                var v = Maybe<Func<int, int>>.Just(x => x + 3);
                var w = Maybe<int>.Just(i);

                var left = u.Apply(v.Apply(w));

                // (.)
                // \g h x -> (g . h) x
                //            g(  h( x ))

                var right = Maybe.Pure(ComposeRight<int, int, int>()).Apply(u).Apply(v).Apply(w);

                return left.Equals(right);
            }
        }

        public class MonadTests
        {
            [Fact]
            public void Bind_nothing_returns_nothing()
            {
                var m = Maybe<int>.Nothing;

                var result = m.Bind(x => Maybe<int>.Just(1));

                Assert.Equal(Maybe<int>.Nothing, result);
            }

            [Fact]
            public void Bind_nothing_does_not_invoke_function()
            {
                var m = Maybe<int>.Nothing;
                var wasCalled = false;

                Func<int, Maybe<int>> f = x =>
                {
                    wasCalled = true;
                    return Maybe<int>.Nothing;
                };

                m.Bind(f);

                Assert.False(wasCalled);
            }

            [Fact]
            public void Bind_just_invokes_function()
            {
                var m = Maybe<int>.Just(1);
                var wasCalled = false;

                Func<int, Maybe<int>> f = x =>
                {
                    wasCalled = true;
                    return Maybe<int>.Nothing;
                };

                m.Bind(f);

                Assert.True(wasCalled);
            }

            [Fact]
            public void Bind_just_returns_value_from_function()
            {
                var m = Maybe<int>.Just(1);
                var returnValue = Maybe<int>.Just(2);

                Func<int, Maybe<int>> f = _ => returnValue;

                var result = m.Bind(f);

                Assert.Equal(returnValue, result);
            }

            [Fact]
            public void Join_nothing_returns_nothing()
            {
                Assert.Equal(Maybe<int>.Nothing, Maybe.Join(Maybe<Maybe<int>>.Nothing));
            }

            [Fact]
            public void Join_just_nothing_returns_nothing()
            {
                Assert.Equal(Maybe<int>.Nothing, Maybe.Join(Maybe<Maybe<int>>.Just(Maybe<int>.Nothing)));
            }

            [Fact]
            public void Join_just_just_returns_just()
            {
                Assert.Equal(Maybe<int>.Just(1), Maybe.Join(Maybe<Maybe<int>>.Just(Maybe<int>.Just(1))));
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

                Func<int, Maybe<int>> k = x => Maybe<int>.Just(x + 1);

                var left = Maybe.Pure(a).Bind(k);
                var right = k(a);

                return left.Equals(right);
            }

            [Property]
            public bool Right_identity(byte b)
            {
                // m >>= return = m
                // bind m pure  = m

                var m = Maybe<byte>.Just(b);

                var left = m.Bind(Maybe.Pure);
                var right = m;

                return left.Equals(right);
            }

            [Fact]
            public void Right_identity_Nothing()
            {
                // m >>= return = m
                // bind m pure  = m

                var m = Maybe<byte>.Nothing;

                var left = m.Bind(Maybe.Pure);
                var right = m;

                Assert.Equal(left, right);
            }

            [Property]
            public bool Associativity(int i)
            {
                // m >>=  (\x -> k x >>= h)    = (m >>= k) >>= h
                // bind m (\x -> bind (k x) h) = bind (bind m k) h

                Func<int, Maybe<int>> k = x => Maybe<int>.Just(x + 3);
                Func<int, Maybe<int>> h = x => Maybe<int>.Just(x * 2);
                var m = Maybe<int>.Just(i);

                var left = m.Bind(x => k(x).Bind(h));
                var right = m.Bind(k).Bind(h);

                return left.Equals(right);
            }

            [Fact]
            public void Associativity_Nothing()
            {
                // m >>=  (\x -> k x >>= h)    = (m >>= k) >>= h
                // bind m (\x -> bind (k x) h) = bind (bind m k) h

                Func<int, Maybe<int>> k = x => Maybe<int>.Just(x + 3);
                Func<int, Maybe<int>> h = x => Maybe<int>.Just(x * 2);
                var m = Maybe<int>.Nothing;

                var left = m.Bind(x => k(x).Bind(h));
                var right = m.Bind(k).Bind(h);

                Assert.Equal(left, right);
            }
        }

        public class KleisliTests
        {
            [Fact]
            public void Composition_returns_combined_result()
            {
                Func<int, Maybe<int>> f = i => Maybe<int>.Just(i + 3);
                Func<int, Maybe<int>> g = i => Maybe<int>.Just(i * 2);

                var h = f.KBind(g);

                Assert.Equal(Maybe<int>.Just((1 + 3) * 2), h(1));
            }

            [Fact]
            public void Returns_nothing_when_first_function_produces_nothing()
            {
                Func<int, Maybe<int>> f = i => Maybe<int>.Nothing;
                Func<int, Maybe<int>> g = i => Maybe<int>.Just(i * 2);

                var h = f.KBind(g);

                Assert.Equal(Maybe<int>.Nothing, h(1));
            }

            [Fact]
            public void Second_function_is_not_invoked_when_first_function_produces_nothing()
            {
                var wasCalled = false;

                Func<int, Maybe<int>> f = i => Maybe<int>.Nothing;
                Func<int, Maybe<int>> g = i =>
                {
                    wasCalled = true;
                    return null;
                };

                f.KBind(g)(0);

                Assert.False(wasCalled);
            }

            [Fact]
            public void Returns_nothing_when_second_function_produces_nothing()
            {
                Func<int, Maybe<int>> f = i => Maybe<int>.Just(i + 3);
                Func<int, Maybe<int>> g = i => Maybe<int>.Nothing;

                var h = f.KBind(g);

                Assert.Equal(Maybe<int>.Nothing, h(1));
            }
        }
    }
}
