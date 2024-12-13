using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;


// TODO:
//  - separate the code logic (high)
//  - implement pagination (high)
//  - implement search (medium)
//  - implement charts (low)
//  - implement profiles (very low)
//  - implement date search (high)
//  - implement sorting (high)
namespace ExpenseTracker
{
    public partial class Form1 : Form
    {
        private Renderer renderer;
        private Database database;
        private Label[] cells;
        List<ExpenseData> data { get; set; }
        int currentPage = 1;
        int itemsPerPage = 10;

        long startTime;
        long endTime;

        int relNum1 = 1;
        int relNum2 = 2;
        int relNum3 = 3;
        int relNum4 = 4;
        int currentTotalPage;

        private int previousSelectedIndex = 2;

        public Form1()
        {
            // Initialize database
            database = new Database("test.db");
            database.CreateTable();
            LoadData(Utils.UnixCurrentTime(), Utils.UnixCurrentTime());

            // Initialize components
            InitializeComponent();

            // Initialize cell tables
            cells = new Label[] {
                category_row_1,  type_row_1,  date_row_1,  amount_row_1,
                category_row_2,  type_row_2,  date_row_2,  amount_row_2,
                category_row_3,  type_row_3,  date_row_3,  amount_row_3,
                category_row_4,  type_row_4,  date_row_4,  amount_row_4,
                category_row_5,  type_row_5,  date_row_5,  amount_row_5,
                category_row_6,  type_row_6,  date_row_6,  amount_row_6,
                category_row_7,  type_row_7,  date_row_7,  amount_row_7,
                category_row_8,  type_row_8,  date_row_8,  amount_row_8,
                category_row_9,  type_row_9,  date_row_9,  amount_row_9,
                category_row_10, type_row_10, date_row_10, amount_row_10,
            };

            // Draw components
            DrawSort();
            DrawDatePickerRange();
            startTime = Utils.StringTimeToUnixTime(start_date_input.Text);
            endTime = Utils.StringTimeToUnixTime(end_date_input.Text);


            // these are hex codes for color, you can change it if you want
            Theme theme = new Theme(
                "#0C0B0B", // crust
                "#232222", // mantle
                "#FFFFFF", // foreground/text
                "#6DFF65"  // green
                );

            renderer = new Renderer(this, theme);
            renderer.RenderWindow();
            ApplyTheme(theme);

            // TODO: render this component in renderer class
            date_picker.Format = DateTimePickerFormat.Custom;
            date_picker.ShowUpDown = false;
            date_picker.Value = DateTime.Now;
            long currentTime = Utils.UnixCurrentTime();

            DrawPagination(currentPage);
        }

        public List<ExpenseData> GetPageData(int pageNumber)
        {
            int startIndex = (pageNumber - 1) * itemsPerPage;

            var pageData = data
                .Skip(startIndex)
                .Take(itemsPerPage)
                .ToList();

            return pageData;
        }

        public void DrawSort() 
        {
            sort_combo_box.Items.Add("Category");
            sort_combo_box.Items.Add("Type");
            sort_combo_box.Items.Add("Date");
            sort_combo_box.Items.Add("Amount");
            sort_combo_box.SelectedIndex = 2;
            sort_combo_box.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        public void DrawDatePickerRange()
        {
            long currentTime = Utils.UnixCurrentTime();
            start_date_input.Text = Utils.UnixTimeToDateTimeString(currentTime);
            end_date_input.Text = Utils.UnixTimeToDateTimeString(currentTime);
        }

        public void DrawPaginationButton()
        {



        }
        
        // Previous button
        private void button1_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                currentPage--;
                DrawPagination(currentPage);
            }
            
        }

        // Next button
        private void next_button_Click(object sender, EventArgs e)
        {
                currentPage++;
                DrawPagination(currentPage);
        }


        public void LoadData(long start, long end)
        {
            data = database.QueryDataDateRange(start, end);

        }
        public void DrawPagination(int pageNumber)
        {
            List<ExpenseData> data = GetPageData(pageNumber);

            
            for (int i = 0; i < itemsPerPage; i++)
            {
                cells[i * 4].Text = "";
                cells[i * 4 + 1].Text = "";
                cells[i * 4 + 2].Text = "";
                cells[i * 4 + 3].Text = "";
            }

            // Iterate through each item in the data and make sure we do not exceed the maximum row count
            for (int i = 0; i < data.Count && i < itemsPerPage; i++)
            {
                cells[i * 4].Text = data[i].category;                                     // category
                cells[i * 4 + 1].Text = data[i].type;                                     // type
                cells[i * 4 + 2].Text = Utils.UnixTimeToDateTimeString(data[i].unixTime); // date
                cells[i * 4 + 3].Text = data[i].amount.ToString("F2");                    // amount
            }

            total_amount_holder.Text = CalculateTotalAmount().ToString("F2");
            transaction_amount_holder.Text = CalculateTotalTransaction().ToString();
            page_number.Text = currentPage.ToString();
            this.Invalidate();
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            renderer.RenderShadowSidePanel(e);
            renderer.RenderShadowDateRange(e);
            renderer.RenderShadowDataTable(e);
        }

