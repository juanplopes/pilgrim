# Getting Started

* Install latest Visual Studio extension (VSIX).
* Create new Pilgrim Migrations Project.
* Follow the wizard.
* Create new Migration.

# Running Migrations

The code needed to run the migrations is basically:

``` csharp
class Program
{
    static void Main(string[] args)
    {
        var migrator = new DbMigrator(
            new MigratorOptions("ConnectionName").FromAssembly(Assembly.GetExecutingAssembly()));
        migrator.MigrateToLast();
    }
}
```

The template with code generation is a little more complicated, but keep in mind you just need this to start.

# Migrations Examples

## Adding new table
``` csharp
[Migration(20111212064831)]
public class M20111212064831 : FluentMigration
{
    public override void Up(SchemaAction schema)
    {
        schema.AddTable("groups", t => t.AddString("name"));

        schema.AddTable("users", t =>
        {
            t.AddString("email").NotNullable();
            t.AddDateTime("registered_at");

            t.AddInt32("group_id").AutoForeignKey("groups");
        });
    }

    public override void Down(SchemaAction schema)
    {
        schema.RemoveTable("users");
        schema.RemoveTable("groups");
    }
}
```

## Changing table fields
``` csharp
[Migration(20111212064900)]
public class M20111212064900 : FluentMigration
{
    public override void Up(SchemaAction schema)
    {
        schema.ChangeTable("users", t =>
        {
            t.AddInt32("accesses");
            t.RenameColumn("registered_at", "registration_date");
        });
    }

    public override void Down(SchemaAction schema)
    {
        schema.ChangeTable("users", t =>
        {
            t.RemoveColumn("accesses");
            t.RenameColumn("registration_date", "registered_at");
        });
    }
}
```

## Renaming table
``` csharp
[Migration(20111212065952)]
public class M20111212065952 : FluentMigration
{
    public override void Up(SchemaAction schema)
    {
        schema.RenameTable("groups", "t_groups");
        schema.RenameTable("users", "t_users");
    }

    public override void Down(SchemaAction schema)
    {
        schema.RenameTable("t_groups", "groups");
        schema.RenameTable("t_users", "users");
    }
}
```

## Adding unique key
``` csharp
[Migration(20111212070830)]
public class M20111212070830 : FluentMigration
{
    public override void Up(SchemaAction schema)
    {
        schema.ChangeTable("users", t =>
            t.AddString("login").Unique("uk_login"));


        schema.ChangeTable("groups", t =>
            t.AddUniqueColumns("uk_group_scope",
                t.AddString("scope"),
                t.WithColumn("name")));
    }

    public override void Down(SchemaAction schema)
    {
        schema.ChangeTable("users", t =>
            t.RemoveUniqueConstraint("uk_login"));

        schema.ChangeTable("groups", t =>
        {
            t.RemoveUniqueConstraint("uk_group_scope");
            t.RemoveColumn("scope");
        });
    }
}
```

