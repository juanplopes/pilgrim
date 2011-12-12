# Getting Started

``` csharp
[Migration(20111212064831)]
public class M20111212064831 : FluentMigration
{
    public override void Up(SchemaAction schema)
    {
        schema.AddTable("users", t =>
        {
            t.AddString("email").NotNullable();
            t.AddDateTime("registered_at");
        });
    }

    public override void Down(SchemaAction schema)
    {
        schema.RemoveTable("users");
    }
}
```