        private void add_Click(object sender, EventArgs e)
        {
            string category_entry = category_input.Text;
            string type_entry = type_input.Text;
            string amount_entry = amount_input.Text;
            string date_entry = date_picker.Text;
            if (string.IsNullOrWhiteSpace(category_entry))
            {
                MessageBox.Show("Category cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            double amount;
            if (string.IsNullOrWhiteSpace(amount_entry) || !double.TryParse(amount_entry, out amount) || amount <= 0)
            {
                MessageBox.Show("Amount must be a valid number greater than zero.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(date_entry))
            {
                MessageBox.Show("Date cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            long unixTime = 0;

            DateTime parsedDate;

            if (DateTime.TryParseExact(date_entry, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                if (parsedDate.Date == DateTime.Today)
                {
                    unixTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
                }
                else
                {
                    unixTime = ((DateTimeOffset)parsedDate).ToUnixTimeSeconds();
                }
            }
            ExpenseData data = new ExpenseData(category_entry, type_entry, amount, unixTime);
            database.InsertData(data);
            LoadData(Utils.UnixCurrentTime(), Utils.UnixCurrentTime());
            DrawPagination(currentPage);
        }

        public double CalculateTotalAmount()
        {
            double total = 0;
            foreach (var item in data)
            {
                total += item.amount;
            }
            return total;
        }

        public double CalculateTotalTransaction()
        {
            return database.QueryDataDateRange(startTime, endTime).Count;
        }

        private void filter_date_Click(object sender, EventArgs e)
        {
            // TODO: need validation
            startTime = Utils.StringTimeToUnixTime(start_date_input.Text);
            endTime = Utils.StringTimeToUnixTime(end_date_input.Text);

            LoadData(startTime, endTime);
            DrawPagination(currentPage);
        }

        private void sort_combo_box_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (sort_combo_box.SelectedIndex != previousSelectedIndex)
            {
                previousSelectedIndex = sort_combo_box.SelectedIndex;
                for (int i = 0; i < itemsPerPage; i++)
                {
                    cells[i * 4].Text = "";
                    cells[i * 4 + 1].Text = "";
                    cells[i * 4 + 2].Text = "";
                    cells[i * 4 + 3].Text = "";
                }
                switch (sort_combo_box.SelectedIndex)
                {
                    case 0:
                        SortByCategory();
                        break;
                    case 1:
                        SortByType();
                        break;
                    case 2:
                        SortByDate();
                        break;
                    case 3:
                        SortByAmount();
                        break;

                }
            }
        }

        private void SortByCategory()
        {
            // Use LINQ to order by category (alphabetically) TODO: research if alphabvetical is same as lexicographical sorting
            data = data.OrderBy(expense => expense.category).ToList();
            DrawPagination(currentPage);
        }
        private void SortByType()
        {
            data = data.OrderBy(expense => expense.type).ToList();
            DrawPagination(currentPage);
        }
        private void SortByDate()
        {
            data = data.OrderBy(expense => expense.unixTime).ToList();
            DrawPagination(currentPage);
        }
        private void SortByAmount()
        {
            data = data.OrderByDescending(expense => expense.amount).ToList();
            DrawPagination(currentPage);
        }

        private void ApplyTheme(Theme theme)
        {
            // Modify theme for sidepanel components
            logo_label.BackColor = theme.mantle;
            logo_label.ForeColor = theme.foreground;

            category_label.BackColor = theme.mantle;
            category_label.ForeColor = theme.foreground;
            type_label.BackColor = theme.mantle;
            type_label.ForeColor = theme.foreground;
            amount_label.BackColor = theme.mantle;
            amount_label.ForeColor = theme.foreground;
            date_label.BackColor = theme.mantle;
            date_label.ForeColor = theme.foreground;

            category_input.BackColor = theme.mantle;
            category_input.ForeColor = theme.foreground;
            type_input.BackColor = theme.mantle;
            type_input.ForeColor = theme.foreground;
            amount_input.BackColor = theme.mantle;
            amount_input.ForeColor = theme.foreground;
            date_picker.BackColor = theme.mantle;
            date_picker.ForeColor = theme.foreground;

            add_button.BackColor = theme.mantle;
            add_button.ForeColor = theme.foreground;
            import_button.BackColor = theme.mantle;
            import_button.ForeColor = theme.foreground;
            export_button.BackColor = theme.mantle;
            export_button.ForeColor = theme.foreground;

            // Modify theme for date-range panel components
            // month.BackColor     = theme.mantle;
            // month.ForeColor     = theme.foreground;
            choose_date_label.BackColor = theme.mantle;
            choose_date_label.ForeColor = theme.foreground;
            start_date_input.BackColor = theme.mantle;
            start_date_input.ForeColor = theme.foreground;
            separator.BackColor = theme.mantle;
            separator.ForeColor = theme.foreground;
            end_date_input.BackColor = theme.mantle;
            end_date_input.ForeColor = theme.foreground;
            // year.BackColor      = theme.mantle;
            // year.ForeColor      = theme.foreground;

            // Modify theme for search and sort components
            search_label.BackColor = theme.crust;
            search_label.ForeColor = theme.foreground;
            search_box.BackColor = theme.crust;
            search_box.ForeColor = theme.foreground;

            sort_label.BackColor = theme.crust;
            sort_label.ForeColor = theme.foreground;
            sort_combo_box.BackColor = theme.crust;
            sort_combo_box.ForeColor = theme.foreground;

            // Modify theme for Data Table panel components
            category_column.BackColor = theme.mantle;
            type_column.BackColor = theme.mantle;
            date_column.BackColor = theme.mantle;
            amount_column.BackColor = theme.mantle;

            category_column.ForeColor = theme.foreground;
            type_column.ForeColor = theme.foreground;
            date_column.ForeColor = theme.foreground;
            amount_column.ForeColor = theme.foreground;

            total_amount_label.BackColor = theme.mantle;
            total_amount_label.ForeColor = theme.foreground;

            transaction_label.BackColor = theme.mantle;
            transaction_label.ForeColor = theme.foreground;
            total_amount_holder.BackColor = theme.mantle;
            total_amount_holder.ForeColor = theme.green;

            transaction_amount_holder.BackColor = theme.mantle;
            transaction_amount_holder.ForeColor = theme.foreground;

            // Modify the theme for the table cells
            for (int i = 0; i < this.cells.Length; i += 4)
            {
                cells[i].BackColor = theme.mantle;
                cells[i + 1].BackColor = theme.mantle;
                cells[i + 2].BackColor = theme.mantle;
                cells[i + 3].BackColor = theme.mantle;

                cells[i].ForeColor = theme.foreground;
                cells[i + 1].ForeColor = theme.foreground;
                cells[i + 2].ForeColor = theme.foreground;
                cells[i + 3].ForeColor = theme.foreground;

                cells[i].Text = "";
                cells[i + 1].Text = "";
                cells[i + 2].Text = "";
                cells[i + 3].Text = "";
            }

            // Modify pagination theme
            previous_button.BackColor = theme.mantle;
            previous_button.ForeColor = theme.foreground;
            previous_button.FlatStyle = FlatStyle.Flat;
            previous_button.FlatAppearance.BorderSize = 0;
            page_number.BackColor = theme.mantle;
            page_number.ForeColor = theme.foreground;
            page_number.FlatStyle = FlatStyle.Flat;
            page_number.FlatAppearance.BorderSize = 0;
            next_button.BackColor = theme.mantle;
            next_button.ForeColor = theme.foreground;
            next_button.FlatStyle = FlatStyle.Flat;
            next_button.FlatAppearance.BorderSize = 0;

            filter_date.BackColor = theme.mantle;
            filter_date.ForeColor = theme.foreground;
            filter_date.FlatStyle = FlatStyle.Flat;
            filter_date.FlatAppearance.BorderSize = 1;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void logo_text_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void label1_Click_2(object sender, EventArgs e)
        {

        }

        private void sort_label_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_3(object sender, EventArgs e)
        {

        }





        private void category_row_1_Click(object sender, EventArgs e)
        {

        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click_1(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void label23_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_4(object sender, EventArgs e)
        {

        }



        private void label1_Click_5(object sender, EventArgs e)
        {

        }


    }
}
