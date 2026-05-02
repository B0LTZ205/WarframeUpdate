namespace WarframeUpdate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class AddExtendedModels : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserProfiles",
                c => new
                    {
                        UserId = c.String(maxLength: 128, nullable: false),
                        FirstName = c.String(maxLength: 100),
                        LastName = c.String(maxLength: 100),
                        PhoneNumber = c.String(maxLength: 15),
                        ProfilePictureUrl = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.FileUploads",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(maxLength: 128, nullable: false),
                        OriginalFileName = c.String(maxLength: 255, nullable: false),
                        StoredFileName = c.String(maxLength: 500, nullable: false),
                        FileExtension = c.String(maxLength: 10, nullable: false),
                        FileType = c.String(maxLength: 50, nullable: false),
                        FileSizeBytes = c.Long(nullable: false),
                        FilePath = c.String(maxLength: 500, nullable: false),
                        UploadedAt = c.DateTime(nullable: false),
                        Description = c.String(maxLength: 1000),
                        IsPublic = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.AdminActivityLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdminUserId = c.String(maxLength: 128, nullable: false),
                        Action = c.String(maxLength: 100, nullable: false),
                        Details = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AdminUserId, cascadeDelete: false)
                .Index(t => t.AdminUserId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.AdminActivityLogs", "AdminUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FileUploads", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserProfiles", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.AdminActivityLogs", new[] { "AdminUserId" });
            DropIndex("dbo.FileUploads", new[] { "UserId" });
            DropIndex("dbo.UserProfiles", new[] { "UserId" });
            DropTable("dbo.AdminActivityLogs");
            DropTable("dbo.FileUploads");
            DropTable("dbo.UserProfiles");
        }
    }
}