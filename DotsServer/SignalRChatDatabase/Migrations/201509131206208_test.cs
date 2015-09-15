namespace SignalRChatDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.GameTables", "PlayerOneId", c => c.String(maxLength: 20));
            AlterColumn("dbo.GameTables", "PlayerTwoId", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.GameTables", "PlayerTwoId", c => c.String(maxLength: 15));
            AlterColumn("dbo.GameTables", "PlayerOneId", c => c.String(maxLength: 15));
        }
    }
}
