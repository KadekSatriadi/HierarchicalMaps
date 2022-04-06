using System;
using System.Collections.Generic;
using UnityEngine;

public static class PerceptionFormula
{

    /// <summary>
    /// Return visual angle from distance and size  http://elvers.us/perception/visualAngle/
    /// </summary>
    /// <returns></returns>
    public static float CalculateVisualAngle(float size, float distance)
    {
        return 2 * Mathf.Atan((size * 0.5f) / distance);
    }

    /// <summary>
    /// Return size from known angle and distance
    /// </summary>
    /// <param name="size1"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static float CalculateSizeFromVisualAngleDistance(float angle, float distance)
    {
        return 2 * distance * Mathf.Tan(angle * 0.5f);
    }

    /// <summary>
    /// Return distance from known visual angle and size
    /// </summary>
    /// <param name="angle"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public static float CalculateDistanceFromVisualAngleSize(float angle, float size)
    {
        return (size * 0.5f) / (Mathf.Tan(angle * 0.5f));
    }
}

public static class UtilityFunctions
{
    public static class Sortings
    {
        /// <summary>
        /// https://www.tutorialspoint.com/insertion-sort-in-chash
        /// </summary>
        /// <returns></returns>
        public static int[] InsertionSort(int[] arr)
        {
            int n = arr.Length, i, j, val, flag;
            for (i = 1; i < n; i++)
            {
                val = arr[i];
                flag = 0;
                for (j = i - 1; j >= 0 && flag != 1;)
                {
                    if (val < arr[j])
                    {
                        arr[j + 1] = arr[j];
                        j--;
                        arr[j + 1] = val;
                    }
                    else flag = 1;
                }
            }

            return arr;
        }

        public static float[] InsertionSort(float[] arr)
        {
            int n = arr.Length, i, j, flag;
            float val;
            for (i = 1; i < n; i++)
            {
                val = arr[i];
                flag = 0;
                for (j = i - 1; j >= 0 && flag != 1;)
                {
                    if (val < arr[j])
                    {
                        arr[j + 1] = arr[j];
                        j--;
                        arr[j + 1] = val;
                    }
                    else flag = 1;
                }
            }

            return arr;
        }
    }
}

public static class MapFormula 
{
    /// <summary>
    /// Formula based on the data at https://gis.stackexchange.com/questions/20705/what-are-standard-scales-or-zoom-levels-for-map-applications
    /// </summary>
    /// <param name="z1"></param>
    /// <returns></returns>
    public static double ZoomLevelToMeter(float z1)
    {
        return (double)(57960929.14 * Math.Pow(Math.E, -0.649334108 * z1));
    }

    public static double MeterToZoomLevel(float m)
    {
        double numerator = Math.Log(m / 57960929.14, Math.E);
        return numerator / -0.649334108;
    }

    /// <summary>
    /// Return meter Since the growth of zoom relative to meter is not linear, we need to estimate the values
    /// </summary>
    /// <param name="z1">Initial zoom</param>
    /// <param name="z2">Current zoom</param>
    /// <param name="m">Initial meter</param>
    /// <returns></returns>
    public static double ZoomToMeterInterpolation(float z1, float z2, float m)
    {
        double a = ZoomLevelToMeter(z1);
        double b = ZoomLevelToMeter(z2);
        double ratio = a / b;
        return m * ratio;
    }

    /// <summary>
    /// Return zoom level 
    /// </summary>
    /// <param name="m1">Current size</param>
    /// <param name="m2">Previous size</param>
    /// <param name="z1">Previous zoom</param>
    /// <returns></returns>
    public static double MeterToZoomInterpolation(float m1, float m2, float z1)
    {
        double meter1 = ZoomLevelToMeter(z1);
        double meter2 = m2 * (meter1 / m1);
        double z2 = MeterToZoomLevel((float) meter2);
        return z2;
    }

   

