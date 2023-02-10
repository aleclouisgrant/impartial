using Impartial;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ImpartialUI
{
    public partial class App : Application
    {
        public static IDatabaseProvider DatabaseProvider { get; } = new SqlDatabaseProvider("Data Source=desktop;Initial Catalog=Impartial;Integrated Security=True");
    }
}
