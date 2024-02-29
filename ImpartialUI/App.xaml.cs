using Impartial;
using ImpartialUI.Services.DatabaseProvider;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImpartialUI
{
    public partial class App : Application
    {
        //public static IDatabaseProvider DatabaseProvider { get; } = new SqlDatabaseProvider("Data Source=desktop;Initial Catalog=Impartial;Integrated Security=True");
        //public static IDatabaseProvider DatabaseProvider { get; } = new LocalDatabaseProvider(populateWithTestData: true);
        public static IDatabaseProvider DatabaseProvider { get; } = new PgDatabaseProvider(host: "localhost", user: "postgres", dbName: "WCS-SS-DB", port: "5432", password: "*Firenice18");

        public static List<IJudge> JudgesDb { get; set; } = new List<IJudge>();
        public static List<ICompetitor> CompetitorsDb { get; set;} = new List<ICompetitor>();

        public App()
        {
            RefreshCaches().Wait();
            InitializeComponent();
        }

        public async Task RefreshCaches()
        {
            CompetitorsDb = (await DatabaseProvider.GetAllCompetitorsAsync()).OrderBy(c => c.FullName).ToList();
            JudgesDb = (await DatabaseProvider.GetAllJudgesAsync()).OrderBy(c => c.FullName).ToList();
        }
    }
}
