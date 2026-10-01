using System;
using System.IO;
using SQLite;
using System.Linq.Expressions;
using Microsoft.Maui.Storage;


namespace MAUISql.Data
{
    public class DatabaseContext : IAsyncDisposable
    {
        private const string DbName = "MyDatabase.db3";
        private static string DbPath
        {
            get
            {
                #if NET6_0_ANDROID || NET6_0_IOS || NET6_0_MACCATALYST || NET7_0_ANDROID || NET7_0_IOS || NET7_0_MACCATALYST || NET8_0_ANDROID || NET8_0_IOS || NET8_0_MACCATALYST
                                // Utilise FileSystem quand on est dans MAUI
                                return Path.Combine(FileSystem.AppDataDirectory, DbName);
                #else
                                // Fallback pour projets non-MAUI
                                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), DbName);
                #endif
            }
        }

        private SQLiteAsyncConnection _connection;
        private SQLiteAsyncConnection Database =>
            (_connection ??= new SQLiteAsyncConnection(DbPath,
                SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.SharedCache));

        private async Task CreateTableIfNotExists<TTable>() where TTable : class, new()
        {
            await Database.CreateTableAsync<TTable>();
        }

        private async Task<AsyncTableQuery<TTable>> GetTableAsync<TTable>() where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return Database.Table<TTable>();
        }

        public async Task<IEnumerable<TTable>> GetAllAsync<TTable>() where TTable : class, new()
        {
            var table = await GetTableAsync<TTable>();
            return await table.ToListAsync();
        }

        public async Task<IEnumerable<TTable>> GetFileteredAsync<TTable>(Expression<Func<TTable, bool>> predicate) where TTable : class, new()
        {
            var table = await GetTableAsync<TTable>();
            return await table.Where(predicate).ToListAsync();
        }

        private async Task<TResult> Execute<TTable, TResult>(Func<Task<TResult>> action) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await action();
        }

        public async Task<TTable> GetItemByKeyAsync<TTable>(object primaryKey) where TTable : class, new()
        {
            //await CreateTableIfNotExists<TTable>();
            //return await Database.GetAsync<TTable>(primaryKey);
            return await Execute<TTable, TTable>(async () => await Database.GetAsync<TTable>(primaryKey));
        }

        public async Task<bool> AddItemAsync<TTable>(TTable item) where TTable : class, new()
        {
            //await CreateTableIfNotExists<TTable>();
            //return await Database.InsertAsync(item) > 0;
            return await Execute<TTable, bool>(async () => await Database.InsertAsync(item) > 0);
        }

        public async Task<bool> UpdateItemAsync<TTable>(TTable item) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await Database.UpdateAsync(item) > 0;
        }

        public async Task<bool> DeleteItemAsync<TTable>(TTable item) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await Database.DeleteAsync(item) > 0;
        }

        public async Task<bool> DeleteItemByKeyAsync<TTable>(object primaryKey) where TTable : class, new()
        {
            await CreateTableIfNotExists<TTable>();
            return await Database.DeleteAsync<TTable>(primaryKey) > 0;
        }

        public async ValueTask DisposeAsync() => await _connection?.CloseAsync();
    }
}
