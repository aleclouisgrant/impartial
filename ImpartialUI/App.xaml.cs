using Impartial;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ImpartialUI
{
    public partial class App : Application
    {
        public static IDatabaseProvider DatabaseProvider { get; } = new MongoDatabaseProvider("Impartial");
    }
}
