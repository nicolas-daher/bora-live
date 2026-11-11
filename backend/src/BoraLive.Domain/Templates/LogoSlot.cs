using BoraLive.Domain.Common;

namespace BoraLive.Domain.Templates;

/// <summary>
/// O lugar reservado para o logo do streamer em um layout.
/// O logo é encaixado dentro de um quadrado, sem distorcer.
/// </summary>
public sealed record LogoSlot
{
    private LogoSlot(double x, double y, double size)
    {
        X = x;
        Y = y;
        Size = size;
    }

    /// <summary>Posição horizontal do centro do quadrado (0 a 1, relativa à largura).</summary>
    public double X { get; }

    /// <summary>Posição vertical do centro do quadrado (0 a 1, relativa à altura).</summary>
    public double Y { get; }

    /// <summary>Lado do quadrado como fração da altura da arte.</summary>
    public double Size { get; }

    public static Result<LogoSlot> Create(double x, double y, double size)
    {
        if (x is not (>= 0 and <= 1) || y is not (>= 0 and <= 1))
        {
            return LogoSlotErrors.PositionOutOfRange;
        }

        if (size is not (> 0 and <= 1))
        {
            return LogoSlotErrors.SizeOutOfRange;
        }

        return new LogoSlot(x, y, size);
    }
}

public static class LogoSlotErrors
{
    public static readonly Error PositionOutOfRange = new(
        "logo_slot.position_out_of_range",
        "A posição do logo precisa ficar entre 0 e 1.");

    public static readonly Error SizeOutOfRange = new(
        "logo_slot.size_out_of_range",
        "O tamanho do logo precisa ser maior que 0 e no máximo 1.");
}
