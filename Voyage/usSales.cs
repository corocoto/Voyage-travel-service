﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace Voyage
{
    public partial class usSales : UserControl
    {
        SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["SqlCon"].ConnectionString);
        DataTable dt;
        SqlDataAdapter adapter;
        BindingSource bs;
        public usSales()
        {
            InitializeComponent();
            LoadDataFromTable();
            this.ForeColor = Color.FromArgb(0, 71, 160);
        }

        //начальная настройка для отображения и работы с данными
        void starterSettings()
        {
            dt = new DataTable();
            adapter.Fill(dt);
            bs = new BindingSource();
            bs.DataSource = dt;
            textAboutSale.Text = $"При покупке билетов группой от 3-ех и более человек " +
            $"предоставляется скидка {Convert.ToString(((DataRowView)this.bs.Current).Row["Sale"])}%" +
            $" каждому";
        }

        //переход к след. пункту
        private void addNewClientsWithSales_Click(object sender, EventArgs e)
        {
            SqlCommand command = new SqlCommand("SELECT Count(ID_Client) from tGroupsClients where ID_Group=" + lIdOfGroup.Text, connection);
            connection.Open();
            int freeCount = Convert.ToInt32(lCountOfGroup.Text) - Convert.ToInt32(command.ExecuteScalar()) - Convert.ToInt32(nudCountOfPeople.Value);
            if (freeCount < 0)
            {
                MessageBox.Show("Извините, на данный момент все свободные места закончились", "Лимит свободных мест", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                int AbroadDoc = 0;
                if (lCountry.Text != "Россия")
                {
                    AbroadDoc = 1;
                } 
                ClientsWithSales cws = new ClientsWithSales(Convert.ToInt32(lIdOfGroup.Text), Convert.ToInt32(nudCountOfPeople.Value), AbroadDoc);
                cws.ShowDialog();
            }
            connection.Close();
        }

        //загрузка данных
        void LoadDataFromTable()
        {
            try
            {
                adapter = new SqlDataAdapter("SELECT tRoutes.ID_Route, tRoutes.sCountry, tRoutes.sNameOfRoute, tRoutes.Sale, " +
                    "tGroups.ID_Group, tGroups.sCount, tGroups.sName FROM tGroups INNER JOIN " +
                    "tGroupsRoutes ON tGroups.ID_Group =tGroupsRoutes.ID_Group " +
                    "inner join tRoutes ON tGroupsRoutes.ID_Route = tRoutes.ID_Route", connection);
                starterSettings();
                cbNameOfRoute.DataSource = bs;
                cbNameOfRoute.ValueMember = "ID_Route";
                cbNameOfRoute.DisplayMember = "sNameOfRoute";
                lIdOfGroup.DataBindings.Clear();
                lIdOfGroup.DataBindings.Add(new Binding("Text", bs, "ID_Group"));
                lCountOfGroup.DataBindings.Clear();
                lCountOfGroup.DataBindings.Add(new Binding("Text", bs, "sCount"));
                lCountry.DataBindings.Clear();
                lCountry.DataBindings.Add(new Binding("Text", bs, "sCountry"));
            }
            catch (NullReferenceException)
            {
                MessageBox.Show(
                "Прежде чем начать работу с разделом 'Скидки', необходимо добавить группу",
                "Предупреждение",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
                mainElements.Visible=false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        
        //обновление данных о скидке на выбранный маршрут
        private void cbNameOfRoute_DropDownClosed(object sender, EventArgs e)
        {
            adapter = new SqlDataAdapter("SELECT Sale from tRoutes where sNameOfRoute=" + "'" + 
            cbNameOfRoute.Text + "'", connection);
            starterSettings();
        }

    }
}