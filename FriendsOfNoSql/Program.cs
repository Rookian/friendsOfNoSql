using System;
using Marten;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Marten.Linq.SoftDeletes;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;

namespace FriendsOfNoSql
{
    public class Customer
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<Hobby> Hobbies { get; set; }
    }

    public class Hobby
    {
        public string Name { get; set; }
    }

    public class StoreFactory
    {
        public const string ConnectionString = "Server=friendsofnosqlpostgres.postgres.database.azure.com;Database=postgres;Port=5432;User Id=postgres@friendsofnosqlpostgres;Password=Password123!;SslMode=Require;";
        public static DocumentStore CreateStore()
        {

            var store = DocumentStore.For(_ =>
            {
                _.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
                _.Schema.For<Customer>();
                _.Connection(ConnectionString);
            });
            return store;
        }
    }

    public class Program
    {

        public static async Task Main()
        {
            var store = StoreFactory.CreateStore();

            store.Schema.ApplyAllConfiguredChangesToDatabase();

            Migrate();

            using (var session = store.OpenSession())
            {

                var customer = new Customer
                {
                    Id = 1,
                    Name = "Testbbbbb",
                    Hobbies = new List<Hobby>
                {
                    new Hobby{Name = "Fischen"},
                    new Hobby{Name = "Coden mit k"}
                }
                };

                var hobby = new Hobby();
                session.Store(customer);
                await session.SaveChangesAsync();
            }

            using (var session = store.QuerySession())
            {
                var findById = session.Json.FindById<Customer>(1);
                var hobbies = session.Query<Customer>()
                    .Where(x => x.Hobbies.Any(h => h.Name == "Fischen"))
                    .SelectMany(x => x.Hobbies)
                    .Where(x => x.Name == "Fischen")
                    .ToCommand().CommandText;
            }
        }

        private static void Migrate()
        {
            var migrationsAssembly = typeof(Program).Assembly;

            using (var connection = new NpgsqlConnection(StoreFactory.ConnectionString))
            {
                var databaseProvider = new PostgresqlDatabaseProvider(connection);
                //new SimpleMigrator<X, B>(migrationsAssembly, null);
                var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
                migrator.Load();
                migrator.MigrateToLatest();
            }
        }
    }

    public class MartenConnection
    {
        public IDocumentSession Session { get; set; }
    }

    public abstract class MartenMigration : IMigration<MartenConnection>
    {
        public void RunMigration(MigrationRunData<MartenConnection> data)
        {
            Session = data.Connection.Session;

            Up().GetAwaiter().GetResult();
        }

        protected IDocumentSession Session { get; set; }
        protected abstract Task Up();
    }

    [Migration(1, "Rename Customer name in Fullname")]
    public class CreateCustomer : Migration
    {
        protected override void Up()
        {

        }

        protected override void Down()
        {
            throw new NotImplementedException();
        }
    }


    [Migration(2, "#2 Rename Customer name in Fullname")]
    public class Bla : Migration
    {
        protected override void Up()
        {
        }

        protected override void Down()
        {
            throw new NotImplementedException();
        }
    }

    [Migration(4, "#4 Rename Customer name in Fullname")]
    public class Bla4 : Migration
    {
        protected override void Up()
        {
            
        }

        protected override void Down()
        {
            throw new NotImplementedException();
        }
    }

    [Migration(3, "#3 Rename Customer name in Fullname")]
    public class Bla3 : Migration
    {
        protected override void Up()
        {

        }

        protected override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
