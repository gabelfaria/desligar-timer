using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace DesligarTimer
{
    internal sealed class MainForm : Form
    {
        private static readonly Color Navy = Color.FromArgb(27, 40, 56);
        private static readonly Color Accent = Color.FromArgb(0, 120, 212);
        private static readonly Color AccentHover = Color.FromArgb(0, 99, 177);
        private static readonly Color Page = Color.FromArgb(242, 245, 248);
        private static readonly Color Muted = Color.FromArgb(103, 116, 130);
        private static readonly Color White = Color.White;

        private Label lblHora;
        private Label lblRestante;
        private RadioButton rbDuracao;
        private RadioButton rbExata;
        private PickerNumber nudH;
        private PickerNumber nudM;
        private PickerNumber nudS;
        private Label lblHoras;
        private Label lblMinutos;
        private Label lblSegundos;
        private Label lblHoraExata;
        private MaskedTextBox mskHora;
        private ComboBox cboAcao;
        private Button btnAgendar;
        private Button btnCancelar;
        private Timer relogio;
        private DateTime? alvo;
        private Modo modo;
        private Acao acao;
        private NotifyIcon bandeja;
        private bool saindo;
        private readonly bool iniciarNaBandeja;
        private int duracaoHoras;
        private int duracaoMinutos;
        private int duracaoSegundos;
        private int horaExataHoras;
        private int horaExataMinutos;
        private int horaExataSegundos;

        public MainForm(bool iniciarNaBandeja)
        {
            this.iniciarNaBandeja = iniciarNaBandeja;
            alvo = null;
            modo = Modo.Temporizador;
            acao = Acao.Desligar;
            saindo = false;
            duracaoHoras = 0;
            duracaoMinutos = 30;
            duracaoSegundos = 0;
            DateTime proximoHorario = DateTime.Now.AddHours(1);
            horaExataHoras = proximoHorario.Hour;
            horaExataMinutos = proximoHorario.Minute;
            horaExataSegundos = 0;

            Text = "Desligar Timer";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = true;
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.Manual;
            ClientSize = new Size(340, 445);
            BackColor = Page;
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            DoubleBuffered = true;
            Icon appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Icon = appIcon ?? SystemIcons.Application;

            ConstruirInterface();
            ConstruirBandeja();
            PosicionarPertoDoRelogio();

            relogio = new Timer();
            relogio.Interval = 500;
            relogio.Tick += delegate { Atualizar(); };
            relogio.Start();
            AtualizarModo();
            Atualizar();

            FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                if (!saindo)
                {
                    e.Cancel = true;
                    ShowInTaskbar = false;
                    Hide();
                    if (bandeja != null)
                        bandeja.ShowBalloonTip(2500, "Desligar Timer", "A janela foi fechada e o programa continua na área de notificação.", ToolTipIcon.Info);
                }
            };

            Resize += delegate
            {
                if (WindowState == FormWindowState.Minimized && !this.iniciarNaBandeja && bandeja != null)
                {
                    ShowInTaskbar = false;
                    WindowState = FormWindowState.Normal;
                    Hide();
                    bandeja.ShowBalloonTip(2500, "Desligar Timer", "O programa foi minimizado para a área de notificação.", ToolTipIcon.Info);
                }
            };

            if (iniciarNaBandeja)
            {
                ShowInTaskbar = false;
                Shown += delegate
                {
                    BeginInvoke(new MethodInvoker(delegate { Hide(); }));
                };
            }
        }

        private void ConstruirInterface()
        {
            Panel topo = new Panel();
            topo.Bounds = new Rectangle(0, 0, 340, 61);
            topo.BackColor = Navy;
            Controls.Add(topo);

            Panel faixa = new Panel();
            faixa.Bounds = new Rectangle(0, 0, 4, 61);
            faixa.BackColor = Accent;
            topo.Controls.Add(faixa);

            Label titulo = CriarLabel("Desligar Timer", new Rectangle(18, 5, 300, 29), 15F, White, FontStyle.Bold, ContentAlignment.MiddleLeft);
            topo.Controls.Add(titulo);
            Label subtitulo = CriarLabel("Temporizador do Windows", new Rectangle(20, 34, 300, 17), 8.5F, Color.FromArgb(198, 211, 225), FontStyle.Regular, ContentAlignment.MiddleLeft);
            topo.Controls.Add(subtitulo);

            Panel painelRelogio = CriarCartao(new Rectangle(16, 73, 308, 62));
            Controls.Add(painelRelogio);
            Label legendaHora = CriarLabel("HORA ATUAL", new Rectangle(13, 5, 282, 17), 8F, Muted, FontStyle.Bold, ContentAlignment.MiddleLeft);
            painelRelogio.Controls.Add(legendaHora);
            lblHora = CriarLabel("00:00:00", new Rectangle(12, 20, 130, 30), 15F, Navy, FontStyle.Bold, ContentAlignment.MiddleLeft);
            painelRelogio.Controls.Add(lblHora);
            lblRestante = CriarLabel("PRONTO", new Rectangle(146, 19, 148, 25), 7.5F, Muted, FontStyle.Bold, ContentAlignment.MiddleRight);
            painelRelogio.Controls.Add(lblRestante);

            Label tituloConfig = CriarLabel("Configurar temporizador", new Rectangle(18, 148, 304, 24), 12F, Navy, FontStyle.Bold, ContentAlignment.MiddleLeft);
            Controls.Add(tituloConfig);
            Label ajudaConfig = CriarLabel("Role os números ou clique para editar.", new Rectangle(19, 171, 302, 17), 8.5F, Muted, FontStyle.Regular, ContentAlignment.MiddleLeft);
            Controls.Add(ajudaConfig);

            rbDuracao = CriarOpcao("Duração", new Rectangle(16, 195, 150, 30));
            rbExata = CriarOpcao("Hora exata", new Rectangle(174, 195, 150, 30));
            rbDuracao.Checked = true;
            rbDuracao.CheckedChanged += delegate { AtualizarModo(); };
            rbExata.CheckedChanged += delegate { AtualizarModo(); };
            Controls.Add(rbDuracao);
            Controls.Add(rbExata);

            Panel painelEntrada = CriarCartao(new Rectangle(16, 234, 308, 86));
            Controls.Add(painelEntrada);

            lblHoras = CriarLabel("HORAS", new Rectangle(15, 7, 76, 15), 7.5F, Muted, FontStyle.Bold, ContentAlignment.MiddleCenter);
            lblMinutos = CriarLabel("MINUTOS", new Rectangle(116, 7, 76, 15), 7.5F, Muted, FontStyle.Bold, ContentAlignment.MiddleCenter);
            lblSegundos = CriarLabel("SEGUNDOS", new Rectangle(217, 7, 76, 15), 7.5F, Muted, FontStyle.Bold, ContentAlignment.MiddleCenter);
            painelEntrada.Controls.Add(lblHoras);
            painelEntrada.Controls.Add(lblMinutos);
            painelEntrada.Controls.Add(lblSegundos);

            nudH = CriarNumero(new Rectangle(18, 25, 76, 53), 23, 0, "HORAS");
            nudM = CriarNumero(new Rectangle(116, 25, 76, 53), 59, 30, "MIN");
            nudS = CriarNumero(new Rectangle(214, 25, 76, 53), 59, 0, "SEG");
            painelEntrada.Controls.Add(nudH);
            painelEntrada.Controls.Add(nudM);
            painelEntrada.Controls.Add(nudS);
            Label separador1 = CriarLabel(":", new Rectangle(94, 27, 22, 48), 22F, Color.FromArgb(104, 119, 132), FontStyle.Regular, ContentAlignment.MiddleCenter);
            Label separador2 = CriarLabel(":", new Rectangle(192, 27, 22, 48), 22F, Color.FromArgb(104, 119, 132), FontStyle.Regular, ContentAlignment.MiddleCenter);
            painelEntrada.Controls.Add(separador1);
            painelEntrada.Controls.Add(separador2);

            lblHoraExata = CriarLabel("HORÁRIO (24H)", new Rectangle(20, 7, 268, 15), 8F, Muted, FontStyle.Bold, ContentAlignment.MiddleCenter);
            lblHoraExata.Visible = false;
            painelEntrada.Controls.Add(lblHoraExata);
            mskHora = new MaskedTextBox();
            mskHora.Mask = "00:00";
            mskHora.PromptChar = '0';
            mskHora.Text = DateTime.Now.AddHours(1).ToString("HHmm");
            mskHora.TextAlign = HorizontalAlignment.Center;
            mskHora.Font = new Font("Segoe UI", 13F, FontStyle.Regular, GraphicsUnit.Point);
            mskHora.ForeColor = Navy;
            mskHora.BackColor = White;
            mskHora.BorderStyle = BorderStyle.FixedSingle;
            mskHora.Size = new Size(110, 31);
            mskHora.Location = new Point(140, 39);
            mskHora.Visible = false;
            painelEntrada.Controls.Add(mskHora);

            Label legendaAcao = CriarLabel("AO FINALIZAR", new Rectangle(18, 329, 304, 16), 8F, Muted, FontStyle.Bold, ContentAlignment.MiddleLeft);
            Controls.Add(legendaAcao);
            cboAcao = new ComboBox();
            cboAcao.Bounds = new Rectangle(16, 347, 308, 30);
            cboAcao.DropDownStyle = ComboBoxStyle.DropDownList;
            cboAcao.FlatStyle = FlatStyle.Flat;
            cboAcao.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            cboAcao.ForeColor = Navy;
            cboAcao.BackColor = White;
            cboAcao.Items.AddRange(new object[] { "Desligar o Windows", "Suspender (Sleep)", "Hibernar" });
            cboAcao.SelectedIndex = 0;
            cboAcao.SelectedIndexChanged += delegate { acao = (Acao)cboAcao.SelectedIndex; };
            Controls.Add(cboAcao);

            btnAgendar = CriarBotao("INICIAR TEMPORIZADOR", new Rectangle(16, 388, 198, 36), Accent, White, true);
            btnCancelar = CriarBotao("CANCELAR", new Rectangle(220, 388, 104, 36), White, Navy, false);
            btnCancelar.Enabled = false;
            btnAgendar.Click += delegate { Agendar(); };
            btnCancelar.Click += delegate { Cancelar("Temporizador cancelado."); };
            Controls.Add(btnAgendar);
            Controls.Add(btnCancelar);

            Label rodape = CriarLabel("O aplicativo fica disponível na área de notificação.", new Rectangle(18, 428, 304, 15), 7.5F, Muted, FontStyle.Regular, ContentAlignment.MiddleCenter);
            Controls.Add(rodape);
        }

        private void ConstruirBandeja()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Abrir", null, delegate { MostrarJanela(); });
            menu.Items.Add("Cancelar temporizador", null, delegate { Cancelar("Temporizador cancelado."); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Sair", null, delegate { Sair(); });

            bandeja = new NotifyIcon();
            bandeja.Icon = Icon;
            bandeja.Text = "Desligar Timer - sem temporizador";
            bandeja.ContextMenuStrip = menu;
            bandeja.Visible = true;
            bandeja.DoubleClick += delegate { MostrarJanela(); };
        }

        private static Panel CriarCartao(Rectangle bounds)
        {
            Panel panel = new Panel();
            panel.Bounds = bounds;
            panel.BackColor = White;
            return panel;
        }

        private static Label CriarLabel(string texto, Rectangle bounds, float tamanho, Color cor, FontStyle estilo, ContentAlignment alinhamento)
        {
            Label label = new Label();
            label.Text = texto;
            label.Bounds = bounds;
            label.AutoSize = false;
            label.Font = new Font("Segoe UI", tamanho, estilo, GraphicsUnit.Point);
            label.ForeColor = cor;
            label.BackColor = Color.Transparent;
            label.TextAlign = alinhamento;
            return label;
        }

        private static RadioButton CriarOpcao(string texto, Rectangle bounds)
        {
            RadioButton radio = new MetroOption();
            radio.Text = texto;
            radio.Bounds = bounds;
            radio.Appearance = Appearance.Button;
            radio.FlatStyle = FlatStyle.Flat;
            radio.FlatAppearance.BorderSize = 0;
            radio.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 237, 248);
            radio.BackColor = Color.FromArgb(229, 234, 240);
            radio.ForeColor = Muted;
            radio.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            radio.TextAlign = ContentAlignment.MiddleCenter;
            radio.Cursor = Cursors.Hand;
            radio.TabStop = false;
            radio.UseVisualStyleBackColor = false;
            return radio;
        }

        private static PickerNumber CriarNumero(Rectangle bounds, int maximo, int inicial, string unidade)
        {
            PickerNumber numero = new PickerNumber(maximo, inicial, unidade);
            numero.Bounds = bounds;
            return numero;
        }

        private static Button CriarBotao(string texto, Rectangle bounds, Color fundo, Color textoCor, bool principal)
        {
            Button button = new Button();
            button.Text = texto;
            button.Bounds = bounds;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = principal ? 0 : 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(202, 211, 220);
            button.FlatAppearance.MouseOverBackColor = principal ? AccentHover : Color.FromArgb(232, 238, 244);
            button.BackColor = fundo;
            button.ForeColor = textoCor;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            button.Cursor = Cursors.Hand;
            button.UseVisualStyleBackColor = false;
            return button;
        }

        private void AtualizarModo()
        {
            Modo novoModo = rbExata.Checked ? Modo.HoraExata : Modo.Temporizador;
            if (novoModo != modo)
            {
                if (modo == Modo.Temporizador)
                {
                    duracaoHoras = (int)nudH.Value;
                    duracaoMinutos = (int)nudM.Value;
                    duracaoSegundos = (int)nudS.Value;
                }
                else
                {
                    horaExataHoras = (int)nudH.Value;
                    horaExataMinutos = (int)nudM.Value;
                    horaExataSegundos = (int)nudS.Value;
                }

                modo = novoModo;
                if (modo == Modo.Temporizador)
                {
                    nudH.Value = duracaoHoras;
                    nudM.Value = duracaoMinutos;
                    nudS.Value = duracaoSegundos;
                }
                else
                {
                    nudH.Value = horaExataHoras;
                    nudM.Value = horaExataMinutos;
                    nudS.Value = horaExataSegundos;
                }
            }

            bool porDuracao = modo == Modo.Temporizador;
            nudH.Visible = true;
            nudM.Visible = true;
            nudS.Visible = true;
            lblHoras.Visible = true;
            lblMinutos.Visible = true;
            lblSegundos.Visible = true;
            mskHora.Visible = false;
            lblHoraExata.Visible = false;

            rbDuracao.BackColor = porDuracao ? Accent : Color.FromArgb(229, 234, 240);
            rbDuracao.ForeColor = porDuracao ? White : Muted;
            rbExata.BackColor = !porDuracao ? Accent : Color.FromArgb(229, 234, 240);
            rbExata.ForeColor = !porDuracao ? White : Muted;
        }

        private void Agendar()
        {
            DateTime horario;
            if (modo == Modo.Temporizador)
            {
                int segundos = (int)nudH.Value * 3600 + (int)nudM.Value * 60 + (int)nudS.Value;
                if (segundos <= 0)
                {
                    MessageBox.Show("Informe uma duração maior que zero.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                horario = DateTime.Now.AddSeconds(segundos);
            }
            else
            {
                int horas = (int)nudH.Value;
                int minutos = (int)nudM.Value;
                int segundos = (int)nudS.Value;
                horario = DateTime.Today.AddHours(horas).AddMinutes(minutos).AddSeconds(segundos);
                if (horario <= DateTime.Now)
                {
                    MessageBox.Show("Esse horário já passou hoje.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            acao = (Acao)cboAcao.SelectedIndex;
            string pergunta = "Deseja realmente " + NomeAcao().ToLowerInvariant() + " o Windows às " + horario.ToString("HH:mm:ss") + "?";
            if (MessageBox.Show(pergunta, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            alvo = horario;
            DefinirControlesAtivos(false);
            Atualizar();
        }

        private void DefinirControlesAtivos(bool ativos)
        {
            rbDuracao.Enabled = ativos;
            rbExata.Enabled = ativos;
            nudH.Enabled = ativos;
            nudM.Enabled = ativos;
            nudS.Enabled = ativos;
            cboAcao.Enabled = ativos;
            btnAgendar.Enabled = ativos;
            btnCancelar.Enabled = !ativos;
        }

        private void Cancelar(string mensagem)
        {
            if (!alvo.HasValue)
                return;

            alvo = null;
            DefinirControlesAtivos(true);
            lblRestante.Text = "PRONTO";
            AtualizarTooltip();
            bandeja.ShowBalloonTip(2500, "Desligar Timer", mensagem, ToolTipIcon.Info);
        }

        private void Atualizar()
        {
            lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
            if (alvo.HasValue)
            {
                TimeSpan restante = alvo.Value - DateTime.Now;
                if (restante.TotalSeconds <= 0)
                {
                    Executar();
                    return;
                }
                lblRestante.Text = "ATIVO";
                nudH.Value = (int)restante.TotalHours;
                nudM.Value = restante.Minutes;
                nudS.Value = restante.Seconds;
            }
            AtualizarTooltip();
        }

        private void AtualizarTooltip()
        {
            string texto = alvo.HasValue
                ? NomeAcaoVerbo() + " em " + Formatar(alvo.Value - DateTime.Now) + " (" + alvo.Value.ToString("HH:mm") + ")"
                : "Desligar Timer - sem temporizador";
            if (texto.Length > 63)
                texto = texto.Substring(0, 63);
            bandeja.Text = texto;
        }

        private string NomeAcaoVerbo()
        {
            if (acao == Acao.Suspender) return "Suspende";
            if (acao == Acao.Hibernar) return "Hiberna";
            return "Desliga";
        }

        private string NomeAcao()
        {
            if (acao == Acao.Suspender) return "suspensão";
            if (acao == Acao.Hibernar) return "hibernação";
            return "desligamento";
        }

        private void Executar()
        {
            DateTime horario = alvo.Value;
            alvo = null;
            DefinirControlesAtivos(true);
            lblRestante.Text = "PRONTO";

            try
            {
                if (acao == Acao.Desligar)
                    Process.Start("shutdown.exe", "/s /t 0");
                else if (acao == Acao.Suspender)
                    Application.SetSuspendState(PowerState.Suspend, true, false);
                else
                    Application.SetSuspendState(PowerState.Hibernate, true, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Não foi possível executar a ação: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            AtualizarTooltip();
            bandeja.ShowBalloonTip(3000, "Desligar Timer", NomeAcao() + " executado às " + horario.ToString("HH:mm:ss") + ".", ToolTipIcon.Info);
        }

        private static string Formatar(TimeSpan tempo)
        {
            return string.Format("{0:00}:{1:00}:{2:00}", (int)tempo.TotalHours, tempo.Minutes, tempo.Seconds);
        }

        private void MostrarJanela()
        {
            PosicionarPertoDoRelogio();
            ShowInTaskbar = true;
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }

        private void PosicionarPertoDoRelogio()
        {
            Rectangle area = Screen.FromPoint(Cursor.Position).WorkingArea;
            Location = new Point(area.Right - Width - 18, area.Bottom - Height - 18);
        }

        private void Sair()
        {
            saindo = true;
            if (bandeja != null)
                bandeja.Visible = false;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (relogio != null)
                    relogio.Dispose();
                if (bandeja != null)
                {
                    bandeja.Visible = false;
                    bandeja.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }

    internal sealed class PickerNumber : UserControl
    {
        private readonly int maximum;
        private readonly TextBox input;
        private int value;

        public PickerNumber(int maximum, int initialValue, string unit)
        {
            this.maximum = maximum;
            value = Math.Max(0, Math.Min(maximum, initialValue));
            BackColor = Color.FromArgb(35, 48, 60);
            ForeColor = Color.FromArgb(231, 239, 246);
            BorderStyle = BorderStyle.None;
            TabStop = true;
            Cursor = Cursors.IBeam;

            input = new TextBox();
            input.Dock = DockStyle.Fill;
            input.Multiline = true;
            input.BorderStyle = BorderStyle.None;
            input.TextAlign = HorizontalAlignment.Center;
            input.Font = new Font("Segoe UI Light", 25F, FontStyle.Regular, GraphicsUnit.Point);
            input.ForeColor = Color.FromArgb(233, 240, 246);
            input.BackColor = Color.FromArgb(35, 48, 60);
            input.Padding = new Padding(0, 3, 0, 0);
            input.MaxLength = 2;
            input.ShortcutsEnabled = true;
            input.AccessibleName = unit;
            input.KeyPress += ValidarTecla;
            input.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.KeyCode == Keys.Enter)
                {
                    ConfirmarTexto();
                    e.SuppressKeyPress = true;
                }
            };
            input.Leave += delegate { ConfirmarTexto(); };
            input.MouseWheel += delegate(object sender, MouseEventArgs e) { Alterar(e.Delta > 0 ? 1 : -1); };
            input.Enter += delegate { input.SelectAll(); };
            Controls.Add(input);

            MouseWheel += delegate(object sender, MouseEventArgs e) { Alterar(e.Delta > 0 ? 1 : -1); };
            UpdateDisplay();
        }

        public decimal Value
        {
            get { return value; }
            set
            {
                int novo = Math.Max(0, Math.Min(maximum, Decimal.ToInt32(value)));
                if (this.value == novo)
                {
                    UpdateDisplay();
                    return;
                }
                this.value = novo;
                UpdateDisplay();
                if (ValueChanged != null)
                    ValueChanged(this, EventArgs.Empty);
            }
        }

        public event EventHandler ValueChanged;

        private void ValidarTecla(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void Alterar(int delta)
        {
            int novo = value + delta;
            if (novo > maximum)
                novo = 0;
            if (novo < 0)
                novo = maximum;
            Value = novo;
        }

        private void UpdateDisplay()
        {
            if (input != null && !input.Focused)
                input.Text = value.ToString("00");
        }

        private void ConfirmarTexto()
        {
            int novo;
            if (!int.TryParse(input.Text, out novo))
                novo = value;
            Value = novo;
        }
    }

    internal sealed class MetroOption : RadioButton
    {
        public MetroOption()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
        }

        protected override bool ShowFocusCues
        {
            get { return false; }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(BackColor);
            TextRenderer.DrawText(
                e.Graphics,
                Text,
                Font,
                ClientRectangle,
                ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
