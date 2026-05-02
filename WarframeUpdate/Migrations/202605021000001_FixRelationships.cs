namespace WarframeUpdate.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class FixRelationships : DbMigration
    {
        public override void Up()
        {
            // No changes needed if the previous migration was not applied
            // This migration is mainly for documentation purposes
            // The OnModelCreating configuration in ApplicationDbContext handles relationships
        }

        public override void Down()
        {
        }
    }
}