using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 単語の英字化ルール
/// ・シ＝shi（siはｘ）
/// ・ツ＝tsu（tuはｘ）
/// ・ン＝n（nnはｘ）
/// ・シャシュショ＝sha,shu,sho（sya,syu,syoはｘ）
/// ・チャチュチョ＝ch,chu,cho（cya,cyu,cyoはｘ）
/// </summary>
namespace Mahjong
{
    /// <summary>
    /// 絵柄
    /// </summary>
    public enum Group
    {
        Manz = 0,   //  マンズ
        Pinz,       //  ピンズ
        Souz,       //  ソウズ
        Jihai,      //  字牌

        Invalid = -1,
    }

    /// <summary>
    /// 書かれている内容
    /// </summary>
    public enum Id
    {
        N1,         //  1
        N2,         //  2
        N3,         //  3
        N4,         //  4
        N5,         //  5
        N6,         //  6
        N7,         //  7
        N8,         //  8
        N9,         //  9

        Ton,        //  東
        Nan,        //  南
        Sha,        //  西
        Pei,        //  北

        Haku,       //  白
        Hatsu,      //  撥
        Chun,       //  中

        Invalid = -1,
    }

    /// <summary>
    /// 定数
    /// </summary>
    public enum Constants
    {
        /// <summary>
        /// 各絵柄の種類数
        /// </summary>
        MaxGroup = 4,
        /// <summary>
        /// 字牌以外の数
        /// </summary>
        MaxGroupNumber = 3,

        /// <summary>
        /// 書かれている内容の種類数
        /// </summary>
        MaxId = 16,
        /// <summary>
        /// 数字の数
        /// </summary>
        MaxIdNumber = 9,
        /// <summary>
        /// 東西南北
        /// </summary>
        MaxIdDirection = 4,
        /// <summary>
        /// 三元牌数
        /// </summary>
        MaxIdSangen = 3,
        /// <summary>
        /// 字牌種類数
        /// </summary>
        MaxIdJihai = MaxIdDirection + MaxIdSangen,

        /// <summary>
        /// 数字牌の総数
        /// </summary>
        MaxPaiNumberCount = MaxIdNumber * MaxIdDirection * MaxGroupNumber,
        /// <summary>
        /// 字牌の総数
        /// </summary>
        MaxPaiJihaiCount = MaxIdDirection * MaxIdDirection + MaxIdSangen * MaxIdDirection,
        /// <summary>
        /// パイの最大数
        /// </summary>
        MaxPaiCount = MaxPaiNumberCount + MaxPaiJihaiCount,

        /// <summary>
        /// 通常の所持数
        /// </summary>
        DefaultHolderCount = 3 * 4 + 2,    //  1メンツ(3枚) * 4 + アタマ

        /// <summary>
        /// 最大所持数
        /// </summary>
        MaxHolderCount = 16 + 2,       //  槓子(4枚) * 4 + アタマ
    }
    

    /// <summary>
    /// 役
    /// </summary>
    public enum Yaku
    {
        //  1役
        Tanyao,                     //  タンヤオ
        Pinhu,                      //  ピンフ
        Iipekou,                    //  イーペーコー
        Reach,                      //  リーチ
        Ippatsu,                    //  一発
        Menzentsumo,                //  ツモ
        Chankanhou,                 //  チャンカンホウ
        Rinshan,                    //  嶺上開花
        Haiteitsumo,                //  海底ツモ
        Haiteiron,                  //  海底ロン
        Haku,                       //  白
        Hatsu,                      //  撥
        Chun,                       //  中
        Ton,                        //  東
        Nan,                        //  南
        Sha,                        //  西
        Pei,                        //  北
        TonField,                   //  東　場風
        NanField,                   //  南　場風
        ShaField,                   //  西　場風 
        PeiField,                   //  北　場風

        //  2役
        Sanshoku,                   //  三色同順
        Ikkitsukan,                 //  一気通貫
        Toitoi,                     //  対々和
        Sanankou,                   //  三暗刻
        Sanrenkou,                  //  三連刻
        Chanta,                     //  チャンタ
        Honroutou,                  //  混老頭
        Shousangen,                 //  小三元
        DoubleReach,                //  ダブルリーチ
        Sanshokudoupon,             //  三色同ポン
        Sankantsu,                  //  三槓子
        
        //  2役特殊
        Chitoitsu,                  //  七対子

        //  3役
        Junchan,                    //  ジュンチャン
        Honitsu,                    //  混一色
        Ryanpekou,                  //  リャンペーコー

        //  6役
        Chinitsu,                   //  清一色

        //  役満
        Daisangen,                  //  大三元
        Suankou,                    //  四暗刻
        Surenkou,                   //  四連刻
        Sukantsu,                   //  四槓子
        Tsuisou,                    //  字一色
        Shousushi,                  //  小四喜
        Daisushi,                   //  大四喜
        Ryuisou,                    //  緑一色
        Chinroutou,                 //  清老頭
        Chuurenpoutou,              //  九蓮宝燈
        Kokushimusou,               //  国士無双
        Tenhou,                     //  天和
        Chihou,                     //  地和
        Renhou,                     //  人和
        Daisharin,                  //  大車輪
        Renchan8,                   //  八連荘

        MaxYaku,
    }
    public static class YakuExtention
    {
        public static bool IsYakuman(this Yaku yaku)
        {
            return yaku >= Yaku.Daisangen;  //  Yakuの定義順に依存するので注意
        }
    }
}

    
