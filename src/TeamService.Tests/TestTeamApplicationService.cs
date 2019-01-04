using System.Linq;
using System.Threading.Tasks;
using DFDS.TeamService.Tests.Builders;
using DFDS.TeamService.Tests.TestDoubles;
using DFDS.TeamService.WebApi.Models;
using Xunit;

namespace DFDS.TeamService.Tests
{
    public class TestTeamApplicationService
    {
        [Fact]
        public async Task expected_member_is_added_to_team()
        {
            var team = new TeamBuilder().Build();

            var sut = new TeamApplicationServiceBuilder()
                .WithTeamRepository(new StubTeamRepository(team))
                .Build();

            var stubMemberEmail = "foo@bar.com";
            
            await sut.JoinTeam(team.Id, stubMemberEmail);
            
            Assert.Equal(new[]{stubMemberEmail}, team.Members.Select(x => x.Email));
        }

        [Fact]
        public async Task adding_the_same_member_to_a_team_multiple_times_yields_single_membership()
        {
            var team = new TeamBuilder().Build();

            var sut = new TeamApplicationServiceBuilder()
                .WithTeamRepository(new StubTeamRepository(team))
                .Build();

            var stubMemberEmail = "foo@bar.com";
            
            await sut.JoinTeam(team.Id, stubMemberEmail);
            await sut.JoinTeam(team.Id, stubMemberEmail);
            
            Assert.Equal(new[]{stubMemberEmail}, team.Members.Select(x => x.Email));
        }

        [Fact]
        public async Task expected_member_is_removed_from_team()
        {
            var stubMemberEmail = "foo@bar.com";

            var team = new TeamBuilder()
                .WithMembers(stubMemberEmail)
                .Build();

            var sut = new TeamApplicationServiceBuilder()
                .WithTeamRepository(new StubTeamRepository(team))
                .Build();

            await sut.LeaveTeam(team.Id, stubMemberEmail);

            Assert.Empty(team.Members);
        }

        [Fact]
        public async Task removing_non_existing_member_from_a_team_does_not_throw_exception()
        {
            var team = new TeamBuilder().Build();

            var sut = new TeamApplicationServiceBuilder()
                .WithTeamRepository(new StubTeamRepository(team))
                .Build();

            var nonExistingMemberEmail = "foo@bar.com";
            var thrownException = await Record.ExceptionAsync(() => sut.LeaveTeam(team.Id, nonExistingMemberEmail));

            Assert.Null(thrownException);
        }
    }

    public class TeamApplicationServiceBuilder
    {
        private ITeamRepository _teamRepository;
        private IRoleService _roleService;

        public TeamApplicationServiceBuilder()
        {
            _teamRepository = Dummy.Of<ITeamRepository>();
            _roleService = Dummy.Of<IRoleService>();
        }

        public TeamApplicationServiceBuilder WithTeamRepository(ITeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
            return this;
        }

        public TeamApplicationServiceBuilder WithRoleService(IRoleService roleService)
        {
            _roleService = roleService;
            return this;
        }
        
        public TeamApplicationService Build()
        {
            return new TeamApplicationService(
                    teamRepository: _teamRepository,
                    roleService: _roleService
                );
        }
    }
}