using System;
using Microsoft.SPOT;
using MaCRo.Tools;

namespace MaCRo.Core.SLAM
{
    partial class SLAMAlgorithm
    {
        //#define SWAP( x , y ) ( x ^= y ^= x ^= y )

        private void ts_map_laser_ray(int x1, int y1, int x2, int y2, int xp, int yp, int value, int alpha)
        {
            unsafe
            {
                int x2c, y2c, dx, dy, dxc, dyc, error, errorv, derrorv, x;
                int incv, sincv, incerrorv, incptrx, incptry, pixval, horiz, diago;
                //ts_map_pixel_t  ptr ;
                int ptr;
                if (x1 < 0 || x1 >= TS_MAP_SIZE || y1 < 0 || y1 >= TS_MAP_SIZE)
                    return; // Robot is out of map
                x2c = x2; y2c = y2;
                // Clipping
                if (x2c < 0)
                {
                    if (x2c == x1) return;
                    y2c += (y2c - y1) * (-x2c) / (x2c - x1);
                    x2c = 0;
                }
                if (x2c >= TS_MAP_SIZE)
                {
                    if (x1 == x2c) return;
                    y2c += (y2c - y1) * (TS_MAP_SIZE - 1 - x2c) / (x2c - x1);
                    x2c = TS_MAP_SIZE - 1;
                }
                if (y2c < 0)
                {
                    if (y1 == y2c) return;
                    x2c += (x1 - x2c) * (-y2c) / (y1 - y2c);
                    y2c = 0;
                }
                if (y2c >= TS_MAP_SIZE)
                {
                    if (y1 == y2c) return;
                    x2c += (x1 - x2c) * (TS_MAP_SIZE - 1 - y2c) / (y1 - y2c);
                    y2c = TS_MAP_SIZE - 1;
                }
                dx = (int)exMath.Abs(x2 - x1); dy = (int)exMath.Abs(y2 - y1);
                dxc = (int)exMath.Abs(x2c - x1); dyc = (int)exMath.Abs(y2c - y1);
                incptrx = (x2 > x1) ? 1 : -1;
                incptry = (y2 > y1) ? TS_MAP_SIZE : -TS_MAP_SIZE;
                sincv = (value > TS_NO_OBSTACLE) ? 1 : -1;
                if (dx > dy)
                {
                    derrorv = (int)exMath.Abs(xp - x2);
                }
                else
                {
                    //SWAP(dx, dy); SWAP(dxc, dyc); SWAP(incptrx, incptry);
                    dx ^= dy;
                    dy ^= dx;
                    dx ^= dy;

                    dxc ^= dyc;
                    dyc ^= dxc;
                    dxc ^= dyc;

                    incptrx ^= incptry;
                    incptry ^= incptrx;
                    incptrx ^= incptry;

                    derrorv = (int)exMath.Abs(yp - y2);
                }
                error = 2 * dyc - dxc;
                horiz = 2 * dyc;
                diago = 2 * (dyc - dxc);
                errorv = derrorv / 2;
                incv = (value - TS_NO_OBSTACLE) / derrorv;
                incerrorv = value - TS_NO_OBSTACLE - derrorv * incv;
                //ptr = (map.map) + y1 * TS_MAP_SIZE + x1;
                ptr = y1 * TS_MAP_SIZE + x1;
                pixval = TS_NO_OBSTACLE;
                for (x = 0; x <= dxc; x++, ptr += incptrx)
                {
                    if (x > dx - 2 * derrorv)
                    {
                        if (x <= dx - derrorv)
                        {
                            pixval += incv;
                            errorv += incerrorv;
                            if (errorv > derrorv)
                            {
                                pixval += sincv;
                                errorv -= derrorv;
                            }
                        }
                        else
                        {
                            pixval -= incv;
                            errorv -= incerrorv;
                            if (errorv < 0)
                            {
                                pixval -= sincv;
                                errorv += derrorv;
                            }
                        }
                    }
                    // Integration into the map
                    map.map[ptr] = (ushort)(((256 - alpha) * map.map[ptr] + alpha * pixval) >> 8);
                    //*ptr = ((256 - alpha) * (*ptr) + alpha * pixval) >> 8;
                    if (error > 0)
                    {
                        ptr += incptry;
                        error += diago;
                    }
                    else error += horiz;
                }
            }
        }

        internal void ts_map_update(ts_scan_t scan, ts_position_t pos, int quality)
        {
            //cos sin quality
            double c, s, q;
            //Pos scan rotated
            double x2p, y2p;
            //x1y1: Pos scaled ||xpyp: Absolute pos scan
            int i, x1, y1, x2, y2, xp, yp, value;
            double add, dist;
            c = exMath.Cos(pos.theta * exMath.PI / 180);
            s = exMath.Sin(pos.theta * exMath.PI / 180);
            x1 = (int)exMath.Floor(pos.x * TS_MAP_SCALE + 0.5);
            y1 = (int)exMath.Floor(pos.y * TS_MAP_SCALE + 0.5);
            // Translate and rotate scan to robot position
            for (i = 0; i != scan.nb_points; i++)
            {
                //Before TS_HOLE_WIDTH
                x2p = c * scan.x[i] - s * scan.y[i];
                y2p = s * scan.x[i] + c * scan.y[i];
                xp = (int)exMath.Floor((pos.x + x2p) * TS_MAP_SCALE + 0.5);
                yp = (int)exMath.Floor((pos.y + y2p) * TS_MAP_SCALE + 0.5);
                /*************************************************************************/
                //CAUTION: If x2p or y2p are too high, the Sqrt operation will be very slow
                double x2p_meter = x2p / 1000;
                double y2p_meter = y2p / 1000;
                dist = exMath.Sqrt(x2p_meter * x2p_meter + y2p_meter * y2p_meter);
                dist *= 1000;
                /************************************************************************/

                add = TS_HOLE_WIDTH / 2 / dist;
                //After TS_HOLE_WIDTH
                x2p *= TS_MAP_SCALE * (1 + add);
                y2p *= TS_MAP_SCALE * (1 + add);
                x2 = (int)exMath.Floor(pos.x * TS_MAP_SCALE + x2p + 0.5);
                y2 = (int)exMath.Floor(pos.y * TS_MAP_SCALE + y2p + 0.5);
                if (scan.value[i] == TS_NO_OBSTACLE)
                {
                    q = quality / 2;
                    value = TS_NO_OBSTACLE;
                }
                else
                {
                    q = quality;
                    value = TS_OBSTACLE;
                }
                ts_map_laser_ray(x1, y1, x2, y2, xp, yp, value, (int)q);
            }
        }
    }
}
