﻿namespace ScottPlot.DataSources;

public class SignalSourceGenericList<T> : SignalSourceBase, ISignalSource
{
    private readonly List<T> Ys;
    public override int Length => Ys.Count;

    public SignalSourceGenericList(List<T> ys, double period)
    {
        Ys = ys;
        Period = period;
    }

    public IReadOnlyList<double> GetYs()
    {
        return NumericConversion.GenericToDoubleArray(Ys);
    }

    public override SignalRangeY GetLimitsY(int firstIndex, int lastIndex)
    {
        double min = double.PositiveInfinity;
        double max = double.NegativeInfinity;

        for (int i = firstIndex; i <= lastIndex; i++)
        {
            T genericValue = Ys[i];
            double value = NumericConversion.GenericToDouble(ref genericValue);
            min = Math.Min(min, value);
            max = Math.Max(max, value);
        }

        return new SignalRangeY(min, max);
    }

    public PixelColumn GetPixelColumn(IAxes axes, int xPixelIndex)
    {
        float xPixel = axes.DataRect.Left + xPixelIndex;
        double xRangeMin = axes.GetCoordinateX(xPixel);
        float xUnitsPerPixel = (float)(axes.XAxis.Width / axes.DataRect.Width);
        double xRangeMax = xRangeMin + xUnitsPerPixel;

        if (RangeContainsSignal(xRangeMin, xRangeMax) == false)
            return PixelColumn.WithoutData(xPixel);

        // determine column limits horizontally
        int i1 = GetIndex(xRangeMin, true);
        int i2 = GetIndex(xRangeMax, true);
        float yEnter = axes.GetPixelY(NumericConversion.GenericToDouble(Ys, i1) + YOffset);
        float yExit = axes.GetPixelY(NumericConversion.GenericToDouble(Ys, i2) + YOffset);

        // determine column span vertically
        double yMin = double.PositiveInfinity;
        double yMax = double.NegativeInfinity;
        for (int i = i1; i <= i2; i++)
        {
            double value = NumericConversion.GenericToDouble(Ys, i);
            yMin = Math.Min(yMin, value);
            yMax = Math.Max(yMax, value);
        }
        yMin += YOffset;
        yMax += YOffset;

        float yBottom = axes.GetPixelY(yMin);
        float yTop = axes.GetPixelY(yMax);

        return new PixelColumn(xPixel, yEnter, yExit, yBottom, yTop);
    }
}
