using System.Collections;
using System.Collections.Generic;

namespace Mahjong
{
    /// <summary>
    /// 点数計算
    /// </summary>
    public class Point 
    {
        /// <summary>
        /// 各符、各飜数による点数
        /// </summary>
        class Record
        {
            /// <summary>
            /// 翻数
            /// </summary>
            public int Han;
            /// <summary>
            /// 符
            /// </summary>
            public int Fu;
            /// <summary>
            /// 親のロンアガリ
            /// </summary>
            public int RonFromOya;
            /// <summary>
            /// 子のロンアガリ
            /// </summary>
            public int RonFromKo;
            /// <summary>
            /// 親のツモアガリ
            /// </summary>
            public int TsumoFromOya;
            /// <summary>
            /// 子のツモアガリ（親の支払い）
            /// </summary>
            public int TsumoFromKoForOya;
            /// <summary>
            /// 子のツモアガリ（他子の支払い）
            /// </summary>
            public int TsumoFromKoForKo;
        }
        /// <summary>
        /// Record生成
        /// </summary>
        /// <param name="han"></param>
        /// <param name="fu"></param>
        /// <param name="oyaron"></param>
        /// <param name="koron"></param>
        /// <param name="oyatsumo"></param>
        /// <param name="kotsumoForOya"></param>
        /// <param name="kotsumoForKo"></param>
        /// <returns></returns>
        private static Record Def(int han, int fu, int oyaron, int koron, int oyatsumo, int kotsumoForOya, int kotsumoForKo)
        {
            return new Record()
            {
                Han = han,
                Fu = fu,
                RonFromOya=oyaron,
                RonFromKo=koron,
                TsumoFromOya=oyatsumo,
                TsumoFromKoForOya=kotsumoForOya,
                TsumoFromKoForKo=kotsumoForKo
            };
        }
        /// <summary>
        /// 表
        /// </summary>
        static readonly Record[] Table = new Record[]
        {
            Def( 1,  20,  1500,    1000,     500,     500,        300 ),         // 20符1翻に限って30符とし、2飜以上の場合は20符で計算する ( wiki 喰い平和形 参照 )
            Def( 1,  30,  1500,    1000,     500,     500,        300 ),
            Def( 1,  40,  2000,    1300,     700,     700,        400 ),
            Def( 1,  50,  2400,    1600,     800,     800,        400 ),
            Def( 1,  60,  2900,    2000,    1000,    1000,        500 ),
            Def( 1,  70,  3400,    2300,    1200,    1200,        600 ),
            Def( 1,  80,  3900,    2600,    1300,    1300,        700 ),
            Def( 1,  90,  4400,    2900,    1500,    1500,        800 ),
            Def( 1, 100,  4800,    3200,    1600,    1600,        800 ),
            Def( 1, 110,  5300,    3600,    1800,    1800,        900 ),

            Def( 2,  20,     0,       0,     700,     700,        400 ),
            Def( 2,  25,  2400,    1600,     800,     800,        400 ),
            Def( 2,  30,  2900,    2000,    1000,    1000,        500 ),
            Def( 2,  40,  3900,    2600,    1300,    1300,        700 ),
            Def( 2,  50,  4800,    3200,    1600,    1600,        800 ),
            Def( 2,  60,  5800,    3900,    2000,    2000,       1000 ),
            Def( 2,  70,  6800,    4500,    2300,    2300,       1200 ),
            Def( 2,  80,  7700,    5200,    2600,    2600,       1300 ),
            Def( 2,  90,  8700,    5800,    2900,    2900,       1500 ),
            Def( 2, 100,  9600,    6400,    3200,    3200,       1600 ),
            Def( 2, 110, 10600,    7100,    3600,    3600,       1800 ),

            Def( 3,  20,     0,       0,    1300,    1300,        700 ),
            Def( 3,  25,  4800,    3200,    1600,    1600,        800 ),
            Def( 3,  30,  5800,    3900,    2000,    2000,       1000 ),
            Def( 3,  40,  7700,    5200,    2600,    2600,       1300 ),
            Def( 3,  50,  9600,    6400,    3200,    3200,       1600 ),
            Def( 3,  60, 11600,    7700,    3900,    3900,       2000 ),

            Def( 4,  20,     0,       0,    2600,    2600,       1300 ),
            Def( 4,  25,  9600,    6400,    3200,    3200,       1600 ),
            Def( 4,  30, 11600,    7700,    3900,    3900,       2000 ),
        };
        /// <summary>
        /// 計算一般呼び出し用
        /// </summary>
        /// <param name="han">翻数</param>
        /// <param name="fu">符</param>
        /// <param name="oya">親？</param>
        /// <param name="tsumo">ツモアガリ？</param>
        /// <returns>(親の支払い額, 子の支払額)</returns>
        public (int, int) Calculate(int han, int fu, bool oya, bool tsumo)
        {
            if (tsumo)
            {
                return CalculateTsumo(han, fu, oya);
            }
            else
            {
                var point = CalculateRon(han, fu, oya);
                return (0, point);
            }
        }
        /// <summary>
        /// ツモアガリ計算
        /// </summary>
        /// <param name="han"></param>
        /// <param name="fu"></param>
        /// <param name="oya">親アガリの場合true</param>
        /// <returns>(親の支払い額, 子の支払額)</returns>
        public (int,int) CalculateTsumo(int han, int fu, bool oya)
        {
            //  マンガン未満を検索
            foreach (var r in Table)
            {
                if (r.Han == han && r.Fu == fu)
                {
                    if (oya)
                    {
                        return (r.TsumoFromOya, r.TsumoFromOya);
                    }
                    else
                    {
                        return (r.TsumoFromKoForOya, r.TsumoFromKoForKo);
                    }
                }
            }
            //  マンガン以上を計算
            switch (han)
            {
                //  満貫
                case 3:
                case 4:
                case 5:
                    return oya ? (0, 4000) : (4000, 2000);
                //  跳満
                case 6:
                case 7:
                    return oya ? (0, 6000) : (6000, 3000);
                //  倍満
                case 8:
                case 9:
                case 10:
                    return oya ? (0, 8000) : (8000, 4000);
                //  三倍満
                case 11:
                case 12:
                    return oya ? (0, 12000) : (12000, 6000);
                //  役満
                case 13:
                default:
                    var ratio = han / 13;
                    return oya ? (0, 16000 * ratio) : (16000 * ratio, 8000 * ratio);
            }
            
            throw new System.Exception();
        }
        /// <summary>
        /// ロンアガリ計算
        /// </summary>
        /// <param name="han"></param>
        /// <param name="fu"></param>
        /// <param name="oya">親がロンアガリならtrue</param>
        /// <returns>支払額</returns>
        public int CalculateRon(int han, int fu, bool oya)
        {
            //  マンガン未満を検索
            foreach (var r in Table)
            {
                if (r.Han == han && r.Fu == fu)
                {
                    if (oya)
                    {
                        return r.RonFromOya;
                    }
                    else
                    {
                        return r.RonFromKo;
                    }
                }
            }
            //  マンガン以上を計算
            //  マンガン以上を計算
            switch (han)
            {
                //  満貫
                case 3:
                case 4:
                case 5:
                    return oya ? 12000 : 8000;
                //  跳満
                case 6:
                case 7:
                    return oya ? 18000 : 12000;
                //  倍満
                case 8:
                case 9:
                case 10:
                    return oya ? 24000 : 16000;
                //  三倍満
                case 11:
                case 12:
                    return oya ? 32000 : 24000;
                //  役満
                case 13:
                default:
                    var ratio = han / 13;
                    return oya ? 48000 * ratio : 32000 * ratio;
            }

            throw new System.Exception();
        }
    }
}