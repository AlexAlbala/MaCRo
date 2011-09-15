using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaCRoGS.SLAM
{
    partial class SLAMAlgorithm
    {
        Random rand = new Random();

        public double ts_random_normal(double m, double s)
        {
            double x;
            random.hz = (long)SHR3(random);
            random.iz = (ulong)random.hz & 127;
            x = ((ulong)Math.Abs(random.hz) < random.kn[random.iz]) ? random.hz * random.wn[random.iz] : ts_random_normal_fix(); // Generic version
            return x * s + m;
        }

        private void ts_random_init(ts_randomizer_t d, ulong jsrseed)
        {
            const double m1 = 2147483648.0;

            double dn = 3.442619855899;
            double tn = dn;
            double vn = 9.91256303526217e-3;
            double q;
            int i;
            d.jsr = jsrseed;

            // Set up tables for Normal	  
            q = vn / Math.Exp(-0.5 * dn * dn);
            d.kn[0] = (ulong)((dn / q) * m1);
            d.kn[1] = 0;
            d.wn[0] = q / m1;
            d.wnt[0] = q;
            d.wn[127] = dn / m1;
            d.wnt[127] = dn;
            d.fn[0] = 1.0;
            d.fn[127] = Math.Exp(-0.5 * dn * dn);

            for (i = 126; i >= 1; i--)
            {
                dn = Math.Sqrt(-2.0 * Math.Log(vn / dn + Math.Exp(-0.5 * dn * dn), Math.E));
                d.kn[i + 1] = (ulong)((dn / tn) * m1);
                tn = dn;
                d.fn[i] = Math.Exp(-0.5 * dn * dn);
                d.wn[i] = dn / m1;
                d.wnt[i] = dn;
            }
        }

        public double ts_random_normal_fix()
        {
            ts_randomizer_t d = random;
            const double r = 3.442620; 	// The starting of the right tail 	
            double x, y;
            for (; ; )
            {
                x = d.hz * d.wn[d.iz];
                if (d.iz == 0)
                { // iz==0, handle the base strip
                    do
                    {
                        x = -Math.Log(UNI(d), Math.E) * 0.2904764;
                        // .2904764 is 1/r				
                        y = -Math.Log(UNI(d), Math.E);
                    } while (y + y < x * x);
                    return (d.hz > 0) ? r + x : -r - x;
                }

                // iz>0, handle the wedges of other strips		
                if (d.fn[d.iz] + UNI(d) * (d.fn[d.iz - 1] - d.fn[d.iz]) < Math.Exp(-.5 * x * x))
                    return x;
                // Start all over		
                d.hz = (long)SHR3(d);
                d.iz = (ulong)d.hz & 127;
                if ((ulong)Math.Abs(d.hz) < d.kn[d.iz])
                    return (d.hz * d.wn[d.iz]);
            }
        }

        public ulong SHR3(ts_randomizer_t d)
        {
            d.jz = d.jsr;
            d.jsr ^= (d.jsr << 13);
            d.jsr ^= (d.jsr >> 17);
            d.jsr ^= (d.jsr << 5);
            return d.jz + d.jsr;
        }

        public double UNI(ts_randomizer_t d)
        {
            return .5 + SHR3(d) * .2328306e-9;
        }

        public double randomPosition(double x, double stDev)
        {
            double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rand.NextDouble();

            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)

            if (rand.NextDouble() > 0.5)
                stDev *= -1;
            double randNormal = x + stDev * randStdNormal; //random normal(mean,stdDev^2)

            return randNormal;

        }
    }
}
