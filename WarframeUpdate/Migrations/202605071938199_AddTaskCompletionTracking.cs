namespace WarframeUpdate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddTaskCompletionTracking : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.AdminActivityLogs", "AdminUserId", "dbo.AspNetUsers");
            CreateTable(
                "dbo.TaskCompletions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserTaskId = c.Int(nullable: false),
                        CompletedBy = c.String(nullable: false, maxLength: 128),
                        CompletionNotes = c.String(maxLength: 500),
                        CompletedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.CompletedBy)
                .ForeignKey("dbo.UserTasks", t => t.UserTaskId, cascadeDelete: true)
                .Index(t => t.UserTaskId)
                .Index(t => t.CompletedBy);
            
            CreateTable(
                "dbo.UserTasks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        Title = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 1000),
                        Status = c.Int(nullable: false),
                        Priority = c.Int(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        CompletedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            AddForeignKey("dbo.AdminActivityLogs", "AdminUserId", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AdminActivityLogs", "AdminUserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserTasks", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.TaskCompletions", "UserTaskId", "dbo.UserTasks");
            DropForeignKey("dbo.TaskCompletions", "CompletedBy", "dbo.AspNetUsers");
            DropIndex("dbo.UserTasks", new[] { "UserId" });
            DropIndex("dbo.TaskCompletions", new[] { "CompletedBy" });
            DropIndex("dbo.TaskCompletions", new[] { "UserTaskId" });
            DropTable("dbo.UserTasks");
            DropTable("dbo.TaskCompletions");
            AddForeignKey("dbo.AdminActivityLogs", "AdminUserId", "dbo.AspNetUsers", "Id");
        }
    }
}
