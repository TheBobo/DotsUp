namespace SignalRChatDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class playerBoxes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GameTables", "PlayerOneBoxes", c => c.Int(nullable: false));
            AddColumn("dbo.GameTables", "PlayerTwoBoxes", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GameTables", "PlayerTwoBoxes");
            DropColumn("dbo.GameTables", "PlayerOneBoxes");
        }
    }
}
