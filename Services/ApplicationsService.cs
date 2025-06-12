using Microsoft.EntityFrameworkCore;
using RBACapi.Data;
using RBACapi.Models;
using RBACapi.Services.Interfaces;

namespace RBACapi.Services
{
    public class ApplicationsService : IApplicationsService
    {
        private readonly ApplicationDbContext _context;

        public ApplicationsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CM_APPLICATIONS>> GetAllApplicationsAsync()
        {
            return await _context.Applications
                .Select(app => new CM_APPLICATIONS
                {
                    APP_CODE = app.APP_CODE,
                    NAME = app.NAME,
                    TITLE = app.TITLE,
                    DESC = app.DESC,
                    ACTIVE = app.ACTIVE,
                    BASE_URL = app.BASE_URL,
                    LOGIN_URL = app.LOGIN_URL,
                    CREATED_BY = app.CREATED_BY,
                    CREATED_DATETIME = app.CREATED_DATETIME,
                    UPDATED_BY = app.UPDATED_BY,
                    UPDATED_DATETIME = app.UPDATED_DATETIME,
                })
                .ToListAsync();
        }

        public async Task<CM_APPLICATIONS?> GetApplicationByCodeAsync(string code)
        {
            return await _context.Applications.FindAsync(code);
        }

        public async Task CreateApplicationAsync(CM_APPLICATIONS app)
        {
            _context.Applications.Add(app);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateApplicationAsync(CM_APPLICATIONS app)
        {
            var existing = await _context.Applications.FindAsync(app.APP_CODE);
            if (existing == null) return false;

            _context.Entry(existing).CurrentValues.SetValues(app);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteApplicationAsync(string code)
        {
            var existing = await _context.Applications.FindAsync(code);
            if (existing == null) return false;

            _context.Applications.Remove(existing);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}