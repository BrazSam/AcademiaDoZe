namespace AcademiaDoZe.Presentation.AppMaui
{
    // Application conflita com o nome da nossa camada de aplicação
    // Incluir o namespace completo, Microsoft.Maui.Controls.Application, para evitar conflito// Direcionando para a classe Application do MAUI
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}