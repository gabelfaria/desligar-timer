using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace DesligarTimer
{
    internal static class Program
    {
        private const string NomeRegistro = "DesligarTimer";
        private const string CaminhoRegistro = @"Software\Microsoft\Windows\CurrentVersion\Run";

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                ConfigurarInicializacaoAutomatica();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "O aplicativo não conseguiu configurar a inicialização automática do Windows.\r\n\r\n" + ex.Message,
                    "Desligar Timer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            bool iniciarNaBandeja = Array.IndexOf(args, "--startup") >= 0;
            Application.Run(new MainForm(iniciarNaBandeja));
        }

        private static void ConfigurarInicializacaoAutomatica()
        {
            string comando = "\"" + Application.ExecutablePath + "\" --startup";
            using (RegistryKey chave = Registry.CurrentUser.CreateSubKey(CaminhoRegistro))
            {
                if (chave == null)
                    throw new InvalidOperationException("Não foi possível acessar a configuração de inicialização do usuário.");

                object existente = chave.GetValue(NomeRegistro);
                if (!string.Equals(Convert.ToString(existente), comando, StringComparison.OrdinalIgnoreCase))
                    chave.SetValue(NomeRegistro, comando, RegistryValueKind.String);
            }
        }
    }
}
