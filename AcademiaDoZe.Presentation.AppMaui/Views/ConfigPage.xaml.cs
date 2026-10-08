using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;
namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class ConfigPage : ContentPage
{
    public ConfigPage()
    {
        InitializeComponent();
        CarregarTema();
        CarregarBanco();
    }
    private void CarregarTema()
    {
        // uso de expressão switch para carregar o índice selecionado
        TemaPicker.SelectedIndex = Preferences.Get("Tema", "system") switch { "light" => 0, "dark" => 1, _ => 2, };
    }
    private async void OnSalvarTemaClicked(object sender, EventArgs e)
    {
        string selectedTheme = TemaPicker.SelectedIndex switch { 0 => "light", 1 => "dark", _ => "system" };
        Preferences.Set("Tema", selectedTheme);
        // Disparar mensagem para uso na recarga dinâmica
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));
        await DisplayAlert("Sucesso", "Dados salvos com sucesso!", "OK");
        // Navegar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }

    #region Banco de Dados
    // métodos aqui

    private void CarregarBanco()
    {
        DatabaseTypePicker.Items.Clear();
        foreach (var tipo in Enum.GetValues<AppDatabaseType>())
        {
            DatabaseTypePicker.Items.Add(tipo.ToString());
        }
        var bancoAtual = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());
        DatabaseTypePicker.SelectedItem = bancoAtual;
        AtualizarInterfacePorTipoBanco();
    }
    private void OnDatabaseTypeChanged(object? sender, EventArgs? e)
    {
        AtualizarInterfacePorTipoBanco();
    }


    private void AtualizarInterfacePorTipoBanco()
    {
        if (DatabaseTypePicker.SelectedItem is not string selectedTypeStr ||
        !Enum.TryParse<AppDatabaseType>(selectedTypeStr, out var selectedType))
        {
            return;
        }
        switch (selectedType)
        {
            case AppDatabaseType.Sqlite:
                SqliteInfoCard.IsVisible = true;
                SqliteContainer.IsVisible = true;
                ServidorBancoGrid.IsVisible = false;
                CredenciaisGrid.IsVisible = false;
                ComplementoLabel.Text = "Complemento (ex: Default Timeout=5;)";
                ComplementoEntry.Placeholder = "Default Timeout=5;";
                var defaultSqlitePath = DeviceInfo.Platform == DevicePlatform.WinUI
                ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
                : Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");
                SqliteCaminhoEntry.Text = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", defaultSqlitePath));
                ComplementoEntry.Text = Preferences.Get("Sqlite_Complemento", Preferences.Get("Complemento", "Default Timeout=5;"));
                break;
            case AppDatabaseType.SqlServer:
                SqliteInfoCard.IsVisible = false;
                SqliteContainer.IsVisible = false;
                ServidorBancoGrid.IsVisible = true;
                CredenciaisGrid.IsVisible = true;
                ServidorEntry.Placeholder = "Ex:localhost,1433";
                BancoEntry.Placeholder = "Ex: db_academia_do_ze";
                UsuarioEntry.Placeholder = "Ex: sa";
                ComplementoLabel.Text = "Complemento (SSL / Timeout / Criptografia)";
                ComplementoEntry.Placeholder = "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;";
                ServidorEntry.Text = Preferences.Get("SqlServer_Servidor", Preferences.Get("Servidor", "localhost,1433"));
                BancoEntry.Text = Preferences.Get("SqlServer_Banco", Preferences.Get("Banco", "db_academia_do_ze"));
                UsuarioEntry.Text = Preferences.Get("SqlServer_Usuario", Preferences.Get("Usuario", "sa"));
                SenhaEntry.Text = Preferences.Get("SqlServer_Senha", Preferences.Get("Senha", "#Bananadepijama123"));
                ComplementoEntry.Text = Preferences.Get("SqlServer_Complemento", Preferences.Get("Complemento", "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;"));
                break;
            case AppDatabaseType.MySql:
                SqliteInfoCard.IsVisible = false;
                SqliteContainer.IsVisible = false;
                ServidorBancoGrid.IsVisible = true;
                CredenciaisGrid.IsVisible = true;
                ServidorEntry.Placeholder = "Ex: localhost";
                BancoEntry.Placeholder = "Ex: db_academia_do_ze";
                UsuarioEntry.Placeholder = "Ex: braz";
                ComplementoLabel.Text = "Complemento (Porta / Timeout)";
                ComplementoEntry.Placeholder = "Connection Timeout=5;Default Command Timeout=30;";
                ServidorEntry.Text = Preferences.Get("MySql_Servidor", Preferences.Get("Servidor", "localhost"));
                BancoEntry.Text = Preferences.Get("MySql_Banco", Preferences.Get("Banco", "db_academia_do_ze"));
                UsuarioEntry.Text = Preferences.Get("MySql_Usuario", Preferences.Get("Usuario", "braz"));
                SenhaEntry.Text = Preferences.Get("MySql_Senha", Preferences.Get("Senha", "SamuelBraz"));
                ComplementoEntry.Text = Preferences.Get("MySql_Complemento", Preferences.Get("Complemento", "Connection Timeout=5;Default Command Timeout=30;"));
                break;
        }
    }


    private async void OnSalvarBdClicked(object? sender, EventArgs e)
    {
        if (DatabaseTypePicker.SelectedItem is not string selectedTypeStr ||
        !Enum.TryParse<AppDatabaseType>(selectedTypeStr, out var selectedType))
        {
            await DisplayAlertAsync("Aviso", "Selecione um tipo de banco de dados válido.", "OK");
            return;
        }
        if (selectedType == AppDatabaseType.Sqlite)
        {
            if (string.IsNullOrWhiteSpace(SqliteCaminhoEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o caminho do arquivo do banco SQLite.", "OK");
                return;
            }
            var caminho = SqliteCaminhoEntry.Text.Trim();
            var complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;
            Preferences.Set("Sqlite_Caminho", caminho);
            Preferences.Set("Sqlite_Complemento", complemento);
            // Chaves de compatibilidade
            Preferences.Set("SqliteCaminho", caminho);
            Preferences.Set("Complemento", complemento);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(ServidorEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o servidor do banco de dados.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(BancoEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o nome do banco de dados.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(UsuarioEntry.Text))
            {
                await DisplayAlertAsync("Validação", "Informe o usuário do banco de dados.", "OK");
                return;
            }
            var servidor = ServidorEntry.Text.Trim();
            var banco = BancoEntry.Text.Trim();
            var usuario = UsuarioEntry.Text.Trim();
            var senha = SenhaEntry.Text ?? string.Empty;
            var complemento = ComplementoEntry.Text?.Trim() ?? string.Empty;
            var prefix = selectedType == AppDatabaseType.SqlServer ? "SqlServer" : "MySql";
            Preferences.Set($"{prefix}_Servidor", servidor);
            Preferences.Set($"{prefix}_Banco", banco);
            Preferences.Set($"{prefix}_Usuario", usuario);
            Preferences.Set($"{prefix}_Senha", senha);
            Preferences.Set($"{prefix}_Complemento", complemento);
            // Chaves de compatibilidade
            Preferences.Set("Servidor", servidor);
            Preferences.Set("Banco", banco);
            Preferences.Set("Usuario", usuario);
            Preferences.Set("Senha", senha);
            Preferences.Set("Complemento", complemento);
        }
        Preferences.Set("DatabaseType", selectedType.ToString());
        // Disparar a mensagem para recarga dinâmica (processada pelo ConfigurationHelper)
        WeakReferenceMessenger.Default.Send(new BancoPreferencesUpdatedMessage("BancoAlterado"));
        await DisplayAlertAsync("Sucesso", $"Configurações do banco de dados ({selectedType}) salvas com sucesso!", "OK");
        // Navegar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }
    #endregion

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        // retornar para dashboard
        await Shell.Current.GoToAsync("//dashboard");
    }
    // Ao fechar a página, chama WeakReferenceMessenger.Default.UnregisterAll(this); para evitar vazamentos de memória - memory leaks
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Desinscreve o mensageiro para evitar memory leaks
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}