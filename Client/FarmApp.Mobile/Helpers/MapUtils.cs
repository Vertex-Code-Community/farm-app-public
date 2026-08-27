namespace FarmApp.Mobile.Helpers;

public static class MapUtils
{
    public static double[][] GetSquare(double[] center, double size, double zoom)
    {
        var boundingBox = CalculateBoundingBox(center, size, zoom);

        return new double[][]
        {
            new double[] { boundingBox[0][0], boundingBox[0][1] },
            new double[] { boundingBox[1][0], boundingBox[0][1] },
            new double[] { boundingBox[1][0], boundingBox[1][1] },
            new double[] { boundingBox[0][0], boundingBox[1][1] },
            new double[] { boundingBox[0][0], boundingBox[0][1] }
        };
    }

    private static double[][] CalculateBoundingBox(double[] center, double size, double zoom)
    {
        var halfSize = size / 2;
        var lngLatSize = halfSize * (360 / (Math.Pow(2, zoom) * 512));

        // Adjust for the aspect ratio at the given center point
        var aspectRatio = Math.Cos(DegreeToRadian(center[1]));
        var lngSize = lngLatSize / aspectRatio;

        return new double[][]
        {
            new double[] { center[0] - lngSize, center[1] + lngLatSize },
            new double[] { center[0] + lngSize, center[1] - lngLatSize }
        };
    }

    private static double DegreeToRadian(double degree)
    {
        return degree * (Math.PI / 180);
    }
    
    private static double[] CalculateMedian(double[] point1, double[] point2)
    {
        // Ensure each point has exactly two coordinates (x, y)
        if (point1.Length != 2 || point2.Length != 2)
        {
            throw new ArgumentException("Each point must have exactly two coordinates");
        }

        // Calculate the median point between two points
        return new double[]
        {
            (point1[0] + point2[0]) / 2.0,
            (point1[1] + point2[1]) / 2.0
        };
    }
    
    public static double[][] CalculateMedianPoints(double[][] pointsArray)
    {
        // Check if the array has at least two points
        if (pointsArray.Length < 2)
        {
            throw new ArgumentException("At least two points are required");
        }

        // Initialize array to hold median points
        var medianPoints = new double[pointsArray.Length][];

        // Calculate median points for each pair of consecutive points
        for (var i = 0; i < pointsArray.Length - 1; i++)
        {
            var currentPoint = pointsArray[i];
            var nextPoint = pointsArray[i + 1];
            var medianPoint = CalculateMedian(currentPoint, nextPoint);
            medianPoints[i] = medianPoint;
        }

        // Calculate and add the median point between the last and first points
        var firstPoint = pointsArray[0];
        var lastPoint = pointsArray[^1];
        var medianPointFirstLast = CalculateMedian(lastPoint, firstPoint);
        medianPoints[pointsArray.Length - 1] = medianPointFirstLast;

        return medianPoints;
    }
    
    
}