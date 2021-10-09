using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong
{
    /// <summary>
    /// アガリチェックする
    /// あくまでアガれる構成であるかを判定するだけ
    /// 役や符のチェックは別処理
    /// </summary>
    public class AgariCheck
    {
        /// <summary>
        /// アガリ成立判定
        /// </summary>
        /// <param name="pais">手持ち牌</param>
        /// <param name="pattern">アガリパターン（複数ありえる）</param>
        /// <returns></returns>
        public bool IsOK(IEnumerable<DistributedPai> pais, ref AgariPattern pattern)
        {
            pattern.Reset();

            //  国士無双と七対子は同時に成立しないので
            //  ・別々にチェックして大丈夫
            //  ・先に国士無双をチェックして大丈夫
            if (IsOKOfKokushimusou(pais, ref pattern))
            {
                return true;
            }

            //  七対子が成立しても、二盃口の可能性があるため、通常の成立もチェックする
            if (IsOKOfChitoitsu(pais, ref pattern))
            {
                
            }
            if (IsOKOfOtherwise(pais, ref pattern))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 国士無双アガリ判定
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="agariPattern"></param>
        /// <returns></returns>
        public bool IsOKOfKokushimusou(IEnumerable<DistributedPai> pais, ref AgariPattern agariPattern)
        {
            //  副露しているものがあれば不成立
            var paisWithoutFuro = pais.Where(p => p.IsFuro).Any();
            if (paisWithoutFuro) return false;

            //  ヤオチュウ13種があるか調べる
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
            var keys = dic.Keys.ToList();
            foreach(var k in keys)
            {
                var count = pais.Where(_ => _.IsSame(k.Item1, k.Item2)).Count();
                if (count == 0) return false;

                dic[k] += count;
                if (dic[k] > 1)
                {
                    head = true;
                }
            }
            //  アタマが無いなら不成立
            if (!head) return false;

            //  メンツを構成する
            var e = pais;
            MentsuList result = new MentsuList();
            Mentsu mentsu = null;
            foreach(var key in dic.Keys)
            {
                if (2 == dic[key])
                {
                    e = RemoveHead(e, key.Item1, key.Item2, out mentsu);
                    if (e == null || mentsu == null)
                    {
                        return false;
                    }
                    result.Add(mentsu);
                }
                else if (1 == dic[key])
                {
                    e = RemovePai(e, key.Item1, key.Item2, out mentsu);
                    if (e == null || mentsu == null)
                    {
                        return false;
                    }
                    result.Add(mentsu);
                }
            }
            if (result.Count() != 13) throw new System.Exception(); //  13種揃っていないのはおかしい

            agariPattern.CopyIfNotContained(result);
            return true;
        }

        /// <summary>
        /// 七対子アガリ判定
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="agariPattern"></param>
        /// <returns></returns>
        public bool IsOKOfChitoitsu(IEnumerable<DistributedPai> pais, ref AgariPattern agariPattern)
        {
            //  副露しているものがあれば不成立
            var paisWithoutFuro = pais.Where(p => p.IsFuro).Any();
            if (paisWithoutFuro) return false;

            //  同じ牌が４つある場合は不成立
            foreach (var p in pais)
            {
                int n = 0;
                foreach (var pp in pais)
                {
                    if (p.IsSame(pp.Group, pp.Id))
                    {
                        n += 1;
                    }
                }
                if (n == 4)
                {
                    return false;
                }
            }

            //  アタマが7個あるか調べる
            MentsuList result = new MentsuList();
            foreach (var p in pais)
            {
                result.Clear();

                var e = pais;
                bool loop = true;
                do
                {
                    Mentsu mentsuHead = null;
                    
                    e = RemoveHead(e, e.ElementAt(0).Group, e.ElementAt(0).Id, out mentsuHead);
                    //  アタマが見つからないなら、不成立
                    if (e == null || mentsuHead == null)
                    {
                        return false;
                    }
                    //  アタマがあった場合はeが無くなるまで次のアタマを探し、
                    //  Mentsuが7個になったら七対子成立とする
                    if (e != null && mentsuHead != null)
                    {
                        result.Add(mentsuHead);
                        if (e.Count() == 0 && result.Count() == 7)
                        {
                            //  成立しているので登録する
                            agariPattern.CopyIfNotContained(result);
                            loop = false;
                        }
                    }
                } while (loop);
            }
            return true;
        }

        /// <summary>
        /// 国士無双、七対子以外のアガリ判定
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="agariPattern"></param>
        /// <returns></returns>
        public bool IsOKOfOtherwise(IEnumerable<DistributedPai> pais, ref AgariPattern agariPattern)
        {
            bool ret = false;

            //  副露しているものは除く
            var paisWithoutFuro = pais.Where(p => !p.IsFuro);

            //  副露しているものだけ抽出してMentsu化する
            MentsuList furoOnly = new MentsuList();
            RemoveFuroMentsu(pais.Except(paisWithoutFuro), ref furoOnly);

            //  １つずつ調べる
            {
                var mentsuList = new MentsuList();
                Mentsu mentsuHead = null;
                foreach (var pai in paisWithoutFuro)
                {
                    //  アタマを決める
                    //      paiをアタマにする

                    //  アタマを取り除く
                    var paisWithoutHead = RemoveHead(paisWithoutFuro, pai.Group, pai.Id, out mentsuHead);
                    if (paisWithoutHead == null || mentsuHead == null)
                    {
                        continue;
                    }

                    //  コーツを取り除き、前からシュンツを取り除く
                    {
                        mentsuList.Clear();

                        var e = RemoveAllKotsu(paisWithoutHead, ref mentsuList);
                        e = RemoveAllShuntsuFromTop(e, ref mentsuList);
                        if (e != null)
                        {
                            if (e.Count() == 0)
                            {
                                ret |= true;

                                //  アタマ登録
                                mentsuList.Add(mentsuHead);
                                //  副露があれば登録
                                mentsuList.AddRange(furoOnly);

                                //  アガリパターン登録
                                agariPattern.CopyIfNotContained(mentsuList);
                            }
                        }
                    }
                    //  前からシュンツを取り除き、コーツを取り除く
                    {
                        mentsuList.Clear();

                        var e = RemoveAllShuntsuFromTop(paisWithoutHead, ref mentsuList); 
                        e = RemoveAllKotsu(e, ref mentsuList);
                        if (e != null)
                        {
                            if (e.Count() == 0)
                            {
                                ret |= true;

                                //  アタマ登録
                                mentsuList.Add(mentsuHead);
                                //  副露があれば登録
                                mentsuList.AddRange(furoOnly);

                                //  アガリパターン登録
                                agariPattern.CopyIfNotContained(mentsuList);
                            }
                        }
                    }
                    //  コーツを取り除き、後ろからシュンツを取り除く
                    {
                        mentsuList.Clear();

                        var e = RemoveAllKotsu(paisWithoutHead, ref mentsuList);
                        e = RemoveAllShuntsuFromTop(e, ref mentsuList);
                        if (e != null)
                        {
                            if (e.Count() == 0)
                            {
                                ret |= true;

                                //  アタマ登録
                                mentsuList.Add(mentsuHead);
                                //  副露があれば登録
                                mentsuList.AddRange(furoOnly);

                                //  アガリパターン登録
                                agariPattern.CopyIfNotContained(mentsuList);
                            }
                        }
                    }
                    //  後ろからシュンツを取り除き、コーツを取り除く
                    {
                        mentsuList.Clear();

                        var e = RemoveAllShuntsuFromTail(paisWithoutHead, ref mentsuList);
                        e = RemoveAllKotsu(e, ref mentsuList);
                        if (e != null)
                        {
                            if (e.Count() == 0)
                            {
                                ret |= true;

                                //  アタマ登録
                                mentsuList.Add(mentsuHead);
                                //  副露があれば登録
                                mentsuList.AddRange(furoOnly);

                                //  アガリパターン登録
                                agariPattern.CopyIfNotContained(mentsuList);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// paisから指定牌を取り除いてあたらしいシーケンスを返す
        /// 無い場合はnullを返す
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="serial"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemovePai(IEnumerable<DistributedPai> pais, int serial)
        {
            var remove = pais.FirstOrDefault(p => p.Serial == serial);
            if (remove == null)
            {
                return null;
            }
            return pais.Except(new List<DistributedPai>() { remove });
        }

        /// <summary>
        /// paisから指定牌を取り除いてあたらしいシーケンスを返す
        /// 無い場合はnullを返す
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemovePai(IEnumerable<DistributedPai> pais, Group g, Id i)
        {
            var remove = pais.FirstOrDefault(p => p.IsSame(g, i));
            if (remove == null)
            {
                return null;
            }
            return pais.Except(new List<DistributedPai>() { remove });
        }

        /// <summary>
        /// paisから指定牌を取り除いてあたらしいシーケンスを返す
        /// 無い場合はnullを返す
        /// 取り除けた場合はMentsuとして返す
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="mentsu"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemovePai(IEnumerable<DistributedPai> pais, Group g, Id i, out Mentsu mentsu)
        {
            mentsu = null;

            var e1 = RemovePai(pais, g, i);
            if (e1 == null) return null;

            var removed = pais.Except(e1);

            mentsu = new Mentsu();
            mentsu.AddPai(removed.ElementAt(0), null, null, null);

            return e1;
        }

        /// <summary>
        /// paisから指定牌をアタマとして取り除いてあたらしいシーケンスを返す
        /// 無い場合はnullを返す
        /// アタマをMetsuとして返す
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="mentsu"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemoveHead(IEnumerable<DistributedPai> pais, Group g, Id i, out Mentsu mentsu)
        {
            mentsu = null;

            var e1 = RemovePai(pais, g, i);
            if (e1 == null) return null;

            var e2 = RemovePai(e1, g, i);
            if (e2 == null) return null;

            var head = pais.Except(e2);
            UnityEngine.Debug.Assert(head.Count() == 2);

            mentsu = new Mentsu();
            mentsu.AddPai(head.ElementAt(0), head.ElementAt(1), null, null);

            return e2;
        }

        /// <summary>
        /// paisから指定牌をコーツとして取り除いてあたらしいシーケンスを返す
        /// 無い場合はnullを返す
        /// コーツをMetsuとして返す
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="mentsu"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemoveKotsu(IEnumerable<DistributedPai> pais, Group g, Id i, out Mentsu mentsu)
        {
            mentsu = null;

            var e1 = RemovePai(pais, g, i);
            if (e1 == null) return null;

            var e2 = RemovePai(e1, g, i);
            if (e2 == null) return null;

            var e3 = RemovePai(e2, g, i);
            if (e3 == null) return null;

            var kotsu = pais.Except(e3);
            UnityEngine.Debug.Assert(kotsu.Count() == 3);

            mentsu = new Mentsu();
            mentsu.AddPai(kotsu.ElementAt(0), kotsu.ElementAt(1), kotsu.ElementAt(2), null);
            
            return e3;
        }

        /// <summary>
        /// paisにある牌を1つずつチェックしてコーツとして取り除いてあたらしいシーケンスを返す
        /// 無い場合はnullを返す
        /// コーツをMetsuとして登録してMetsuListを返す
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="mentsuList"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemoveAllKotsu(IEnumerable<DistributedPai> pais, ref MentsuList mentsuList)
        {
            IEnumerable<DistributedPai> e = pais;
            Group g = Group.Invalid;
            Id i = Id.Invalid;
            foreach (var p in e)
            {
                if (p.IsSame(g, i))
                {
                    continue;
                }

                Mentsu mentsu;
                var ee = RemoveKotsu(e, p.Group, p.Id, out mentsu);
                if (ee != null)
                {
                    e = ee;

                    UnityEngine.Debug.Assert(mentsu != null);
                    mentsuList.Add(mentsu);
                }
                g = p.Group;
                i = p.Id;
            }
            return e;
        }

        /// <summary>
        /// paisの中から指定牌を先頭としたシュンツを取り除いてあたらしいシーケンスを返す
        /// 無い場合はnullを返す
        /// シュンツをMentsuとして返す
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="g"></param>
        /// <param name="i"></param>
        /// <param name="mentsu"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemoveShuntsu(IEnumerable<DistributedPai> pais, Group g, Id i, out Mentsu mentsu)
        {
            mentsu = null;

            if (g == Group.Jihai) return null;

            var e1 = RemovePai(pais, g, i);
            if (e1 == null) return null;

            var e2 = RemovePai(e1, g, i + 1);
            if (e2 == null) return null;

            var e3 = RemovePai(e2, g, i + 2);
            if (e3 == null) return null;

            var kotsu = pais.Except(e3);
            UnityEngine.Debug.Assert(kotsu.Count() == 3);

            mentsu = new Mentsu();
            mentsu.AddPai(kotsu.ElementAt(0), kotsu.ElementAt(1), kotsu.ElementAt(2), null);

            return e3;
        }

        /// <summary>
        /// paisの中から１つずつチェックしてシュンツを取り除きあたらしいシーケンスを返す
        /// シュンツがあればMentuListへ登録する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="mentsuList"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemoveAllShuntsuInternal(IEnumerable<DistributedPai> pais, ref MentsuList mentsuList)
        {
            var e = pais;
            foreach (var p in e)
            {
                Mentsu mentsu;
                var ee = RemoveShuntsu(e, p.Group, p.Id, out mentsu);
                if (ee != null)
                {
                    e = ee;

                    UnityEngine.Debug.Assert(mentsu != null);
                    mentsu.Sort((a, b) => a.CompareTo(b));
                    mentsuList.Add(mentsu);
                }
            }
            return e;
        }
        /// <summary>
        /// paisの先頭からシュンツを取り除きあたらしいシーケンスを返す
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="mentsuList"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemoveAllShuntsuFromTop(IEnumerable<DistributedPai> pais, ref MentsuList mentsuList)
        {
            return RemoveAllShuntsuInternal(pais, ref mentsuList);
        }
        /// <summary>
        /// paisの反対巡からシュンツを取り除きあたらしいシーケンスを返す
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="mentsuList"></param>
        /// <returns></returns>
        private IEnumerable<DistributedPai> RemoveAllShuntsuFromTail(IEnumerable<DistributedPai> pais, ref MentsuList mentsuList)
        {
            IEnumerable<DistributedPai> e = pais.Reverse();
            return RemoveAllShuntsuInternal(pais, ref mentsuList);
        }
        /// <summary>
        /// paisの中から副露メンツを取り除き、mentsuListへ追加する
        /// </summary>
        /// <param name="pais"></param>
        /// <param name="mentsuList"></param>
        private void RemoveFuroMentsu(IEnumerable<DistributedPai> pais, ref MentsuList mentsuList)
        {
            int s = 0;
            foreach(var p in pais)
            {
                if (p.IsFuro && s != p.SerialFuro)
                {
                    var eFuroMentsu = pais.Where(_ => p.SerialFuro == _.SerialFuro);
                    
                    var counts = eFuroMentsu.Count();
                    if (!(counts == 3 || counts == 4)) throw new System.Exception();

                    var mentsu = new Mentsu();
                    if (counts == 3)
                    {
                        mentsu.AddPai(eFuroMentsu.ElementAt(0), eFuroMentsu.ElementAt(1), eFuroMentsu.ElementAt(2), null);
                    }
                    else
                    {
                        mentsu.AddPai(eFuroMentsu.ElementAt(0), eFuroMentsu.ElementAt(1), eFuroMentsu.ElementAt(2), eFuroMentsu.ElementAt(3));
                    }
                    mentsuList.Add(mentsu);

                    s = p.SerialFuro;
                }
            }
        }
    }
}