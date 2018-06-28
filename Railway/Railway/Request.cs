namespace Lette.Functional.CSharp.Railway
{
    public class Request
    {
        public Request((string name, string email) input) : this(input.name, input.email) {}

        public Request(string name, string email)
        {
            Name = name;
            Email = email;
        }

        public string Name { get; }
        public string Email { get; }

        public Request WithName(string newName) => new Request(newName, Email);
        public Request WithEmail(string newEmail) => new Request(Name, newEmail);
    }
}