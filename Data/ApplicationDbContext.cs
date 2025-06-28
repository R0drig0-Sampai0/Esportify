using Esportify.Models;
using Microsoft.EntityFrameworkCore;

namespace Esportify.Data
{
    public class EsportifyContext : DbContext
    {
        public EsportifyContext(DbContextOptions<EsportifyContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Game> Games { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            /*************** Relação 1:1 ***************/
            /// Uma equipa tem um líder.
            modelBuilder.Entity<Team>()
                .HasOne(t => t.Leader)
                .WithMany()
                .HasForeignKey(t => t.LeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne<UserProfile>()
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            /*************** Relação 1:N ***************/

            /// Um jogo pode ter vários torneios.
            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Game)
                .WithMany(g => g.Tournaments)
                .HasForeignKey(t => t.GameId);

            /// Um user pode criar vários torneios.
            modelBuilder.Entity<Tournament>()
                .HasOne(t => t.Creator)
                .WithMany()
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            /*************** Relação N:N ***************/

            /// Uma equipa pode ter vários membros e um membro pode pertencer a várias equipas.
            modelBuilder.Entity<TeamMember>()
                .HasKey(tm => new { tm.TeamId, tm.UserId });

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.Team)
                .WithMany(t => t.TeamMembers)
                .HasForeignKey(tm => tm.TeamId);

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.User)
                .WithMany(u => u.TeamMemberships)
                .HasForeignKey(tm => tm.UserId);

            /// Um torneio pode ter várias equipas registadas e uma equipa pode estar registada em vários torneios.
            modelBuilder.Entity<Registration>()
                .HasKey(r => new { r.TournamentId, r.TeamId });

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Team)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TeamId);

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Tournament)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TournamentId);

        }
    }

}
