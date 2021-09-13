using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mahjong
{
    public class AgariCheck
    {
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
            if (IsOKOfChitoitsu(pais, ref pattern))
            {
                return true;
            }
            if (IsOKOfOtherwise(pais, ref pattern))
            {
                return true;
            }
            return false;
        }

        public bool IsOKOfKokushimusou(IEnumerable<DistributedPai> pais, ref AgariPattern pattern)
        {
            //  TODO
            return false;
        }
        public bool IsOKOfChitoitsu(IEnumerable<DistributedPai> pais, ref AgariPattern pattern)
        {
            //  TODO
            return false;
        }
        public bool IsOKOfOtherwise(IEnumerable<DistributedPai> pais, ref AgariPattern pattern)
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
                var mentsuHead = new Mentsu();
                var agariPattern = new AgariPattern();
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
                                mentsuList.Concat(furoOnly);

                                //  アガリパターン登録
                                agariPattern.Add(mentsuList);
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
                                mentsuList.Concat(furoOnly);

                                //  アガリパターン登録
                                agariPattern.Add(mentsuList);
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
                                mentsuList.Concat(furoOnly);

                                //  アガリパターン登録
                                agariPattern.Add(mentsuList);
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
                                mentsuList.Concat(furoOnly);

                                //  アガリパターン登録
                                agariPattern.Add(mentsuList);
                            }
                        }
                    }
                }
            }
            return ret;
        }
        private IEnumerable<DistributedPai> RemovePai(IEnumerable<DistributedPai> pais, int serial)
        {
            var remove = pais.FirstOrDefault(p => p.Serial == serial);
            if (remove == null)
            {
                return null;
            }
            return pais.Except(new List<DistributedPai>() { remove });
        }
        private IEnumerable<DistributedPai> RemovePai(IEnumerable<DistributedPai> pais, Group g, Id i)
        {
            var remove = pais.FirstOrDefault(p => p.IsSame(g, i));
            if (remove == null)
            {
                return null;
            }
            return pais.Except(new List<DistributedPai>() { remove });
            /*
            var removed = pais.Where(p =>
            {
                if (!found && p.IsSame(g, i))
                {
                    found = true;
                    return false;
                }
                return true;
            });
            var after = removed.Count();
            if (prev != after)
            {
                var l = removed.ToList();
                return l;
            }
            return null;
            */
        }
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
        private IEnumerable<DistributedPai> RemoveAllShuntsuFromTop(IEnumerable<DistributedPai> pais, ref MentsuList mentsuList)
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
                var ee = RemoveShuntsu(e, p.Group, p.Id, out mentsu);
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
        private IEnumerable<DistributedPai> RemoveAllShuntsuFromTail(IEnumerable<DistributedPai> pais, ref MentsuList mentsuList)
        {
            IEnumerable<DistributedPai> e = pais.Reverse();
            Group g = Group.Invalid;
            Id i = Id.Invalid;
            foreach (var p in e)
            {
                if (p.IsSame(g, i))
                {
                    continue;
                }

                Mentsu mentsu;
                var ee = RemoveShuntsu(e, p.Group, p.Id, out mentsu);
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
        private void RemoveFuroMentsu(IEnumerable<DistributedPai> pais, ref MentsuList mentsuList)
        {
            int s = 0;
            foreach(var p in pais)
            {
                if (p.IsFuro && s != p.SerialFuro)
                {
                    var eFuroMentsu = pais.Where(_ => p.SerialFuro == _.SerialFuro);
                    
                    var counts = eFuroMentsu.Count();
                    UnityEngine.Debug.Assert(counts == 3 || counts == 4);

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