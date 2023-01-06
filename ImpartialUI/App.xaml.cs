using Impartial;
using System.Windows;

namespace ImpartialUI
{
    public partial class App : Application
    {
        //public static IDatabaseProvider DatabaseProvider { get; } = new LocalDatabaseProvider();
        public static IDatabaseProvider DatabaseProvider { get; } = new SqlDatabaseProvider("Data Source=desktop;Initial Catalog=Impartial;Integrated Security=True");
    }
}
