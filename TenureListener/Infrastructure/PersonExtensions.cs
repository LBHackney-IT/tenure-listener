using Hackney.Shared.Person.Boundary.Response;
using System;

namespace TenureListener.Infrastructure
{
    public static class PersonExtensions
    {
        public static string GetFullName(this PersonResponseObject person)
        {
            if (person is null) throw new ArgumentNullException(nameof(person));

            string firstName = FormatNamePart(person.FirstName);
            string middleName = FormatNamePart(person.MiddleName);
            string surname = FormatNamePart(person.Surname);
            return $"{person.Title}{firstName}{middleName}{surname}";
        }

        private static string FormatNamePart(string part)
        {
            return string.IsNullOrEmpty(part) ? string.Empty : $" {part}";
        }
    }
}
