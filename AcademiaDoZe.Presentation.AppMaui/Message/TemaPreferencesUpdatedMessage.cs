using CommunityToolkit.Mvvm.Messaging.Messages;
namespace AcademiaDoZe.Presentation.AppMaui.Message;

public sealed class TemaPreferencesUpdatedMessage(string value) : ValueChangedMessage<string>(value)
{
}
// ValueChangedMessage<T> é uma classe base do toolkit para mensagens que carregam um valor.