using Infrastructure;
using Microsoft.Data.Sqlite;
using System;

namespace DBImport
{
    public class Z21_New_Import
    {


        public Z21_New_Import(Database db, string path)
        {
            if (db is null || string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Arguments cant be null!");



        }
    }
}
