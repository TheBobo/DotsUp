namespace SignalRChatDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ispaid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GameTables", "Paid", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GameTables", "Paid");
        }
    }
}
