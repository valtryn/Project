using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Entity.Core.Metadata.Edm;
using System.Runtime.CompilerServices;

namespace ExpenseTracker
{
    public struct Database
    {
        public string dbpath { get; set; }
        private SQLiteConnection connection { get; set; }

        // Documentation: Database constructor
        // this sets the database file path (file.db)
        // this opens a connection to the database
        // should be called like this: Database db = new Database("path of database file");
        public Database(string path)
        {
            this.dbpath = path;
            string connectionQuery = $"Data Source={dbpath}";
            this.connection = new SQLiteConnection(connectionQuery);

            this.connection.Open();
        }

        // Documentation: this method will check if there is already a table
        // it will create something like this
        // +--------------------------------------+
        // |id  | category | type | amount | date |
        // +--------------------------------------+
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

        // Documentation: this is for inserting our data
        // liek this
        // +--------------------------------------+
        // |id  | category | type | amount | date |
        // +--------------------------------------+
        // |           Currently empty            |
        // +--------------------------------------+

        // if we call InsertData(data)
        // +---------------------------------------------+
        // |id  | category | type  | amount | date       |
        // +---------------------------------------------+
        // |1   | food     |grocery| 500    | 10/10/2024 |
        // +---------------------------------------------+
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

        // Documentation: this retrieves data based on a range of start - end date
        // Documentation: this is for inserting our data
        // +---------------------------------------------+
        // |id  | category | type  | amount | date       |
        // +---------------------------------------------+
        // |1   | food     |grocery| 500    | 10/10/2024 |
        // +---------------------------------------------+
        // |2   | movie    |idk    | 300    | 11/10/2024 |
        // +---------------------------------------------+
        // |3   | food     |online | 500    | 12/10/2024 |
        // +---------------------------------------------+
        // if we call QueryDataDateRange("10/10/2024", "11/10/2024")
        // this will return an ArrayList of data
        // retrieved data: 
        // { "food,grocery,500,10/10/2024" } && { "movie,idk,300,11/10/2024" }
        // notice the 3rd one is not included because it is out of range
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

    // Documentation: structure of Data so that we can easily use it in functions without passing multiple data
    // if we pass like this: InsertData("category", "type", amount, date) its very time consuming
    // but we use this structure it will be easier InsertData(data);
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
