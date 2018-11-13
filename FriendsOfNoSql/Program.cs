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
                _.AutoCreateSchemaObjects = AutoCreate.All;
                _.Schema.For<Customer>();
                _.Connection(ConnectionString);
                //_.CreateDatabases = expressions => expressions.ForTenant().WithOwner("fo");
            });
            return store;
        }
    }

    public class Program
    {

        public static async Task Main()
        {
            var store = StoreFactory.CreateStore();

            store.Schema.WritePatch("1.initial.sql");
            var sql = store.Schema.ToDDL();
            Debug.WriteLine(sql);
            //store.Schema.AssertDatabaseMatchesConfiguration();
            store.Schema.ApplyAllConfiguredChangesToDatabase();

            var migrationsAssembly = typeof(Program).Assembly;

            using (var connection = new NpgsqlConnection(StoreFactory.ConnectionString))
            {
                var databaseProvider = new PostgresqlDatabaseProvider(connection);
                var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
                migrator.Load();
                migrator.MigrateToLatest();
            }

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
    }

    [Migration(1, "Rename Customer name in Fullname")]
    public class CreateCustomer : Migration
    {
        private DocumentStore _documentStore;

        public CreateCustomer()
        {
            _documentStore = StoreFactory.CreateStore();
        }

        protected override void Up()
        {
            using (var documentSession = _documentStore.OpenSession())
            {
                documentSession.Patch<Customer>(x => true).Rename("", x => x.Name);
                documentSession.SaveChanges();
            }
        }

        protected override void Down()
        {
            throw new NotImplementedException();
        }
    }


    [Migration(2, "#2 Rename Customer name in Fullname")]
    public class Bla : Migration
    {
        private DocumentStore _documentStore;

        public Bla()
        {
            _documentStore = StoreFactory.CreateStore();
        }

        protected override void Up()
        {
            using (var documentSession = _documentStore.OpenSession())
            {
                documentSession.Patch<Customer>(x => true).Rename("FullName", x => x.Name);
                documentSession.SaveChanges();
            }
        }

        protected override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
