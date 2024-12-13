using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace ExpenseTracker
{
    public struct Database
    {
        public string dbpath { get; set; }
        private SQLiteConnection connection { get; set; }

        public Database(string path)
        {
            this.dbpath = path;
            string connectionQuery = $"Data Source={dbpath}";
            this.connection = new SQLiteConnection(connectionQuery);

            this.connection.Open();
        }

        public void CreateTable()
        {
            string query = @"
            CREATE TABLE IF NOT EXISTS ExpenseData (
                    id         INTEGER PRIMARY KEY AUTOINCREMENT,
                    category   TEXT    NOT NULL,
                    type       TEXT    NOT NULL,
                    amount     TEXT    NOT NULL,
                    date       INTEGER NOT NULL
            );";

            using (var command = new SQLiteCommand(query, this.connection))
            {
                command.ExecuteNonQuery();
                Console.WriteLine("table creation success");
            }
        }

        public void InsertData(ExpenseData data)
        {
            string query = "INSERT INTO ExpenseData (category, type, amount, date) VALUES (@category, @type, @amount, @date)";
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@category", data.category);
                command.Parameters.AddWithValue("@type", data.type);
                command.Parameters.AddWithValue("@amount", data.amount);
                command.Parameters.AddWithValue("@date", data.unixTime);

                command.ExecuteNonQuery();
            }
        }

        public List<ExpenseData> QueryDataDateRange(long startUnix, long endUnix)
        {
            List<ExpenseData> row = new List<ExpenseData>();

            // Convert Unix timestamps to DateTime objects for the start and end dates
            DateTime startDate = Utils.UnixTimeToMonthYear(startUnix);
            DateTime endDate = Utils.UnixTimeToMonthYear(endUnix);

            // Extract the start and end dates (month and year only)
            DateTime startOfMonth = new DateTime(startDate.Year, startDate.Month, 1);
            DateTime endOfMonth = new DateTime(endDate.Year, endDate.Month, DateTime.DaysInMonth(endDate.Year, endDate.Month));

            // Convert to Unix timestamps for comparison with the database
            long startUnixTimestamp = ((DateTimeOffset)startOfMonth).ToUnixTimeSeconds();
            long endUnixTimestamp = ((DateTimeOffset)endOfMonth).ToUnixTimeSeconds();

            Console.WriteLine("Start Date: " + startOfMonth.ToString("MM/dd/yyyy"));
            Console.WriteLine("End Date: " + endOfMonth.ToString("MM/dd/yyyy"));

            // Adjust the SQL query to compare Unix timestamps
            //string query = @"SELECT * FROM ExpenseData WHERE date BETWEEN @startDate AND @endDate"; // note(russel): maybe we should use ASC?
            string query = @"SELECT * FROM ExpenseData WHERE date BETWEEN @startDate AND @endDate ORDER BY date DESC";

            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@startDate", startUnixTimestamp);
                command.Parameters.AddWithValue("@endDate", endUnixTimestamp);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string category = reader["category"].ToString();
                        string type = reader["type"].ToString();
                        double amount = 0;

                        if (reader["amount"] != DBNull.Value)
                        {
                            amount = Convert.ToDouble(reader["amount"]);
                        }

                        long dateUnixTime = Convert.ToInt64(reader["date"]);
                        Console.WriteLine("added");

                        row.Add(new ExpenseData(category, type, amount, dateUnixTime));
                    }
                }
            }
            return row;
        }

    }

    public struct ExpenseData
    {
        public string category { get; set; }
        public string type { get; set; }
        public double amount { get; set; }
        public long unixTime { get; set; }

        public ExpenseData(string category, string type, double amount, long unixTime)
        {
            this.category = category;
            this.type = type;
            this.amount = amount;
            this.unixTime = unixTime;
        }
    }
}
