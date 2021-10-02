using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong
{
    public struct YakuResult
    {
        public Yaku Yaku;
    }
    public interface IYakuChecker
    {
        int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result);
    }

    /// <summary>
    /// 役属性
    /// </summary>
    public class YakuAttribute : System.Attribute
    {
        /// <summary>
        /// 計算順
        /// </summary>
        public int Priority { get; private set; }

        public YakuAttribute(int priority)
        {
            Priority = priority;
        }

        public const int SpYakuman = 200;
        public const int Yakuman = 100;
        public const int Normal = 10;
    }
    public static class YakuPriorityExtention
    {
        public static int Priority(this System.Type type, int ifundefined = -1)
        {
            var attrs = type.GetCustomAttributes(attributeType: typeof(YakuAttribute), inherit: true);
            if (attrs != null && attrs.Length != 0)
            {
                foreach (var a in attrs)
                {
                    var attr = a as YakuAttribute;
                    if (attr != null)
                    {
                        return attr.Priority;
                    }
                }
            }
            return ifundefined;
        }
    }
    public class YakuCheck
    {
        private IEnumerable<IYakuChecker> _checkers = null;

        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  IYakuChecker実装クラスを抽出する
            if (_checkers == null)
            {
                var type = typeof(IYakuChecker);
                var checkersType = System.Reflection.Assembly.GetAssembly(type)  //  IYakuCheckerが定義されているAssemblyを取得する
                    .GetTypes() //  Assembly内のClassを全て取得する
                    .Where(x => !x.IsInterface && type.IsAssignableFrom(x))   //interface ではなく、IYakuCheckerを実装しているクラスを抽出する
                    .OrderByDescending(x => x.Priority())
                    ;

                //  IYakuCheckerをインスタンス化
                _checkers = checkersType.Select(type => System.Activator.CreateInstance(type) as IYakuChecker);
            }

            var agari = new List<System.Tuple<IYakuChecker, int>>();
            foreach (var y in _checkers)
            {
                int r = y.Calculate(mentsu, context, player, ref result);
                if (0 != r)
                {
                    agari.Add(new System.Tuple<IYakuChecker, int>(y, r));
                    UnityEngine.Debug.Log($"agari is {y.GetType().Name}");
                }
            }
            return agari.Select(_ => _.Item2).Sum();
        }
    }


    #region 1役
    /// <summary>
    /// タンヤオ
    /// </summary>
    public class TannyaoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            foreach (var m in mentsu)
            {
                //  そもそも成立していない
                if (!(m.IsKotsu || m.IsHead || m.IsShuntsu || m.IsKantsu))
                {
                    return 0;
                }
                //  ヤオチュー牌の場合は非成立
                if (m.IsYaochuu)
                {
                    return 0;
                }

                //  喰いタン無効なら
                //  TODO
                if (true)
                {
                    //  鳴いてるなら非成立
                    if (m.IsNaki)
                    {
                        return 0;
                    }
                }
            }
            return 1;
        }
    }
    /// <summary>
    /// ピンフ
    /// </summary>
    public class PinhuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  コーツ、カンツをもっているなら不成立
            if (mentsu.HasKotsuOrKantsu)
            {
                return 0;
            }
            //  シュンツでなければ不成立
            var agariMentsu = mentsu.AgariMentsu;
            if (!agariMentsu.IsShuntsu) return 0;
            //  カンチャンなら不成立
            if (agariMentsu[1].IsAgari) return 0;
            //  ペンチャンだと不成立
            if ((agariMentsu[0].Id == Id.N1 && agariMentsu[2].IsAgari)
                || (agariMentsu[2].Id == Id.N9 && agariMentsu[0].IsAgari))
            {
                return 0;
            }
            //  アタマのチェック
            foreach (var m in mentsu)
            {
                if (m.IsHead)
                {
                    if (m[0].Group == Group.Jihai)
                    {
                        //  アタマが三元牌なら不成立
                        if (m[0].Id == Id.Chun || m[0].Id == Id.Haku || m[0].Id == Id.Hatsu)
                        {
                            return 0;
                        }
                        //  アタマが場風牌なら不成立
                        if (m[0].Id == context.FieldToId)
                        {
                            return 0;
                        }
                        //  アタマが自風牌なら不成立
                        if (m[0].Id == context.PlayerToId(player.Index))
                        {
                            return 0;
                        }
                    }
                }
            }
            //  成立
            return 1;
        }
    }
    /// <summary>
    /// イーペーコー
    /// </summary>
    public class IipeikoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  リャンペーコーが成立するなら、イーペーコーは不成立
            var tmpResult = new YakuResult();
            var ykRyanpeko = new RyanpekouChecker();
            if (0 != ykRyanpeko.Calculate(mentsu, context, player, ref tmpResult))
            {
                return 0;
            }

            //  鳴いてるなら不成立
            bool naki = mentsu.Where(m => m.IsNaki).Any();
            if (naki) return 0;

            //  同じシュンツが2個あれば成立
            for (int i = 0; i < mentsu.Count() - 1; ++i)
            {
                for (int j = i + 1; j < mentsu.Count(); ++j)
                {
                    if (mentsu[i].IsShuntsu && mentsu[j].IsShuntsu)
                    {
                        if (mentsu[i].IsSame(mentsu[j]))
                        {
                            return 1;
                        }
                    }
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// リーチ
    /// </summary>
    public class ReachChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            return player.IsReach ? 1 : 0;
        }
    }
    /// <summary>
    /// 一発
    /// </summary>
    public class IppatsuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            return player.IsIppatsu ? 1 : 0;
        }
    }
    /// <summary>
    /// ツモ
    /// </summary>
    public class TsumoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var naki = mentsu.Where(_ => _.IsNaki).Count();
            if (naki > 0) return 0;

            var m = mentsu.AgariMentsu;
            if (m == null) throw new System.Exception();

            var ron = m.Where(p => p.IsRonAgari).Any();
            if (ron) return 0;

            var tsumo = m.Where(p => p.IsTsumoAgari).Any();
            if (tsumo) return 1;

            throw new System.Exception();   //  ここに来たら不具合
        }
    }
    /// <summary>
    /// チャンカンホウ
    /// </summary>
    public class ChankanhouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  TODO
            return 0;
        }
    }
    /// <summary>
    /// 嶺上開花
    /// </summary>
    public class RinsyankaihouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  TODO
            return 0;
        }
    }
    /// <summary>
    /// 海底ツモ
    /// </summary>
    public class HaiteiTsumoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            if (!context.GetPaiManager().IsEmpty())
            {
                return 0;
            }

            var last = mentsu.AgariMentsu.Where(p => p.IsTsumoAgari).Any();
            if (last)
            {
                return 1; 
            }
            return 0;
        }
    }
    /// <summary>
    /// 海底ロン
    /// </summary>
    public class HaiteiRonChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            if (!context.GetPaiManager().IsEmpty())
            {
                return 0;
            }

            var last = mentsu.AgariMentsu.Where(p => p.IsRonAgari).Any();
            if (last)
            {
                return 1;
            }
            return 0;
        }
    }
    
    /// <summary>
    /// 白
    /// </summary>
    public class HakuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            if (mentsu.FilterKotsuKantsuWithId(Id.Haku).Any())
            {
                return 1;
            }
            return 0;
        }
    }
    /// <summary>
    /// 撥
    /// </summary>
    public class HatsuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            if (mentsu.FilterKotsuKantsuWithId(Id.Hatsu).Any())
            {
                return 1;
            }
            return 0;
        }
    }
    /// <summary>
    /// 中
    /// </summary>
    public class ChunChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            if (mentsu.FilterKotsuKantsuWithId(Id.Chun).Any())
            {
                return 1;
            }
            return 0;
        }
    }
    /// <summary>
    /// 東
    /// </summary>
    public class TonChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var id = Id.Ton;
            if (mentsu.FilterKotsuKantsuWithId(id).Any())
            {
                if (id == context.PlayerToId(player.Index))
                {
                    return 1;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 南
    /// </summary>
    public class NanChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var id = Id.Nan;
            if (mentsu.FilterKotsuKantsuWithId(id).Any())
            {
                if (id == context.PlayerToId(player.Index))
                {
                    return 1;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 西
    /// </summary>
    public class ShaChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var id = Id.Sha;
            if (mentsu.FilterKotsuKantsuWithId(id).Any())
            {
                if (id == context.PlayerToId(player.Index))
                {
                    return 1;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 北
    /// </summary>
    public class PeiChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var id = Id.Pei;
            if (mentsu.FilterKotsuKantsuWithId(id).Any())
            {
                if (id == context.PlayerToId(player.Index))
                {
                    return 1;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 東　場風
    /// </summary>
    public class TonFieldChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var id = Id.Ton;
            if (mentsu.FilterKotsuKantsuWithId(id).Any())
            {
                if (id == context.FieldToId)
                {
                    return 1;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 南　場風
    /// </summary>
    public class NanFieldChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var id = Id.Nan;
            if (mentsu.FilterKotsuKantsuWithId(id).Any())
            {
                if (id == context.FieldToId)
                {
                    return 1;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 西　場風 
    /// </summary>
    public class ShaFieldChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var id = Id.Sha;
            if (mentsu.FilterKotsuKantsuWithId(id).Any())
            {
                if (id == context.FieldToId)
                {
                    return 1;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 北　場風
    /// </summary>
    public class PeiFieldChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var id = Id.Pei;
            if (mentsu.FilterKotsuKantsuWithId(id).Any())
            {
                if (id == context.FieldToId)
                {
                    return 1;
                }
            }
            return 0;
        }
    }
    #endregion  //1役
    #region 2役
    /// <summary>
    /// 三色同順
    /// </summary>
    public class SansyokuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var shunts = mentsu.Where(_ => _.IsShuntsu);
            var naki = mentsu.Where(_ => _.IsNaki).Any();

            //  シュンツが３つより少ないなら非成立
            if (shunts.Count() < 3) return 0;
            //  マンズ、ピンズ、ソウズが揃っていないなら非成立
            var manShunts = shunts.Where(_ => _[0].Group == Group.Manz);
            var pinShunts = shunts.Where(_ => _[0].Group == Group.Pinz);
            var souShunts = shunts.Where(_ => _[0].Group == Group.Souz);
            if (manShunts.Count() == 0 || pinShunts.Count() == 0 || souShunts.Count() == 0)
            {
                return 0;
            }
            //  すべてシュンツなので、最初のPaiが同じIdなら成立している
            //  正確な判定は省略する
            foreach (var m in manShunts)
            {
                foreach (var p in pinShunts)
                {
                    foreach (var s in souShunts)
                    {
                        if (m[0].Id == p[0].Id
                            && m[0].Id == s[0].Id)
                        {
                            return naki ? 1 : 2;
                        }
                    }
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 一気通貫
    /// </summary>
    public class IkkitsukanChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var shunts = mentsu.Where(_ => _.IsShuntsu);
            var naki = mentsu.Where(_ => _.IsNaki).Any();

            //  シュンツが３つより少ないなら非成立
            if (shunts.Count() < 3) return 0;

            //  123のシュンツを検索
            //  正確な判定は省略する
            foreach (var s123 in shunts.Where(_ => _[0].Id == Id.N1))
            {
                //  456で123と同じ絵を検索
                if (shunts.Where(_ => _[0].Group == s123[0].Group && _[0].Id == Id.N4).Count() > 0)
                {
                    //  789で123と同じ絵を検索
                    if (shunts.Where(_ => _[0].Group == s123[0].Group && _[0].Id == Id.N7).Count() > 0)
                    {
                        return naki ? 1 : 2;
                    }
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 対々和
    /// </summary>
    public class ToitoiChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var kotsuKantsu = mentsu.FilterKotsuKantsu();

            if (kotsuKantsu.Count() == 4)
            {
                return 2;
            }
            return 0;
        }
    }
    /// <summary>
    /// 三暗刻
    /// </summary>
    public class SanankoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var ankoAnkantsu = mentsu.Where(_ => _.IsAnnkantsu || _.IsAnnko);

            if (ankoAnkantsu.Count() < 3)
            {
                return 0;
            }
            return 2;
        }
    }
    public static class Ext
    {
        private static bool Has(
            this IEnumerable<Mentsu> mentsu,
            System.Func<Mentsu, bool> filter,
            System.Tuple<Group, Id> target,
            System.Func<Group, Id, System.Tuple<Group, Id>> selector,
            int requestNum,
            ref int foundNum)
        {
            foreach (var m in mentsu)
            {
                if (filter(m))
                {
                    foundNum += 1;
                    if (foundNum >= requestNum)
                    {
                        return true;
                    }
                    else
                    {
                        return Has(
                            mentsu.Except(new List<Mentsu> { m }),
                            filter,
                            selector(target.Item1, target.Item2),
                            selector,
                            requestNum,
                            ref foundNum
                        );
                    }
                }
            }
            return false;
        }
        public static bool HasRenko(this IEnumerable<Mentsu> mentsu, Group g, Id i, int requestNum)
        {
            int foundNum = 0;
            return Has(
                mentsu: mentsu,
                filter: m => (m.IsKantsu || m.IsKotsu) && m[0].Group != Group.Jihai && m[0].IsSame(g, i),
                target: new System.Tuple<Group, Id>(g, i),
                selector: (g, i) => new System.Tuple<Group, Id>(g, i + 1),
                requestNum: requestNum,
                foundNum: ref foundNum
            );
        }
    }
    /// <summary>
    /// 三連刻
    /// </summary>
    public class SanrenkoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var kotsuKantsu = mentsu.Where(_ => (_.IsKotsu || _.IsKantsu) && _[0].Group != Group.Jihai);

            if (kotsuKantsu.Count() < 3)
            {
                return 0;
            }

            foreach (var m in kotsuKantsu)
            {
                if (kotsuKantsu.HasRenko(m[0].Group, m[0].Id, 3))
                {
                    return 2;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// チャンタ
    /// </summary>
    public class ChantaChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            bool naki = false;
            bool jihai = false;
            foreach (var m in mentsu)
            {
                if (!m.IsYaochuu) return 0;

                if (m[0].Group == Group.Jihai) jihai = true;
                if (m.IsNaki) naki = true;
            }
            if (!jihai) return 0;
            return naki ? 1 : 2;
        }
    }
    /// <summary>
    /// 混老頭
    /// </summary>
    public class HonroutouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            foreach (var m in mentsu)
            {
                if (!m.IsYaochuu) return 0;
                if (m.IsShuntsu) return 0;
            }
            return 2;
        }
    }
    /// <summary>
    /// 小三元
    /// </summary>
    public class ShousangenChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var sangen = mentsu.Where(_ => _.IsSangen);
            bool head = false;
            bool haku = false;
            bool chun = false;
            bool hatsu = false;
            foreach (var m in sangen)
            {
                if (m.IsHead) head = true;
                if (m[0].Id == Id.Chun) chun = true;
                if (m[0].Id == Id.Haku) haku = true;
                if (m[0].Id == Id.Hatsu) hatsu = true;
            }
            return head && haku && chun && hatsu ? 2 : 0;
        }
    }
    /// <summary>
    /// 三色同ポン
    /// </summary>
    public class SansyokudouponChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var kotsuKantsu = mentsu.Where(_ => (_.IsKotsu || _.IsKantsu) && _[0].Group != Group.Jihai);

            if (kotsuKantsu.Count() < 3)
            {
                return 0;
            }


            foreach (var m in kotsuKantsu)
            {
                bool hasManz = false;
                bool hasPinz = false;
                bool hasSouz = false;
                foreach (var mm in kotsuKantsu)
                {
                    if (m[0].Serial == mm[0].Serial)
                    {
                        switch (mm[0].Group)
                        {
                            case Group.Manz: hasManz = true; break;
                            case Group.Pinz: hasPinz = true; break;
                            case Group.Souz: hasSouz = true; break;
                        }
                    }
                    else
                    if (m[0].Id == mm[0].Id)
                    {
                        switch (mm[0].Group)
                        {
                            case Group.Manz: hasManz = true; break;
                            case Group.Pinz: hasPinz = true; break;
                            case Group.Souz: hasSouz = true; break;
                        }
                    }
                }
                if (hasManz && hasPinz && hasSouz)
                {
                    return 2;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 三槓子
    /// </summary>
    public class SankantsuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var kantsu = mentsu.Where(_ => _.IsKantsu);
            if (kantsu.Count() == 3)
            {
                return 2;
            }
            return 0;
        }
    }
    /// <summary>
    /// ダブルリーチ
    /// </summary>
    public class DoublereachChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            if (player.IsDoubleReach && !player.IsReach)
            {
                throw new System.Exception();   //  DoubleReach と Reach は別々にtrueになっていることはありえない
            }
            return player.IsDoubleReach ? 1 : 0;
        }
    }
    #endregion  //2役
    #region 2役特殊
    /// <summary>
    /// 七対子
    /// </summary>
    public class ChitoitsuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            if (mentsu.Count() != 7) return 0;
            foreach (var m in mentsu)
            {
                if (!m.IsHead) return 0;
            }
            return 2;
        }
    }
    #endregion //2役特殊
    #region 3役
    /// <summary>
    /// ジュンチャン
    /// </summary>
    public class JunchanChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  ヤオチューでないものがあれば不成立
            var notYaochuu = mentsu.Where(_ => !_.IsYaochuu);
            if (notYaochuu.Any()) return 0;

            var yaochuu = mentsu.Where(_ => _.IsYaochuu);

            //  字牌があるなら不成立
            var jihai = yaochuu.Where(_ => _[0].Group == Group.Jihai).Any();
            if (jihai) return 0;

            var naki = yaochuu.Where(_ => _.IsNaki).Any();

            return naki ? 2 : 3;
        }
    }
    /// <summary>
    /// 混一色
    /// </summary>
    public class HonitsuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var jihai = mentsu.Where(_ => _.IsYaochuu && _[0].Group == Group.Jihai);

            //  字牌が無いなら不成立
            if (!jihai.Any()) return 0;

            //  数牌だけにする
            var numbers = mentsu.Except(jihai);
            if (!numbers.Any()) return 0;  //  絵柄がないと字一色がありえるので

            //  異なる絵柄があれば不成立
            var group = numbers.First()[0].Group;


            foreach (var m in numbers)
            {
                if (m[0].Group != group)
                {
                    return 0;
                }
            }

            var naki = mentsu.Where(_ => _.IsNaki).Any();
            return naki ? 2 : 3;
        }
    }
    /// <summary>
    /// リャンペーコー
    /// </summary>
    public class RyanpekouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  鳴いてるなら不成立
            bool naki = mentsu.Where(m => m.IsNaki).Any();
            if (naki) return 0;

            //  同じシュンツが2個x2セットあれば成立
            int set = 0;
            for (int i = 0; i < mentsu.Count() - 1; ++i)
            {
                for (int j = i + 1; j < mentsu.Count(); ++j)
                {
                    if (mentsu[i].IsShuntsu && mentsu[j].IsShuntsu)
                    {
                        if (mentsu[i].IsSame(mentsu[j]))
                        {
                            set += 1;
                            if (set >= 2) return 3;
                        }
                    }
                }
            }
            return 0;
        }
    }
    #endregion
    #region 6役
    /// <summary>
    /// 清一色
    /// </summary>
    public class ChinitsuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  字牌があるなら不成立
            var jihai = mentsu.Where(_ => _[0].Group == Group.Jihai).Any();
            if (jihai) return 0;


            //  異なる絵柄があれば不成立
            var group = mentsu.First()[0].Group;

            foreach (var m in mentsu)
            {
                if (m[0].Group != group)
                {
                    return 0;
                }
            }

            var naki = mentsu.Where(_ => _.IsNaki).Any();
            return naki ? 5 : 6;
        }
    }
    #endregion
    #region 役満
    /// <summary>
    /// 大三元
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class DaisangenChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var sangen = mentsu.Where(_ => (_.IsKotsu || _.IsKantsu) && _.IsSangen).ToList();

            //  3つないなら不成立
            if (sangen.Count() != 3) return 0;

            //  
            bool haku = false;
            bool chun = false;
            bool hatsu = false;
            foreach (var m in sangen)
            {
                if (m[0].Id == Id.Chun) chun = true;
                if (m[0].Id == Id.Haku) haku = true;
                if (m[0].Id == Id.Hatsu) hatsu = true;
            }
            return haku && chun && hatsu ? 13 : 0;
        }
    }
    /// <summary>
    /// 四暗刻
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class SuankoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  メンゼンでなければ不成立
            var naki = mentsu.Where(_ => _.IsNaki).Any();
            if (naki) return 0;

            //  暗刻、暗槓が４つないと不成立
            var ankoAnkantsu = mentsu.Where(_ => _.IsAnnkantsu || _.IsAnnko);

            if (ankoAnkantsu.Count() < 4)
            {
                return 0;
            }
            return 13;
        }
    }
    /// <summary>
    /// 四連刻
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class SurenkoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var kotsuKantsu = mentsu.Where(_ => (_.IsKotsu || _.IsKantsu) && _[0].Group != Group.Jihai);

            if (kotsuKantsu.Count() < 4)
            {
                return 0;
            }

            foreach (var m in kotsuKantsu)
            {
                if (kotsuKantsu.HasRenko(m[0].Group, m[0].Id, 4))
                {
                    return 13;
                }
            }
            return 0;
        }
    }
    /// <summary>
    /// 四槓子
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class SukantsuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var kantsu = mentsu.Where(_ => _.IsKantsu);
            if (kantsu.Count() == 4)
            {
                return 13;
            }
            return 0;
        }
    }
    /// <summary>
    /// 字一色
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class TsuisoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var numbers = mentsu.Where(_ => _.IsSameGroup && _[0].Group != Group.Jihai);
            if (numbers.Any()) return 0;

            return 13;
        }
    }
    /// <summary>
    /// 小四喜
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class ShousushiChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            var kaze = mentsu.Where(_ => _.IsKaze);
            if (kaze.Count() != 4) return 0;

            var hasHead = kaze.Where(_ => _.IsHead).Any();
            if (!hasHead) return 0;

            bool hasT = false;
            bool hasN = false;
            bool hasS = false;
            bool hasP = false;

            foreach (var m in kaze)
            {
                switch (m[0].Id)
                {
                    case Id.Ton: hasT = true; break;
                    case Id.Nan: hasN = true; break;
                    case Id.Sha: hasS = true; break;
                    case Id.Pei: hasP = true; break;
                    default: return 0;
                }
            }
            return hasT && hasN && hasS && hasP ? 13 : 0;
        }
    }
    /// <summary>
    /// 大四喜
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class DaisushiChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result) 
        {
            var kaze = mentsu.Where(_ => _.IsKaze);
            if (kaze.Count() != 4) return 0;

            var hasHead = kaze.Where(_ => _.IsHead).Any();
            if (hasHead) return 0;

            bool hasT = false;
            bool hasN = false;
            bool hasS = false;
            bool hasP = false;

            foreach (var m in kaze)
            {
                switch (m[0].Id)
                {
                    case Id.Ton: hasT = true; break;
                    case Id.Nan: hasN = true; break;
                    case Id.Sha: hasS = true; break;
                    case Id.Pei: hasP = true; break;
                    default: return 0;
                }
            }
            return hasT && hasN && hasS && hasP ? 13 : 0;
        }
    }
    /// <summary>
    /// 緑一色
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class RyuisoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  緑でないものがあれば不成立
            var notGreen = mentsu.Where(_ => !_.IsGreen);
            if (notGreen.Any()) return 0;

            return 13;
        }
    }
    /// <summary>
    /// 清老頭
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class ChinroutouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            foreach (var m in mentsu)
            {
                if (!m.IsYaochuu) return 0;
                if (m.IsShuntsu) return 0;
                if (m[0].Group == Group.Jihai) return 0;
            }
            return 13;
        }
    }
    /// <summary>
    /// 九蓮宝燈
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class ChuurenpoutouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  字牌または鳴きがあるなら不成立
            var jihaiOrNaki = mentsu.Where(_ => (_.IsNaki || _[0].Group == Group.Jihai));
            if (jihaiOrNaki.Any()) return 0;

            //  1,1,1,2,3,4,5,6,7,8,9,9,9,* の構成かしらべるため、カウントする
            var dic = new Dictionary<Id, int>() 
            {
                { Id.N1, 0 },
                { Id.N2, 0 },
                { Id.N3, 0 },
                { Id.N4, 0 },
                { Id.N5, 0 },
                { Id.N6, 0 },
                { Id.N7, 0 },
                { Id.N8, 0 },
                { Id.N9, 0 },
            };
            foreach(var m in mentsu)
            {
                foreach (var p in m)
                {
                    dic[p.Id] += 1;
                }
            }
            //  所定数を満たさない場合は不成立
            foreach (var k in dic.Keys)
            {
                int c = 1;
                if (k == Id.N1 || k == Id.N9) c = 3;
                if (dic[k] < c)
                {
                    return 0;
                }
            }
            //  この時点で成立している
            //  TODO：純正九蓮宝燈ならダブル役満
            return 13;
        }
    }
    /// <summary>
    /// 国士無双
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class KokushimusouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  アタマ以外の通常メンツがあるなら不成立
            var normalmentsu = mentsu.Where(_ => _.IsNaki || _.IsShuntsu || _.IsKotsu || _.IsKantsu);
            if (normalmentsu.Any()) return 0;

            //  TODO：純正国士無双ならダブル役満
            var dic = new Dictionary<(Group, Id), int> 
            {
                { (Group.Manz, Id.N1), 0 },
                { (Group.Manz, Id.N9), 0 },
                { (Group.Pinz, Id.N1), 0 },
                { (Group.Pinz, Id.N9), 0 },
                { (Group.Souz, Id.N1), 0 },
                { (Group.Souz, Id.N9), 0 },

                { (Group.Jihai, Id.Ton), 0 },
                { (Group.Jihai, Id.Nan), 0 },
                { (Group.Jihai, Id.Sha), 0 },
                { (Group.Jihai, Id.Pei), 0 },

                { (Group.Jihai, Id.Chun), 0 },
                { (Group.Jihai, Id.Haku), 0 },
                { (Group.Jihai, Id.Hatsu), 0 },
            };
            bool head = false;
            foreach(var m in mentsu)
            {
                if (m.IsHead)
                {
                    head = true;
                }
                foreach (var p in m)
                {
                    if (!dic.ContainsKey((p.Group, p.Id))) return 0;
                    
                    dic[(p.Group, p.Id)] += 1;
                }
            }
            if (!head) return 0;
            foreach (var k in dic.Keys)
            {
                if (dic[k] < 1)
                {
                    return 0;
                }
            }
            return 13;
        }
    }
    /// <summary>
    /// 天和
    /// </summary>
    [Yaku(priority: YakuAttribute.SpYakuman)]
    public class TenhouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  1巡目以外不成立
            if (context.TurnCount != 0) 
            {
                return 0;
            }
            //  親以外不成立
            if (context.CurrentOyaPlayer != player.Index)
            {
                return 0;
            }
            //  ツモアガリのみになる
            var p = mentsu.AgariMentsu.Where(_ => _.IsTsumoAgari);
            if (p.Count() != 1) return 0;
            return 13;
        }
    }
    /// <summary>
    /// 地和
    /// </summary>
    [Yaku(priority: YakuAttribute.SpYakuman)]
    public class ChihouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  1巡目以外不成立
            if (context.TurnCount != 0)
            {
                return 0;
            }
            //  親は不成立
            if (context.CurrentOyaPlayer == player.Index)
            {
                return 0;
            }
            //  ツモアガリのみになる
            var p = mentsu.AgariMentsu.Where(_ => _.IsTsumoAgari);
            if (p.Count() != 1) return 0;
            return 13;
        }
    }
    /// <summary>
    /// 人和
    /// </summary>
    [Yaku(priority: YakuAttribute.SpYakuman)]
    public class RenhouChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  1巡目以外不成立
            if (context.TurnCount != 0)
            {
                return 0;
            }
            //  親は不成立
            if (context.CurrentOyaPlayer == player.Index)
            {
                return 0;
            }
            //  ツモアガリのみになる
            var p = mentsu.AgariMentsu.Where(_ => _.IsRonAgari);
            if (p.Count() != 1) return 0;
            return 13;
        }
    }
    /// <summary>
    /// 大車輪
    /// </summary>
    [Yaku(priority: YakuAttribute.Yakuman)]
    public class DaisharinChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  ピンズ以外は不成立
            if (mentsu.Any(_ => _[0].Group != Group.Pinz))
            {
                return 0;
            }

            //  2～8が2個ずつ無いと成立しない
            var ids = new Id[] 
            {
                Id.N2,
                Id.N3,
                Id.N4,
                Id.N5,
                Id.N6,
                Id.N7,
                Id.N8,
            };
            foreach(var i in ids)
            {
                int c = 0;
                foreach (var m in mentsu)
                {
                    foreach (var p in m)
                    {
                        if(p.Id == i)
                        {
                            ++c;
                        }
                    }
                }
                if (c != 2)
                {
                    return 0;
                }
            }
            return 13;
        }
    }
    /// <summary>
    /// 八連荘
    /// </summary>
    [Yaku(priority: YakuAttribute.SpYakuman)]
    public class Renchan8Checker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            return 0;
        }
    }
    #endregion
}