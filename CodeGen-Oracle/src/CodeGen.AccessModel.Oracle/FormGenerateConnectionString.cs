using CodeGen.AccessModel.Oracle.Utils;
using CodeGen.Plugin.Base;
using System;
using System.Windows.Forms;

namespace CodeGen.AccessModel.Oracle
{
    public partial class FormGenerateConnectionString : Form, IConnectionStringForm
    {
        #region properties
        #endregion

        #region initialization

        public FormGenerateConnectionString()
        {
            InitializeComponent();
        }

        #endregion

        #region methods

        public void LoadLocalVariables()
        {

        }

        private void CleanControls()
        {
            txtDataSource.Clear();
            txtUserID.Clear();
            txtPassword.Clear();
        }

        public string GetConnectionString()
        {
            return DatabaseUtils.CreateBasicConnectionString(txtDataSource.Text, txtUserID.Text, txtPassword.Text);
        }

        public bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(txtDataSource.Text))
            {
                MessageBoxHelper.ValidationMessage("Property Server was not set");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUserID.Text))
            {
                MessageBoxHelper.ValidationMessage("Property User ID was not set");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBoxHelper.ValidationMessage("Property Password was not set");
                return false;
            }

            return true;
        }

        #endregion

        #region events

        private void btnAccept_Click(object sender, EventArgs e)
        {
            try
            {
                if (ValidateForm())
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ProcessException(ex);
            }
        }

        #endregion
    }
}
