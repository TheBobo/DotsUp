namespace SignalRChatDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class playersIds : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GameTables", "PlayerOneId", c => c.String(maxLength: 15));
            AddColumn("dbo.GameTables", "PlayerTwoId", c => c.String(maxLength: 15));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GameTables", "PlayerTwoId");
            DropColumn("dbo.GameTables", "PlayerOneId");
        }
    }
}
