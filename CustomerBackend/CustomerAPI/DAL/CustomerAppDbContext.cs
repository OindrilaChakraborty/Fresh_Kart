using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CustomerAPI.DAL
{
    public partial class CustomerAppDbContext : DbContext
    {
        public CustomerAppDbContext()
        {
        }

        public CustomerAppDbContext(DbContextOptions<CustomerAppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TLoginMst> TLoginMsts { get; set; }
        public virtual DbSet<TUserMst> TUserMsts { get; set; }
        public virtual DbSet<TCategoryMst> TCategoryMsts { get; set; }
        public virtual DbSet<TProductMst> TProductMsts { get; set; }
        public virtual DbSet<TOrderMst> TOrderMsts { get; set; }
        public virtual DbSet<TOrderProductMapping> TOrderProductMappings { get; set; }
        public virtual DbSet<TReportGenServ> TReportGenServs { get; set; }
        public virtual DbSet<TNotificationMst> TNotificationMsts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var configBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var configSection = configBuilder.GetSection("ConnectionStrings");
            var connectionString = configSection["DefaultConnection"] ?? null;
            optionsBuilder.UseSqlServer(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TLoginMst>(entity =>
            {
                entity.HasKey(e => e.login_id);
                entity.ToTable("T_Login_Mst");
                entity.Property(e => e.email_id).HasMaxLength(100).IsRequired();
                entity.Property(e => e.password).IsRequired();
                entity.Property(e => e.user_type).HasMaxLength(20);
            });

            modelBuilder.Entity<TUserMst>(entity =>
            {
                entity.HasKey(e => e.user_id);
                entity.ToTable("T_User_Mst");
                entity.Property(e => e.user_name).HasMaxLength(100);
                entity.Property(e => e.city).HasMaxLength(50);
                entity.Property(e => e.pincode).HasMaxLength(10);
                entity.Property(e => e.gender).HasMaxLength(10);

                entity.HasOne(d => d.TLoginMst)
                    .WithMany(p => p.TUserMsts)
                    .HasForeignKey(d => d.login_id)
                    .HasConstraintName("FK_User_Login");
            });

            modelBuilder.Entity<TCategoryMst>(entity =>
            {
                entity.HasKey(e => e.cat_id);
                entity.ToTable("T_Category_Mst");
                entity.Property(e => e.cat_name).HasMaxLength(100);
            });

            modelBuilder.Entity<TProductMst>(entity =>
            {
                entity.HasKey(e => e.prod_id);
                entity.ToTable("T_Product_Mst");
                entity.Property(e => e.prod_name).HasMaxLength(100);
                entity.Property(e => e.price).HasPrecision(10, 2);
                entity.Property(e => e.quantity).HasDefaultValue(0);

                entity.HasOne(d => d.TCategoryMst)
                    .WithMany(p => p.TProductMsts)
                    .HasForeignKey(d => d.cat_id)
                    .HasConstraintName("FK_Product_Category");
            });

            modelBuilder.Entity<TOrderMst>(entity =>
            {
                entity.HasKey(e => e.ord_id);
                entity.ToTable("T_Order_Mst");
                entity.Property(e => e.ord_date).HasColumnType("datetime");
                entity.Property(e => e.ord_status).HasMaxLength(30);
                entity.Property(e => e.ord_price).HasPrecision(10, 2);

                entity.HasOne(d => d.TLoginMst)
                    .WithMany(p => p.TOrderMsts)
                    .HasForeignKey(d => d.ordered_by)
                    .HasConstraintName("FK_Order_Login");

                entity.HasOne(d => d.TProductMst)
                    .WithMany(p => p.TOrderMsts)
                    .HasForeignKey(d => d.prod_id)
                    .HasConstraintName("FK_Order_Product");
            });

            modelBuilder.Entity<TOrderProductMapping>(entity =>
            {
                entity.HasKey(e => e.opm_id);
                entity.ToTable("t_order_product_mapping");
                entity.Property(e => e.price).HasPrecision(10, 2);

                entity.HasOne(d => d.TOrderMst)
                    .WithMany(p => p.TOrderProductMappings)
                    .HasForeignKey(d => d.order_id)
                    .HasConstraintName("FK_OrderMapping_Order");

                entity.HasOne(d => d.TProductMst)
                    .WithMany(p => p.TOrderProductMappings)
                    .HasForeignKey(d => d.product_id)
                    .HasConstraintName("FK_OrderMapping_Product");
            });

            modelBuilder.Entity<TReportGenServ>(entity =>
            {
                entity.HasKey(e => e.report_gen_id);
                entity.ToTable("T_Report_Gen_Serv");
                entity.Property(e => e.requested_by).HasMaxLength(100);
                entity.Property(e => e.report_type).HasMaxLength(50);

                entity.HasOne(d => d.TOrderMst)
                    .WithMany(p => p.TReportGenServs)
                    .HasForeignKey(d => d.ord_id)
                    .HasConstraintName("FK_Report_Order");
            });

            modelBuilder.Entity<TNotificationMst>(entity =>
            {
                entity.HasKey(e => e.not_id);
                entity.ToTable("T_Notification_Mst");

                entity.HasOne(d => d.TReportGenServ)
                    .WithOne(p => p.TNotificationMst)
                    .HasForeignKey<TNotificationMst>(d => d.report_gen_id)
                    .HasConstraintName("FK_Notification_Report");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
