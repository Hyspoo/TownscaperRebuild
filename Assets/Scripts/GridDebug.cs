using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VisualDebugging.Example
{
    public static class GridDebug
    {
        public static void DebugDrawPoints(List<Point> mPoints)
        {
            VisualDebug.Initialize();
            VisualDebug.SetColour(Colours.lightGreen, Colours.veryDarkGrey);

            for (int i = 0; i < mPoints.Count; i++)
            {
                VisualDebug.BeginFrame("Draw point", true);
                VisualDebug.DrawPointWithLabel(mPoints[i].mPosition, .03f, i.ToString(), 1);
            }
            VisualDebug.Save();
        }

        static void DrawPoints(List<Point> mPoints)
        {
            VisualDebug.Initialize();
            VisualDebug.SetColour(Colours.lightGreen, Colours.veryDarkGrey);
            VisualDebug.BeginFrame("Draw point", true);

            for (int i = 0; i < mPoints.Count; i++)
            {
                VisualDebug.DrawPointWithLabel(mPoints[i].mPosition, .03f, i.ToString(), 1);
            }
        }

        public static void DebugDrawTriangles(List<Point> mPoints, List<Triangle> mTriangles)
        {
            DrawPoints(mPoints);

            VisualDebug.SetColour(Colours.lightGreen, Colours.veryDarkGrey);

            for (int i = 0; i < mTriangles.Count; i++)
            {
                if (!mTriangles[i].mValid) continue;

                VisualDebug.BeginFrame("Draw Triangle", true);
                VisualDebug.DrawArrow(mPoints[mTriangles[i].mA].mPosition, mPoints[mTriangles[i].mB].mPosition, .1f);
                VisualDebug.BeginFrame("Draw Triangle", true);
                VisualDebug.DrawArrow(mPoints[mTriangles[i].mB].mPosition, mPoints[mTriangles[i].mC].mPosition, .1f);
                VisualDebug.BeginFrame("Draw Triangle", true);
                VisualDebug.DrawArrow(mPoints[mTriangles[i].mC].mPosition, mPoints[mTriangles[i].mA].mPosition, .1f);
            }

            VisualDebug.Save();
        }

        public static void DebugDrawQuads(List<Point> mPoints, List<Quad> mQuads)
        {
            DrawPoints(mPoints);

            VisualDebug.SetColour(Colours.lightGreen, Colours.veryDarkGrey);

            for (int i = 0; i < mQuads.Count; i++)
            {
                VisualDebug.BeginFrame("Draw Quad", true);
                VisualDebug.DrawArrow(mPoints[mQuads[i].mA].mPosition, mPoints[mQuads[i].mB].mPosition, .1f);
                VisualDebug.BeginFrame("Draw Quad", true);
                VisualDebug.DrawArrow(mPoints[mQuads[i].mB].mPosition, mPoints[mQuads[i].mC].mPosition, .1f);
                VisualDebug.BeginFrame("Draw Quad", true);
                VisualDebug.DrawArrow(mPoints[mQuads[i].mC].mPosition, mPoints[mQuads[i].mD].mPosition, .1f);
                VisualDebug.BeginFrame("Draw Quad", true);
                VisualDebug.DrawArrow(mPoints[mQuads[i].mD].mPosition, mPoints[mQuads[i].mA].mPosition, .1f);
            }

            VisualDebug.Save();
        }

        public static void DebugDrawNeighbours(List<Point> mPoints, int pointIndex, Neighbours neighbours)
        {
            VisualDebug.Initialize();
            VisualDebug.SetColour(Colours.lightGreen, Colours.veryDarkGrey);
            VisualDebug.DrawPointWithLabel(mPoints[pointIndex].mPosition, .03f, pointIndex.ToString(), 1);
            foreach (int i in neighbours.mNeighbour)
            {
                VisualDebug.DrawPointWithLabel(mPoints[i].mPosition, .03f, i.ToString(), 1);
            }
            VisualDebug.Save();
        }

        public static void ResetDebug()
        {
            VisualDebug.Initialize();
            VisualDebug.Save();
        }
    }
}
