namespace StorageHub.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveOAuth1Tokens : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.StorageServices", "UserToken");
            DropColumn("dbo.StorageServices", "UserSecret");
        }
        
        public override void Down()
        {
            AddColumn("dbo.StorageServices", "UserSecret", c => c.String());
            AddColumn("dbo.StorageServices", "UserToken", c => c.String());
        }
    }
}
