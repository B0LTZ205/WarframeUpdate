namespace WarframeUpdate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateDatabase : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AdminActivityLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AdminUserId = c.String(nullable: false, maxLength: 128),
                        Action = c.String(nullable: false, maxLength: 100),
                        Details = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.AdminUserId)
                .Index(t => t.AdminUserId);
            
            CreateTable(
                "dbo.FileUploads",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        OriginalFileName = c.String(nullable: false, maxLength: 255),
                        StoredFileName = c.String(nullable: false, maxLength: 500),
                        FileExtension = c.String(nullable: false, maxLength: 10),
                        FileType = c.String(nullable: false, maxLength: 50),
                        FileSizeBytes = c.Long(nullable: false),
                        FilePath = c.String(nullable: false, maxLength: 500),
                        UploadedAt = c.DateTime(nullable: false),
                        Description = c.String(maxLength: 1000),
                        IsPublic = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.UserProfiles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
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
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdminActivityLogs", "AdminUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserProfiles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.FileUploads", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.UserProfiles", new[] { "UserId" });
            DropIndex("dbo.FileUploads", new[] { "UserId" });
            DropIndex("dbo.AdminActivityLogs", new[] { "AdminUserId" });
            DropTable("dbo.UserProfiles");
            DropTable("dbo.FileUploads");
            DropTable("dbo.AdminActivityLogs");
        }
    }
}
