using ClothesShop.Areas.Admin.Controllers;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using System.Web.Services.Description;

[assembly: OwinStartupAttribute(typeof(ClothesShop.Startup))]
namespace ClothesShop
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            JobStorage.Current = new SqlServerStorage("DefaultConnection");
            BackupAndRestoreController backupSchedule = new BackupAndRestoreController();
            RecurringJob.AddOrUpdate(() => backupSchedule.AutoBackupDatabase(), Cron.Daily);
            app.UseHangfireServer();
        }
    }
}
