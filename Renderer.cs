using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace ExpenseTracker
{

    public struct Theme
    {
        public Color crust      { get; set; }
        public Color mantle     { get; set; }
        public Color foreground { get; set; }
        public Color green      { get; set; }
        //public string red { get; set; }
        
        // Documentation: for theming only
        // converts hexadecimals to appropriate data type that is accepted by the c# Winform
        public Theme(string crust, string mantle, string foreground, string green)
        {
            this.crust      = HexToColor(crust);
            this.mantle     = HexToColor(mantle);
            this.foreground = HexToColor(foreground);
            this.green      = HexToColor(green);
        }
        public static Color HexToColor(string hex)
        {
            return ColorTranslator.FromHtml(hex);
        }
    }

    // Documentation: this class draws various important components to make our programming more appealing
    // Renderer() is the constructor
    // RenderWindow()
    // - draws the actual window
    // - sets the size, border style, disables Maximize, sets the background color, sets the title and icon of the window
    // RenderShadowSidePanel()
    // - draws the shadow/box in the add data panel 
    // RenderShadowSidePanel()
    // - draws the shadow/box in the add panel 
    // RenderShadowDateRange()
    // - draws the shadow/box in the setting for date range panel 
    // RenderShadowDataTable()
    // - draws the lines you see in the table
    public class Renderer
    {
        private Theme theme;
        private Form form;

        public Renderer(Form1 form, Theme theme) 
        {
            this.form = form;
            this.theme = theme;
        }

        public void RenderWindow()
        {
            form.MinimumSize     = new Size(1280, 720);
            form.MaximumSize     = new Size(1280, 720);
            form.FormBorderStyle = FormBorderStyle.FixedSingle;
            form.MaximizeBox     = false;
            form.MinimizeBox     = true;
            form.SizeGripStyle   = SizeGripStyle.Hide;
            form.BackColor       = theme.crust;
            form.Text            = "Expense Tracker";
            form.Icon            = new Icon("logo.ico");
        }

        public void RenderShadowSidePanel(PaintEventArgs e)
        {
            // draw tracker main panel
            using (SolidBrush brush = new SolidBrush(theme.mantle))
            {
                // note(russel): Rectangle parameters are X, Y, Width, Height respectively.
                Rectangle rect = new Rectangle(20, 25, 380, 640);
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        public void RenderShadowDateRange(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(theme.mantle))
            {
                Rectangle rect = new Rectangle(413, 25, 840, 50);
                e.Graphics.FillRectangle(brush, rect);
            }
        }

        public void RenderShadowDataTable(PaintEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(theme.mantle))
            {
                Rectangle rect = new Rectangle(413, 163, 840, 502); 
                e.Graphics.FillRectangle(brush, rect);
            }

            using (Pen pen = new Pen(theme.foreground, 1))
            {
                //e.Graphics.DrawLine(pen, new Point(414, 205), new Point(1252, 205));
                //e.Graphics.DrawLine(pen, new Point(414, 245), new Point(1252, 245));
                //e.Graphics.DrawLine(pen, new Point(414, 285), new Point(1252, 285));
                int gap = 42;
                int numberOfLines = 11;
                for (int i = 0; i < numberOfLines; i++)
                {
                    int currentY = 205 + (i * gap); // Calculate the Y position for each line
                    e.Graphics.DrawLine(pen, new Point(414, currentY), new Point(1252, currentY));
                }
            }
        }
    }


}
