﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GenericPdv
{
    public partial class FrenteDeCaixa : Form
    {
        public FrenteDeCaixa()
        {
            InitializeComponent();
            cpf = null;
        }
        //Adaptadores
        DataSetGnPdvTableAdapters.ItensDaVendaTableAdapter itensVenda = new DataSetGnPdvTableAdapters.ItensDaVendaTableAdapter();
        DataSetGnPdvTableAdapters.VendaTableAdapter venda = new DataSetGnPdvTableAdapters.VendaTableAdapter();
        DataSetGnPdvTableAdapters.ProdutoTableAdapter produto = new DataSetGnPdvTableAdapters.ProdutoTableAdapter();
        DataSetGnPdvTableAdapters.FuncionarioTableAdapter funcionario = new DataSetGnPdvTableAdapters.FuncionarioTableAdapter();
        DataSetGnPdvTableAdapters.CaixaTableAdapter caixa = new DataSetGnPdvTableAdapters.CaixaTableAdapter();
        //Variaveis Globais
        public double valorTotal { get; set; }
        public string[,] itensDaLista { get; set; }

        bool cpfNota = true;
        int count = 1;
        string[] itens = new string[6];
        public string cpf { get; set; }

        private void txtBusca_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (string.IsNullOrEmpty(txtPesquisa.Text)) { return; }
            // verificar nome de produto para pesquisa
            if (e.KeyChar == (char)13)
            {
                try
                {
                    //pergunda se precisa de cpf 
                    if (cpfNota)
                    {
                        Cpf cpfJanela = new Cpf();
                        cpfJanela.ShowDialog();
                        cpf = cpfJanela.cpfCli;
                        cpfNota = false;
                        lbStatus.Text = "Caixa Ocupado.";

                        btConsultar.Enabled = true;
                        btSangria.Enabled = false;
                        btFechamento.Enabled = false;
                        btFinalizar.Enabled = true;
                        btCancelarVenda.Enabled = true;
                        btRemover.Enabled = true;
                    }

                    var aux = Convert.ToDouble(txtQtd.Text);
                    var prod = produto.GetDataById(Convert.ToInt32(txtPesquisa.Text));
                    // se o produto for ativo
                    if (Convert.ToBoolean(prod[0]["prodStatus"]) == true)
                    {
                        itens[0] = count.ToString();
                        itens[1] = txtCodProd.Text = (prod[0]["idProduto"].ToString());
                        itens[2] = prod[0]["prodNome"].ToString();
                        lbNomeProduto.Text = prod[0]["prodNome"].ToString().ToUpper();
                        itens[3] = txtQtd.Text;
                        txtQuantidade.Text = txtQtd.Text + " UN";

                        // verificar se o produto em um valor em oferta e atribuir ao valor unitario
                        DateTime data1 = DateTime.Today;
                        if (DateTime.TryParse(prod[0]["prodDataInicio"].ToString(), out data1).Equals(true) && DateTime.TryParse(prod[0]["prodDataFim"].ToString(), out data1).Equals(true))
                        {
                            txtValorUni.Text = string.Format("{0,-10:C}", Convert.ToDouble(prod[0]["prodVenda"]));
                            itens[4] = prod[0]["prodDesconto"].ToString();
                        }
                        else
                        {
                            txtValorUni.Text = string.Format("{0,-10:C}", Convert.ToDouble(prod[0]["prodVenda"]));
                            itens[4] = prod[0]["prodVenda"].ToString();
                        }

                        aux = aux * Convert.ToDouble(itens[4]);
                        txtSubTotal.Text = string.Format("{0,-10:C}", aux);
                        valorTotal += aux;
                        itens[5] = aux.ToString();
                        txtValorTotal.Text = string.Format("{0,-10:C}", valorTotal);
                        txtPesquisa.Text = "";
                        txtPesquisa.Focus();

                        ListViewItem item = new ListViewItem(itens[0]);
                        for (int i = 1; i <= 5; i++)
                        {
                            item.SubItems.Add(itens[i]);
                        }
                        listView.Items.Add(item);
                        count++;
                        txtQtd.Text = "1";
                    }
                    else
                    {
                        Alerta alerta = new Alerta("Não encontrado.");
                        alerta.ShowDialog();
                        txtPesquisa.Focus();
                    }
                }
                catch (Exception ex)
                {
                    Alerta alerta = new Alerta("Não encontrado.");
                    alerta.ShowDialog();
                    txtPesquisa.Focus();
                }
            }
        }

        private void btRemover_Click(object sender, EventArgs e)
        {
            // trocar focus para a listview
            listView.Focus();
            //navegar pela setas cima e baixo
            // ao apertar enter 
            // remover o item selecionado atualizar preço

            foreach (ListViewItem eachItem in listView.SelectedItems)
            {

                listView.Items.Remove(eachItem);
            }


            //if (listView.Items.Count > 0)
            //{
            //    listView.Items.Remove(listView.SelectedItems[0]);
            //}
        }
        
        private void btFinalizar_Click(object sender, EventArgs e)
        {
            // verificar erro de objeto e pq não esta aceitando o 
            
            itensDaLista = new string[this.listView.Items.Count , 6];

            for (int i = 0; i < this.listView.Items.Count; i++)
            {
                for (int j = 0; j <= 5; j++)
                {
                    //passando os itens e colunas para uma variavel publica que vai receber os itens e salvar no  banco na tela pagamento
                    itensDaLista[i , j] = listView.Items[i].SubItems[j].Text;
                }
            }
            Pagamento pagamento = new Pagamento(itensDaLista, valorTotal, cpf, this);
            pagamento.ShowDialog();

            // limpando array dos itens ja salvos
            for (int i = 0; i < itens.Length; i++)
            {
                itens[i] = "";
            }
            
            itens = new string[6];
            listView.Items.Clear();
            count = 0;
            cpfNota = true;
            cpf = null;
            valorTotal = 0.0;
            txtCodProd.Text = "";
            txtPesquisa.Text = "";
            txtQuantidade.Text = "";
            txtSubTotal.Text = "";
            txtValorUni.Text = "";
            txtValorTotal.Text = "";
            lbNomeProduto.Text = "";
        }

        private void btClouse_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void btMinimized_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btConsultar_Click(object sender, EventArgs e)
        {
            // verificar se ha alguma operação em aberto
            consulta consulta = new consulta();
            consulta.ShowDialog();
        }

        private void btSangria_Click(object sender, EventArgs e)
        {
            AutenticacaoValidacao autenticacao = new AutenticacaoValidacao(2);
            autenticacao.ShowDialog();
            lbStatus.Text = "Caixa em Sangria.";

        }

        private void frenteDeCaixa_Load(object sender, EventArgs e)
        {
            lbNomeUsuario.Text = "Vendedor: " + AutenticacaoDeFuncionario.funcLogado.ToString();
            
            lbStatus.Text = "Caixa aberto - Livre";
            btFinalizar.Enabled = false;
            btCancelarVenda.Enabled = false;
            btRemover.Enabled = false;

            txtQtd.Text = "1";


        }

        private void btFechamento_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Deseja fazer o fechamento do Caixa?", "Confirmar", MessageBoxButtons.YesNo) == DialogResult.Yes) {
                
                AutenticacaoValidacao autenticacao = new AutenticacaoValidacao(3);
                autenticacao.ShowDialog();
                this.Dispose();
            }
        }

        private void btCancelarVenda_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Deseja realmente cancelar esta venda?", "Cancelar", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                limpar();
                Alerta alerta = new Alerta("Venda Cancelada.");
                alerta.ShowDialog();
            }
        }

        private void HotKey_KeyDown(object sender, KeyPressEventArgs e)
        {
            //fechar venda
            if(e.KeyChar == (char)112)
            {
                btFechamento_Click(null, null);
            }
            //Consultar Item
            if (e.KeyChar == (char)113)
            {

            }
            //Remover item
            if (e.KeyChar == (char)114)
            {

            }
            //Cancelar Venda
            if (e.KeyChar == (char)115)
            {

            }
            //Sangria
            if (e.KeyChar == (char)116)
            {

            }
            //Fechamento
            if (e.KeyChar == (char)117)
            {

            }
            //Trocar usuario
            if (e.KeyChar == (char)118)
            {

            }
        }

        private void btLogout_Click(object sender, EventArgs e)
        {
            AutenticacaoValidacao autenticacao = new AutenticacaoValidacao(0);
        }

        public void limpar()
        {
            itens = new string[6];
            listView.Items.Clear();
            count = 0;
            cpfNota = true;
            cpf = null;
            valorTotal = 0.0;
            txtCodProd.Text = "";
            txtPesquisa.Text = "";
            txtQuantidade.Text = "";
            txtSubTotal.Text = "";
            txtValorUni.Text = "";
            txtValorTotal.Text = "";
            lbNomeProduto.Text = "";

            btConsultar.Enabled = true;
            btSangria.Enabled = true;
            btFechamento.Enabled = true;
            lbStatus.Text = "Caixa aberto - Livre";
            btFinalizar.Enabled = false;
            btCancelarVenda.Enabled = false;
            btRemover.Enabled = false;

        }
    }
}
