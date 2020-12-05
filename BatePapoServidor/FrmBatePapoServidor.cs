using System.Data;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace BatePapoServidor
{
    public partial class FrmBatePapoServidor : Form
    {

        private DataSet ds;
        private DataTable dt;
        private delegate void AtualizaStatuCall(string Stmgs);
        
        public FrmBatePapoServidor()
        {
            InitializeComponent();
        }

        private void btnInicio_Click(object sender, System.EventArgs e)
        {
            DataRow dr = dt.NewRow();
            dr[0] = txtIP.Text;
            dt.Rows.Add(dr);
            ds.WriteXml("c:/BatePapo/logServidor.xml");

            if (txtIP.Text == string.Empty)
            {
                MessageBox.Show("Informe o Endereço do IP.");
                txtIP.Focus();
                return;
            }

            try
            {
                IPAddress AddressIP = IPAddress.Parse(txtIP.Text);
                BatePapo servidor = new BatePapo(AddressIP);
                BatePapo.StatusChanged += new StatuEventHandler(servidor_StatusChanged);
                servidor.IniciaBatePapo();
                txtLogMgs.AppendText("Monitorando Bate Papo.... \r\n");
            }
            catch (System.Exception erro)
            {

                MessageBox.Show("Erro de Conexão: " + erro.Message);
            }
        }

        public void servidor_StatusChanged(object sender, StatuEventArgs e)
        {
            this.Invoke(new AtualizaStatuCall(this.AtualizaStatu), new object[] { e.EventMgs });
        }

        private void AtualizaStatu(string stMgs)
        {
            DataRow dr = dt.NewRow();
            dr[1] = txtLogMgs.Text;
            dt.Rows.Add(dr);
            ds.WriteXml("c:/BatePapo/logServidor.xml");
            txtLogMgs.AppendText(stMgs + " \r\n");
        }

        private void FrmBatePapoServidor_Load(object sender, System.EventArgs e)
        {
            ds = new DataSet();

            try
            {
                ds.ReadXml("c:/BatePapo/logServidor.xml");
                dt = ds.Tables["Servidor"];
            }
            catch (FileNotFoundException ex)
            {
                dt = new DataTable("Servidor");
                dt.Columns.Add("ServidorIP");
                dt.Columns.Add("Msg");
                ds.Tables.Add(dt);
            }
        }

    }
}
