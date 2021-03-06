using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong
{
    /// <summary>
    /// 符計算
    /// </summary>
    public class FuCheck
    {
        /// <summary>
        /// 計算
        /// </summary>
        /// <param name="mentsu"></param>
        /// <param name="context"></param>
        /// <param name="player"></param>
        /// <param name="yakus"></param>
        /// <returns></returns>
        public int Calculate(MentsuList mentsu, Context context, Player player, IEnumerable<Yaku> yakus)
        {
            bool hasChitoitsu = yakus.Any(_ => _ == Yaku.Chitoitsu);

            int fu = 0;
            if (hasChitoitsu)
            {
                fu = 25;
            }
            else
            {
                fu = CalcNormalMentsuList(mentsu, context, player, yakus);
            }
            return fu;
        }

        /// <summary>
        /// 通常役（七対子以外）の場合の符計算をする
        /// </summary>
        /// <param name="mentsu"></param>
        /// <param name="context"></param>
        /// <param name="player"></param>
        /// <param name="yakus"></param>
        /// <returns></returns>
        private int CalcNormalMentsuList(MentsuList mentsu, Context context, Player player, IEnumerable<Yaku> yakus)
        {
            //  副底 フーテイ。必ず与えられる20符。符底ともいう
            int fu = CalcFutei();

            //  アガリメンツ
            var agariMentsu = mentsu.AgariMentsu;
            //  鳴き
            var naki = mentsu.Any(_ => _.IsNaki);
            //  ツモアガリ
            var tsumoAgari = agariMentsu.Any(_ => _.IsTsumoAgari);

            //  面前加符（メンゼンカフ。門前でロン和了した場合に10符が与えられる）
            fu += CalcMenzenKafu(!tsumoAgari, !naki);

            //  メンツ、雀頭による加符
            fu += CalcMentsuFu(mentsu, context, player);

            //  待ち牌による加符
            fu += CalcAgariMentsuFu(agariMentsu);

            //  ピンフのチェックによるツモ符
            fu += CalcAgariYakuFu(yakus, tsumoAgari);

            //  喰い平和ロン上がりは３０符
            fu = CalcKuiPinhu(fu);

            //  下一桁は繰り上がり
            fu += 9;
            fu /= 10;
            fu *= 10;

            return fu;
        }

        /// <summary>
        /// フーテイ（基本符）
        /// </summary>
        /// <returns></returns>
        private int CalcFutei()
        {
            return 20;
        }

        /// <summary>
        /// 面前加符 （メンゼンカフ。門前でロン和了した場合に10符が与えられる）
        /// </summary>
        /// <param name=""></param>
        /// <param name="menzen"></param>
        /// <returns></returns>
        private int CalcMenzenKafu(bool ron, bool menzen)
        {
            if (ron && menzen)
            {
                return 10;
            }
            return 0;
        }
        /// <summary>
        /// 各メンツ、アタマの構成による加符
        /// </summary>
        /// <param name="mentsu"></param>
        /// <returns></returns>
        private int CalcMentsuFu(MentsuList mentsu, Context context, Player player)
        {
            int fu = 0;
            foreach (var m in mentsu)
            {
                if (m.IsShuntsu)
                {
                    continue;
                }
                else if (m.IsKotsu)
                {
                    var kafu = m.IsYaochuu ? 8 : 4;
                    if (m.IsNaki) kafu /= 2;
                    fu += kafu;
                }
                else if (m.IsKantsu)
                {
                    var kafu = m.IsYaochuu ? 32 : 16;
                    if (m.IsNaki) kafu /= 2;
                    fu += kafu;
                }
                else if (m.IsHead && m[0].Group == Group.Jihai)
                {
                    //  三元牌
                    if (m.IsSangen)
                    {
                        fu += 2;
                    }
                    //  自風
                    else if (context.PlayerToId(player.Index) == m[0].Id)
                    {
                        fu += 2;
                    }
                    //  場風
                    else if (context.FieldToId == m[0].Id)
                    {
                        fu += 2;
                    }
                }
            }
            return fu;
        }

        /// <summary>
        ///  待ち牌による加符
        /// </summary>
        /// <param name="agariMentsu"></param>
        /// <returns></returns>
        private int CalcAgariMentsuFu(Mentsu agariMentsu)
        {
            int fu = 0;
            // シャボ
            if (agariMentsu.IsKotsu)
            {
                //  0
            }
            // シュンツ
            else if (agariMentsu.IsShuntsu)
            {
                // カンチャン
                if (agariMentsu[1].IsAgari)
                {
                    fu += 2;
                }
                // ペンチャン
                else if ((agariMentsu[0].Id == Id.N1 && agariMentsu[2].IsAgari)
                || (agariMentsu[2].Id == Id.N9 && agariMentsu[0].IsAgari))
                {
                    fu += 2;
                }
                // 両面待ち
                else
                {
                    //  0
                }
            }
            // 単騎待ち
            else if (agariMentsu.IsHead)
            {
                fu += 2;
            }
            return fu;
        }
        /// <summary>
        /// 和了の方法による符（ツモ和了の場合、2符が加算される）鳴いていても加算する
        /// ピンフ成立の場合は計算しない
        /// </summary>
        /// <returns></returns>
        private int CalcAgariYakuFu(IEnumerable<Yaku> yakus, bool tsumoAgari)
        {
            bool pinhu = yakus.Any(_ => _ == Yaku.Pinhu);
            if (pinhu) return 0;
            if (tsumoAgari)
            {
                return 2;
            }
            return 0;
        }
        /// <summary>
        /// 最低符は30符になる
        /// 20符の場合は食いピンフの可能性がある
        /// </summary>
        /// <param name="fu"></param>
        /// <returns></returns>
        private int CalcKuiPinhu(int fu)
        {
            return fu <= 20 ? 30 : fu;
        }
    }
    
}