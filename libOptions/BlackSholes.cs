using System;

namespace libOptions
{
    
    public static class BlackSholes
    {
        private const double Pi = 3.141592653589793238462643;

        static public int ImplVol(double dPx, double dUnderPx, double dStrike, double dT, double dR, double dQ, bool isCall,
              double dSigMin,
              out double  dImplVol)
        {
            const int nMaxIter = 50;
            const double dEps = 10e-6;

            dImplVol = double.NaN;
            
            double dTmp = isCall ? Math.Max(dUnderPx*Math.Exp(-dQ*dT)-dStrike*Math.Exp(-dR*dT),0) : 
                Math.Max(dStrike*Math.Exp(-dR*dT)-dUnderPx*Math.Exp(-dQ*dT),0);
               
            if(Math.Abs(dPx-dTmp)<=Math.Sqrt(dEps)) return 3;
               
            double dSig  = dSigMin; /* initial estimate */
            double dSig1 = double.NaN;
            bool bDone=false;
            var greeks = new Greeks();
            for(var i =0;i<nMaxIter&&(!bDone);++i)
            {
                double dVal;
                if(BlackSholesCals(dUnderPx, dStrike, dT, dR, dQ, isCall, dSig, out dVal, greeks)!= 0)  return 1;
                dSig1 = dSig - (dVal - dPx)/greeks.Vega;                 /* compute the new estimate of sigma using Newton’s method */
                if (dEps > Math.Abs((dSig1 - dSig)/dSig1)) bDone=true;   /* check whether the specified  accuracy has been reached */
                dSig = dSig1; /* up date sigma */
            }
            dImplVol = dSig1; /* return the estimate for sigma */
            return !bDone ? 3 : 0;
        }

           static public int
                 ImplVol_Newton(double dPx, double dUnderPx, double dStrike, double dT, double dR, double dQ, bool isCall,out double dImplVol)
           {
               const int nMaxIter = 10000;
               const double dEps = 10e-6;

               dImplVol = Math.Sqrt(Math.Abs(Math.Log(dUnderPx / dStrike) + dR * dT) * 2 / dT);
               
               var greeks = new Greeks();
               double dTryPx;
               if (BlackSholesCals(dUnderPx, dStrike, dT, dR, dQ, isCall, dImplVol, out dTryPx, greeks) != 0) return 1;
               double dMinDiff = Math.Abs(dPx - dTryPx);

               var bDone = false;
               for (var i = 0; i < nMaxIter; ++i)
               {
                   if (Math.Abs(dPx - dTryPx) < dEps || Math.Abs(dPx - dTryPx) > dMinDiff)
                   {
                       bDone = true;
                       break;
                   }

                   dImplVol = dImplVol - (dTryPx - dPx) / greeks.Vega;
                   if (BlackSholesCals(dUnderPx, dStrike, dT, dR, dQ, isCall, dImplVol, out dTryPx, greeks) != 0) return 1;
                   dMinDiff = Math.Abs(dPx - dTryPx);
               }
               return !bDone ? 3 : 0;
           }

           static public int ImplVol_Bsect(double dPx, double dUnderPx, double dStrike, double dT, double dR, double dQ, bool isCall, out double dImplVol)
           {
               const int nMaxIter = 100;
               double dLowVol = 0.005;
               double dHighVol = 5;
               const double dEps = 1.0e-6;

               dImplVol = double.NaN;

               double  dLowPx;
               double  dHighPx;
               if(BlackSholesCals(dUnderPx, dStrike, dT, dR, dQ, isCall, dLowVol, out dLowPx,null )!= 0)   return 1;
               if(BlackSholesCals(dUnderPx, dStrike, dT, dR, dQ, isCall, dHighVol, out dHighPx,null )!= 0)  return 1;

               var bDone=false;
               dImplVol = dLowVol + (dPx - dLowPx) * (dHighVol - dLowVol) / (dHighPx - dLowPx);
               for (var i = 0; i < nMaxIter; ++i)
               {
                   double  dTryPx;
                   if (BlackSholesCals(dUnderPx, dStrike, dT, dR, dQ, isCall, dImplVol, out dTryPx, null) != 0) return 2;

                   if (Math.Abs(dTryPx - dPx) < dEps) {bDone = true;break;}

                   if (dTryPx < dPx) dLowVol = dImplVol;
                   else dHighVol = dImplVol;

                   if(BlackSholesCals(dUnderPx, dStrike, dT, dR, dQ, isCall, dLowVol, out dLowPx,null )!= 0)   return 2;
                   if(BlackSholesCals(dUnderPx, dStrike, dT, dR, dQ, isCall, dHighVol, out dHighPx,null )!= 0)  return 2;
                   dImplVol = dLowVol + (dPx - dLowPx) * (dHighVol - dLowVol) / (dHighPx - dLowPx);
               }
               return !bDone ? 3 : 0;
           }