    /// <summary>
    /// Calculate distance between two lat long points https://andrew.hedges.name/experiments/haversine/
    /// </summary>
    /// <param name="lat1"></param>
    /// <param name="lon1"></param>
    /// <param name="lat2"></param>
    /// <param name="lon2"></param>
    /// <returns></returns>
    public static double DistanceBetweenLatLons(double lat1, double lon1, double lat2, double lon2, int R)
    {
        var dlon = lon2 - lon1;
        var dlat = lat2 - lat1;
        double a = ((Math.Sin(dlat / 2)) * (Math.Sin(dlat / 2))) + Math.Cos(lat1) * Math.Cos(lat2) * ((Math.Sin(dlon / 2))* (Math.Sin(dlon / 2)));
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        double d = R * c;

        return d;
    }

    /// <summary>
    /// Get points of Quadratic Bezier curve
    /// </summary>
    /// <param name="P0"></param>
    /// <param name="P2"></param>
    /// <param name="lengthControlPoints">Lenght of control point</param>
    /// <param name="dir">Direction of control point</param>
    /// <returns></returns>
    public static List<Vector3> GetQuadBezierPoints(Vector3 P0, Vector3 P2, float lengthControlPoints, Vector3 dir, int segments)
    {
        List<Vector3> points = new List<Vector3>();

        //Create control points
        Vector3 direction = P2 - P0;
        Vector3 P1 = (P0 + direction * 0.5f) + dir * lengthControlPoints;

        //Create points
        points.Add(P0);

        float inc = 1f / segments;
        float t = 0;
        for (int i = 0; i < segments; i++)
        {
            Vector3 Pt = Mathf.Pow((1 - t), 2) * P0 + 2 * (1 - t) * t * P1 + Mathf.Pow(t, 2) * P2;
            points.Add(Pt);

            t += inc;
        }

        points.Add(P2);

        return points;
    }

    public static List<Vector3> GetCubicBezierPoints(Vector3 P0, Vector3 P3, Vector3 P1, Vector3 P2, int segments)
    {
        List<Vector3> points = new List<Vector3>();

        //Create points
        points.Add(P0);

        float inc = 1f / segments;
        float t = 0;
        for (int i = 0; i < segments; i++)
        {
            Vector3 Pt = (Mathf.Pow((1 - t), 3) * P0)
                         + 3 * Mathf.Pow((1 - t), 2) * t * P1
                         + 3 * (1 - t) * t * t * P2
                         + t * t * t * P3;
            points.Add(Pt);

            t += inc;
        }

        points.Add(P3);

        return points;
    }

    public static List<Vector3> GetCubicBezierPoints(Vector3 P0, Vector3 P3, float h, Vector3 dir, int segments)
    {
        List<Vector3> points = new List<Vector3>();

        //Create control points
        float hc = (4f / 3f) * h;
        Vector3 P1 = (P0 + (dir.normalized * hc));
        Vector3 P2 = (P3 + (dir.normalized * hc));
        //Create points
        points.Add(P0);

        float inc = 1f / segments;
        float t = 0;
        for (int i = 0; i < segments; i++)
        {
            Vector3 Pt = (Mathf.Pow((1 - t), 3) * P0)
                         + 3 * Mathf.Pow((1 - t), 2) * t * P1 
                         + 3 * (1 - t) * t * t * P2 
                         + t * t * t * P3;
            points.Add(Pt);

            t += inc;
        }

        points.Add(P3);

        return points;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="polar"></param>
    /// <param name="elevation"></param>
    /// <param name="center"></param>
    /// <returns></returns>
    public static Vector3 PolarToCartesian(float radius, float polar, float elevation, Vector3 center)
    {
        polar *= Mathf.Deg2Rad;
        elevation *= Mathf.Deg2Rad;

        float z = radius * Mathf.Cos(elevation) * Mathf.Cos(polar);
        float y = radius * Mathf.Sin(elevation);
        float x = radius * Mathf.Cos(elevation) * Mathf.Sin(polar);

        return new Vector3(x,y,z) + center;
    }
}
