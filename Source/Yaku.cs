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
    public class YakuChecker
    {
        static private IEnumerable<IYakuChecker> _checkers = null;

        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  Sampleクラスを抽出する
            var type = typeof(IYakuChecker);
            if (_checkers == null)
            {
                var checkersType = System.Reflection.Assembly.GetAssembly(type)  //  IYakuCheckerが定義されているAssemblyを取得する
                    .GetTypes() //  Assembly内のClassを全て取得する
                    .Where(x => !x.IsInterface && type.IsAssignableFrom(x))   //interface ではなく、IYakuCheckerを実装しているクラスを抽出する
                    ;//.ToArray();

                //  IYakuCheckerをインスタンス化
                _checkers = checkersType.Select(type => System.Activator.CreateInstance(type) as IYakuChecker);            
            }

            var agari = new List<IYakuChecker>();
            foreach (var y in _checkers)
            {
                if (0 != y.Calculate(mentsu, context, player, ref result))
                {
                    agari.Add(y);
                }
            }
            return 0;
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
            foreach(var m in mentsu)
            {
                if (m.IsHead)
                {
                    if (m[0].Group == Group.Jihai)
                    {
                        //  アタマが三元牌なら不成立
                        if (m[0].Id == Id.Chun|| m[0].Id == Id.Haku || m[0].Id == Id.Hatsu)
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
    //Iipekou,                    //  イーペーコー
    public class IIpeikoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  リャンペーコーが成立するなら、イーペーコーは不成立
            YakuRyanpekou ykRyanpeko = new YakuRyanpekou();
            if (0 != ykRyanpeko.Calculate(mentsu, context, player, ref result))
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
    //Reach,                      //  リーチ
    //Ippatsu,                    //  一発
    //Menzentsumo,                //  ツモ
    //Chankanhou,                 //  チャンカンホウ
    //Rinsyan,                    //  嶺上開花
    //Haiteitsumo,                //  海底ツモ
    //Haiteiron,                  //  海底ロン
    //Haku,                       //  白
    //Hatsu,                      //  撥
    //Chun,                       //  中
    //Ton,                        //  東
    //Nan,                        //  南
    //Sha,                        //  西
    //Pei,                        //  北
    //TonField,                   //  東　場風
    //NanField,                   //  南　場風
    //ShaField,                   //  西　場風 
    //PeiField,                   //  北　場風
    #endregion  //1役
    // 
    ////  2役
    //Sansyoku,                   //  三色同順
    //Ikkitsukan,                 //  一気通貫
    //Toitoi,                     //  対々和
    //Sannannkou,                 //  三暗刻
    //Sannrenkou,                 //  三連刻
    //Chanta,                     //  チャンタ
    //Honroutou,                  //  混老頭
    //Shousangen,                 //  小三元
    //DoubleReach,                //  ダブルリーチ
    //Sansyokudoupon,             //  三色同ポン
    //Sankantsu,                  //  三槓子
    // 
    ////  2役特殊
    //Chitoitsu,                  //  七対子
    // 
    ////  3役
    //Junchan,                    //  ジュンチャン
    //Honitsu,                    //  混一色
    //Ryanpekou,                  //  リャンペーコー
    public class YakuRyanpekou : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, Context context, Player player, ref YakuResult result)
        {
            //  TODO
            return 0;
        }
    }
    // 
    ////  6役
    //Chinnitsu,                  //  清一色
    // 
    ////  役満
    //Daisangen,                  //  大三元
    //Suannkou,                   //  四暗刻
    //Surenkou,                   //  四連刻
    //Sukantsu,                   //  四槓子
    //Tsuiso,                     //  字一色
    //Shousushi,                  //  小四喜
    //Daisushi,                   //  大四喜
    //Ryuisou,                    //  緑一色
    //Chinroutou,                 //  清老頭
    //Chuurenpoutou,              //  九蓮宝燈
    //Kokushimusou,               //  国士無双
    //Tenhou,                     //  天和
    //Chihou,                     //  地和
    //Renhou,                     //  人和
    //Daisharin,                  //  大車輪
    //Renchan8,                   //  八連荘

    public class YakuCheck
    {
        public void Initialize()
        {
            var type = typeof(IYakuChecker);
            var checkerTypes = System.Reflection.Assembly.GetAssembly(type)
                .GetTypes()
                .Where(x => !x.IsInterface && !x.IsAbstract && type.IsAssignableFrom(x))
                .ToArray();
            foreach (var checker in checkerTypes)
            {
                var instance = System.Activator.CreateInstance(checker) as IYakuChecker;
                _checkers.Add(instance);
            }
        }



        private List<IYakuChecker> _checkers = new List<IYakuChecker>();
    }
}