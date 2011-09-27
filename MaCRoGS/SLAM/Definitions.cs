using System;

namespace MaCRoGS.SLAM
{
    public class tinySLAM
    {
        private static SLAMAlgorithm a;
        private static int updates = 0;

        public static void Initialize()
        {
            a = new SLAMAlgorithm();
        }

        public static void NewScan(ts_scan_t scan, ts_position_t position)
        {
            //a.ts_distance_scan_to_map(scan, position);
            if (a != null)
            {
                a.ts_map_update(scan, position, 50);
                updates++;
            }
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

        public static float MapScale()
        {
            return SLAMAlgorithm.TS_MAP_SCALE;
        }

        public static ushort ScanSize()
        {
            return SLAMAlgorithm.TS_SCAN_SIZE;
        }

        public static ushort HoleWidth()
        {
            return SLAMAlgorithm.TS_HOLE_WIDTH;
        }

        public static int NumUpdates()
        {
            return updates;
        }

        public static ts_position_t MonteCarlo_UpdatePosition(ts_scan_t scan,ts_position_t startPos,double sigma_xy,double sigma_theta,int stop, int bd,out int quality)
        {
            return a.ts_monte_carlo_search(scan, startPos, sigma_xy, sigma_theta, stop, bd, out quality);
            //return a.montecarlo_position;
        }
    }

    partial class SLAMAlgorithm
    {
        internal static readonly ushort TS_SCAN_SIZE = 512;
        internal static readonly ushort TS_MAP_SIZE = 1024;
        internal static readonly float TS_MAP_SCALE = 0.5f;//celda / mm
        internal static readonly ushort TS_DISTANCE_NO_DETECTION = 800;
        internal static readonly ushort TS_NO_OBSTACLE = 65500;
        internal static readonly ushort TS_OBSTACLE = 0;
        internal static readonly ushort TS_HOLE_WIDTH = 100;

        private ts_map_t map;

        public ts_map_t MAP { get { return map; } }

        public ts_position_t montecarlo_position { get; set; }

        private ts_randomizer_t random;

        public SLAMAlgorithm()
        {
            montecarlo_position = new ts_position_t();
            random = new ts_randomizer_t();
            this.ts_random_init(random, 0xdead);
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

    public class ts_randomizer_t
    {
        public ulong jz;
        public ulong jsr;
        public long hz;
        public ulong iz;
        public ulong[] kn;
        public double[] wnt;
        public double[] wn;
        public double[] fn;

        public ts_randomizer_t()
        {
            kn = new ulong[128];
            wnt = new double[128];
            wn = new double[128];
            fn = new double[128];
        }
    }

}
