using KuGou.Net.Protocol.Session;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace KgWebApi.Net.Services;

public sealed class KgWebSessionPersistence : ISessionPersistence
{
    private const long SessionId = 1;
    private readonly string _connectionString;
    private readonly object _syncRoot = new();

    public KgWebSessionPersistence(IHostEnvironment hostEnvironment, IConfiguration configuration)
    {
        var configuredConnectionString = configuration.GetConnectionString("KgWebApi");
        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            _connectionString = configuredConnectionString;
        }
        else
        {
            var dbPath = Path.Combine(hostEnvironment.ContentRootPath, "App_Data", "kgwebapi.db");
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                Cache = SqliteCacheMode.Shared
            };
            _connectionString = builder.ToString();
        }

        InitializeDatabase();
    }

    public KgSession? Load()
    {
        lock (_syncRoot)
        {
            try
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText =
                    """
                    SELECT UserId,
                           Token,
                           VipType,
                           VipToken,
                           Dfid,
                           Mid,
                           Uuid,
                           InstallDev,
                           InstallMac,
                           InstallGuid,
                           T1
                    FROM KgSessions
                    WHERE Id = $id
                    LIMIT 1;
                    """;
                command.Parameters.AddWithValue("$id", SessionId);

                using var reader = command.ExecuteReader();
                if (!reader.Read()) return null;

                return new KgSession
                {
                    UserId = GetString(reader, 0, "0"),
                    Token = GetString(reader, 1, ""),
                    VipType = GetString(reader, 2, "0"),
                    VipToken = GetString(reader, 3, ""),
                    Dfid = GetString(reader, 4, "-"),
                    Mid = GetString(reader, 5, "-"),
                    Uuid = GetString(reader, 6, "-"),
                    InstallDev = GetString(reader, 7, ""),
                    InstallMac = GetString(reader, 8, ""),
                    InstallGuid = GetString(reader, 9, ""),
                    T1 = reader.IsDBNull(10) ? "" : reader.GetString(10)
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public void Save(KgSession session)
    {
        lock (_syncRoot)
        {
            try
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText =
                    """
                    INSERT INTO KgSessions (
                        Id,
                        UserId,
                        Token,
                        VipType,
                        VipToken,
                        Dfid,
                        Mid,
                        Uuid,
                        InstallDev,
                        InstallMac,
                        InstallGuid,
                        T1,
                        UpdatedAtUtc
                    )
                    VALUES (
                        $id,
                        $userId,
                        $token,
                        $vipType,
                        $vipToken,
                        $dfid,
                        $mid,
                        $uuid,
                        $installDev,
                        $installMac,
                        $installGuid,
                        $t1,
                        $updatedAtUtc
                    )
                    ON CONFLICT(Id) DO UPDATE SET
                        UserId = excluded.UserId,
                        Token = excluded.Token,
                        VipType = excluded.VipType,
                        VipToken = excluded.VipToken,
                        Dfid = excluded.Dfid,
                        Mid = excluded.Mid,
                        Uuid = excluded.Uuid,
                        InstallDev = excluded.InstallDev,
                        InstallMac = excluded.InstallMac,
                        InstallGuid = excluded.InstallGuid,
                        T1 = excluded.T1,
                        UpdatedAtUtc = excluded.UpdatedAtUtc;
                    """;

                command.Parameters.AddWithValue("$id", SessionId);
                command.Parameters.AddWithValue("$userId", session.UserId);
                command.Parameters.AddWithValue("$token", session.Token);
                command.Parameters.AddWithValue("$vipType", session.VipType);
                command.Parameters.AddWithValue("$vipToken", session.VipToken);
                command.Parameters.AddWithValue("$dfid", session.Dfid);
                command.Parameters.AddWithValue("$mid", session.Mid);
                command.Parameters.AddWithValue("$uuid", session.Uuid);
                command.Parameters.AddWithValue("$installDev", session.InstallDev);
                command.Parameters.AddWithValue("$installMac", session.InstallMac);
                command.Parameters.AddWithValue("$installGuid", session.InstallGuid);
                command.Parameters.AddWithValue("$t1", (object?)session.T1 ?? DBNull.Value);
                command.Parameters.AddWithValue("$updatedAtUtc", DateTimeOffset.UtcNow.ToString("O"));

                command.ExecuteNonQuery();
            }
            catch
            {
            }
        }
    }

    public void Clear()
    {
        lock (_syncRoot)
        {
            try
            {
                using var connection = OpenConnection();
                using var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM KgSessions WHERE Id = $id;";
                command.Parameters.AddWithValue("$id", SessionId);
                command.ExecuteNonQuery();
            }
            catch
            {
            }
        }
    }

    private void InitializeDatabase()
    {
        lock (_syncRoot)
        {
            var dataSource = new SqliteConnectionStringBuilder(_connectionString).DataSource;
            var dir = Path.GetDirectoryName(dataSource);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                CREATE TABLE IF NOT EXISTS KgSessions (
                    Id INTEGER NOT NULL PRIMARY KEY,
                    UserId TEXT NOT NULL,
                    Token TEXT NOT NULL,
                    VipType TEXT NOT NULL,
                    VipToken TEXT NOT NULL,
                    Dfid TEXT NOT NULL,
                    Mid TEXT NOT NULL,
                    Uuid TEXT NOT NULL,
                    InstallDev TEXT NOT NULL,
                    InstallMac TEXT NOT NULL,
                    InstallGuid TEXT NOT NULL,
                    T1 TEXT NULL,
                    UpdatedAtUtc TEXT NOT NULL
                );
                """;
            command.ExecuteNonQuery();
        }
    }

    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private static string GetString(SqliteDataReader reader, int ordinal, string fallback)
    {
        return reader.IsDBNull(ordinal) ? fallback : reader.GetString(ordinal);
    }
}
