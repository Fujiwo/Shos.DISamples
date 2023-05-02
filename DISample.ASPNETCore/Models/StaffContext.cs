using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AspnetCoreMvc.Models
{
    public class StaffContext : DbContext
    {
        public ILogger<StaffContext> Logger { get; private set; }

        public virtual DbSet<Staff> Staffs { get; set; } = null!;

        //public StaffContext(DbContextOptions<StaffContext> options)
        //    : base(options)
        //{}

        public StaffContext(DbContextOptions<StaffContext> options, ILogger<StaffContext> logger)
            : base(options)
            => Logger = logger;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=20230502;Integrated Security=True;TrustServerCertificate=True");
        }
    }

    public class Staff
    {
        [Key]
        public int Number { get; set; }
        [Required]
        [MaxLength(100, ErrorMessage ="3文字～100文字の範囲で入力してください。")]
        [MinLength(3, ErrorMessage = "3文字～100文字の範囲で入力してください。")]
        public string Name { get; set; } = "";
    }
}
