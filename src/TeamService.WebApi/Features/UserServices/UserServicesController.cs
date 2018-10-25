using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DFDS.TeamService.WebApi.Features.Teams.Domain.Models;
using DFDS.TeamService.WebApi.Features.Teams.Domain.Repositories;
using DFDS.TeamService.WebApi.Features.UserServices.model;
using Microsoft.AspNetCore.Mvc;

namespace DFDS.TeamService.WebApi.Features.MyServices
{
    public class UserServicesController: ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ITeamRepository _teamRepository;
        
        public UserServicesController(
            IUserRepository userRepository, 
            ITeamRepository teamRepository
        )
        {
            _userRepository = userRepository;
            _teamRepository = teamRepository;
        }
        
        [HttpGet("api/users/{userId}/services")]
        public async Task<ActionResult<TeamsDTO>> GetServices(string userId)
        {
            var user = await _userRepository.GetById(userId);

            if (user == null)
            {
                return new ActionResult<TeamsDTO>(NotFound());
            }

            // TODO find membership in the proper way
//            var teamsWithUser = (await _teamRepository).GetAll().Where(t => t.Members.Any(u => u.Id == userId));
            var teamsWithUser = await _teamRepository.GetAll();
            
            if (teamsWithUser.Any() == false)
            {
                return new TeamsDTO{Items = new TeamDTO[0]};
            }


            var teamsResponse = new TeamsDTO
            {
                Items = teamsWithUser.Select(t =>
                    CreateTeam(t)
                )
            };
            


            return teamsResponse;
        }

        
        public TeamDTO CreateTeam(Team team)
        {
            var teamDTO = new TeamDTO
            {
                Name = team.Name,
                Department = team.Department,
                Services = CreateServices(team)
            };


            return teamDTO;
        }

        
        public IEnumerable<ServiceDTO> CreateServices(Team team)
        {
            var services = new List<ServiceDTO>();
            services.Add(CreateAwsConsoleService(team.Id));

            return services;
        } 
        
        
        public ServiceDTO CreateAwsConsoleService(Guid teamId)
        {
            return new ServiceDTO
            {
                Name = "AWS Console",
                Location = $"http://localhost:8080/api/teams/{teamId}/aws/console-url"
            };
        }
    }
}