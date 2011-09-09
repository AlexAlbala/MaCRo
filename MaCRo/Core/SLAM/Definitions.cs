using System;
using Microsoft.SPOT;

namespace MaCRo.Core.SLAM
{
    public class tinySLAM
    {
        private static SLAMAlgorithm a;

        public static void Initialize()
        {
            a = new SLAMAlgorithm();
        }

        public static void NewScan(ts_scan_t scan, ts_position_t position)
        {
            //a.ts_distance_scan_to_map(scan, position);
            if (a != null)
                a.ts_map_update(scan, position, 50);
        }

        public static ts_map_t Map()
        {
            if (a != null)
            {
                if (a.MAP != null)
                {
                    return a.MAP;
                }
                else
                {
                    return new ts_map_t();
                }
            }
            else
            {
                return new ts_map_t();
            }
        }

        public static ushort MapSize()
        {
            return SLAMAlgorithm.TS_MAP_SIZE;
        }

        public static ts_position_t Position()
        {
            return null;
            //if(a.
        }
    }

    partial class SLAMAlgorithm
    {
        internal static readonly ushort TS_SCAN_SIZE = 400;
        internal static readonly ushort TS_MAP_SIZE = 52;
        internal static readonly float TS_MAP_SCALE = 0.05f;//celda / mm
        internal static readonly ushort TS_DISTANCE_NO_DETECTION = 800;
        internal static readonly ushort TS_NO_OBSTACLE = 65500;
        internal static readonly ushort TS_OBSTACLE = 0;
        internal static readonly ushort TS_HOLE_WIDTH = 600;

        private ts_map_t map;

        public ts_map_t MAP { get { return map; } }

        public SLAMAlgorithm()
        {
            this.ts_map_init();
        }

    }

    public class ts_map_t
    {
        public ushort[] map;

        public ts_map_t()
        {
            map = new ushort[SLAMAlgorithm.TS_MAP_SIZE * SLAMAlgorithm.TS_MAP_SIZE];
        }
    }

    public class ts_scan_t
    {
        public float[] x, y;
        public ushort[] value;
        public ushort nb_points;

        public ts_scan_t()
        {
            x = new float[SLAMAlgorithm.TS_SCAN_SIZE];
            y = new float[SLAMAlgorithm.TS_SCAN_SIZE];
            value = new ushort[SLAMAlgorithm.TS_SCAN_SIZE];

            initScan();
        }

        private void initScan()
        {
            int x, initval;
            //ts_map_pixel_t * ptr ;
            //ushort ptr;
            initval = (SLAMAlgorithm.TS_OBSTACLE + SLAMAlgorithm.TS_NO_OBSTACLE) / 2;
            for (x = 0; x < SLAMAlgorithm.TS_SCAN_SIZE; x++)
            {
                value[x] = (ushort)initval;
            }
        }
    }

    public class ts_position_t
    {
        public double x, y; //mm
        public double theta;//degrees
    }
}
