using Android.App;
using System.Collections.Generic;
using SQLite;
using System.Threading.Tasks;
using IncomeRelatedRent.Models;
using System.IO;

namespace IncomeRelatedRent.Data
{
    public class DatabaseHelper
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseHelper(string dbPath)
        {
            if (!File.Exists(dbPath))
            {
                using (var asset = Application.Context.Assets.Open("WeeklyPAYEDeductions.db"))
                using (var file = File.Create(dbPath))
                {
                    asset.CopyTo(file);
                }
            }

            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<TaxDeductionsData>().Wait();
        }

        public Task<List<TaxDeductionsData>> GetTaxDataAsync()
        {
            return _database.Table<TaxDeductionsData>().ToListAsync();
        }

        public Task<int> InsertTaxDataAsync(TaxDeductionsData taxDeductionsData)
        {
            return _database.InsertAsync(taxDeductionsData);
        }
    }
}