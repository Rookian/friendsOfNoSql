using Marten;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten.Linq.SoftDeletes;

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

    public class Program
    {
        public static async Task Main()
        {
            var store = DocumentStore.For(_ =>
            {
                _.AutoCreateSchemaObjects = AutoCreate.All;
                _.Connection("Server=friendsofnosqlpostgres.postgres.database.azure.com;Database=postgres;Port=5432;User Id=postgres@friendsofnosqlpostgres;Password=Password123!;SslMode=Require;");
                //_.CreateDatabases = expressions => expressions.ForTenant().WithOwner("fo");
            });

            store.Schema.ApplyAllConfiguredChangesToDatabase();

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
}
