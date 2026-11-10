namespace BoraLive.Domain.Common;

/// <summary>
/// O resultado de uma operação que pode falhar por um motivo esperado.
/// Ou é sucesso e carrega um valor, ou é falha e carrega um <see cref="Common.Error"/>.
/// </summary>
public sealed class Result<T>
{
    private readonly T? _value;

    // Construtores privados: só dá para criar um Result por Success() ou Failure().
    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    /// <summary>O erro, quando a operação falhou. Nulo em caso de sucesso.</summary>
    public Error? Error { get; }

    /// <summary>
    /// O valor, quando a operação deu certo.
    /// Ler o valor de uma falha é erro de programação, e por isso lança exceção.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            $"Não dá para ler o valor de um resultado com falha ({Error!.Code}).");

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    // Conversões implícitas: permitem escrever "return valor;" ou "return erro;"
    // em um método que devolve Result<T>, sem chamar Success/Failure toda vez.
    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
