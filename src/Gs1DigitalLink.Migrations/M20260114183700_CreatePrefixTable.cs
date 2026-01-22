using FluentMigrator;

namespace Gs1DigitalLink.Migrations;

[Migration(20260114183700)]
public class M20260114183700_CreatePrefixTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Prefix")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("CompanyPrefix").AsString(13)
            .WithColumn("IsLinksetDefault").AsBoolean()
            .WithColumn("Value").AsString(255).NotNullable().Unique();
    }
}
