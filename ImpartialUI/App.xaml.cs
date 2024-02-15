using Impartial;
using System.Windows;

namespace ImpartialUI
{
    public partial class App : Application
    {
        //public static IDatabaseProvider DatabaseProvider { get; } = new SqlDatabaseProvider("Data Source=desktop;Initial Catalog=Impartial;Integrated Security=True");
        public static IDatabaseProvider DatabaseProvider { get; } = new PgDatabaseProvider(host: "localhost", user: "postgres", dbName: "WCS-SS-DB", port: "5432", password: "*Firenice18");
    }
}
