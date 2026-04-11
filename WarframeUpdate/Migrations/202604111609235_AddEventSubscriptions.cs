namespace WarframeUpdate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddEventSubscriptions : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EventSubscriptions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        EventType = c.String(nullable: false, maxLength: 50),
                        IsSubscribed = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.EventSubscriptions", "UserId", "dbo.AspNetUsers");
            DropIndex("dbo.EventSubscriptions", new[] { "UserId" });
            DropTable("dbo.EventSubscriptions");
        }
    }
}
