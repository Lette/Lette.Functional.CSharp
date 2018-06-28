using System;

namespace Lette.Functional.CSharp.Railway
{
    public static class ActionExtensions
    {
        public static Func<T, T> Passthrough<T>(this Action<T> action)
        {
            return input =>
            {
                action(input);
                return input;
            };
        }

        public static Action<Request> CallWithEmail(this Action<string> emailAction)
        {
            return request => emailAction(request.Email);
        }
    }
}