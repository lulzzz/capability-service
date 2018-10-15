using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFDS.TeamService.WebApi.Features.Teams.Domain.Models;
using DFDS.TeamService.WebApi.Features.Teams.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DFDS.TeamService.WebApi.Features.Teams.Infrastructure.Persistence
{
    public class DbTeamRepository : ITeamRepository
    {
        private readonly TeamServiceDbContext _context;

        public DbTeamRepository(TeamServiceDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Team>> GetAll()
        {
            return await _context.Teams
                .Include(x => x.Memberships).ThenInclude(x => x.User)
                .ToListAsync();
        }

        public async Task<Team> GetById(Guid id)
        {
            return await _context.Teams.SingleOrDefaultAsync(x => x.Id == id);
        }

        public async Task Add(Team team)
        {
            await _context.Teams.AddAsync(team);
        }

        public Task<bool> Exists(Guid id)
        {
            return _context.Teams.AnyAsync(x => x.Id == id);
        }

        public Task<bool> ExistsWithName(string name)
        {
            return _context.Teams.AnyAsync(x => x.Name == name);
        }
    }
}