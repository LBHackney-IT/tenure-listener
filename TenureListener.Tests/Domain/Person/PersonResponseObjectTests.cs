using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using TenureListener.Domain.Person;
using Xunit;

namespace TenureListener.Tests.Domain.Person
{
    public class PersonResponseObjectTests
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

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_AllEmpty(Title title)
        {
            var person = CreatePerson(title, string.Empty, string.Empty, string.Empty);
            person.FullName.Should().Be($"{title}");
        }

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_FirstNameEmpty(Title title)
        {
            var person = CreatePerson(title, string.Empty);
            person.FullName.Should().Be($"{title} {MiddleName} {Surname}");
        }

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_MiddleNameEmpty(Title title)
        {
            var person = CreatePerson(title, FirstName, string.Empty);
            person.FullName.Should().Be($"{title} {FirstName} {Surname}");
        }

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_SurnameEmpty(Title title)
        {
            var person = CreatePerson(title, FirstName, MiddleName, string.Empty);
            person.FullName.Should().Be($"{title} {FirstName} {MiddleName}");
        }

        [Theory]
        [MemberData(nameof(Titles))]
        public void FullNameTest_AllPopulated(Title title)
        {
            var person = CreatePerson(title);
            person.FullName.Should().Be($"{title} {FirstName} {MiddleName} {Surname}");
        }
    }
}
