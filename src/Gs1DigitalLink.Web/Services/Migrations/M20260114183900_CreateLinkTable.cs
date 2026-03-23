using FluentMigrator;

namespace Gs1DigitalLink.Web.Services.Migrations;

[Migration(20260114183900)]
public class M20260114183900_CreateLinkTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Link")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("PrefixId").AsInt32().ForeignKey("Prefix", "Id")
            .WithColumn("LinkType").AsString(255).NotNullable()
            .WithColumn("Language").AsString(6).Nullable()
            .WithColumn("ContentType").AsString(255).Nullable()
            .WithColumn("RedirectUrl").AsString(255).NotNullable()
            .WithColumn("Title").AsString(255).NotNullable()
            .WithColumn("AvailableFrom").AsDateTime().NotNullable()
            .WithColumn("AvailableTo").AsDateTime().Nullable();
    }
}
