using FluentMigrator;

namespace Gs1DigitalLink.Api.Services.Migrations;

[Migration(20260118184700)]
public class M20260118184700_CreateInsightTable : AutoReversingMigration
{
    public override void Up()
    {
        Create.Table("Insight")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("DigitalLink").AsString(255)
            .WithColumn("Timestamp").AsDateTime().NotNullable()
            .WithColumn("LinkType").AsString(50).Nullable()
            .WithColumn("Languages").AsString(255)
            .WithColumn("CandidateCount").AsInt32();
    }
}