        static public int BlackSholesCals(double dUnderPx, double dStrike, double dT, double dR, double dQ, bool isCall,
            double dSigma,out double dPx,Greeks greeks)
        {

            const double dEps = 10e-6;

            dPx = double.NaN;
            if( (dStrike < dEps) || (dSigma < dEps) || (dT < dEps) ) return 2;/* Check if any of the the input  arguments are too small */

            var dExpQt = Math.Exp(-dQ * dT);
            var dExpRt = Math.Exp(-dR * dT);

            var d1 = Math.Log(dUnderPx / dStrike) + (dR - dQ + (dSigma * dSigma / 2.0)) * dT;
            d1 = d1/(dSigma*Math.Sqrt(dT));
            var d2 = d1-dSigma*Math.Sqrt(dT);

            if (isCall) dPx =  dUnderPx * dExpQt * CumNormDistrib(d1) - dStrike * dExpRt * CumNormDistrib(d2);
            else        dPx = -dUnderPx * dExpQt * CumNormDistrib(-d1) + dStrike * dExpRt * CumNormDistrib(-d2);

            if (greeks!=null) 
            {
                double dPnd = Math.Exp(-d1 * d1 / 2.0) / Math.Sqrt(2.0 * Pi);
                
                if (isCall)
                {
                    greeks.Delta = (CumNormDistrib(d1)) * dExpQt; /* delta */
                    greeks.Theta = -dUnderPx * dExpQt * dPnd * dSigma / (2.0 * Math.Sqrt(dT))
                    + dQ * dUnderPx * CumNormDistrib(d1) * dExpQt - dR * dStrike * dExpRt * CumNormDistrib(d2); /* theta */
                    greeks.Rho = dStrike * dT * dExpRt * CumNormDistrib(d2); /* rho */
                }
                else
                {
                    greeks.Delta = (CumNormDistrib(d1) - 1.0) * dExpQt; /* delta */
                    greeks.Theta = -dUnderPx * dExpQt * dPnd * dSigma / (2.0 * Math.Sqrt(dT)) -
                    dQ * dUnderPx * CumNormDistrib(-d1) * dExpQt + dR * dStrike * dExpRt * CumNormDistrib(-d2); /* theta */
                    greeks.Rho = -dStrike * dT * dExpRt * CumNormDistrib(-d2); /* rho */
                }
                greeks.Gamma = dPnd * dExpQt / (dUnderPx * dSigma * Math.Sqrt(dT)); /* gamma */
                greeks.Vega = dUnderPx * dExpQt * Math.Sqrt(dT) * dPnd; /* vega */
                greeks.IV = dSigma;
            }
            return 0;
        }

        static public double CumNormDistrib(double d)
        {
            const double a1 = 0.31938153;
            const double a2 = -0.356563782;
            const double a3 = 1.781477937;
            const double a4 = -1.821255978;
            const double a5 = 1.330274429;
            const double rsqrt2Pi = 0.39894228040143267793994605993438;

            var k = 1.0 / (1.0 + 0.2316419 * Math.Abs(d));

            var cnd = rsqrt2Pi * Math.Exp(-0.5 * d * d) *
                  (k * (a1 + k * (a2 + k * (a3 + k * (a4 + k * a5)))));

            if (d > 0) cnd = 1.0 - cnd;
            return cnd;
        }
    }
}
