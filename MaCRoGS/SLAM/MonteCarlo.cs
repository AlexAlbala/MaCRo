using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaCRoGS.SLAM
{
    partial class SLAMAlgorithm
    {
        public ts_position_t ts_monte_carlo_search(ts_scan_t scan, ts_position_t start_pos, double sigma_xy, double sigma_theta, int stop, int bd, out int quality)
        {
            ts_position_t currentpos, bestpos, lastbestpos;
            int currentdist;
            int bestdist, lastbestdist;
            int counter = 0;

            if (stop < 0)
            {
                stop = -stop;
            }

            currentpos = new ts_position_t();
            bestpos = new ts_position_t();
            lastbestpos = new ts_position_t();

            currentpos.x = lastbestpos.x = start_pos.x;
            currentpos.y = lastbestpos.y = start_pos.y;
            currentpos.theta = lastbestpos.theta = start_pos.theta;

            currentdist = ts_distance_scan_to_map(scan, currentpos);
            bestdist = lastbestdist = currentdist;

            do
            {
                currentpos.x = lastbestpos.x;
                currentpos.y = lastbestpos.y;
                currentpos.theta = lastbestpos.theta;

                //currentpos.x = ts_random_normal(currentpos.x, sigma_xy);
                //currentpos.y = ts_random_normal(currentpos.y, sigma_xy);
                //currentpos.theta = ts_random_normal(currentpos.theta, sigma_theta);

                currentpos.x = randomPosition(currentpos.x, sigma_xy);
                currentpos.y = randomPosition(currentpos.y, sigma_xy);
                currentpos.theta = randomPosition(currentpos.theta, sigma_theta);                

                currentdist = ts_distance_scan_to_map(scan, currentpos);

                if (currentdist < bestdist)
                {
                    bestdist = currentdist;
                    bestpos.x = currentpos.x;
                    bestpos.y = currentpos.y;
                    bestpos.theta = currentpos.theta;
                }
                else
                {
                    counter++;
                }
                if (counter > stop / 3)
                {
                    if (bestdist < lastbestdist)
                    {
                        lastbestpos.x = bestpos.x;
                        lastbestpos.y = bestpos.y;
                        lastbestpos.theta = bestpos.theta;
                        lastbestdist = bestdist;
                        counter = 0;
                        sigma_xy *= 0.5;
                        sigma_theta *= 0.5;
                    }
                }
            } while (counter < stop);
            if (bd != 0)
                bd = bestdist;

            quality = bd;
            return bestpos;
        }
    }
}
