using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using SIISMinimalAPI.Data;

namespace SIISMinimalAPI.Features.OnBoarding
{
    public class OnBoardingHandler(AppDbContext context) : IOnBoadringService
    {
        private readonly AppDbContext _context = context;
        public async Task CreateOnBoarding(OnBoardingDto onBoardingDto, CancellationToken ct)
        {
            try
            {
                var existingStud = await _context.Students.AsNoTracking()
                .FirstOrDefaultAsync( 
                    t => t.Email.ToLower() == onBoardingDto.Student.Email.ToLower(), 
                    ct);
                if (existingStud is not null)
                {
                    throw new DuplicateNameException("Student with this email is already registered");
                }

                var newOnboadingUser = OnBoardingEntityMapper.ToStudentModel(onBoardingDto);
                await _context.AddAsync(newOnboadingUser);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
