using System.Collections;
using System.Collections.Generic;

namespace Mahjong
{
    public struct YakuInfo
    {
        public int _menzenYakuNum;      //  メンゼンでの役数
        public int _nakiYakuNum;        //  鳴いた時の役数
    }

    /// <summary>
    /// 役に関するデータベース
    /// ハードコードで用意する分は、デフォルトとなる値を想定する
    /// 本来はスクリプタブルオブジェクトにすることと、
    /// ユーザーがカスタマイズできるようにする
    /// </summary>
    public class Database
    {
        static private readonly Dictionary<Yaku, YakuInfo> _yakuInfos = new Dictionary<Yaku, YakuInfo>()
        {
            { Yaku.Tannyao, new YakuInfo() { _menzenYakuNum = 1, _nakiYakuNum = 1 } }
        };
    }
}