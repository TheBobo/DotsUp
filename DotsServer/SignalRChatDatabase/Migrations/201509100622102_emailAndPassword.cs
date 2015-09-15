namespace SignalRChatDatabase.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class emailAndPassword : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Password", c => c.String(maxLength: 200));
            AddColumn("dbo.Users", "Email", c => c.String(maxLength: 60));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Email");
            DropColumn("dbo.Users", "Password");
        }
    }
}
