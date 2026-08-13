namespace AcademiaDoZe.Domain.Common;

public class Result<T>
{
    public T? Value { get; }
    public IReadOnlyCollection<Notification> Notifications { get; }
    public bool IsSuccess => Notifications.Count == 0;
    public bool IsFailure => Notifications.Count != 0;
    private Result(T? value, IEnumerable<Notification> notifications)
    {
        Value = value;
        Notifications = notifications.ToList().AsReadOnly();
    }
    public static Result<T> Success(T value) => new(value, []);
    public static Result<T> Failure(IEnumerable<Notification> notifications) => new(default, notifications); public static Result<T> Failure(string propriedade, string mensagem) => new(default, [new Notification(propriedade, mensagem)]); public static Result<T> Failure(Notification notification) => new(default, [notification]);
}