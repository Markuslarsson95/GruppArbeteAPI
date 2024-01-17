using Infrastructure.DatabaseContext;
using Infrastructure.Repository.AuthorRepository;
using Infrastructure.Repository.UserRepository;
using Infrastructure.Repository.BookRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddDbContext<HarryPotterContext>(options =>
            {
                var connectionString = "Server=(local)\\SQLEXPRESS;Database=HarryPotter;Trusted_Connection=True;TrustServerCertificate=true";
                options.UseSqlServer(connectionString);
            });
            services.AddScoped<IAuthorRepository, AuthorRepository>();
            services.AddScoped<IUserRepository, UserRepository>();


            return services;
        }
    }
}
