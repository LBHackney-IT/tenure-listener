using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using TenureListener.Domain;
using Xunit;

namespace TenureListener.Tests.Domain
{
    public class TenureTypesTests
    {
        private static List<string> AllCodes => new List<string>(new[]
        {
            "LTA", "COM", "DEC", "FRE", "FRS",
            "INT", "LEA", "LHS", "LTA", "MPA",
            "NON", "PVG", "RSL", "RTM", "SEC",
            "SHO", "SLL", "SPS", "SSE", "TAF",
            "TBB", "TGA", "THL", "THO", "TLA",
            "TPL", "TRA"
        });

        [Fact]
        public void GetPersonTenureTypeTestNotResponsibleHM()
        {
            var codes = AllCodes.Except(new[] { "TGA" });
            foreach (var code in codes)
            {
                var tt = new TenureType { Code = code };
                tt.GetPersonTenureType(false).Should().Be(PersonTenureType.HouseholdMember);
            }
        }

        [Fact]
        public void GetPersonTenureTypeTestNotResponsibleOccupant()
        {
            var tt = new TenureType { Code = "TGA" };
            tt.GetPersonTenureType(false).Should().Be(PersonTenureType.Occupant);

        }

        [Fact]
        public void GetPersonTenureTypeTestResponsibleFreeholder()
        {
            foreach (var code in TenureTypes.FreeholderCodes)
            {
                var tt = new TenureType { Code = code };
                tt.GetPersonTenureType(true).Should().Be(PersonTenureType.Freeholder);
            }
        }

        [Fact]
        public void GetPersonTenureTypeTestResponsibleLeaseholder()
        {
            foreach (var code in TenureTypes.LeaseholderCodes)
            {
                var tt = new TenureType { Code = code };
                tt.GetPersonTenureType(true).Should().Be(PersonTenureType.Leaseholder);
            }
        }

        [Fact]
        public void GetPersonTenureTypeTestResponsibleTenant()
        {
            var codes = AllCodes.Except(TenureTypes.FreeholderCodes.Union(TenureTypes.LeaseholderCodes));
            foreach (var code in codes)
            {
                var tt = new TenureType { Code = code };
                tt.GetPersonTenureType(true).Should().Be(PersonTenureType.Tenant);
            }
        }
    }
}
