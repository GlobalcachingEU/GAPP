﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace GAPPSF.MapProviders.OSMBinMap
{
    public enum TextPathAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public enum TextPathPosition
    {
        OverPath = 0,
        CenterPath = 1,
        UnderPath = 2
    }

    public static class GraphicsExtension
    {
        private static readonly TextOnPath TEXT_ON_PATH = new TextOnPath();

        public static RectangleF[] MeasureString(this Graphics graphics, string s, Font font, Brush brush, GraphicsPath graphicsPath)
        {
            return MeasureString(graphics, s, font, brush, TextPathAlign.Left, TextPathPosition.CenterPath, 100, graphicsPath);
        }

        public static RectangleF[] MeasureString(this Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, GraphicsPath graphicsPath)
        {
            return MeasureString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, graphicsPath);
        }

        public static void DrawString(this Graphics graphics, string s, Font font, Brush brush, GraphicsPath graphicsPath)
        {
            DrawString(graphics, s, font, brush, TextPathAlign.Left, TextPathPosition.CenterPath, 100, graphicsPath);
        }

        public static void DrawString(this Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, GraphicsPath graphicsPath)
        {
            DrawString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, graphicsPath);
        }

        public static RectangleF[] MeasureString(this Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace, GraphicsPath graphicsPath)
        {
            return MeasureString(graphics, s, font, brush, textPathAlign, textPathPosition, 100, 0,graphicsPath);
        }

        public static void DrawString(this Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace, GraphicsPath graphicsPath)
        {
            DrawString(graphics, s, font, brush, textPathAlign, textPathPosition, 100,0, graphicsPath);
        }

        public static RectangleF[] MeasureString(this Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace,float rotateDegree, GraphicsPath graphicsPath)
        {
            TEXT_ON_PATH.Text = s;
            TEXT_ON_PATH.Font = font;
            TEXT_ON_PATH.FillColorTop = brush;
            TEXT_ON_PATH.TextPathPathPosition = textPathPosition;
            TEXT_ON_PATH.TextPathAlignTop = textPathAlign;
            TEXT_ON_PATH.PathDataTop = graphicsPath.PathData;
            TEXT_ON_PATH.LetterSpacePercentage = letterSpace;
            TEXT_ON_PATH._graphics = graphics;
            TEXT_ON_PATH._graphicsPath = graphicsPath;
            TEXT_ON_PATH._measureString = true;
            TEXT_ON_PATH._rotateDegree = rotateDegree;
            TEXT_ON_PATH.DrawTextOnPath();
            return TEXT_ON_PATH._regionList.ToArray();
        }

        public static void DrawString(this Graphics graphics, string s, Font font, Brush brush, TextPathAlign textPathAlign, TextPathPosition textPathPosition, int letterSpace,float rotateDegree, GraphicsPath graphicsPath)
        {
            TEXT_ON_PATH.Text = s;
            TEXT_ON_PATH.Font = font;
            TEXT_ON_PATH.FillColorTop = brush;
            TEXT_ON_PATH.TextPathPathPosition = textPathPosition;
            TEXT_ON_PATH.TextPathAlignTop = textPathAlign;
            TEXT_ON_PATH.PathDataTop = graphicsPath.PathData;
            TEXT_ON_PATH.LetterSpacePercentage = letterSpace;
            TEXT_ON_PATH._graphics = graphics;
            TEXT_ON_PATH._graphicsPath = graphicsPath;
            TEXT_ON_PATH._measureString = false;
            TEXT_ON_PATH._rotateDegree = rotateDegree;
            TEXT_ON_PATH.DrawTextOnPath();
  
        }

    }


    internal class TextOnPath
    {
        private PathData _pathdata;
        private string _text;
        private Font _font;
        private Color _color = Color.Black;
        private Brush _fillBrush = new SolidBrush(Color.Black);
        private TextPathAlign _pathalign = TextPathAlign.Center;
        private int _letterspacepercentage = 100;
        private TextPathPosition _textPathPathPosition = TextPathPosition.CenterPath;
        public Exception LastError;
        internal Graphics _graphics;
        internal GraphicsPath _graphicsPath;
        internal bool _measureString;
        internal List<RectangleF> _regionList = new List<RectangleF>();
        internal float _rotateDegree;
        public TextPathPosition TextPathPathPosition
        {
            get { return _textPathPathPosition; }
            set { _textPathPathPosition = value; }
        }
        public PathData PathDataTop
        {
            get { return _pathdata; }
            set { _pathdata = value; }
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public Font Font
        {
            get { return _font; }
            set { _font = value; }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public Brush FillColorTop
        {
            get { return _fillBrush; }
            set { _fillBrush = value; }
        }

        public TextPathAlign TextPathAlignTop
        {
            get { return _pathalign; }
            set { _pathalign = value; }
        }

        public int LetterSpacePercentage
        {
            get { return _letterspacepercentage; }
            set { _letterspacepercentage = value; }
        }
        public void DrawTextOnPath(PathData pathdata, string text, Font font, Color color, Brush fillcolor, int letterspacepercentage)
        {

            _pathdata = pathdata;
            _text = text;
            _font = font;
            _color = color;
            _fillBrush = fillcolor;
            _letterspacepercentage = letterspacepercentage;

            DrawTextOnPath();
        }


        public void DrawTextOnPath()
        {
            PointF[] tmpPoints;
            PointF[] points = new PointF[25001];
            int count = 0;
            GraphicsPath gp = new GraphicsPath(_pathdata.Points, _pathdata.Types) { FillMode = FillMode.Winding };
            _regionList.Clear();
            gp.Flatten(null, 1);
            try
            {
                PointF tmpPoint = gp.PathPoints[0];
                int i;
                for (i = 0; i <= gp.PathPoints.Length - 2; i++)
                {
                    if (gp.PathTypes[i + 1] == (byte)PathPointType.Start | (gp.PathTypes[i] & (byte)PathPointType.CloseSubpath) == (byte)PathPointType.CloseSubpath)
                    {
                        tmpPoints = GetLinePoints(gp.PathPoints[i], tmpPoint, 1);
                        Array.ConstrainedCopy(tmpPoints, 0, points, count, tmpPoints.Length);
                        count += 1;
                        tmpPoint = gp.PathPoints[i + 1];
                    }
                    else
                    {
                        tmpPoints = GetLinePoints(gp.PathPoints[i], gp.PathPoints[i + 1], 1);
                        Array.ConstrainedCopy(tmpPoints, 0, points, count, tmpPoints.Length);
                        count += tmpPoints.Length - 1;

                    }
                }
                tmpPoints = new PointF[count];
                Array.Copy(points, tmpPoints, count);
                points = CleanPoints(tmpPoints);

                /* nah, be safe
                if (points.Length == 0)
                {
                    points = new PointF[2];
                    points[0] = gp.PathPoints[0];
                    points[1] = gp.PathPoints[gp.PathPoints.Length-1];
                }
                 * */
                if (points.Length > 0)
                {
                    count = points.Length - 1;
                    DrawText(points, count);
                }
            }
            catch (Exception ex)
            {
                LastError = ex;


            }
        }
        private static PointF[] CleanPoints(PointF[] points)
        {

            int i;
            PointF[] tmppoints = new PointF[points.Length + 1];
            PointF lastpoint = default(PointF);
            int count = 0;

            for (i = 0; i <= points.Length - 1; i++)
            {
                if (i == 0 | points[i].X != lastpoint.X | points[i].Y != lastpoint.Y)
                {
                    tmppoints[count] = points[i];
                    count += 1;
                }
                lastpoint = points[i];
            }


            points = new PointF[count];
            Array.Copy(tmppoints, points, count);

            return points;
        }

        private void DrawText(PointF[] points, int maxPoints)
        {

            GraphicsPath gp = new GraphicsPath(_pathdata.Points, _pathdata.Types) { FillMode = FillMode.Winding };
            gp.Flatten();
            gp.Dispose();
            Graphics g = _graphics;
            GraphicsContainer graphicsContainer = g.BeginContainer();
            //g.TranslateTransform(_graphicsPath.GetBounds().X, _graphicsPath.GetBounds().Y);
            int count = 0;
            PointF point1 = default(PointF);
            int charStep = 0;
            double maxWidthText = default(double);
            int i;

            for (i = 0; i <= _text.Length - 1; i++)
            {
                maxWidthText += StringRegion(g, i);
            }

            switch (_pathalign)
            {
                case TextPathAlign.Left:
                    point1 = points[0];
                    count = 0;
                    break;
                case TextPathAlign.Center:
                    count = (int)((maxPoints - maxWidthText) / 2);
                    if (count > 0)
                    {
                        point1 = points[count];
                    }
                    else
                    {
                        point1 = points[0];
                    }

                    break;
                case TextPathAlign.Right:
                    count = (int)(maxPoints - maxWidthText - (double)StringRegion(g, _text.Length - 1) * LetterSpacePercentage / 100);
                    if (count > 0)
                    {
                        point1 = points[count];
                    }
                    else
                    {
                        point1 = points[0];
                    }

                    break;
            }

            while (!(charStep > _text.Length - 1))
            {
                int lStrWidth = (int)(StringRegion(g, charStep) * LetterSpacePercentage / 100);
                if ((count + lStrWidth / 2) >= 0 & (count + lStrWidth) < maxPoints)
                {
                    count += lStrWidth;
                    PointF point2 = points[count];
                    PointF point = points[count - lStrWidth / 2];
                    double angle = GetAngle(point1, point2);
                    DrawRotatedText(g, _text[charStep].ToString(), (float)angle, point);
                    point1 = points[count];
                }
                else
                {
                    count += lStrWidth;
                }
                charStep += 1;
            }
            g.EndContainer(graphicsContainer);
        }

        private RectangleF StringRegionValue(Graphics g, int textpos)
        {

            string measureString = _text.Substring(textpos, 1);
            int numChars = measureString.Length;
            CharacterRange[] characterRanges = new CharacterRange[numChars + 1];
            StringFormat stringFormat = new StringFormat
            {
                Trimming = StringTrimming.None,
                FormatFlags =
                    StringFormatFlags.NoClip | StringFormatFlags.NoWrap |
                    StringFormatFlags.LineLimit
            };
            SizeF size = g.MeasureString(_text, _font, 100);
            RectangleF layoutRect = new RectangleF(0f, 0f, size.Width, size.Height);
            characterRanges[0] = new CharacterRange(0, 1);
            stringFormat.FormatFlags = StringFormatFlags.NoClip;
            stringFormat.SetMeasurableCharacterRanges(characterRanges);
            stringFormat.Alignment = StringAlignment.Near;
            Region[] stringRegions = g.MeasureCharacterRanges(_text.Substring(textpos), _font, layoutRect, stringFormat);
            return stringRegions[0].GetBounds(g);
        }

        private float StringRegion(Graphics g, int textpos)
        {
            return StringRegionValue(g, textpos).Width;
        }

        private static double GetAngle(PointF point1, PointF point2)
        {
            double c = Math.Sqrt(Math.Pow((point2.X - point1.X), 2) + Math.Pow((point2.Y - point1.Y), 2));
            if (c == 0)
            {
                return 0;
            }
            if (point1.X > point2.X)
            {
                //We must change the side where the triangle is
                return Math.Asin((point1.Y - point2.Y) / c) * 180 / Math.PI - 180;
            }
            return Math.Asin((point2.Y - point1.Y) / c) * 180 / Math.PI;


        }

        private static StringFormat _stringFormat = new StringFormat { Alignment = StringAlignment.Center };
        private void DrawRotatedText(Graphics gr, string text, float angle, PointF pointCenter)
        {
            angle -= _rotateDegree;
            gr.SmoothingMode = SmoothingMode.HighQuality;
            gr.CompositingQuality = CompositingQuality.HighQuality;
            gr.TextRenderingHint = TextRenderingHint.AntiAlias;
            GraphicsPath graphicsPath = new GraphicsPath(FillMode.Winding);
            int x = (int)pointCenter.X;
            int y = (int)pointCenter.Y;

            switch (TextPathPathPosition)
            {
                case TextPathPosition.OverPath:
                    graphicsPath.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, new Point(x, (int)(y - _font.Size)), _stringFormat);
                    break;
                case TextPathPosition.CenterPath:
                    graphicsPath.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, new Point(x, (int)(y - _font.Size / 2)), _stringFormat);
                    break;
                case TextPathPosition.UnderPath:
                    graphicsPath.AddString(text, _font.FontFamily, (int)_font.Style, _font.Size, new Point(x, y), _stringFormat);
                    break;
            }


            Matrix rotationMatrix = new Matrix();
            rotationMatrix.RotateAt(angle, new PointF(x, y));
            graphicsPath.Transform(rotationMatrix);
            if (!_measureString)
            {
                gr.DrawPath(new Pen(_color), graphicsPath);
                gr.FillPath(_fillBrush, graphicsPath);
            }
            else
            {
                _regionList.Add(graphicsPath.GetBounds());
            }

            graphicsPath.Dispose();
        }



        public PointF[] GetLinePoints(PointF p1, PointF p2, int stepWitdth)
        {

            int lCount = 0;
            PointF[] tmpPoints = new PointF[10001];
            long ix;
            long iy;
            int dd;
            int id;
            int lStep = stepWitdth;

            p1.X = (int)p1.X;
            p1.Y = (int)p1.Y;
            p2.X = (int)p2.X;
            p2.Y = (int)p2.Y;
            long width = (long)(p2.X - p1.X);
            long height = (long)(p2.Y - p1.Y);
            long d = 0;

            if (width < 0)
            {
                width = -width;
                ix = -1;
            }
            else
            {
                ix = 1;
            }

            if (height < 0)
            {
                height = -height;
                iy = -1;
            }
            else
            {
                iy = 1;
            }

            if (width > height)
            {
                dd = (int)(width + width);
                id = (int)(height + height);

                do
                {
                    if (lStep == stepWitdth)
                    {
                        tmpPoints[lCount].X = p1.X;
                        tmpPoints[lCount].Y = p1.Y;
                        lCount += 1;
                    }
                    else
                    {
                        lStep = lStep + stepWitdth;
                    }
                    if ((int)p1.X == (int)p2.X) break;

                    p1.X = p1.X + ix;
                    d = d + id;

                    if (d > width)
                    {
                        p1.Y = p1.Y + iy;
                        d = d - dd;
                    }
                }

                while (true);
            }
            else
            {
                dd = (int)(height + height);
                id = (int)(width + width);

                do
                {
                    if (lStep == stepWitdth)
                    {
                        tmpPoints[lCount].X = p1.X;
                        tmpPoints[lCount].Y = p1.Y;
                        lCount += 1;
                    }
                    else
                    {
                        lStep = lStep + stepWitdth;
                    }
                    if ((int)p1.Y == (int)p2.Y) break;

                    p1.Y = p1.Y + iy;
                    d = d + id;

                    if (d > height)
                    {
                        p1.X = p1.X + ix;
                        d = d - dd;
                    }
                }
                while (true);
            }

            PointF[] tmpPoints2 = new PointF[lCount];

            Array.Copy(tmpPoints, tmpPoints2, lCount);

            return tmpPoints2;
        }
    }
}
