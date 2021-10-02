using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong
{
    /// <summary>
    /// ひとメンツの構成
    /// </summary>
    public class Mentsu : List<DistributedPai>
    {
        /// <summary>
        /// コンストラクタ
        /// 最大４つまでしかAddできない
        /// </summary>
        public Mentsu()
            : base(capacity: 4)
        {

        }

        /// <summary>
        /// パイの追加
        /// 引数はp1から詰めてもらう
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        public void AddPai(DistributedPai p1, DistributedPai p2, DistributedPai p3, DistributedPai p4)
        {
            if (p1 == null) throw new System.ArgumentNullException("p1 is null.");
            if (p1 != null && p2 == null && p3 != null && p4 == null) throw new System.ArgumentNullException("p2 is null. but p3 is not null.");  //  p2がnullなのに、p3がnullでない
            if (p1 != null && p2 == null && p3 == null && p4 != null) throw new System.ArgumentNullException("p2 is null. but p4 is not null.");  //  p2がnullなのに、p4がnullでない
            if (p1 != null && p2 != null && p3 == null && p4 != null) throw new System.ArgumentNullException("p3 is null. but p4 is not null.");  //  p3がnullなのに、p4がnullでない

            base.Add(p1);
            if (p2 != null) base.Add(p2);
            if (p3 != null) base.Add(p3);
            if (p4 != null) base.Add(p4);
        }

        /// <summary>
        /// List型を継承しているので、安易にAddさせないための措置
        /// 通常はAddPaiを使う
        /// </summary>
        /// <param name="pai"></param>
        [System.Obsolete]
        public new void Add(DistributedPai pai)
        {
            throw new System.NotSupportedException();
        }

        /// <summary>
        /// すべて同じ柄か
        /// ただし1枚時のみは、falseとする
        /// </summary>
        public bool IsSameGroup
        {
            get
            {
                if (Count < 1) return false;

                var g = base[0].Group;
                foreach (var e in this)
                {
                    if (g != e.Group) { return false; }
                }
                return true;
            }
        }
        /// <summary>
        /// 同じメンツ構成か
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool IsSame(Mentsu m)
        {
            if (m.Count() != this.Count())
            {
                return false;
            }
            for (int i=0; i< m.Count(); ++i)
            {
                if (!m[i].IsSame(this[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// シュンツか？
        /// </summary>
        public bool IsShuntsu
        {
            get
            {
                //  シュンツは3つで成立するので3つ以外ならfalse
                if (base.Count != 3)
                {
                    return false;
                }
                //  シュンツはマンズ、ピンズ、ソウズのみで成立するので字牌ならfalse
                if (base[0].Group == Group.Jihai
                    || base[1].Group == Group.Jihai
                    || base[2].Group == Group.Jihai)
                {
                    return false;
                }
                //  ズがそろっていないならfalse
                if (!IsSameGroup)
                {
                    return false;
                }
                //  2つ目3つ目のパイが連続になっていないならfalse
                var n1 = base[0].Id;
                var n2 = n1 + 1;
                var n3 = n1 + 2;
                if (base[1].Id != n2
                    || base[2].Id != n3)
                {
                    return false;
                }
                //  シュンツである
                return true;
            }
        }

        /// <summary>
        /// 刻子か？
        /// </summary>
        public bool IsKotsu
        {
            get
            {
                //  3つでなければならない
                if (Count != 3) return false;
                //  同じズでなければならない
                if (!IsSameGroup) return false;
                //  同じ数字でなければならない
                if (base[0].Id != base[1].Id || base[0].Id != base[2].Id) return false;
                //  刻子である
                return true;
            }
        }

        /// <summary>
        /// 暗刻か？
        /// </summary>
        public bool IsAnnko
        {
            get
            {
                //  刻子でなければならない
                if (!IsKotsu) return false;
                //  メンゼンでなければならない
                if (base[0].IsTrashed || base[1].IsTrashed || base[2].IsTrashed) return false;
                //  暗刻である
                return true;
            }
        }

        /// <summary>
        /// 槓子か？
        /// </summary>
        public bool IsKantsu
        {
            get
            {
                //  4つでなければならない
                if (Count != 4) return false;
                //  同じズでなければならない
                if (!IsSameGroup) return false;
                //  同じ数字でなければならない
                if (base[0].Id != base[1].Id || base[0].Id != base[2].Id || base[0].Id != base[3].Id) return false;
                //  槓子である
                return true;
            }
        }

        /// <summary>
        /// 暗槓か？
        /// </summary>
        public bool IsAnnkantsu
        {
            get
            {
                //  槓子でなければならない
                if (!IsKantsu) return false;
                //  メンゼンでなければならない
                if (base[0].IsTrashed || base[1].IsTrashed || base[2].IsTrashed || base[3].IsTrashed) return false;
                //  暗槓である
                return true;
            }
        }

        /// <summary>
        /// アタマか？
        /// </summary>
        public bool IsHead
        {
            get
            {
                //  2つでなければならない
                if (Count != 2) return false;
                //  同じズ、同じ数字でなければならない
                if (!IsSameGroup) return false;
                if (base[0].Id != base[1].Id) return false;
                return true;
            }
        }
        /// <summary>
        /// 副露したメンツか？
        /// </summary>
        public bool IsFuro
        {
            get 
            {
                //  3つ以上でなければならない
                var c = Count;
                if (c < 3) return false;
                var tempc = 0;
                foreach (var p in this)
                {
                    if (p.IsFuro) tempc += 1;
                }
                if (tempc == 0)
                {
                    //  副露していない
                    return false;
                }
                if (c != tempc) throw new System.Exception();
                return true;
            }
        }
        /// <summary>
        /// 鳴いたメンツか？
        /// </summary>
        public bool IsNaki
        {
            get
            {
                //  3つ以上でなければならない
                if (Count < 3) return false;
                //  シュンツかコーツかカンツでなければいけない
                if (!(IsShuntsu || IsKantsu || IsKotsu))
                {
                    return false;
                }
                //  捨てられていないといけない
                var naki = this.Any(p => p.IsTrashed);
                return naki;
            }
        }
        /// <summary>
        /// 三元牌のメンツか？
        /// アタマ、コーツ、カンツの場合のみtrueとなりえる
        /// </summary>
        public bool IsSangen
        {
            get
            {
                if (Count < 2) return false;
                //  シュンツであってはいけない
                if (IsShuntsu) return false;
                //  字牌でなければならない
                if (base[0].Group != Group.Jihai) return false;
                //  同じグループになっていなければならない
                if (!IsSameGroup) return false;
                //  三元牌でなければならない
                if (base[0].Id == Id.Chun
                    || base[0].Id == Id.Haku
                    || base[0].Id == Id.Hatsu)
                {
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// 風牌のメンツか？
        /// アタマ、コーツ、カンツの場合のみtrueとなりえる
        /// </summary>
        public bool IsKaze
        {
            get
            {
                if (Count < 2) return false;
                //  シュンツであってはいけない
                if (IsShuntsu) return false;
                //  字牌でなければならない
                if (base[0].Group != Group.Jihai) return false;
                //  同じグループになっていなければならない
                if (!IsSameGroup) return false;
                //  三元牌でなければならない
                if (base[0].Id == Id.Ton
                    || base[0].Id == Id.Nan
                    || base[0].Id == Id.Sha
                    || base[0].Id == Id.Pei)
                {
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// ヤオチュー牌のメンツか？
        /// </summary>
        public bool IsYaochuu
        {
            get
            {
                //  シュンツなら1、9を含まなければならない
                if (IsShuntsu)
                {
                    if (base[0].Id == Id.N1 || base[2].Id == Id.N9)
                    {
                        return true;
                    }
                }
                //  アタマ、刻子、槓子なら1、9、字牌でなければならない
                else if (IsHead || IsKotsu || IsKantsu)
                {
                    if (base[0].Group == Group.Jihai)
                    {
                        return true;
                    }
                    else if (base[0].Id == Id.N1 || base[0].Id == Id.N9)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// ソウズメンツもしくは撥か？
        /// 緑一色用
        /// </summary>
        public bool IsGreen
        {
            get
            {
                if (!IsSameGroup) return false;
                if (IsShuntsu)
                {
                    if (base[0].Group != Group.Souz) return false;
                    if (this.Any(_ => _.Id == Id.N1 || _.Id == Id.N5 || _.Id == Id.N7 || _.Id == Id.N9)) return false;
                }
                else if (IsHead || IsKotsu || IsKantsu)
                {
                    if (base[0].Group == Group.Jihai)
                    {
                        if (base[0].Id != Id.Hatsu) return false;
                    }
                    else if (base[0].Group != Group.Souz)
                    {
                        return false;
                    }
                    else if (base[0].Group == Group.Souz)
                    {
                        if (this.Any(_ => _.Id == Id.N1 || _.Id == Id.N5 || _.Id == Id.N7 || _.Id == Id.N9)) return false;
                    }
                }
                return true;
            }
        }
        /// <summary>
        /// 上がり牌をもつメンツか？
        /// ロン上がり、ツモアガリは問わない
        /// </summary>
        public bool IsAgariMentsu
        {
            get
            {
                bool f = false;
                foreach(var p in this)
                {
                    if (p.IsRonAgari || p.IsTsumoAgari)
                    {
                        f = true;
                        break;
                    }
                }
                return f;
            }
        }
        /// <summary>
        /// XX, XX, XXのような構成で文字列出力
        /// 副露しているものは(XX, XX, XX)と出力する
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            var furo = IsFuro;
            if (furo) sb.Append("(");
            foreach (var p in this)
            {
                sb.Append(p.ToShortString());
                if (p != this.Last()) 
                {
                    sb.Append(",");
                }
            }
            if (furo) sb.Append(")");
            return sb.ToString();
        }
    }

    /// <summary>
    /// Mentsuクラスを保持するリスト
    /// </summary>
    public class MentsuList : List<Mentsu>
    {
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MentsuList()
            : base(capacity: 13) //  国士無双用
        {

        }
        /// <summary>
        /// 上がりメンツを取得
        /// 無いのに呼び出した場合は、エラーとする
        /// </summary>
        public Mentsu AgariMentsu
        {
            get
            {
                foreach (var m in this)
                {
                    if (m.IsAgariMentsu)
                    {
                        return m;
                    }
                }
                throw new System.Exception();
            }
        }
        /// <summary>
        /// コーツ、カンツを持っているか？
        /// </summary>
        public bool HasKotsuOrKantsu
        {
            get
            {
                return this.Where(m => m.IsKantsu || m.IsKotsu).Any();
            }
        }
        /// <summary>
        /// コーツ、カンツでフィルター
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Mentsu> FilterKotsuKantsu()
        {
            return this.Where(_ => _.IsKotsu || _.IsKantsu);
        }
        /// <summary>
        /// 指定のIdでコーツ、カンツをフィルター
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Mentsu> FilterKotsuKantsuWithId(Id id)
        {
            return this.FilterKotsuKantsu().Where(_ => _[0].Id == id);
        }

        /// <summary>
        /// メンツリスト同士を比較する
        /// 同じ構成ならtrue
        /// Mentsuの並び順、Paiの並び順は無関係とする
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool IsSameMentsu(MentsuList lhs, MentsuList rhs)
        {
            if (lhs.Count() != rhs.Count()) return false;

            foreach (var lm in lhs)
            {
                bool hasSame = false;
                foreach (var rm in rhs)
                {
                    if (lm.IsSame(rm))
                    {
                        hasSame |= true;
                    }
                }
                if (!hasSame) { return false; }
            }
            return true;
        }
        /// <summary>
        /// XX, XX, XX | YY, YY, YY | のような文字列になる
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var m in this) 
            {
                sb.Append(m.ToString());
                if (this.Last() != m)   sb.Append(" | ");
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// アガリパターン保持
    /// </summary>
    public class AgariPattern
    {
        /// <summary>
        /// MentsuListをコピーして保持する
        /// アガリ構成数でないとエラーになる
        /// </summary>
        /// <param name="pattern">コピー元</param>
        public void CopyIfNotContained(MentsuList pattern)
        {
            if (IsContained(pattern))
            {
                return;
            }
            Copy(pattern);
        }
        /// <summary>
        /// コピーする
        /// 重複しているかはチェックしないので、IsContainedを必要に応じて呼び出すこと
        /// </summary>
        /// <param name="pattern"></param>
        private void Copy(MentsuList pattern)
        {
            if (pattern.Count() != 5        //  通常メンツ
                && pattern.Count() != 7     //  七対子
                && pattern.Count() != 13)   //  国士無双
            {
                throw new System.Exception();
            }

            MentsuList copy = new MentsuList();
            foreach (var m in pattern)
            {
                copy.Add(m);
            }
            
            _patterns.Add(copy);
        }
        /// <summary>
        /// そのメンツ構成をすでに登録しているか？
        /// </summary>
        /// <param name="mentsu"></param>
        /// <returns></returns>
        public bool IsContained(MentsuList mentsu)
        {
            foreach (var p in _patterns)
            {
                if (MentsuList.IsSameMentsu(p, mentsu))
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// アガリパターンをクリアする
        /// </summary>
        public void Reset()
        {
            _patterns.Clear();
        }
        /// <summary>
        /// アガリパターン取得
        /// </summary>
        public IEnumerable<MentsuList> Patterns
        {
            get { return _patterns; }
        }
        private List<MentsuList> _patterns = new List<MentsuList>();
    }
}

