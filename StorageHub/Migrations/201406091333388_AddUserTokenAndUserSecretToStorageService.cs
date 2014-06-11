namespace StorageHub.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserTokenAndUserSecretToStorageService : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StorageServices", "UserToken", c => c.String());
            AddColumn("dbo.StorageServices", "UserSecret", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StorageServices", "UserSecret");
            DropColumn("dbo.StorageServices", "UserToken");
        }
    }
}
