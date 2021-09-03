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
        int Calculate(MentsuList mentsu, ref YakuResult result);
    }
    ////  1役
    //Tannyao,                    //  タンヤオ
    public class TannyaoChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, ref YakuResult result)
        {
            foreach (var m in mentsu)
            {
                //  ヤオチュー牌の場合は非成立
                if (m.IsYaochuu)
                {
                    return 0;
                }

                //  喰いタン
                //  TODO
                if (true)
                {
                    if (m.IsFuro) 
                    {
                        return 0;
                    }
                }
            }
            return 0;
        }
    }
    //Pinhu,                      //  ピンフ
    public class PinhuChecker : IYakuChecker
    {
        public int Calculate(MentsuList mentsu, ref YakuResult result)
        {
            return 0;
        }
    }
    //Iipekou,                    //  イーペーコー
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