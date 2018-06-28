using System;

namespace Lette.Functional.CSharp.Railway
{
    public class UseCase
    {
        public static readonly Func<string> Execute =
            () =>
            {
                var input = ("        Your Name        ", "Your.Name@EMAIL.com");

                var receiveRequest = ReceiveRequest.ToResult("Could not receive request.");
                var validate = ResultExtensions.Bind(ValidateRequest2);
                var canonicalizeName = ResultExtensions.Map(CanonicalizeName);
                var canonicalizeEmail = ResultExtensions.Map(CanonicalizeEmail);
                var updateDb = ResultExtensions.Map(UpdateDbFromRequest);
                var sendEmail = ResultExtensions.TryMap(SendEmail.CallWithEmail());

                //var workFlow = receiveRequest
                //    .Compose(validate)
                //    .Compose(canonicalizeName)
                //    .Compose(canonicalizeEmail)
                //    .Compose(updateDb)
                //    .Compose(sendEmail)
                //    .Compose(CreateMessage);

                //return input.ForwardPipe(workFlow);
                //return workFlow.Invoke(input);
                //return workFlow(input);

                return input
                    .ForwardPipe(receiveRequest)
                    .ForwardPipe(validate)
                    .ForwardPipe(canonicalizeName)
                    .ForwardPipe(canonicalizeEmail)
                    .ForwardPipe(updateDb)
                    .ForwardPipe(sendEmail)
                    .ForwardPipe(CreateMessage);
            };

        private static readonly Func<(string, string), Maybe<Request>> ReceiveRequest =
            data =>
                Maybe<Request>.Just(new Request(data));

        private static readonly Func<(string, string), Maybe<Request>> FailReceiveRequest =
            _ =>
                Maybe<Request>.Nothing;

        private static readonly Func<Request, Result<Request>> ValidateRequest =
            request => request
                .ForwardPipe(
                    NameNotBlank
                        .Compose(ResultExtensions.Bind(NameNotTooLong))
                        .Compose(ResultExtensions.Bind(EmailNotBlank))
                );

        private static readonly Func<Request, Result<Request>> NameNotBlank =
            request =>
                !string.IsNullOrWhiteSpace(request.Name)
                    ? Result<Request>.Ok(request)
                    : Result<Request>.Error("Name is required.");

        private static readonly Func<Request, Result<Request>> NameNotTooLong =
            request =>
                request.Name.Trim().Length <= 50
                    ? Result<Request>.Ok(request)
                    : Result<Request>.Error("Name must be 50 characters or less.");

        private static readonly Func<Request, Result<Request>> EmailNotBlank =
            request =>
                !string.IsNullOrWhiteSpace(request.Email)
                    ? Result<Request>.Ok(request)
                    : Result<Request>.Error("Email is required.");

        private static readonly Func<Request, Result<Request>> ValidateRequest2 =
            NameNotBlank
                .Compose(ResultExtensions.Bind(NameNotTooLong))
                .Compose(ResultExtensions.Bind(EmailNotBlank));

        private static readonly Func<Request, Request> CanonicalizeName =
            request => request.WithName(request.Name.Trim());

        private static readonly Func<Request, Request> CanonicalizeEmail =
            request => request.WithEmail(request.Email.ToLowerInvariant().Trim());

        private static readonly Action<Request> UpdateDbFromRequest =
            request =>
            {
                // do nothing
            };

        private static readonly Action<string> SendEmail =
            email =>
            {
                if (email.StartsWith("fail")) throw new InvalidOperationException("Sending mail failed!");
            };

        private static readonly Func<Result<Request>, string> CreateMessage =
            request => request.Match(
                ok: obj => $"DONE! Name: {obj.Name}, Email: {obj.Email}",
                error: msg => $"FAIL! Message: {msg}");
    }
}