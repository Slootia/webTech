namespace Laba3_4.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Security.Cryptography;


    public partial class AdminInsertion : DbMigration
    {
        public override void Up()
        {
            Sql("INSERT INTO AspNetRoles (Id, Name) VALUES ('adminRoleId', 'Admin');");
            string adminPassword = "admin";
            string adminPasswordHash = HashPassword(adminPassword);
            string adminSecurityStamp = Guid.NewGuid().ToString("D");
            Sql("INSERT INTO AspNetUsers (Id, FirstName, LastName, Patronymic, Email, EmailConfirmed, PasswordHash, SecurityStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, UserName) VALUES" +
                                         " ('adminId', 'admin', 'admin', 'admin', 'admin@admin.com', 'true', 'ABWUq8zzRuApWgXRgRShQsTSEadmLZKaBteKJT6hLSpe2hwEEixS7jhAdJfm5ZdeCA==', 'fc6b649b-a11e-45e4-ac35-9c31ec704195', 'false', 'false', 'false', 0, 'admin@admin.com');");
            Sql("INSERT INTO AspNetUserRoles (UserId, RoleId) VALUES ('adminId', 'adminRoleId');");
        }

        public override void Down()
        {
        }

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }
    }
}
