using System;

namespace MaCRoGS.SLAM
{
    partial class SLAMAlgorithm
    {
        private int ts_distance_scan_to_map(ts_scan_t scan, ts_position_t pos)
        {
            double c, s;
            int i, x, y, nb_points = 0;
            Int64 sum;
            c = Math.Cos(pos.theta * Math.PI / 180);
            s = Math.Sin(pos.theta * Math.PI / 180);
            // Translate and rotate scan to robot position
            // and compute the distance
            for (i = 0, sum = 0; i != scan.nb_points; i++)
            {
                if (scan.value[i] != TS_NO_OBSTACLE)
                {
                    //ORIGINAL
                    //x = (int)Math.Floor((pos.x + c * scan.x[i] - s * scan.y[i]) * TS_MAP_SCALE + 0.5);
                    //y = (int)Math.Floor((pos.y + s * scan.x[i] + c * scan.y[i]) * TS_MAP_SCALE + 0.5);

                    //MINE
                    x = (int)Math.Floor((pos.x + c * scan.x[i] - s * scan.y[i]) * TS_MAP_SCALE + 0.5);
                    y = (int)Math.Floor((pos.y - s * scan.x[i] + c * scan.y[i]) * TS_MAP_SCALE + 0.5);

                    //Check boundaries
                    if (x >= 0 && x < TS_MAP_SIZE && y >= 0 && y < TS_MAP_SIZE)
                    {
                        sum += map.map[y * TS_MAP_SIZE + x];
                        //sum += Math.Abs(scan.value[i] - map.map[y * TS_MAP_SIZE + x]);
                        nb_points++;
                    }
                }
            }
            if (nb_points > 0)
            {
                sum = sum * 1024 / nb_points;
            }
            else
            {
                sum = int.MaxValue;
            }
            return (int)sum;
        }

        private void ts_map_init()
        {
            map = new ts_map_t();
            int x, y, initval;
            //ts_map_pixel_t * ptr ;
            //ushort ptr;
            initval = (TS_OBSTACLE + TS_NO_OBSTACLE) / 2;
            for (y = 0; y < TS_MAP_SIZE; y++)
            {
                for (x = 0; x < TS_MAP_SIZE; x++)
                {
                    map.map[y * TS_MAP_SIZE + x] = (ushort)initval;
                }
            }
        }
    }
}
