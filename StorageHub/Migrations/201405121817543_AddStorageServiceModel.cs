namespace StorageHub.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddStorageServiceModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StorageServices",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ServiceType = c.Int(nullable: false),
                        AccessToken = c.String(),
                        RefreshToken = c.String(),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StorageServices", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.StorageServices", new[] { "ApplicationUser_Id" });
            DropTable("dbo.StorageServices");
        }
    }
}
