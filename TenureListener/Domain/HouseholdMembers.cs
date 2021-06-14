using System;

namespace TenureListener.Domain
{
    public class HouseholdMembers
    {
        public Guid Id { get; set; }

        public HouseholdMembersType Type { get; set; }

        public string FullName { get; set; }

        public bool IsResponsible { get; set; }

        public DateTime DateOfBirth { get; set; }

        public PersonTenureType PersonTenureType { get; set; }
    }
}
