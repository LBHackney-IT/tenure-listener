using FluentAssertions;
using Hackney.Shared.Person.Boundary.Response;
using Hackney.Shared.Person.Domain;
using System;
using System.Collections.Generic;
using TenureListener.Infrastructure;
using Xunit;

namespace TenureListener.Tests.Infrastructure
{
    public class PersonExtensionsTests
    {
        public static IEnumerable<object[]> Titles()
        {
            foreach (var number in Enum.GetValues(typeof(Title)))
            {
                yield return new object[] { number };
            }
        }

        private const string FirstName = "Bob";
        private const string MiddleName = "Tim";
        private const string Surname = "Roberts";

        private static PersonResponseObject CreatePerson(Title title,
            string fname = FirstName,
            string mname = MiddleName,
            string surname = Surname)
        {
            return new PersonResponseObject()
            {
                Title = title,
                FirstName = fname,
                MiddleName = mname,
                Surname = surname
            };
        }

        [Fact]
        public void FullNameTest_NullInputThrows()
        {
            Action act = () => PersonExtensions.GetFullName(null);
            act.Should().Throw<ArgumentNullException>();
        }

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_AllEmpty(Title title)
        {
            var person = CreatePerson(title, string.Empty, string.Empty, string.Empty);
            person.GetFullName().Should().Be($"{title}");
        }

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_FirstNameEmpty(Title title)
        {
            var person = CreatePerson(title, string.Empty);
            person.GetFullName().Should().Be($"{title} {MiddleName} {Surname}");
        }

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_MiddleNameEmpty(Title title)
        {
            var person = CreatePerson(title, FirstName, string.Empty);
            person.GetFullName().Should().Be($"{title} {FirstName} {Surname}");
        }

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_SurnameEmpty(Title title)
        {
            var person = CreatePerson(title, FirstName, MiddleName, string.Empty);
            person.GetFullName().Should().Be($"{title} {FirstName} {MiddleName}");
        }

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_AllPopulated(Title title)
        {
            var person = CreatePerson(title);
            person.GetFullName().Should().Be($"{title} {FirstName} {MiddleName} {Surname}");
        }
    }
}
